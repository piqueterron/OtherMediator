namespace OtherMediator.Extensions.OpenTelemetry;

using global::OpenTelemetry.Resources;
using Microsoft.Extensions.DependencyInjection;

public static class OtherMediatorOpenTelemetryExtensions
{
    private const string SERVICE_NAME = "OtherMediator";
    private const string SERVICE_VERSION = "1.0";

    public static IServiceCollection AddMediatorOpenTelemetry(this IServiceCollection services, Dictionary<string, object>? attributes = null)
    {
        attributes ??= [];

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(SERVICE_NAME, serviceVersion: SERVICE_VERSION);
                resource.AddEnvironmentVariableDetector();
                resource.AddAttributes(attributes);
            });

        return services;
    }
}
