namespace OtherMediator;

using OtherMediator.Contracts;

/// <summary>
/// Configures the mediator and registers handlers and pipeline behaviors.
/// </summary>
public class MediatorConfiguration : IMediatorConfiguration
{
    private Lifetime _lifetime = Lifetime.Transient;

    /// <summary>
    /// Gets or sets the default service lifetime for handler registrations, by default <b>Transient</b>.
    /// </summary>
    /// <remarks>
    /// In ASP.NET Core, when you register a service in the Dependency Injection (DI) container, you specify its lifetime. This defines how long a service instance lives and how it is shared across different components and requests.
    /// <list type="bullet">Transient — A new instance is created every time the service is requested.</list>
    /// <list type="bullet">Scoped — A single instance is created and shared within the same request.</list>
    /// <list type="bullet">Singleton — A single instance is created once and shared throughout the entire application.</list>
    /// </remarks>
    public Lifetime Lifetime
    {
        get => _lifetime;
        set => _lifetime = value;
    }

    /// <summary>
    /// Adds a middleware to the pipeline that will catch exceptions, by default <b>true</b>.
    /// </summary>
    public bool UseExceptionHandler { get; set; } = true;

    /// <summary>
    /// Gets or sets the strategy used to dispatch tasks or operations, by default <b>Parallel</b>.
    /// </summary>
    /// <remarks>The dispatch strategy determines how tasks are scheduled and executed, such as sequentially
    /// or in parallel. Changing this property affects the concurrency and ordering of dispatched operations.</remarks>
    public DispatchStrategy DispatchStrategy { get; set; } = DispatchStrategy.Parallel;
}
