namespace OtherMediator;

using System.Collections.Concurrent;
using System.Collections.Generic;
using OtherMediator.Contracts;

public static class WarmMediator
{
    private static readonly ConcurrentDictionary<(Type Request, Type Response), Func<Delegate>> _senderCache = new();
    private static readonly ConcurrentDictionary<Type, Func<Delegate>> _publishCache = new();

    public static void WarmRequestHandlers<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> requestHandler, IEnumerable<IPipelineBehavior<TRequest, TResponse>>? pipelineBehaviors)
        where TRequest : IRequest<TResponse>
            => CachingRequestHandlers(requestHandler, pipelineBehaviors);

    public static void WarmNotificationHandlers<TNotification>(INotificationHandler<TNotification> requestHandler, IEnumerable<IPipelineBehavior<TNotification>>? pipelineBehaviors)
        where TNotification : INotification
            => CachingNotificationHandlers(requestHandler, pipelineBehaviors);

    public static ConcurrentDictionary<Type, Delegate> GetNotificationHandlerCache() => new();

    public static ConcurrentDictionary<(Type Request, Type Response), Delegate> GetRequestHandlerCache() => new();

    public static Delegate? GetNotificationHandler(Type notification)
    {
        var key = notification;

        if (_publishCache.TryGetValue(key, out var func))
        {
            return func();
        }

        return null;
    }

    public static Delegate? GetRequestHandler(Type request, Type response)
    {
        var key = (request, response);

        if (_senderCache.TryGetValue(key, out var func))
        {
            return func();
        }

        return null;
    }

    private static void CachingRequestHandlers<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> requestHandler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>>? pipelineBehaviors)
        where TRequest : IRequest<TResponse>
    {
        var key = (typeof(TRequest), typeof(TResponse));

        _senderCache.TryAdd(key, () =>
        {
            if (requestHandler is null)
            {
                throw new InvalidOperationException($"Make sure to register an IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> in the dependency container.");
            }

            pipelineBehaviors ??= [];

            return MiddlewarePipelineBuilder.BuildPipeline(requestHandler, pipelineBehaviors);
        });
    }

    private static void CachingNotificationHandlers<TNotification>(
        INotificationHandler<TNotification> requestHandler,
        IEnumerable<IPipelineBehavior<TNotification>>? pipelineBehaviors)
        where TNotification : INotification
    {
        var key = typeof(TNotification);

        _publishCache.TryAdd(key, () =>
        {
            if (requestHandler is null)
            {
                throw new InvalidOperationException($"Make sure to register an INotificationHandler<{typeof(TNotification).Name}> in the dependency container.");
            }

            pipelineBehaviors ??= [];

            return MiddlewarePipelineBuilder.BuildPipeline(requestHandler, pipelineBehaviors);
        });
    }
}
