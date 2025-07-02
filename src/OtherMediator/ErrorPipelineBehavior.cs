namespace OtherMediator;

using System.Runtime.ExceptionServices;
using OtherMediator.Contracts;

public class ErrorPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken);
        }
        catch (Exception ex)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();

            throw;
        }
    }
}