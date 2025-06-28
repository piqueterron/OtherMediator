namespace OtherMediator.Integration.Tests.Fixtures;

using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;
using OtherMediator.Extensions.OpenTelemetry.Microsoft.DependencyInjection;
using OtherMediator.Integration.Tests.Handlers;
using Xunit;

public class OtherMediatorFixture : IAsyncLifetime
{
    private DotNet.Testcontainers.Containers.IContainer _jaeger;

    public static class JaegerPort
    {
        public static (int Host, int Container) UI = (16686, 16686);
        public static (int Host, int Container) OtlGrpc = (4317, 4317);
        public static (int Host, int Container) OtlHttp = (4318, 4318);
        public static (int Host, int Container) Grpc = (14250, 14250);
    }

    public OtherMediatorFixture()
    {
        _jaeger = new ContainerBuilder()
            .WithImage("jaegertracing/all-in-one:latest")
            .WithName("jaeger")
            .WithPortBinding(JaegerPort.UI.Host, JaegerPort.UI.Container)
            .WithPortBinding(JaegerPort.OtlGrpc.Host, JaegerPort.OtlGrpc.Container)
            .WithPortBinding(JaegerPort.OtlHttp.Host, JaegerPort.OtlHttp.Container)
            .WithPortBinding(JaegerPort.Grpc.Host, JaegerPort.Grpc.Container)
            .WithEnvironment("COLLECTOR_OTLP_ENABLED", "true")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(JaegerPort.UI.Container)
                .UntilPortIsAvailable(JaegerPort.OtlGrpc.Container))
            .Build();
    }

    public TestServer ApiServer()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(OtherMediatorFixture).Assembly.FullName,
            EnvironmentName = Environments.Development
        });

        var otlpEndpoint = new Uri($"http://localhost:{_jaeger.GetMappedPublicPort(JaegerPort.OtlGrpc.Container)}");

        builder.Services
            .AddRouting()
            .AddOpenTelemetry()
            .WithTracing(trace =>
            {
                trace.AddSource(nameof(OtherMediatorFixture))
                    .AddMediatorInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metric =>
            {
                metric.AddMeter(nameof(OtherMediatorFixture))
                    .AddMediatorInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            });

        builder.Services
            .AddMediator(config => config.RegisterServicesFromAssembly<OtherMediatorFixture>())
            .AddMediatorOpenTelemetry();

        builder.WebHost.UseTestServer();

        var app = builder.Build();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/mediator", async (IMediator mediator) =>
                await mediator.Send<TestRequest, TestResponse>(new TestRequest()));
        });

        app.Start();

        return app.GetTestServer();
    }

    public async Task DisposeAsync()
    {
        await _jaeger.StopAsync();
        await _jaeger.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _jaeger.StartAsync();
    }
}