namespace OtherMediator;

using OtherMediator.Contracts;

public sealed class MiddlewarePipeline
{
    /// <summary>
    /// Builds a composed request handling pipeline by wrapping the given handler with the provided pipeline behaviors.
    /// </summary>
    /// <param name="handler">The terminal request handler invoked by the innermost pipeline.</param>
    /// <param name="pipelines">An ordered sequence of pipeline behaviors; the first element in this sequence becomes the outermost wrapper.</param>
    /// <returns>
    /// A delegate that, when invoked, executes the composed pipeline: each behavior delegates to the next, with the final call reaching <paramref name="handler"/>.
    /// If <paramref name="pipelines"/> is empty, the returned delegate is equivalent to <see cref="IRequestHandler{TRequest,TResponse}.Handle"/>.
    /// </returns>
    /// <remarks>
    /// The caller is expected to provide non-null <paramref name="handler"/> and <paramref name="pipelines"/>; the method composes behaviors in reverse to ensure the original sequence order defines the outer-to-inner wrapping.
    /// </remarks>
    public Func<TRequest, CancellationToken, Task<TResponse>> BuildPipeline<TRequest, TResponse>(
        IRequestHandler<TRequest, TResponse> handler,
        IEnumerable<IPipelineBehavior<TRequest, TResponse>> pipelines)
        where TRequest : IRequest<TResponse>
    {
        Func<TRequest, CancellationToken, Task<TResponse>> pipeline = handler.Handle;

        foreach (var behavior in pipelines.Reverse())
        {
            var next = pipeline;
            pipeline = (req, ct) => behavior.Handle(req, next, ct);
        }

        return pipeline;
    }
}