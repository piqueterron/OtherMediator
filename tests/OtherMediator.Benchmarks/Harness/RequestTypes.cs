namespace OtherMediator.Benchmarks.Harness;

public record SimpleRequest(int Id, string Data) : MediatR.IRequest<SimpleResponse>, OtherMediator.Contracts.IRequest<SimpleResponse>;
public record SimpleResponse(string ProcessedData, DateTime Timestamp);

public record ComplexRequest(Guid UserId, List<string> Items, Dictionary<string, object> Metadata)
    : MediatR.IRequest<ComplexResponse>, OtherMediator.Contracts.IRequest<ComplexResponse>;

public record ComplexResponse(
    Guid RequestId,
    int ProcessedItems,
    decimal TotalCost,
    bool Success,
    List<string> Warnings);

public record SimpleNotification(string EventType, DateTime OccurredAt) : MediatR.INotification, OtherMediator.Contracts.INotification;
