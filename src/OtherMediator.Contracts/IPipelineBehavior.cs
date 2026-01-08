namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for pipeline behaviors that intercept and process requests.
/// Pipeline behaviors implement the chain of responsibility pattern for cross-cutting concerns.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed, must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the request.</typeparam>
/// <remarks>
/// <para>
/// Pipeline behaviors wrap the execution of request handlers, allowing you to:
/// <list type="bullet">
/// <item><description>Add logging before and after handler execution</description></item>
/// <item><description>Implement validation logic</description></item>
/// <item><description>Handle exceptions uniformly</description></item>
/// <item><description>Add caching mechanisms</description></item>
/// <item><description>Implement transaction management</description></item>
/// <item><description>Add performance monitoring</description></item>
/// </list>
/// </para>
/// <para>
/// Execution order:
/// <list type="number">
/// <item><description>Behaviors execute in the order they are registered in the DI container</description></item>
/// <item><description>Each behavior calls <paramref name="next"/> to continue the pipeline</description></item>
/// <item><description>The final <paramref name="next"/> call invokes the actual request handler</description></item>
/// <item><description>Behaviors can short-circuit the pipeline by not calling <paramref name="next"/></description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Implementing a logging behavior:
/// <code>
/// public class LoggingBehavior&lt;TRequest, TResponse&gt; : IPipelineBehavior&lt;TRequest, TResponse&gt;
///     where TRequest : IRequest&lt;TResponse&gt;
/// {
///     private readonly ILogger&lt;LoggingBehavior&lt;TRequest, TResponse&gt;&gt; _logger;
///     
///     public LoggingBehavior(ILogger&lt;LoggingBehavior&lt;TRequest, TResponse&gt;&gt; logger) 
///         => _logger = logger;
///     
///     public async Task&lt;TResponse&gt; Handle(
///         TRequest request, 
///         Func&lt;TRequest, CancellationToken, Task&lt;TResponse&gt;&gt; next,
///         CancellationToken cancellationToken)
///     {
///         _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
///         var stopwatch = Stopwatch.StartNew();
///         
///         try
///         {
///             var response = await next(request, cancellationToken);
///             _logger.LogInformation("Handled {RequestType} in {ElapsedMs}ms", 
///                 typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
///             return response;
///         }
///         catch (Exception ex)
///         {
///             _logger.LogError(ex, "Error handling {RequestType}", typeof(TRequest).Name);
///             throw;
///         }
///     }
/// }
/// </code>
/// </example>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request by processing it through the pipeline.
    /// </summary>
    /// <param name="request">The request object to process.</param>
    /// <param name="next">
    /// A delegate representing the next behavior or handler in the pipeline.
    /// Call this delegate to continue pipeline execution.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the response.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Implementations should typically:
    /// <list type="number">
    /// <item><description>Perform pre-processing logic</description></item>
    /// <item><description>Call <paramref name="next"/> to continue the pipeline</description></item>
    /// <item><description>Perform post-processing logic on the response</description></item>
    /// <item><description>Return the (possibly modified) response</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// To short-circuit the pipeline (e.g., for validation failures), return a response
    /// without calling <paramref name="next"/>.
    /// </para>
    /// </remarks>
    Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a contract for pipeline behaviors that intercept and process notifications.
/// Similar to request behaviors but for notifications with no return value.
/// </summary>
/// <typeparam name="TNotification">The type of notification being processed, must implement <see cref="INotification"/>.</typeparam>
/// <remarks>
/// <para>
/// Notification pipeline behaviors work similarly to request behaviors but:
/// <list type="bullet">
/// <item><description>They don't return a value (void/Task)</description></item>
/// <item><description>They wrap the execution of all notification handlers</description></item>
/// <item><description>They execute once per notification, not per handler</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IPipelineBehavior<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the notification by processing it through the pipeline.
    /// </summary>
    /// <param name="notification">The notification object to process.</param>
    /// <param name="next">
    /// A delegate representing the next behavior or the notification dispatcher.
    /// Call this delegate to continue pipeline execution.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TNotification request, Func<TNotification, CancellationToken, Task> next, CancellationToken cancellationToken);
}
