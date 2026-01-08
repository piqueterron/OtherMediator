namespace OtherMediator;

using System.Collections.Concurrent;
using OtherMediator.Contracts;

public sealed class Mediator(IMediatorConfiguration configuration, IContainer container) : IMediator
{
    private readonly IMediatorConfiguration _configuration = configuration;
    private readonly IContainer _container = container;

    private readonly ConcurrentDictionary<(Type Request, Type Response), Delegate> _senderCache = new();
    private readonly ConcurrentDictionary<INotification, IEnumerable<Task>> _publishCache = new();

    /// <inheritdoc />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var tasks = GetOrAddPublishers(notification, cancellationToken);

        if (_configuration.DispatchStrategy == DispatchStrategy.Parallel)
        {
            await Task.WhenAll(tasks);
        }

        if (_configuration.DispatchStrategy == DispatchStrategy.Sequential)
        {
            foreach (var task in tasks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await task;
            }
        }
    }

    /// <inheritdoc />
    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, TResponse>();

        return await sender(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Unit> Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, Unit>();

        return await sender(request, cancellationToken);
    }

    private Func<TRequest, CancellationToken, Task<TResponse>> GetOrAddHandler<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
    {
        var key = (typeof(TRequest), typeof(TResponse));

        return (Func<TRequest, CancellationToken, Task<TResponse>>)_senderCache.GetOrAdd(key, _ =>
        {
            var handler = _container.Resolve<IRequestHandler<TRequest, TResponse>>();

            if (handler is null)
            {
                throw new InvalidOperationException($"Make sure to register an IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> in the dependency container.");
            }

            var pipelines = _container.Resolve<IEnumerable<IPipelineBehavior<TRequest, TResponse>>>();
            pipelines ??= [];

            return MiddlewarePipelineBuilder.BuildPipeline(handler, pipelines);
        });
    }

    private IEnumerable<Task> GetOrAddPublishers<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification
    {
        return _publishCache.GetOrAdd(notification, _ =>
        {
            var handlers = _container.Resolve<IEnumerable<INotificationHandler<TNotification>>>();
            handlers ??= [];

            return handlers.Select(handler => handler.Handle(notification, cancellationToken)).ToArray();
        });
    }
}
