namespace OtherMediator.Integration.Tests.Handlers;

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

public record TestRequest : IRequest<TestResponse>
{
}
public record TestResponse
{
}