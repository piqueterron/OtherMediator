namespace OtherMediator.Integration.Tests.Fixtures;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails = problemDetails;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = exception.GetType().Name,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        await _problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });

        return true;
    }
}
