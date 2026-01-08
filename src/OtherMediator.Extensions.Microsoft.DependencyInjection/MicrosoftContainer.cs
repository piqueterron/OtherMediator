namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

internal class MicrosoftContainer(IServiceProvider serviceProvider) : IContainer
{
    /// <summary>
    /// Resolves a single instance of the specified service type from the underlying Microsoft DI container.
    /// This method returns the first registered implementation or null if the service is not registered.
    /// </summary>
    /// <typeparam name="T">The service type to resolve. Must be a reference type.</typeparam>
    /// <returns>
    /// An instance of type <typeparamref name="T"/> if registered; otherwise, <see langword="null"/>.
    /// For services registered with <see cref="Lifetime.Singleton"/> or <see cref="Lifetime.Scoped"/> lifetimes,
    /// subsequent calls may return the same instance.
    /// </returns>
    /// <remarks>
    /// This implementation delegates to <see cref="ServiceProviderServiceExtensions.GetService{T}(IServiceProvider)"/>.
    /// For services with multiple registrations, only the last registered implementation is returned.
    /// </remarks>
    public T? Resolve<T>() where T : class
    {
        return serviceProvider.GetService<T>();
    }

    /// <summary>
    /// Resolves all registered implementations of a specified service type from the underlying Microsoft DI container.
    /// This method is used when multiple implementations of the same interface or base type are registered.
    /// </summary>
    /// <typeparam name="T">The element type of the returned collection, typically an interface or base class.</typeparam>
    /// <param name="type">The concrete service type to resolve. This should match the registration type.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> containing all registered implementations of the specified type,
    /// or <see langword="null"/> if no implementations are registered. The order of elements matches
    /// the registration order in the DI container.
    /// </returns>
    /// <remarks>
    /// This method constructs a generic type <see cref="IEnumerable{T}"/> dynamically and requests it
    /// from the service provider. When using this method, ensure the <typeparamref name="T"/> parameter
    /// matches the registration type (e.g., pass <c>typeof(IMyService)</c> for services registered as <c>IMyService</c>).
    /// </remarks>
    /// <example>
    /// <code>
    /// // Given registrations: services.AddScoped&lt;IMyService, ServiceA&gt;();
    /// //                      services.AddScoped&lt;IMyService, ServiceB&gt;();
    /// 
    /// var services = container.Resolve&lt;IMyService&gt;(typeof(IMyService));
    /// // Returns IEnumerable&lt;IMyService&gt; containing ServiceA and ServiceB instances
    /// </code>
    /// </example>
    public IEnumerable<T>? Resolve<T>(Type type)
    {
        var serviceType = typeof(IEnumerable<>).MakeGenericType(type);
        return serviceProvider.GetService(serviceType) as IEnumerable<T>;
    }
}
