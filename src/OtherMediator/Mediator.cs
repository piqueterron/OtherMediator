namespace OtherMediator;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using OtherMediator.Contracts;

public sealed class Mediator(IContainer container, MiddlewarePipeline pipeline) : IMediator
{
    private readonly MiddlewarePipeline _pipeline = pipeline;
    private readonly IContainer _c = container;

    private readonly ConcurrentDictionary<Type, Delegate> _senderCache = new();

    public async Task Publish<TNotification>([NotNull] TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var x = _c.Resolve<IEnumerable<INotificationHandler<TNotification>>>();

        x ??= Enumerable.Empty<INotificationHandler<TNotification>>();

        var tasks = x.Select(handler => handler.Handle(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task<TResponse> Send<TRequest, TResponse>([NotNull] TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, TResponse>();

        return await sender(request, cancellationToken);
    }

    public async Task<Unit> Send<TRequest>([NotNull] TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<Unit>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, Unit>();

        return await sender(request, cancellationToken);
    }

    private Func<TRequest, CancellationToken, Task<TResponse>> GetOrAddHandler<TRequest, TResponse>() where TRequest : IRequest<TResponse>
    {
        return (Func<TRequest, CancellationToken, Task<TResponse>>)_senderCache.GetOrAdd(typeof(TRequest), _ =>
        {
            var handler = _c.Resolve<IRequestHandler<TRequest, TResponse>>();

            var pipelines = _c.Resolve<IEnumerable<IPipelineBehavior<TRequest, TResponse>>>();
            pipelines ??= Enumerable.Empty<IPipelineBehavior<TRequest, TResponse>>();

            return _pipeline.BuildPipeline(handler, pipelines);
        });
    }
}