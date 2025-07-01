namespace OtherMediator.Integration.Tests.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using OtherMediator.Contracts;

public class TestSendHandler : IRequestHandler<TestRequest, TestResponse>
{
    public async Task<TestResponse> Handle(TestRequest request, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000); //simulating workload

        return new TestResponse
        {
            Value = request.Value,
            Check = true
        };
    }
}

public class TestPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        return await next(request, cancellationToken);
    }
}

public record TestRequest : IRequest<TestResponse>
{
    public string Value { get; set; }
}

public record TestResponse
{
    public string Value { get; set; }

    public bool Check { get; set; }
}