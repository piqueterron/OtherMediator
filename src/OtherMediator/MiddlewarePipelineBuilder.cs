namespace OtherMediator;

using OtherMediator.Contracts;

internal static class MiddlewarePipelineBuilder
{
    public static Func<TRequest, CancellationToken, Task<TResponse>> BuildPipeline<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        pipelines ??= Array.Empty<IPipelineBehavior<TRequest, TResponse>>();

        Func<TRequest, CancellationToken, Task<TResponse>> step = handler.HandleAsync;

        foreach (var behavior in pipelines.Reverse())
        {
            step = (req, ct) => behavior.Handle(req, step, ct);
        }

        return step;
    }

    public static Func<TNotification, CancellationToken, Task> BuildPipeline<TNotification>(INotificationHandler<TNotification> handler, IEnumerable<IPipelineBehavior<TNotification>> pipelines)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        pipelines ??= Array.Empty<IPipelineBehavior<TNotification>>();

        Func<TNotification, CancellationToken, Task> step = handler.Handle;

        foreach (var behavior in pipelines.Reverse())
        {
            step = (req, ct) => behavior.Handle(req, step, ct);
        }

        return step;
    }
}
