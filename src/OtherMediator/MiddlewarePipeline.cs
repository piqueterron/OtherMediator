namespace OtherMediator;

using OtherMediator.Contracts;

public sealed class MiddlewarePipeline
{
    public Func<TRequest, CancellationToken, Task<TResponse>> BuildPipeline<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines)
        where TRequest : IRequest<TResponse>
    {
        //ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        Func<TRequest, CancellationToken, Task<TResponse>> pipeline = handler.Handle;

        foreach (var behavior in pipelines.Reverse())
        {
            var next = pipeline;
            pipeline = (req, ct) => behavior.Handle(req, next, ct);
        }

        return pipeline;
    }
}