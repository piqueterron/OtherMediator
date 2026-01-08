namespace OtherMediator.Extensions.Microsoft.DependencyInjection;

using global::Microsoft.Extensions.DependencyInjection;
using OtherMediator.Contracts;

/// <summary>
/// Provides a concrete implementation of the <see cref="IContainer"/> abstraction
/// that delegates all dependency resolution operations to the native Microsoft
/// Dependency Injection (DI) system (<see cref="IServiceProvider"/>).
/// </summary>
/// <remarks>
/// <para>
/// This class acts as an <strong>adapter</strong> or <strong>bridge</strong> between the internal
/// architecture of OtherMediator, which uses the <see cref="IContainer"/> abstraction,
/// and the standard DI container of ASP.NET Core and .NET applications.
/// </para>
/// <para>
/// <strong>Primary responsibilities:</strong>
/// <list type="bullet">
/// <item><description>Translate OtherMediator's resolution requests to calls to the Microsoft <see cref="IServiceProvider"/>.</description></item>
/// <item><description>Fully preserve the lifecycle configurations (Singleton, Scoped, Transient) defined in the original DI container.</description></item>
/// <item><description>Support both single service resolution and service collections resolution.</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Key characteristics:</strong>
/// <list type="bullet">
/// <item><description><strong>Transparency:</strong> Does not add additional instance management or lifecycle logic.</description></item>
/// <item><description><strong>Native behavior:</strong> Exactly follows the resolution rules and behaviors of the underlying <see cref="IServiceProvider"/>.</description></item>
/// <item><description><strong>Internal use:</strong> Designed primarily for use by OtherMediator's mediation system and not as a general-purpose Service Locator in application code.</description></item>
/// </list>
/// </para>
/// <example>
/// Example of how this class integrates into OtherMediator initialization:
/// <code>
/// // In the AddOtherMediator() extension
/// public static IServiceCollection AddOtherMediator(this IServiceCollection services)
/// {
///     // ... other configurations
///     services.AddScoped&lt;IContainer, MicrosoftContainer&gt;();
///     // ... more configurations
///     return services;
/// }
/// </code>
/// </example>
/// <seealso cref="IContainer"/>
/// <seealso cref="IServiceProvider"/>
/// </remarks>
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
