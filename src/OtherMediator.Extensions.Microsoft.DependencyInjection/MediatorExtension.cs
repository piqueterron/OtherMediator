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
    /// <summary>
    /// Gets the current <see cref="MediatorConfiguration"/> instance.
    /// </summary>
    public static MediatorConfiguration MediatorConfiguration;

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

        MediatorConfiguration = opt;

        services.AddCoreMediator(opt);

        return services;
    }

    /// <summary>
    /// Registers an open generic pipeline behavior.
    /// For example, to add a custom behavior use:
    /// <c>services.AddOpenPipelineBehavior(typeof(MyGlobalBehavior&lt;,&gt;));</c>
    /// </summary>
    /// <param name="type">The behavior type implementing <see cref="IPipelineBehavior{,}"/>.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This method allows registering a pipeline behavior that applies to all request/response types.
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

        var behaviorInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (behaviorInterface is null)
        {
            throw new ArgumentException("Type must implement IPipelineBehavior<,>.", nameof(type));
        }

        if (!services.Any(s => s.ServiceType == typeof(IPipelineBehavior<,>) && s.ImplementationType == type))
        {
            services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, ServiceLifetime.Singleton));
        }

        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior for a specific request/response type.
    /// For example, to add a custom behavior for <c>MyRequest</c> and <c>MyResponse</c>, use:
    /// <c>services.AddPipelineBehavior&lt;MyRequest, MyResponse&gt;(typeof(MyCustomBehavior&lt;MyRequest, MyResponse&gt;));</c>
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <param name="type">The behavior type.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    /// <remarks>
    /// This allows you to attach custom behavior only for specific requests.
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
    /// Registers the core <see cref="IMediator"/> implementation into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the mediator into.</param>
    private static void AddCoreMediator(this IServiceCollection services, IMediatorConfiguration mediatorConfiguration)
    {
        services.AddSingleton<IMediator>(sp =>
        {
            var container = new MicrosoftContainer(sp);
            var pipeline = new MiddlewarePipeline();

            return new Mediator(container, pipeline, mediatorConfiguration);
        });
    }
}
