namespace OtherMediator.Integration.Tests.Fixtures;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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
    private DotNet.Testcontainers.Containers.IContainer? _jaeger;
    private TestServer _testServer;
    private HttpClient? _client;

    public HttpClient Client => _client ??= _testServer.CreateClient();

    private async Task<TestServer> CreateApiServerAsync()
    {
        var integrationTestInfo = AssemblyExtension.GetIntegrationTestInfo<OtherMediatorFixture>();

        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            ApplicationName = integrationTestInfo.ServiceName,
            EnvironmentName = integrationTestInfo.EnvironmentName
        });

        var otlpEndpoint = new Uri($"http://localhost:{_jaeger.GetMappedPublicPort(JaegerContainer.Ports.OTL_GRPC)}");
        var version = integrationTestInfo.ServiceVersion;

        builder.Services
            .AddRouting()
            .AddProblemDetails()
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(builder.Environment.ApplicationName, serviceVersion: version)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.instance.id"] = Guid.CreateVersion7().ToString(),
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
            .AddMediator()
            .AddMediatorOpenTelemetry()
            .AddExceptionHandler<GlobalExceptionHandler>();

        builder.WebHost.UseTestServer();

        var app = builder.Build();

        app.UseRouting();
        app.UseExceptionHandler();

        app.MapPost("/mediator", async (IMediator mediator, TestRequest request) =>
            await mediator.Send<TestRequest, TestResponse>(request));

        app.MapPost("/mediator-void", async (IMediator mediator, TestRequestUnit request) =>
            await mediator.Send(request));

        app.MapPost("/mediator-exception", async (IMediator mediator, TestExceptionRequest request) =>
            await mediator.Send<TestExceptionRequest, TestResponse>(request));

        app.MapPost("/notification", async (IMediator mediator, TestNotification request) =>
            await mediator.Publish(request));

        await app.StartAsync();

        return app.GetTestServer();
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        _testServer?.Dispose();

        if (_jaeger is not null)
        {
            await _jaeger.StopAsync();
            await _jaeger.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        _jaeger = JaegerContainer.JaegerInitialize();

        await _jaeger.StartAsync();

        _testServer = await CreateApiServerAsync();
    }
}
