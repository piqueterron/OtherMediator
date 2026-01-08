namespace OtherMediator.Contracts;

/// <summary>
/// Defines a contract for handling a specific type of request.
/// Implement this interface to create handlers that process requests and return responses.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle, must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
/// <remarks>
/// <para>
/// Request handlers contain the business logic for processing requests.
/// Each request type should have exactly one handler (unlike notifications).
/// </para>
/// <para>
/// Best practices:
/// <list type="bullet">
/// <item><description>Keep handlers focused on a single responsibility</description></item>
/// <item><description>Use dependency injection for services</description></item>
/// <item><description>Handle validation in pipeline behaviors, not in handlers</description></item>
/// <item><description>Make handlers testable by keeping them free of framework concerns</description></item>
/// </list>
/// </para>
/// <para>
/// Handler execution flow:
/// <list type="number">
/// <item><description>Request is validated (if validation behavior exists)</description></item>
/// <item><description>Authorization is checked (if authorization behavior exists)</description></item>
/// <item><description>Handler is executed with the request</description></item>
/// <item><description>Response is returned through the pipeline</description></item>
/// <item><description>Post-processing behaviors execute (logging, caching, etc.)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// Implementing a request handler:
/// <code>
/// public class CreateUserHandler : IRequestHandler&lt;CreateUserCommand, int&gt;
/// {
///     private readonly IUserRepository _repository;
///     
///     public CreateUserHandler(IUserRepository repository) 
///         => _repository = repository;
///     
///     public async Task&lt;int&gt; HandleAsync(CreateUserCommand request, CancellationToken cancellationToken)
///     {
///         var user = new User 
///         { 
///             Email = request.Email,
///             Name = request.Name
///         };
///         
///         await _repository.AddAsync(user, cancellationToken);
///         await _repository.SaveChangesAsync(cancellationToken);
///         
///         return user.Id;
///     }
/// }
/// </code>
/// </example>
public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously and returns a response.
    /// </summary>
    /// <param name="request">The request object containing input data.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation.
    /// Propagates through the entire handler pipeline.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the response.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the cancellation token.
    /// </exception>
    /// <remarks>
    /// Implementations should:
    /// <list type="bullet">
    /// <item><description>Validate input data (or rely on validation behaviors)</description></item>
    /// <item><description>Perform the core business logic</description></item>
    /// <item><description>Return an appropriate response</description></item>
    /// <item><description>Handle exceptions or let them propagate for global handling</description></item>
    /// <item><description>Check cancellation token periodically for long operations</description></item>
    /// </list>
    /// </remarks>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
