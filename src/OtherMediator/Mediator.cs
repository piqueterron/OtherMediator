namespace OtherMediator;

using OtherMediator.Contracts;

public sealed class Mediator(IMediatorConfiguration configuration) : IMediator
{
    private readonly IMediatorConfiguration _configuration = configuration;

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification, nameof(notification));

        //var handlers = WarmMediator.GetNotificationHandlerCache()[typeof(TNotification)] as IEnumerable<Func<INotification, CancellationToken, Task>>;

        var handlers = WarmMediator.GetNotificationHandler(typeof(TNotification)) as IEnumerable<Func<TNotification, CancellationToken, Task>>;

        var tasks = handlers!.Select(task => task(notification, cancellationToken)).ToArray();

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

    public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = WarmMediator.GetRequestHandler(typeof(TRequest), typeof(TResponse)) as Func<TRequest, CancellationToken, Task<TResponse>>;

        return await sender!(request, cancellationToken);
    }

    public async Task<Unit> Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<Unit>
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var sender = WarmMediator.GetRequestHandler(typeof(TRequest), typeof(Unit)) as Func<TRequest, CancellationToken, Task<Unit>>;

        return await sender!(request, cancellationToken);
    }
}
