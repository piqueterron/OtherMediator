namespace OtherMediator.Extensions.OpenTelemetry.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

public static class OtherMediatorOpenTelemetryExtensions
{
    public static IServiceCollection AddMediatorOpenTelemetry(this IServiceCollection services, Dictionary<string, object>? attributes = null)
    {
        attributes ??= [];

        services.AddSingleton<MediatorInstrumentation>();

        var config = new MediatorConfiguration(services);

        if (!services.Any((ServiceDescriptor d) => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(OpenTelemetryPipelineBehavior<,>)))
        {
            services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(OpenTelemetryPipelineBehavior<,>), config.Lifetime));
        }

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