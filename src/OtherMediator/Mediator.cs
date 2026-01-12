namespace OtherMediator;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OtherMediator.Contracts;

/// <inheritdoc cref="IMediator" />
public sealed class Mediator(IMediatorConfiguration configuration, IContainer container) : IMediator
{
    private readonly IMediatorConfiguration _configuration = configuration;
    private readonly IContainer _container = container;

    private readonly ConcurrentDictionary<(Type Request, Type Response), Func<object, CancellationToken, Task<object>>> _objectSenderCache = new();
    private readonly ConcurrentDictionary<INotification, IEnumerable<Delegate>> _publishCache = new();

    /// <inheritdoc cref="IPublisher" />
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var @delegates = GetOrAddPublishers(notification);

        var tasks = @delegates.Select(task => task(notification, cancellationToken)).ToArray();

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

    /// <inheritdoc cref="ISender" />
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var key = (requestType, responseType);

        var invoker = _objectSenderCache.GetOrAdd(key, _ =>
        {
            var handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            var handler = _container.Resolve(handlerInterface);

            if (handler is null)
            {
                throw new InvalidOperationException($"Make sure to register an IRequestHandler<{requestType.Name}, {responseType.Name}> in the dependency container.");
            }

            var pipelineBehaviorInterface = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
            var pipelinesEnumerableType = typeof(IEnumerable<>).MakeGenericType(pipelineBehaviorInterface);

            var pipelines = _container.Resolve(pipelinesEnumerableType);

            var invokerGeneric = typeof(RequestInvoker<,>).MakeGenericType(requestType, responseType);
            var createMethod = invokerGeneric.GetMethod("Create", new[] { typeof(object), typeof(object) })!;

            var wrapper = (Func<object, CancellationToken, Task<object>>)createMethod.Invoke(null, new[] { handler, pipelines })!;

            return wrapper;
        });

        var result = await invoker(request, cancellationToken);
        return (TResponse)result!;
    }

    private IEnumerable<Func<TNotification, CancellationToken, Task>> GetOrAddPublishers<TNotification>(TNotification notification)
        where TNotification : INotification
    {
        return (IEnumerable<Func<TNotification, CancellationToken, Task>>)_publishCache.GetOrAdd(notification, _ =>
        {
            var handlers = _container.Resolve<IEnumerable<INotificationHandler<TNotification>>>();
            handlers ??= [];

            var pipelines = _container.Resolve<IEnumerable<IPipelineBehavior<TNotification>>>();
            pipelines ??= [];

            List<Func<TNotification, CancellationToken, Task>> builtPipelines = [];

            foreach (var handler in handlers)
            {
                builtPipelines.Add(MiddlewarePipelineBuilder.BuildPipeline(handler, pipelines));
            }

            return builtPipelines;
        });
    }
}
