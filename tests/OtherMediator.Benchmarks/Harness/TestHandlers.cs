namespace OtherMediator.Benchmarks.Harness;

public class SimpleRequestHandler :
    MediatR.IRequestHandler<SimpleRequest, SimpleResponse>,
    OtherMediator.Contracts.IRequestHandler<SimpleRequest, SimpleResponse>
{
    Task<SimpleResponse> MediatR.IRequestHandler<SimpleRequest, SimpleResponse>.Handle(SimpleRequest request, CancellationToken ct)
    {
        return HandleCore(request, ct);
    }

    Task<SimpleResponse> Contracts.IRequestHandler<SimpleRequest, SimpleResponse>.HandleAsync(SimpleRequest request, CancellationToken ct)
    {
        return HandleCore(request, ct);
    }

    private Task<SimpleResponse> HandleCore(SimpleRequest request, CancellationToken ct)
    {
        var response = new SimpleResponse(
            ProcessedData: $"Processed: {request.Data}",
            Timestamp: DateTime.UtcNow
        );
        return Task.FromResult(response);
    }
}

public class ComplexRequestHandler :
    MediatR.IRequestHandler<ComplexRequest, ComplexResponse>,
    OtherMediator.Contracts.IRequestHandler<ComplexRequest, ComplexResponse>
{
    Task<ComplexResponse> MediatR.IRequestHandler<ComplexRequest, ComplexResponse>.Handle(
        ComplexRequest request, CancellationToken ct)
    {
        return HandleCore(request);
    }

    Task<ComplexResponse> OtherMediator.Contracts.IRequestHandler<ComplexRequest, ComplexResponse>.HandleAsync(
        ComplexRequest request, CancellationToken ct)
    {
        return HandleCore(request);
    }

    private Task<ComplexResponse> HandleCore(ComplexRequest request)
    {
        var warnings = new List<string>();

        if (request.Items.Count > 10)
            warnings.Add("Many items detected");

        var response = new ComplexResponse(
            RequestId: Guid.NewGuid(),
            ProcessedItems: request.Items.Count,
            TotalCost: request.Items.Count * 2.5m,
            Success: true,
            Warnings: warnings
        );

        return Task.FromResult(response);
    }
}

public class SimpleNotificationHandler :
    MediatR.INotificationHandler<SimpleNotification>,
    OtherMediator.Contracts.INotificationHandler<SimpleNotification>
{
    Task MediatR.INotificationHandler<SimpleNotification>.Handle(
        SimpleNotification notification, CancellationToken ct)
    {
        return HandleCore(notification);
    }

    Task OtherMediator.Contracts.INotificationHandler<SimpleNotification>.Handle(
        SimpleNotification notification, CancellationToken ct)
    {
        return HandleCore(notification);
    }

    private Task HandleCore(SimpleNotification notification)
    {
        Console.WriteLine($"Notification handled: {notification.EventType}");
        return Task.CompletedTask;
    }
}

public class SecondNotificationHandler :
    MediatR.INotificationHandler<SimpleNotification>,
    OtherMediator.Contracts.INotificationHandler<SimpleNotification>
{
    Task MediatR.INotificationHandler<SimpleNotification>.Handle(
        SimpleNotification notification, CancellationToken ct)
    {
        return HandleCore(notification);
    }

    Task OtherMediator.Contracts.INotificationHandler<SimpleNotification>.Handle(
        SimpleNotification notification, CancellationToken ct)
    {
        return HandleCore(notification);
    }

    private Task HandleCore(SimpleNotification notification)
    {
        if (notification.EventType == "ALERT")
        {
        }
        return Task.CompletedTask;
    }
}
