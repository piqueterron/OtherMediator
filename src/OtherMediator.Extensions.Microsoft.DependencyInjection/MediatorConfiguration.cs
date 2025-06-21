namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using System.Reflection;
using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

/// <summary>
/// Configures the mediator and registers handlers and pipeline behaviors.
/// </summary>
public class MediatorConfiguration
{
    private ServiceLifetime _lifetime = ServiceLifetime.Transient;
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of <see cref="MediatorConfiguration"/>.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    public MediatorConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Gets or sets the default service lifetime for handler registrations.
    /// </summary>
    public ServiceLifetime Lifetime
    {
        get => _lifetime;
        set => _lifetime = value;
    }

    /// <summary>
    /// Registers all handler services from the assembly containing <typeparamref name="TAssembly"/>.
    /// </summary>
    /// <typeparam name="TAssembly">A type from the target assembly.</typeparam>
    public void RegisterServicesFromAssembly<TAssembly>()
        where TAssembly : class
    {
        RegisterServicesFromAssemblies(typeof(TAssembly).Assembly);
    }

    /// <summary>
    /// Registers all handler services from the provided assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [typeof(MediatorConfiguration).Assembly];
        }

        foreach (var assembly in assemblies)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            var alreadyRegistered = new HashSet<(Type, Type)>(_services
                .Where(s => s.ServiceType != null && s.ImplementationType != null)
                .Select(s => (s.ServiceType!, s.ImplementationType!)));

            foreach (var type in types.Where(t => t.IsClass && !t.IsAbstract))
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    var definition = @interface.GetGenericTypeDefinition();

                    if (definition == typeof(IRequestHandler<,>) || definition == typeof(INotificationHandler<>))
                    {
                        var serviceType = @interface.ContainsGenericParameters ? @interface.GetGenericTypeDefinition() : @interface;
                        var implementationType = type.ContainsGenericParameters ? type.GetGenericTypeDefinition() : type;

                        var key = (serviceType, implementationType);
                        if (alreadyRegistered.Contains(key))
                        {
                            continue;
                        }

                        _services.Add(new ServiceDescriptor(serviceType, implementationType, _lifetime));
                        alreadyRegistered.Add(key);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Registers an open generic pipeline behavior.
    /// </summary>
    /// <param name="type">The behavior type implementing <see cref="IPipelineBehavior{,}"/>.</param>
    public void AddOpenPipelineBehavior(Type type)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var behaviorInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));

        if (behaviorInterface is null)
        {
            throw new ArgumentException("Type must implement IPipelineBehavior<,>.", nameof(type));
        }

        if (!_services.Any(s => s.ServiceType == typeof(IPipelineBehavior<,>) && s.ImplementationType == type))
        {
            _services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, _lifetime));
        }
    }

    /// <summary>
    /// Registers a pipeline behavior for a specific request/response type.
    /// </summary>
    /// <typeparam name="TRequest">Request type.</typeparam>
    /// <typeparam name="TResponse">Response type.</typeparam>
    /// <param name="type">The behavior type.</param>
    public void AddPipelineBehavior<TRequest, TResponse>(Type type)
        where TRequest : IRequest<TResponse>
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var serviceType = typeof(IPipelineBehavior<TRequest, TResponse>);

        if (!_services.Any(s => s.ServiceType == serviceType && s.ImplementationType == type))
        {
            _services.Add(new ServiceDescriptor(serviceType, type, _lifetime));
        }
    }
}