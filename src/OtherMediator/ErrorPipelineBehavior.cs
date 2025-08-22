namespace OtherMediator;

using System.Runtime.ExceptionServices;
using OtherMediator.Contracts;

public class ErrorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Invokes the next pipeline behavior/delegate with the given request and returns its response.
    /// </summary>
    /// <param name="request">The incoming request being handled.</param>
    /// <param name="next">Delegate representing the next pipeline step; should return the response for the request.</param>
    /// <param name="cancellationToken">Token to observe while waiting for the next delegate.</param>
    /// <returns>The response produced by the next pipeline behavior.</returns>
    /// <remarks>Any exception thrown by <paramref name="next"/> is propagated to the caller.</remarks>
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}