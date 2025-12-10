namespace OtherMediator.Extensions.OpenTelemetry.Microsoft.DependencyInjection;

using System.Linq;
using global::Microsoft.Extensions.DependencyInjection;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Trace;
using OtherMediator.Contracts;
using OtherMediator.Extensions.Microsoft.DependencyInjection;

/// <summary>
/// Provides extension methods for logging OpenTelemetry instrumentation
/// related to OtherMediator in dependency containers and OpenTelemetry builders.
/// </summary>
public static class OtherMediatorOpenTelemetryExtensions
{
    /// <summary>
    /// Registers the services required to instrument OtherMediator with OpenTelemetry.
    /// - Adds a singleton of <see cref="MediatorInstrumentation"/>.
    /// - Inserts <see cref="OpenTelemetryPipelineBehavior{TRequest,TResponse}"/> at the beginning
    /// of the pipeline behaviors collection if it does not already exist.
    /// </summary>
    /// <param name="services">Service collection where instrumentation will be added.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddMediatorOpenTelemetry(this IServiceCollection services)
    {
        services.AddSingleton<MediatorInstrumentation>();

        var config = new MediatorConfiguration(services);

        if (!services.Any((ServiceDescriptor d) => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(OpenTelemetryPipelineBehavior<,>)))
        {
            services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(OpenTelemetryPipelineBehavior<,>), (ServiceLifetime)config.Lifetime));
        }

        return services;
    }

    /// <summary>
    /// Adds the OtherMediator trace source to the <see cref="TracerProviderBuilder"/>.
    /// Calls <see cref="TracerProviderBuilder.AddSource(string)"/> with the service name
    /// defined in <see cref="MediatorInstrumentation.SERVICE_NAME"/>.
    /// </summary>
    /// <param name="builder">Tracer provider builder to which the source will be added.</param>
    /// <returns>The same <see cref="TracerProviderBuilder"/> to allow chaining.</returns>
    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(MediatorInstrumentation.SERVICE_NAME);

        return builder;
    }

    /// <summary>
    /// Adds the OtherMediator meter to the <see cref="MeterProviderBuilder"/>.
    /// Calls <see cref="MeterProviderBuilder.AddMeter(string)"/> with the service name
    /// defined in <see cref="MediatorInstrumentation.SERVICE_NAME"/>.
    /// </summary>
    /// <param name="builder">Meter provider builder to which the meter will be added.</param>
    /// <returns>The same <see cref="MeterProviderBuilder"/> to allow chaining.</returns>
    public static MeterProviderBuilder AddMediatorInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(MediatorInstrumentation.SERVICE_NAME);

        return builder;
    }
}
