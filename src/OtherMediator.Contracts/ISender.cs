namespace OtherMediator.Contracts;

public interface ISender
{
    Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>;

    Task<Unit> Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<Unit>;
}