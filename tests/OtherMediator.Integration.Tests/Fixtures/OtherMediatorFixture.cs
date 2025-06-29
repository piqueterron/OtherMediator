namespace OtherMediator.Integration.Tests.Fixtures;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;
using OtherMediator.Extensions.OpenTelemetry.Microsoft.DependencyInjection;
using OtherMediator.Integration.Tests.Handlers;
using Xunit;

public class OtherMediatorFixture : IAsyncLifetime
{
    private DotNet.Testcontainers.Containers.IContainer _jaeger;
    private readonly Lazy<TestServer> _testServer;

    public TestServer Server => _testServer.Value;

    public OtherMediatorFixture()
    {
        _testServer = new Lazy<TestServer>(CreateApiServer);
    }

    private TestServer CreateApiServer()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = "OtherMediator.Test",
            EnvironmentName = Environments.Development
        });

        var otlpEndpoint = new Uri($"http://localhost:{_jaeger.GetMappedPublicPort(JaegerPort.OTL_GRPC)}");
        var version = typeof(OtherMediatorFixture).Assembly.GetName()?.Version!.ToString();

        builder.Services
            .AddRouting()
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(builder.Environment.ApplicationName, serviceVersion: version)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.instance.id"] = Guid.NewGuid().ToString(),
                    ["deployment.environment"] = builder.Environment.EnvironmentName
                }))
            .WithTracing(trace =>
            {
                trace.AddMediatorInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metric =>
            {
                metric.AddMediatorInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            });

        builder.Services
            .AddMediator(config =>
            {
                config.RegisterServicesFromAssembly<OtherMediatorFixture>();
                config.AddOpenPipelineBehavior(typeof(TestPipeline<,>));
            })
            .AddMediatorOpenTelemetry();

        builder.WebHost.UseTestServer();

        var app = builder.Build();

        app.UseRouting();

        app.MapGet("/mediator", async (IMediator mediator) =>
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