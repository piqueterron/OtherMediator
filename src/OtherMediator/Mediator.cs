namespace OtherMediator;

using OtherMediator.Contracts;

public sealed class Mediator(IMediatorConfiguration configuration) : IMediator
{
    private readonly IMediatorConfiguration _configuration = configuration;

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        var delegates = WarmMediator.GetNotificationHandlers(typeof(TNotification));

        var handlers = delegates?.Select(d => (Func<object, CancellationToken, Task>)d).ToArray();

        if (handlers is null || handlers.Length == 0)
        {
            return;
        }

        var tasks = handlers.Select(task => task(notification, cancellationToken)).ToArray();

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

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var requestType = request.GetType();

        var sender = WarmMediator.GetRequestHandler(requestType, typeof(TResponse)) as Func<object, CancellationToken, Task<TResponse>>;

        if (sender is null)
        {
            throw new InvalidOperationException($"No request handler registered for {requestType.Name} -> {typeof(TResponse).Name}");
        }

        return await sender(request, cancellationToken);
    }

    public async Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var requestType = request.GetType();

        var sender = WarmMediator.GetRequestHandler(requestType, typeof(Unit)) as Func<object, CancellationToken, Task<Unit>>;

        if (sender is null)
        {
            throw new InvalidOperationException($"No request handler registered for {requestType.Name} -> {typeof(Unit).Name}");
        }

        return await sender(request, cancellationToken);
    }
}
