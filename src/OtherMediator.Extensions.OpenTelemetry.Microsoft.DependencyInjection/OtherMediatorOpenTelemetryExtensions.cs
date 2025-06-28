namespace OtherMediator.Extensions.OpenTelemetry.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Resources;
using global::OpenTelemetry.Trace;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

public static class OtherMediatorOpenTelemetryExtensions
{
    public static IServiceCollection AddMediatorOpenTelemetry(this IServiceCollection services, Dictionary<string, object>? attributes = null)
    {
        attributes ??= [];

        services.AddSingleton<MediatorInstrumentation>();

        var config = new MediatorConfiguration(services);

        config.AddOpenPipelineBehavior(typeof(OpenTelemetryPipelineBehavior<,>));

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(MediatorInstrumentation.SERVICE_NAME, serviceVersion: MediatorInstrumentation.SERVICE_VERSION);
                resource.AddAttributes(attributes);
            });

        return services;
    }

    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(MediatorInstrumentation.SERVICE_NAME);

        return builder;
    }

    public static MeterProviderBuilder AddMediatorInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(MediatorInstrumentation.SERVICE_NAME);

        return builder;
    }
}