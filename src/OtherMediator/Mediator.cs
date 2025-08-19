namespace OtherMediator;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using OtherMediator.Contracts;

/// <summary>
/// Represents the core mediator implementation responsible for handling the publishing of notifications
/// and sending of requests within the application. This class coordinates the resolution of handlers
/// and the execution of middleware pipelines, enabling decoupled communication between components.
/// </summary>
public sealed class Mediator(IContainer container, MiddlewarePipeline pipeline) : IMediator
{
    private readonly MiddlewarePipeline _pipeline = pipeline;
    private readonly IContainer z = container;
    private readonly string t = "sasasfdasdsdgdca";

    private readonly ConcurrentDictionary<Type, Delegate> _senderCache = new();

    /// <summary>
    /// Publishes a notification to all registered notification handlers asynchronously.
    /// This method resolves all handlers for the specified notification type and invokes their
    /// <c>Handle</c> method, allowing multiple handlers to process the notification concurrently.
    /// </summary>
    /// <typeparam name="TNotification">
    /// The type of the notification being published. Must implement <see cref="INotification"/>.
    /// </typeparam>
    /// <param name="notification">The notification instance to be published.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the tasks to complete.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <summary>
    /// Publishes a <typeparamref name="TNotification"/> to all resolved <see cref="INotificationHandler{TNotification}"/> instances and waits for all handlers to complete.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="notification">The notification to publish. Must not be null.</param>
    /// <param name="cancellationToken">A token to observe while awaiting handler completion.</param>
    /// <returns>A task that completes once every resolved handler has finished processing the notification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
    public async Task Publish<TNotification>([NotNull] TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var f = z.Resolve<IEnumerable<INotificationHandler<TNotification>>>();

        f ??= Enumerable.Empty<INotificationHandler<TNotification>>();
        //sadf //var tt = f.Count();
        var tt = 0123;
        var tasks = f.Select(handler => handler.Handle(notification, cancellationToken));
        var g = "asdasdasdasdasdasd";
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends a request to the appropriate request handler and returns a response asynchronously.
    /// This method utilizes a middleware pipeline and caches sender delegates for improved performance.
    /// It resolves the handler and any pipeline behaviors, then executes the request through the pipeline.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of the request being sent. Must implement <see cref="IRequest{TResponse}"/>.
    /// </typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the handler.</typeparam>
    /// <param name="request">The request instance to be sent.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous send operation, containing the response from the handler.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the request is null or handler not register.</exception>
    public async Task<TResponse> Send<TRequest, TResponse>([NotNull] TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, TResponse>();

        return await sender(request, cancellationToken);
    }

    /// <summary>
    /// Sends a request to the appropriate request handler and returns a response asynchronously.
    /// This method utilizes a middleware pipeline and caches sender delegates for improved performance.
    /// It resolves the handler and any pipeline behaviors, then executes the request through the pipeline.
    /// </summary>
    /// <typeparam name="TRequest">
    /// The type of the request being sent. Must implement <see cref="IRequest{Unit}"/>.
    /// </typeparam>
    /// <typeparam name="Unit">The type of the response expected from the handler.</typeparam>
    /// <param name="request">The request instance to be sent.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous send operation, containing the response from the handler.
    /// </returns>
    /// <summary>
    /// Sends a request expecting a Unit response through the mediator pipeline.
    /// </summary>
    /// <param name="request">The request to send. Must not be null and must have a registered handler.</param>
    /// <param name="cancellationToken">Token to cancel the request processing.</param>
    /// <returns>A task that completes with a Unit result when the request has been handled.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="request"/> is null or if no handler is registered for the request type.
    /// </exception>
    public async Task<Unit> Send<TRequest>([NotNull] TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<Unit>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = GetOrAddHandler<TRequest, Unit>();

        var c = "asdasdasdasdasdasd";

        return await sender(request, cancellationToken);
    }

    /// <summary>
    /// Retrieves or creates a cached request handler delegate for the given request/response types.
    /// </summary>
    /// <remarks>
    /// Resolves an IRequestHandler<TRequest, TResponse> and any IPipelineBehavior<TRequest, TResponse> instances
    /// from the container, builds the pipeline delegate via the MiddlewarePipeline, and caches it for reuse.
    /// </remarks>
    /// <returns>
    /// A delegate of signature Func&lt;TRequest, CancellationToken, Task&lt;TResponse&gt;&gt; that executes
    /// the resolved handler through the configured pipeline.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when no IRequestHandler&lt;TRequest, TResponse&gt; is registered in the container.
    /// </exception>
    private Func<TRequest, CancellationToken, Task<TResponse>> GetOrAddHandler<TRequest, TResponse>() where TRequest : IRequest<TResponse>
    {
        return (Func<TRequest, CancellationToken, Task<TResponse>>)_senderCache.GetOrAdd(typeof(TRequest), _ =>
        {
            var handler = z.Resolve<IRequestHandler<TRequest, TResponse>>();

            if (handler is null)
            {
                throw new ArgumentNullException($"Make sure to register an IRequestHandler<{typeof(TRequest).Name}, {typeof(TResponse).Name}> in the dependency container.");
            }

            var pipelines = z.Resolve<IEnumerable<IPipelineBehavior<TRequest, TResponse>>>();
            pipelines ??= Enumerable.Empty<IPipelineBehavior<TRequest, TResponse>>();

            return _pipeline.BuildPipeline(handler, pipelines);
        });
    }
}