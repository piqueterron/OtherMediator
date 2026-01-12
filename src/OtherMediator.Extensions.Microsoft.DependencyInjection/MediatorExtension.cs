namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator;
using OtherMediator.Contracts;

/// <summary>
/// Extension methods to register OtherMediator services into the Microsoft
/// dependency injection <see cref="IServiceCollection"/>.
/// </summary>
public static class MediatorExtension
{
    private static MediatorConfiguration _mediatorConfiguration;

    /// <summary>
    /// Gets the current <see cref="MediatorConfiguration"/> instance.
    /// </summary>
    public static MediatorConfiguration MediatorConfiguration => _mediatorConfiguration ??= new MediatorConfiguration();

    /// <summary>
    /// Adds OtherMediator to the provided <see cref="IServiceCollection"/>.
    /// For example, to register OtherMediator use:
    /// <c>services.AddMediator(config => config.UseExceptionHandler = true);</c>
    /// </summary>
    /// <param name="services">The service collection to register mediator services into.</param>
    /// <param name="config">An optional configuration action to customize <see cref="MediatorConfiguration"/>.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// When <see cref="MediatorConfiguration.UseExceptionHandler"/> is enabled, this method ensures an
    /// <c>ErrorPipelineBehavior&lt;TRequest, TResponse&gt;</c> is registered (if not already present) and then
    /// registers the core <see cref="IMediator"/> implementation as a singleton.
    /// 
    /// <para><b>Important:</b> Handlers are not automatically registered by this method. You must manually
    /// register any <see cref="IRequestHandler{TRequest, TResponse}"/> or <see cref="INotificationHandler{TNotification}"/> 
    /// implementations in the service collection, or rely on a source-generation-based registration mechanism.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var services = new ServiceCollection();
    /// services.AddMediator(config => config.UseExceptionHandler = true);
    /// </code>
    /// </example>
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? config)
    {
        var opt = new MediatorConfiguration();
        config?.Invoke(opt);

        if (opt.UseExceptionHandler)
        {
            if (!services.Any(d => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(ErrorPipelineBehavior<,>)))
            {
                services.Insert(0, ServiceDescriptor.Describe(typeof(IPipelineBehavior<,>), typeof(ErrorPipelineBehavior<,>), ServiceLifetime.Singleton));
            }
        }

        _mediatorConfiguration = opt;

        services.AddCoreMediator();

        return services;
    }

    /// <summary>
    /// Registers an open generic pipeline behavior.
    /// For example, to add a custom behavior use:
    /// <code>
    /// <c>services.AddOpenPipelineBehavior(typeof(MyGlobalBehavior&lt;,&gt;));</c>
    /// </code>
    /// </summary>
    /// <param name="type">The behavior type implementing <see cref="IPipelineBehavior{,}"/> for request/response or <see cref="IPipelineBehavior{}"/> for notifications.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method allows registering a pipeline behavior that applies to all request/response types and notifications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> does not implement <see cref="IPipelineBehavior{,}"/>.</exception>
    /// <example>
    /// <code>
    /// services.AddOpenPipelineBehavior(typeof(MyGlobalBehavior&lt;,&gt;));
    /// </code>
    /// </example>
    public static IServiceCollection AddOpenPipelineBehavior(this IServiceCollection services, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var behaviorInterfaceRequest = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (behaviorInterfaceRequest is not null && !services.Any(s => s.ServiceType == typeof(IPipelineBehavior<,>) && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, ServiceLifetime.Singleton));
        }

        var behaviorInterfaceNotification = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<>));

        if (behaviorInterfaceNotification is not null && !services.Any(s => s.ServiceType == typeof(IPipelineBehavior<>) && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<>), type, ServiceLifetime.Singleton));
        }

        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior for a specific <typeparamref name="TRequest"/>/<typeparamref name="TResponse"/> type.
    /// For example, to add a custom behavior for <c>MyRequest</c> and <c>MyResponse</c>, use:
    /// <code>
    /// <c>services.AddPipelineBehavior&lt;MyRequest, MyResponse&gt;(typeof(MyCustomBehavior&lt;MyRequest, MyResponse&gt;));</c>
    /// </code>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the pipeline behavior will be added.</param>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <param name="type">The behavior type.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This allows you to attach custom behavior only for specific requests.
    /// It prevents duplicate registrations by checking if the same service type and implementation type already exist.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <example>
    /// <code>
    /// services.AddPipelineBehavior&lt;MyRequest, MyResponse&gt;(typeof(MyCustomBehavior&lt;MyRequest, MyResponse&gt;));
    /// </code>
    /// </example>
    public static IServiceCollection AddPipelineBehavior<TRequest, TResponse>(this IServiceCollection services, Type type)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(type);

        var serviceType = typeof(IPipelineBehavior<TRequest, TResponse>);

        if (!services.Any(s => s.ServiceType == serviceType && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(serviceType, type, ServiceLifetime.Singleton));
        }

        return services;
    }

    /// <summary>
    /// Adds a singleton pipeline behavior of the specified concrete type to handle the specific <typeparamref name="TNotification"/> 
    /// to the service collection, but only if an identical registration does not already exist.
    /// For example, to add a custom behavior for <c>MyNotification</c>, use:
    /// <code>
    /// <c>services.AddPipelineBehavior&lt;MyNotification&gt;(typeof(MyCustomBehavior&lt;MyNotification&gt;));</c>
    /// </code>
    /// </summary>
    /// <typeparam name="TNotification">The specific notification type that the pipeline behavior will handle. Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the pipeline behavior will be added.</param>
    /// <param name="type">The concrete type implementing <see cref="IPipelineBehavior{TNotification}"/> to register as a singleton. 
    /// This type must be compatible with <typeparamref name="TNotification"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method allows attaching custom pipeline behavior for a specific notification type.
    /// It prevents duplicate registrations by checking if the same service type and implementation type already exist.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> does not implement <see cref="IPipelineBehavior{TNotification}"/>.</exception>
    /// <example>
    /// <code>
    /// services.AddPipelineBehavior&lt;MyNotification&gt;(typeof(MyCustomBehavior&lt;MyNotification&gt;));
    /// </code>
    /// </example>
    public static IServiceCollection AddPipelineBehavior<TNotification>(this IServiceCollection services, Type type)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(type);

        var serviceType = typeof(IPipelineBehavior<TNotification>);

        if (!services.Any(s => s.ServiceType == serviceType && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(serviceType, type, ServiceLifetime.Singleton));
        }

        return services;
    }

    /// <summary>
    /// Registers the core <see cref="IMediator"/> implementation into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the mediator into.</param>
    private static void AddCoreMediator(this IServiceCollection services)
    {
        services.AddSingleton(sp => new MicrosoftContainer(sp));

        services.AddSingleton<IMediator>(sp =>
        {
            var container = sp.GetRequiredService<MicrosoftContainer>();

            return new Mediator(MediatorConfiguration, container);
        });

        services.AddSingleton<IPublisher>(sp =>
        {
            var container = sp.GetRequiredService<MicrosoftContainer>();

            return new Mediator(MediatorConfiguration, container);
        });

        services.AddSingleton<ISender>(sp =>
        {
            var container = sp.GetRequiredService<MicrosoftContainer>();

            return new Mediator(MediatorConfiguration, container);
        });
    }
}
