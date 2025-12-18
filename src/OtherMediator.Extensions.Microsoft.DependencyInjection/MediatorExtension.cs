namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

/// <summary>
/// Extension methods to register OtherMediator services into the Microsoft
/// dependency injection <see cref="IServiceCollection"/>.
/// </summary>
public static class MediatorExtension
{
    private static MediatorConfiguration _mediatorConfiguration = new();

    /// <summary>
    /// Adds OtherMediator to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register mediator services into.</param>
    /// <param name="config">An optional configuration action to customize <see cref="MediatorConfiguration"/>.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// When <see cref="MediatorConfiguration.UseExceptionHandler"/> is enabled, this method will ensure an
    /// <c>ErrorPipelineBehavior&lt;TRequest, TResponse&gt;</c> is registered (if not already present) and will
    /// then register the core <see cref="IMediator"/> implementation as a singleton.
    /// </remarks>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? config)
    {
        config?.Invoke(_mediatorConfiguration);

        if (_mediatorConfiguration.UseExceptionHandler)
        {
            if (!services.Any((ServiceDescriptor d) => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(ErrorPipelineBehavior<,>)))
            {
                services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(ErrorPipelineBehavior<,>), (ServiceLifetime)_mediatorConfiguration.Lifetime));
            }
        }

        services.AddCoreMediator(_mediatorConfiguration);

        return services;
    }

    /// <summary>
    /// Registers an open generic pipeline behavior.
    /// </summary>
    /// <param name="type">The behavior type implementing <see cref="IPipelineBehavior{,}"/>.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddOpenPipelineBehavior(this IServiceCollection services, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var behaviorInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (behaviorInterface is null)
        {
            throw new ArgumentException("Type must implement IPipelineBehavior<,>.", nameof(type));
        }

        if (!services.Any(s => s.ServiceType == typeof(IPipelineBehavior<,>) && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, (ServiceLifetime)_mediatorConfiguration.Lifetime));
        }

        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior for a specific request/response type.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <param name="type">The behavior type.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddPipelineBehavior<TRequest, TResponse>(this IServiceCollection services, Type type)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(type);

        var serviceType = typeof(IPipelineBehavior<TRequest, TResponse>);

        if (!services.Any(s => s.ServiceType == serviceType && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(serviceType, type, (ServiceLifetime)_mediatorConfiguration.Lifetime));
        }

        return services;
    }

    /// <summary>
    /// Registers the core <see cref="IMediator"/> implementation into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the mediator into.</param>
    private static void AddCoreMediator(this IServiceCollection services, IMediatorConfiguration mediatorConfiguration)
    {
        services.AddSingleton<IMediator>(sp =>
            new Mediator(new MicrosoftContainer(services), new MiddlewarePipeline(), mediatorConfiguration));
    }
}
