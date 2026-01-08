namespace OtherMediator.Contracts;

/// <summary>
/// Provides configuration options for the mediator's behavior and execution strategies.
/// This configuration controls how the mediator processes requests and notifications.
/// </summary>
/// <remarks>
/// <para>
/// The configuration can be applied during mediator setup in the dependency injection container.
/// Different configurations can be used for different mediator instances if required.
/// </para>
/// </remarks>
public interface IMediatorConfiguration
{
    /// <summary>
    /// Gets or sets the lifetime scope for mediator instances and their components.
    /// </summary>
    /// <value>
    /// One of the <see cref="Lifetime"/> values. Default is typically <see cref="Lifetime.Scoped"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This setting affects:
    /// <list type="bullet">
    /// <item><description>Mediator instance lifetime</description></item>
    /// <item><description>Handler instance lifetimes (unless overridden)</description></item>
    /// <item><description>Pipeline behavior lifetimes</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Recommended configurations:
    /// <list type="table">
    /// <listheader>
    /// <term>Lifetime</term>
    /// <description>Use Case</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Lifetime.Transient"/></term>
    /// <description>Stateless applications, high-load scenarios</description>
    /// </item>
    /// <item>
    /// <term><see cref="Lifetime.Scoped"/></term>
    /// <description>Web applications (per request), unit-of-work patterns</description>
    /// </item>
    /// <item>
    /// <term><see cref="Lifetime.Singleton"/></term>
    /// <description>Simple console apps, background services with no scope</description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    Lifetime Lifetime { get; set; }

    /// <summary>
    /// Gets or sets the strategy for dispatching notifications to multiple handlers.
    /// </summary>
    /// <value>
    /// One of the <see cref="DispatchStrategy"/> values. Default is <see cref="DispatchStrategy.Parallel"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This setting only affects notifications (<see cref="INotification"/>) which can have multiple handlers.
    /// Requests (<see cref="IRequest{TResponse}"/>) always have a single handler and ignore this setting.
    /// </para>
    /// <para>
    /// Strategies:
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="DispatchStrategy.Parallel"/></term>
    /// <description>
    /// All handlers execute concurrently using Task.WhenAll. This is faster but handlers
    /// may execute in any order and should not depend on each other.
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="DispatchStrategy.Sequential"/></term>
    /// <description>
    /// Handlers execute one after another in registration order. Use this when handlers
    /// have dependencies or when order of execution matters.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    DispatchStrategy DispatchStrategy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the mediator should wrap handler execution
    /// in a global exception handler.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to enable automatic exception handling; otherwise, <see langword="false"/>.
    /// Default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, the mediator catches unhandled exceptions from handlers and:
    /// <list type="bullet">
    /// <item><description>Logs the exception with full context</description></item>
    /// <item><description>Can apply custom exception handling via pipeline behaviors</description></item>
    /// <item><description>Prevents application crashes from individual handler failures</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// When disabled, exceptions propagate directly to the caller. Disable this only if:
    /// <list type="bullet">
    /// <item><description>You have your own exception handling strategy</description></item>
    /// <item><description>You need to handle specific exceptions at the call site</description></item>
    /// <item><description>Performance is critical and exception handling overhead is unacceptable</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    bool UseExceptionHandler { get; set; }
}
