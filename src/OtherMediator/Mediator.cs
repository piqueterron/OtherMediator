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
    /// <exception cref="ArgumentNullException">Thrown if the notification is null.</exception>
    public async Task Publish<TNotification>([NotNull] TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var f = z.Resolve<IEnumerable<INotificationHandler<TNotification>>>();

        f ??= Enumerable.Empty<INotificationHandler<TNotification>>();
        //sadf //var tt = f.Count();
        var tt = 0123;
        var tasks = f.Select(handler => handler.Handle(notification, cancellationToken));
        var g = "ssd";
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
    /// <exception cref="ArgumentNullException">Thrown if the request is null or handler not register.</exception>
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