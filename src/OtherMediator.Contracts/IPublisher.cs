namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for publishing notifications to their registered handlers.
/// </summary>
/// <remarks>
/// <para>
/// Publishing is a fire-and-forget operation where the publisher doesn't wait for
/// handlers to complete (unless specifically awaited). Multiple handlers can process
/// the same notification.
/// </para>
/// <para>
/// The publisher is responsible for:
/// <list type="bullet">
/// <item><description>Finding all registered handlers for the notification type</description></item>
/// <item><description>Executing handlers according to the <see cref="DispatchStrategy"/></description></item>
/// <item><description>Managing exceptions from handlers</description></item>
/// <item><description>Ensuring proper cancellation token propagation</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IPublisher
{
    /// <summary>
    /// Publishes a notification to all registered handlers asynchronously.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish, must implement <see cref="INotification"/>.</typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the publication.
    /// This token is propagated to all handlers.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// Important considerations:
    /// <list type="bullet">
    /// <item><description>Handlers execute based on the configured <see cref="DispatchStrategy"/></description></item>
    /// <item><description>Exceptions from handlers are aggregated or handled based on configuration</description></item>
    /// <item><description>The notification object should be immutable after publishing</description></item>
    /// <item><description>Publication continues even if some handlers fail (unless configured otherwise)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// For notifications with no handlers, this method completes immediately without error.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Publish a notification
    /// var notification = new OrderShippedNotification { OrderId = 123 };
    /// await publisher.Publish(notification, cancellationToken);
    /// 
    /// // Multiple handlers will receive this notification:
    /// // 1. SendShippingConfirmationHandler
    /// // 2. UpdateInventoryHandler
    /// // 3. NotifyAnalyticsHandler
    /// </code>
    /// </example>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}
