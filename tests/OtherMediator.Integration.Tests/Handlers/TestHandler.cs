namespace OtherMediator.Integration.Tests.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using OtherMediator.Contracts;

public class TestHandler : IRequestHandler<TestRequest, TestResponse>
{
    public Task<TestResponse> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TestResponse());
    }
}

public class TestPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest: IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public record TestRequest : IRequest<TestResponse>
{
}

public record TestResponse
{
}