namespace OtherMediator.Contracts;

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    Task<Unit> Send(IRequest request, CancellationToken cancellationToken = default);
}
