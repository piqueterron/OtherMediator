namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for handling a specific type of notification.
/// Implement this interface to create handlers that process notifications.
/// </summary>
/// <typeparam name="TNotification">The type of notification to handle, must implement <see cref="INotification"/>.</typeparam>
/// <remarks>
/// <para>
/// Notification handlers are similar to event handlers in an event-driven architecture.
/// Multiple handlers can be registered for the same notification type.
/// </para>
/// <para>
/// Execution order when using <see cref="DispatchStrategy.Sequential"/>:
/// <list type="number">
/// <item><description>Handlers execute in the order they were registered in the DI container</description></item>
/// <item><description>If a handler throws an exception, subsequent handlers may not execute (configurable)</description></item>
/// </list>
/// </para>
/// <para>
/// Best practices:
/// <list type="bullet">
/// <item><description>Keep handlers idempotent when possible</description></item>
/// <item><description>Make handlers independent of each other for parallel execution</description></item>
/// <item><description>Use cancellation tokens for long-running operations</description></item>
/// <item><description>Consider using <see cref="IPipelineBehavior{TNotification}"/> for cross-cutting concerns</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Implementing a notification handler:
/// <code>
/// public class SendWelcomeEmailHandler : INotificationHandler&lt;UserRegisteredNotification&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public SendWelcomeEmailHandler(IEmailService emailService) 
///         => _emailService = emailService;
///     
///     public async Task Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
///     {
///         await _emailService.SendWelcomeEmail(notification.Email, cancellationToken);
///     }
/// }
/// </code>
/// </example>
public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification object containing event data.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// Propagates through the entire handler pipeline.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellation token.
    /// </exception>
    /// <remarks>
    /// Implementations should:
    /// <list type="bullet">
    /// <item><description>Check cancellation token periodically for long operations</description></item>
    /// <item><description>Handle their own exceptions or let them propagate for global handling</description></item>
    /// <item><description>Not modify the notification object (treat as immutable)</description></item>
    /// </list>
    /// </remarks>
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}
