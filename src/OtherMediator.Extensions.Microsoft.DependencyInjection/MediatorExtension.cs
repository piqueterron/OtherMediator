namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

/// <summary>
/// Extension methods to register OtherMediator services into the Microsoft
/// dependency injection <see cref="IServiceCollection"/>.
/// </summary>
public static class MediatorExtension
{
    /// <summary>
    /// Adds OtherMediator to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register mediator services into.</param>
    /// <param name="config">An optional configuration action to customize <see cref="MediatorConfiguration"/>.
    /// If <c>null</c>, services will be discovered and registered automatically from assemblies.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// When <see cref="MediatorConfiguration.UseExceptionHandler"/> is enabled, this method will ensure an
    /// <c>ErrorPipelineBehavior&lt;TRequest, TResponse&gt;</c> is registered (if not already present) and will
    /// then register the core <see cref="IMediator"/> implementation as a singleton.
    /// </remarks>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? config)
    {
        var mediatorConfig = new MediatorConfiguration(services);

        if (config is null)
        {
            mediatorConfig.RegisterServicesFromAssemblies();
        }
        else
        {
            config(mediatorConfig);
        }

        if (mediatorConfig.UseExceptionHandler)
        {
            if (!services.Any((ServiceDescriptor d) => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(ErrorPipelineBehavior<,>)))
            {
                services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(ErrorPipelineBehavior<,>), (ServiceLifetime)mediatorConfig.Lifetime));
            }
        }

        services.AddCoreMediator(mediatorConfig);

        return services;
    }

    /// <summary>
    /// Adds OtherMediator to the provided <see cref="IServiceCollection"/> when the caller is
    /// performing manual service registrations.
    /// </summary>
    /// <param name="services">The service collection to register mediator services into.</param>
    /// <param name="config">An action that performs manual registration of required services into the <paramref name="services"/> collection.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddMediatorManualRegistration(this IServiceCollection services, Action<IServiceCollection> config)
    {
        config?.Invoke(services);

        var mediatorConfig = new MediatorConfiguration(services);

        services.AddCoreMediator(mediatorConfig);

        return services;
    }

    /// <summary>
    /// Registers the core <see cref="IMediator"/> implementation into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the mediator into.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    private static IServiceCollection AddCoreMediator(this IServiceCollection services, IMediatorConfiguration mediatorConfiguration)
    {
        services.AddSingleton<IMediator>(sp =>
            new Mediator(new MicrosoftContainer(services), new MiddlewarePipeline(), mediatorConfiguration));

        return services;
    }
}
