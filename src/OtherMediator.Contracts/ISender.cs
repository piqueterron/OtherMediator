namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for sending requests (commands and queries) through the mediator pipeline.
/// Requests are operations that expect a response and typically have exactly one handler.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ISender"/> interface represents the "request/response" side of the mediator pattern,
/// handling operations where a specific result is expected. This is in contrast to <see cref="IPublisher"/>
/// which handles notifications (events) without expecting a response.
/// </para>
/// <para>
/// Key characteristics of request sending:
/// <list type="bullet">
/// <item><description><strong>Single Handler</strong>: Each request type has exactly one handler</description></item>
/// <item><description><strong>Type Safety</strong>: Request/response types are enforced at compile time</description></item>
/// <item><description><strong>Pipeline Execution</strong>: Requests pass through registered pipeline behaviors</description></item>
/// <item><description><strong>Cancellation Support</strong>: All operations support cancellation tokens</description></item>
/// </list>
/// </para>
/// <para>
/// Request types and their purposes:
/// <list type="table">
/// <listheader>
/// <term>Request Type</term>
/// <description>Response Type</description>
/// <description>Purpose</description>
/// <description>Example</description>
/// </listheader>
/// <item>
/// <term>Command</term>
/// <description><see cref="Unit"/> (void) or result ID</description>
/// <description>Perform an action that changes state</description>
/// <description>CreateUserCommand → int (new user ID)</description>
/// </item>
/// <item>
/// <term>Query</term>
/// <description>Data object</description>
/// <description>Retrieve data without side effects</description>
/// <description>GetUserQuery → UserDto</description>
/// </item>
/// </list>
/// </para>
/// <para>
/// Execution pipeline flow:
/// <code>
/// ┌─────────────────────────────────────────────────────────┐
/// │                    Send() called                        │
/// └──────────────────────────┬──────────────────────────────┘
///                            │
///                 ┌──────────▼──────────┐
///                 │  Pipeline Behaviors │
///                 │  • Validation       │
///                 │  • Authorization    │
///                 │  • Logging          │
///                 │  • Caching          │
///                 └──────────┬──────────┘
///                            │
///                 ┌──────────▼──────────┐
///                 │   Request Handler   │
///                 │  • Business Logic   │
///                 │  • Data Access      │
///                 │  • Service Calls    │
///                 └──────────┬──────────┘
///                            │
///                 ┌──────────▼──────────┐
///                 │  Pipeline Behaviors │
///                 │  • Post-processing  │
///                 │  • Result shaping   │
///                 │  • Error handling   │
///                 └──────────┬──────────┘
///                            │
///                 ┌──────────▼──────────┐
///                 │   Return Response   │
///                 └─────────────────────┘
/// </code>
/// </para>
/// </remarks>
/// <example>
/// Basic usage patterns:
/// <code>
/// // Command with response
/// public class CreateOrderCommand : IRequest&lt;int&gt;
/// {
///     public int CustomerId { get; set; }
///     public List&lt;OrderItem&gt; Items { get; set; }
/// }
/// 
/// // Query 
/// public class GetOrderQuery : IRequest&lt;OrderDto&gt;
/// {
///     public int OrderId { get; set; }
/// }
/// 
/// // Command without response (void)
/// public class CancelOrderCommand : IRequest
/// {
///     public int OrderId { get; set; }
/// }
/// </code>
/// </example>
/// <seealso cref="IPublisher"/>
/// <seealso cref="IRequest{TResponse}"/>
/// <seealso cref="IRequestHandler{TRequest, TResponse}"/>
/// <seealso cref="IPipelineBehavior{TRequest, TResponse}"/>
public interface ISender
{
    /// <summary>
    /// Sends a request through the mediator pipeline and returns a response asynchronously.
    /// This is the primary method for executing commands and queries that return a value.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of request to send. Must implement <see cref="IRequest{TResponse}"/>.
    /// This is typically a command (write operation) or query (read operation).
    /// </typeparam>
    /// <typeparam name="TResponse">
    /// The type of response expected from the request handler.
    /// This corresponds to the generic parameter of <see cref="IRequest{TResponse}"/>.
    /// </typeparam>
    /// <param name="request">
    /// The request object containing all necessary data for the operation.
    /// The request should be immutable after creation to ensure thread safety.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the entire operation pipeline.
    /// The token is propagated to all pipeline behaviors and the final handler.
    /// Default is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResponse}"/> that represents the asynchronous operation.
    /// The task result contains the response from the request handler.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Execution details:
    /// <list type="number">
    /// <item>
    /// <description>
    /// The mediator locates the appropriate <see cref="IRequestHandler{TRequest, TResponse}"/>
    /// for the request type. If no handler is registered, an exception is thrown.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// All registered <see cref="IPipelineBehavior{TRequest, TResponse}"/> instances
    /// are executed in the order they were registered in the dependency container.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Each pipeline behavior can:
    /// <list type="bullet">
    /// <item><description>Execute code before the handler (pre-processing)</description></item>
    /// <item><description>Call the next behavior in the chain</description></item>
    /// <item><description>Execute code after the handler (post-processing)</description></item>
    /// <item><description>Short-circuit the pipeline and return a response directly</description></item>
    /// </list>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The final handler executes the business logic and returns a response.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Control returns back through the pipeline behaviors for post-processing.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Error handling:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Exceptions thrown by pipeline behaviors or the handler propagate up the call stack.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The mediator may wrap exceptions or apply custom error handling based on configuration.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <see cref="OperationCanceledException"/> is thrown if the operation is cancelled
    /// via the cancellation token.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Performance considerations:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Request objects should be lightweight DTOs, not containing business logic.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Each pipeline behavior adds overhead; keep behaviors minimal for performance-critical paths.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Consider using response caching behaviors for expensive queries.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the request type, or when the dependency
    /// injection container cannot resolve required dependencies.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// public class OrderService
    /// {
    ///     private readonly ISender _sender;
    ///     
    ///     public OrderService(ISender sender) => _sender = sender;
    ///     
    ///     public async Task&lt;int&gt; CreateOrderAsync(CreateOrderRequest request)
    ///     {
    ///         var command = new CreateOrderCommand
    ///         {
    ///             CustomerId = request.CustomerId,
    ///             Items = request.Items.Select(i => new OrderItem 
    ///             { 
    ///                 ProductId = i.ProductId, 
    ///                 Quantity = i.Quantity 
    ///             }).ToList()
    ///         };
    ///         
    ///         // Send command through mediator pipeline
    ///         var orderId = await _sender.Send&lt;CreateOrderCommand, int&gt;(command);
    ///         
    ///         return orderId;
    ///     }
    ///     
    ///     public async Task&lt;OrderDetails&gt; GetOrderAsync(int orderId)
    ///     {
    ///         var query = new GetOrderQuery { OrderId = orderId };
    ///         
    ///         // Send query through mediator pipeline
    ///         return await _sender.Send&lt;GetOrderQuery, OrderDetails&gt;(query);
    ///     }
    /// }
    /// </code>
    /// 
    /// Using with cancellation:
    /// <code>
    /// public async Task&lt;SearchResults&gt; SearchUsersAsync(
    ///     string term, 
    ///     CancellationToken cancellationToken)
    /// {
    ///     var query = new SearchUsersQuery { SearchTerm = term };
    ///     
    ///     // Pass cancellation token through the pipeline
    ///     return await _sender.Send&lt;SearchUsersQuery, SearchResults&gt;(
    ///         query, 
    ///         cancellationToken);
    /// }
    /// </code>
    /// </example>
    Task<TResponse> Send<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Sends a request that doesn't return a value (void command) through the mediator pipeline.
    /// This is a convenience method for commands where only success/failure matters.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of request to send. Must implement <see cref="IRequest"/> (which is
    /// <see cref="IRequest{TResponse}"/> with <see cref="Unit"/> as the response type).
    /// </typeparam>
    /// <param name="request">
    /// The request object containing all necessary data for the operation.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the entire operation pipeline.
    /// Default is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{Unit}"/> that represents the asynchronous operation.
    /// <see cref="Unit"/> is a singleton type representing "no value" - the task
    /// completes when the operation finishes successfully.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is semantically equivalent to:
    /// <code>
    /// await Send&lt;TRequest, Unit&gt;(request, cancellationToken);
    /// </code>
    /// It exists for convenience when working with void commands, providing cleaner syntax.
    /// </para>
    /// <para>
    /// The <see cref="Unit"/> type:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Represents the absence of a value in generic contexts (similar to <c>void</c>)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Is a singleton - there's only one instance: <c>Unit.Value</c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Allows void operations to participate in generic pipelines and async patterns
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Use this method for commands that:
    /// <list type="bullet">
    /// <item><description>Perform an action but don't need to return data</description></item>
    /// <item><description>Only indicate success or failure (via exceptions)</description></item>
    /// <item><description>Are fire-and-forget within an async context</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Note: Even though the method returns <see cref="Unit"/>, you typically don't
    /// need to use the return value. The task completion indicates successful execution.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered for the request type.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via <paramref name="cancellationToken"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// public class OrderService
    /// {
    ///     private readonly ISender _sender;
    ///     
    ///     public OrderService(ISender sender) => _sender = sender;
    ///     
    ///     public async Task CancelOrderAsync(int orderId)
    ///     {
    ///         var command = new CancelOrderCommand { OrderId = orderId };
    ///         
    ///         // Send void command - no return value needed
    ///         await _sender.Send(command);
    ///         
    ///         // Equivalent to:
    ///         // await _sender.Send&lt;CancelOrderCommand, Unit&gt;(command);
    ///     }
    ///     
    ///     public async Task NotifyUsersAsync()
    ///     {
    ///         var command = new SendNotificationsCommand();
    ///         
    ///         // With cancellation
    ///         using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
    ///         await _sender.Send(command, cts.Token);
    ///     }
    /// }
    /// </code>
    /// 
    /// Void command implementation:
    /// <code>
    /// public class DeleteUserCommand : IRequest
    /// {
    ///     public int UserId { get; set; }
    /// }
    /// 
    /// public class DeleteUserHandler : IRequestHandler&lt;DeleteUserCommand, Unit&gt;
    /// {
    ///     private readonly IUserRepository _repository;
    ///     
    ///     public DeleteUserHandler(IUserRepository repository)
    ///         => _repository = repository;
    ///     
    ///     public async Task&lt;Unit&gt; HandleAsync(
    ///         DeleteUserCommand request, 
    ///         CancellationToken cancellationToken)
    ///     {
    ///         await _repository.DeleteAsync(request.UserId, cancellationToken);
    ///         await _repository.SaveChangesAsync(cancellationToken);
    ///         
    ///         return Unit.Value; // Required return value
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="Unit"/>
    Task<Unit> Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}
