namespace OtherMediator.Contracts;

public interface IRequest<TResponse> { }

public interface IRequest : IRequest<Unit> { }