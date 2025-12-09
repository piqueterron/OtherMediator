namespace OtherMediator.Extensions.OpenTelemetry;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OtherMediator.Contracts;

public class OpenTelemetryPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly MediatorInstrumentation _mediatorInstrumentation;
    private readonly ILogger<OpenTelemetryPipelineBehavior<TRequest, TResponse>> _logger;

    public OpenTelemetryPipelineBehavior(MediatorInstrumentation mediatorInstrumentation, ILogger<OpenTelemetryPipelineBehavior<TRequest, TResponse>> logger)
    {
        _mediatorInstrumentation = mediatorInstrumentation;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, Func<TRequest, CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = _mediatorInstrumentation.GetActivity.StartActivity(requestName, ActivityKind.Internal);

        activity?.SetTag("request.type", typeof(TRequest).FullName!);

        _mediatorInstrumentation.GetRequestCounter.Add(1, new KeyValuePair<string, object?>("request", requestName));

        var sw = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Handling request {RequestName}", requestName);

            var response = await next(request, cancellationToken);

            activity?.SetTag("response.type", typeof(TResponse).FullName!);

            _logger.LogInformation("Handled request {RequestName} with response type {ResponseType}", requestName, typeof(TResponse).FullName);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogError(ex, "Error handling request {RequestName}", requestName);

            throw;
        }
        finally
        {
            sw.Stop();
            _mediatorInstrumentation.GetRequestDuration.Record(sw.Elapsed.TotalMilliseconds, new KeyValuePair<string, object?>("request", requestName));
        }
    }
}
