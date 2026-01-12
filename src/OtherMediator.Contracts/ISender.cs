namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for sending requests and receiving typed responses asynchronously.
/// This interface is a core component of the Mediator pattern implementation,
/// enabling decoupled communication between components in a CQRS/Mediator architecture.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ISender"/> interface abstracts the dispatch mechanism for requests
/// (typically commands or queries) to their corresponding handlers without requiring
/// direct dependencies between the caller and handler implementations.
/// </para>
/// <para>
/// Typical use cases include:
/// <list type="bullet">
/// <item>Executing commands that modify system state</item>
/// <item>Issuing queries that retrieve data</item>
/// <item>Dispatching events in event-driven architectures</item>
/// </list>
/// </para>
/// <para>
/// Implementations of this interface often incorporate cross-cutting concerns
/// such as validation, logging, transaction management, and authorization.
/// </para>
/// </remarks>
/// <example>
/// Basic usage pattern:
/// <code>
/// public class OrderService
/// {
///     private readonly ISender _sender;
///     
///     public OrderService(ISender sender)
///     {
///         _sender = sender;
///     }
///     
///     public async Task&lt;OrderResult&gt; CreateOrderAsync(CreateOrderCommand command)
///     {
///         // Send command through mediator
///         return await _sender.Send(command);
///     }
/// }
/// </code>
/// </example>
public interface ISender
{
    /// <summary>
    /// Asynchronously sends a request and returns a response of the specified type.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of response expected from the request handler.
    /// Must match the response type defined by the <see cref="IRequest{TResponse}"/> implementation.
    /// </typeparam>
    /// <param name="request">
    /// The request object to send. Must implement <see cref="IRequest{TResponse}"/>.
    /// Common implementations include commands, queries, or other request types.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to request cancellation
    /// of the asynchronous operation. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The task result contains the response of type <typeparamref name="TResponse"/>
    /// produced by the request handler.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no appropriate handler is found for the request.
    /// </exception>
    /// <exception cref="System.Exception">
    /// May propagate exceptions thrown by the request handler or pipeline components.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is the single entry point for dispatching all requests in a
    /// mediator-based system. The actual processing is delegated to registered
    /// handlers that implement <see cref="IRequestHandler{TRequest, TResponse}"/>.
    /// </para>
    /// <para>
    /// The method supports the following common patterns:
    /// <list type="bullet">
    /// <item><description>Command execution (returns result or status)</description></item>
    /// <item><description>Query execution (returns data)</description></item>
    /// <item><description>Void operations (use <see cref="Unit"/> or similar)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    );
}
