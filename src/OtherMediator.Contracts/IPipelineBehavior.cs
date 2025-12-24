namespace OtherMediator.Contracts;

public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}

public interface IPipelineBehavior<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification request, Func<TNotification, CancellationToken, Task> next, CancellationToken cancellationToken);
}
