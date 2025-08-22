namespace OtherMediator;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using OtherMediator.Contracts;

public sealed class Mediator(IContainer container, MiddlewarePipeline pipeline) : IMediator
{
    private readonly MiddlewarePipeline _pipeline = pipeline;
    private readonly IContainer _c = container;

    private readonly ConcurrentDictionary<Type, Delegate> _senderCache = new();

    /// <summary>
    /// Publishes a notification to all registered <see cref="INotificationHandler{TNotification}"/> instances and awaits their completion.
    /// </summary>
    /// <param name="notification">The notification to publish. Must not be null.</param>
    /// <param name="cancellationToken">A token to observe while awaiting handler tasks.</param>
    /// <remarks>
    /// Resolves all notification handlers from the container, invokes each handler's <c>Handle</c> method concurrently, and awaits all handler tasks.
    /// If no handlers are registered, this method completes immediately.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public async Task Publish<TNotification>([NotNull] TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var x = _c.Resolve<IEnumerable<INotificationHandler<TNotification>>>();

        x ??= Enumerable.Empty<INotificationHandler<TNotification>>();

        var tasks = x.Select(handler => handler.Handle(notification, cancellationToken));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends a request through the mediator pipeline and returns the handler's response.
    /// </summary>
    /// <param name="request">The request message to send. Must not be null.</param>
    /// <param name="cancellationToken">Token to cancel the request processing.</param>
    /// <returns>A task that represents the asynchronous send operation. The task result is the handler's response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public async Task<TResponse> Send<TRequest, TResponse>([NotNull] TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, TResponse>();

        return await sender(request, cancellationToken);
    }

    /// <summary>
    /// Sends a request that produces no meaningful response (a <see cref="Unit"/> result) through the mediator pipeline.
    /// </summary>
    /// <param name="request">The request to dispatch. Must implement <see cref="IRequest{Unit}"/> and cannot be null.</param>
    /// <param name="cancellationToken">Token to observe while awaiting the request's completion.</param>
    /// <returns>A task that completes with <see cref="Unit"/> when the request has been handled.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
    public async Task<Unit> Send<TRequest>([NotNull] TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<Unit>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, Unit>();

        return await sender(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves or creates and caches a sender delegate for the request type that invokes the request handler pipeline.
    /// </summary>
    /// <remarks>
    /// On first request for a given <typeparamref name="TRequest"/>, the method resolves an <see cref="IRequestHandler{TRequest,TResponse}"/>
    /// and any <see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations from the container, substitutes an empty sequence if none are resolved,
    /// and builds a pipeline delegate via the middleware pipeline. The resulting delegate is cached by request type for subsequent calls.
    /// </remarks>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <returns>
    /// A delegate of signature <c>Func&lt;TRequest, CancellationToken, Task&lt;TResponse&gt;&gt;</c> that executes the handler pipeline for the request.
    /// </returns>
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