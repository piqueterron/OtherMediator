namespace OtherMediator.Contracts;

/// <summary>
/// Marker interface representing a request that expects a response of a specific type.
/// Requests correspond to commands (write operations) or queries (read operations).
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
/// <remarks>
/// <para>
/// Requests are used for operations where:
/// <list type="bullet">
/// <item><description>A response is expected</description></item>
/// <item><description>Only one handler should process the request</description></item>
/// <item><description>The caller needs the result of the operation</description></item>
/// </list>
/// </para>
/// <para>
/// Key characteristics:
/// <list type="bullet">
/// <item><description>One handler per request type</description></item>
/// <item><description>Returns a value to the caller</description></item>
/// <item><description>Can be validated, authorized, and transformed via pipeline behaviors</description></item>
/// <item><description>Supports cancellation tokens</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Defining a request with response:
/// <code>
/// // Command (write operation)
/// public class CreateUserCommand : IRequest&lt;int&gt;
/// {
///     public string Email { get; set; }
///     public string Name { get; set; }
/// }
/// 
/// // Query (read operation)  
/// public class GetUserQuery : IRequest&lt;UserDto&gt;
/// {
///     public int UserId { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="IRequestHandler{TRequest, TResponse}"/>
public interface IRequest<TResponse> { }

/// <summary>
/// Marker interface representing a request that doesn't return a value (void).
/// This is a convenience interface for commands that don't need to return data.
/// </summary>
/// <remarks>
/// <para>
/// This interface inherits from <see cref="IRequest{TResponse}"/> with <see cref="Unit"/> as the response type.
/// <see cref="Unit"/> is a singleton type representing "no value" (similar to void in Task context).
/// </para>
/// <para>
/// Use this interface for commands where you only care about success/failure,
/// not about returning data from the handler.
/// </para>
/// </remarks>
/// <example>
/// Defining a void request:
/// <code>
/// public class DeleteUserCommand : IRequest
/// {
///     public int UserId { get; set; }
/// }
/// 
/// // Usage:
/// await mediator.Send(new DeleteUserCommand { UserId = 123 });
/// </code>
/// </example>
public interface IRequest : IRequest<Unit> { }
