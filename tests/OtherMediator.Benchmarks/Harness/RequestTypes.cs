namespace OtherMediator.Benchmarks.Harness;

// MediatR

public record SimpleRequest(int Id, string Data) : MediatR.IRequest<SimpleResponse>;
public record SimpleResponse(string ProcessedData, DateTime Timestamp);

public record ComplexRequest(Guid UserId, List<string> Items, Dictionary<string, object> Metadata)
    : MediatR.IRequest<ComplexResponse>;

public record ComplexResponse(
    Guid RequestId,
    int ProcessedItems,
    decimal TotalCost,
    bool Success,
    List<string> Warnings);

public record SimpleNotification(string EventType, DateTime OccurredAt) : MediatR.INotification;

// Other Mediator

public record SimpleRequest2(int Id, string Data) : OtherMediator.Contracts.IRequest<SimpleResponse2>;

public record SimpleResponse2(string ProcessedData, DateTime Timestamp);

public record ComplexRequest2(Guid UserId, List<string> Items, Dictionary<string, object> Metadata)
    : OtherMediator.Contracts.IRequest<ComplexResponse2>;

public record ComplexResponse2(
    Guid RequestId,
    int ProcessedItems,
    decimal TotalCost,
    bool Success,
    List<string> Warnings);

public record SimpleNotification2(string EventType, DateTime OccurredAt) : OtherMediator.Contracts.INotification;
