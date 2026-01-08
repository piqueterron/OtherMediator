namespace OtherMediator.Contracts;

/// <summary>
/// Marker interface representing a notification in the mediator pattern.
/// Notifications are events that can have zero or multiple handlers.
/// </summary>
/// <remarks>
/// <para>
/// Notifications are used for events where:
/// <list type="bullet">
/// <item><description>No response is expected</description></item>
/// <item><description>Multiple components need to react to the same event</description></item>
/// <item><description>The sender doesn't need to know which handlers exist</description></item>
/// </list>
/// </para>
/// <para>
/// Key characteristics:
/// <list type="bullet">
/// <item><description>Fire-and-forget semantics</description></item>
/// <item><description>Can have multiple handlers</description></item>
/// <item><description>Handlers cannot return values to the publisher</description></item>
/// <item><description>Execution strategy controlled by <see cref="DispatchStrategy"/></description></item>
/// </list>
/// </para>
/// <example>
/// Defining a notification:
/// <code>
/// public class UserRegisteredNotification : INotification
/// {
///     public string Email { get; set; }
///     public DateTime RegisteredAt { get; set; }
/// }
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="INotificationHandler{TNotification}"/>
/// <seealso cref="IPublisher"/>
public interface INotification { }
