namespace OtherMediator.Benchmarks.Harness;

public class SimpleRequestHandler : MediatR.IRequestHandler<SimpleRequest, SimpleResponse>
{
    public Task<SimpleResponse> Handle(SimpleRequest request, CancellationToken ct)
    {
        var response = new SimpleResponse(
            ProcessedData: $"Processed: {request.Data}",
            Timestamp: DateTime.UtcNow
        );
        return Task.FromResult(response);
    }
}

public class ComplexRequestHandler : MediatR.IRequestHandler<ComplexRequest, ComplexResponse>
{
    public Task<ComplexResponse> Handle(ComplexRequest request, CancellationToken ct)
    {
        var warnings = new List<string>();
        if (request.Items.Count > 10)
            warnings.Add("Many items detected");

        //if (request.Metadata.ContainsKey("priority") &&
        //    request.Metadata["priority"]?.ToString() == "high")
        //{
        //    Thread.Sleep(1);
        //}

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

public class SimpleNotificationHandler : MediatR.INotificationHandler<SimpleNotification>
{
    public Task Handle(SimpleNotification notification, CancellationToken ct)
    {
        Console.WriteLine($"Notification handled: {notification.EventType}");
        return Task.CompletedTask;
    }
}

public class SecondNotificationHandler : MediatR.INotificationHandler<SimpleNotification>
{
    public Task Handle(SimpleNotification notification, CancellationToken ct)
    {
        if (notification.EventType == "ALERT")
        {
        }
        return Task.CompletedTask;
    }
}

// Other Mediator Handlers

public class SimpleRequestHandler2 : OtherMediator.Contracts.IRequestHandler<SimpleRequest2, SimpleResponse2>
{
    public Task<SimpleResponse2> Handle(SimpleRequest2 request, CancellationToken ct)
    {
        var response = new SimpleResponse2(
            ProcessedData: $"Processed: {request.Data}",
            Timestamp: DateTime.UtcNow
        );
        return Task.FromResult(response);
    }
}

public class ComplexRequestHandler2 : OtherMediator.Contracts.IRequestHandler<ComplexRequest2, ComplexResponse2>
{
    public Task<ComplexResponse2> Handle(ComplexRequest2 request, CancellationToken ct)
    {
        var warnings = new List<string>();
        if (request.Items.Count > 10)
            warnings.Add("Many items detected");

        //if (request.Metadata.ContainsKey("priority") &&
        //    request.Metadata["priority"]?.ToString() == "high")
        //{
        //    Thread.Sleep(1);
        //}

        var response = new ComplexResponse2(
            RequestId: Guid.NewGuid(),
            ProcessedItems: request.Items.Count,
            TotalCost: request.Items.Count * 2.5m,
            Success: true,
            Warnings: warnings
        );

        return Task.FromResult(response);
    }
}

public class SimpleNotificationHandler2 : OtherMediator.Contracts.INotificationHandler<SimpleNotification2>
{
    public Task Handle(SimpleNotification2 notification, CancellationToken ct)
    {
        Console.WriteLine($"Notification handled: {notification.EventType}");
        return Task.CompletedTask;
    }
}

public class SecondNotificationHandler2 : OtherMediator.Contracts.INotificationHandler<SimpleNotification2>
{
    public Task Handle(SimpleNotification2 notification, CancellationToken ct)
    {
        if (notification.EventType == "ALERT")
        {
        }
        return Task.CompletedTask;
    }
}
