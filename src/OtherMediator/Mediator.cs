namespace OtherMediator;

using System.Collections.Concurrent;
using OtherMediator.Contracts;

public sealed class Mediator(IContainer container, MiddlewarePipeline pipeline) : IMediator
{
    private readonly MiddlewarePipeline _pipeline = pipeline;
    private readonly IContainer _container = container;

    private readonly ConcurrentDictionary<Type, Delegate> _senderCache = new();

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var handlers = _container.Resolve<IEnumerable<INotificationHandler<TNotification>>>();
        var tasks = handlers.Select(handler => handler.Handle(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = (Func<TRequest, CancellationToken, Task<TResponse>>)_senderCache.GetOrAdd(typeof(TRequest), _ =>
        {
            var handler = _container.Resolve<IRequestHandler<TRequest, TResponse>>();
            var pipelines = _container.Resolve<IEnumerable<IPipelineBehavior<TRequest, TResponse>>>();

            return _pipeline.BuildPipeline(handler, pipelines);
        });

        return await sender(request, cancellationToken);
    }
}