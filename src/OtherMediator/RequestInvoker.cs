namespace OtherMediator;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OtherMediator.Contracts;

internal static class RequestInvoker<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static Func<object, CancellationToken, Task<object>> Create(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IPipelineBehavior<TRequest, TResponse>>? pipelines)
    {
        var pipeline = MiddlewarePipelineBuilder.BuildPipeline(handler, pipelines ?? Array.Empty<IPipelineBehavior<TRequest, TResponse>>());

        return async (reqObj, ct) =>
        {
            var req = (TRequest)reqObj!;
            var res = await pipeline(req, ct).ConfigureAwait(false);
            return (object)res!;
        };
    }

    public static Func<object, CancellationToken, Task<object>> Create(object handlerObj, object? pipelinesObj)
    {
        var handler = (IRequestHandler<TRequest, TResponse>)handlerObj!;
        var pipelines = pipelinesObj as IEnumerable<IPipelineBehavior<TRequest, TResponse>>;
        return Create(handler, pipelines);
    }
}
