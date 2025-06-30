namespace OtherMediator.Integration.Tests.Fixtures;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
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
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private DotNet.Testcontainers.Containers.IContainer _jaeger;
    private TestServer _testServer;

    public HttpClient Client => _testServer.CreateClient();

    private async Task<TestServer> CreateApiServerAsync()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = "OtherMediator.Test",
            EnvironmentName = Environments.Development
        });

        if (_jaeger is null)
        {
            throw new InvalidOperationException("Jaeger container not initialized.");
        }

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
                    ["deployment.environment.name"] = builder.Environment.EnvironmentName,
                    ["host.name"] = Environment.MachineName
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

        app.MapPost("/mediator", async (IMediator mediator, TestRequest request) =>
            await mediator.Send<TestRequest, TestResponse>(request));

        await app.StartAsync();

        return app.GetTestServer();
    }

    public async Task DisposeAsync()
    {
        await _jaeger.StopAsync();
        await _jaeger.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_testServer is not null)
            {
                return;
            }

            _jaeger = JaegerContainer.JaegerInitialize();
            await _jaeger.StartAsync();
            _testServer = await CreateApiServerAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}