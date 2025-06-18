namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using System.Reflection;
using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

public class MediatorConfiguration(IServiceCollection services)
{
    private ServiceLifetime _lifetime = ServiceLifetime.Transient;
    private readonly IServiceCollection _services = services;

    public ServiceLifetime Lifetime
    {
        get => _lifetime;
        set => _lifetime = value;
    }

    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetAssembly(typeof(MediatorConfiguration)) };
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in types)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                        continue;

                    var definition = @interface.GetGenericTypeDefinition();

                    if (definition == typeof(IRequestHandler<,>) || definition == typeof(INotificationHandler<>))
                    {
                        var serviceType = @interface.ContainsGenericParameters ? @interface.GetGenericTypeDefinition() : @interface;
                        var implementationType = type.ContainsGenericParameters ? type.GetGenericTypeDefinition() : type;

                        if (_services.Any(s => s.ServiceType == serviceType && s.ImplementationType == implementationType))
                        {
                            continue;
                        }

                        _services.Add(new ServiceDescriptor(serviceType, implementationType, _lifetime));
                    }
                }
            }
        }
    }

    public void AddOpenPipelineBehavior(Type type)
    {
        _services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), type, _lifetime));
    }

    public void AddPipelineBehavior<TRequest, TResponse>(Type type)
        where TRequest : IRequest<TResponse>
    {
        _services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<TRequest, TResponse>), type, _lifetime));
    }
}