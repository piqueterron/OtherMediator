namespace OtherMediator.Integration.Tests.Fixtures;

using System.Threading.Tasks;
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

    public TestServer ApiServer()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(OtherMediatorFixture).Assembly.FullName,
            EnvironmentName = Environments.Development
        });

        var otlpEndpoint = new Uri($"http://localhost:{_jaeger.GetMappedPublicPort(JaegerPort.OTL_GRPC)}");

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

        app.MapPost("/mediator", async (IMediator mediator) =>
            await mediator.Send<TestRequest, TestResponse>(new TestRequest()));

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
        _jaeger = JaegerContainer.JaegerInitialize();
        await _jaeger.StartAsync();
    }
}