namespace OtherMediator.Contracts;

/// <summary>
/// Represents the primary mediator interface that combines both sending and publishing capabilities.
/// This is the main entry point for all mediator operations in the system.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IMediator"/> pattern facilitates loose coupling between components by preventing direct dependencies.
/// Instead of components calling each other directly, they communicate through the mediator.
/// </para>
/// <para>
/// This interface inherits from both <see cref="ISender"/> and <see cref="IPublisher"/>, providing a unified API for:
/// <list type="bullet">
/// <item><description>Sending requests (commands/queries) that expect a response</description></item>
/// <item><description>Publishing notifications that can have multiple handlers</description></item>
/// </list>
/// </para>
/// <example>
/// Basic usage example:
/// <code>
/// public class MyService
/// {
///     private readonly IMediator _mediator;
///     
///     public MyService(IMediator mediator) => _mediator = mediator;
///     
///     public async Task&lt;UserDto&gt; GetUserAsync(int userId)
///     {
///         var query = new GetUserQuery { UserId = userId };
///         return await _mediator.Send(query);
///     }
///     
///     public async Task NotifyUserUpdatedAsync(int userId)
///     {
///         var notification = new UserUpdatedNotification { UserId = userId };
///         await _mediator.Publish(notification);
///     }
/// }
/// </code>
/// </example>
/// </remarks>
/// <seealso cref="ISender"/>
/// <seealso cref="IPublisher"/>
public interface IMediator : ISender, IPublisher
{
}
