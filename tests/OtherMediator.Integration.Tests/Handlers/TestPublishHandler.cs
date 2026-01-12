namespace OtherMediator.Integration.Tests.Handlers;

using System;
using System.Threading.Tasks;
using OtherMediator.Contracts;
using OtherMediator.Integration;

public record TestNotification : INotification
{
    public string Message { get; set; }
}

public class TestNotificationOneHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200); //simulating workload

        await MonitorManager.SignalAsync();
    }
}

public class TestNotificationTwoHandler : INotificationHandler<TestNotification>
{
    public async Task Handle(TestNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(200); //simulating workload

        await MonitorManager.SignalAsync();
    }
}

public class TestNotificationPipeline<TNotification> : IPipelineBehavior<TNotification>
    where TNotification : INotification
{
    public async Task Handle(TNotification request, Func<TNotification, CancellationToken, Task> next, CancellationToken cancellationToken)
    {
        await next(request, cancellationToken);
    }
}

public class GlobalTestNotificationPipeline<TNotification> : IPipelineBehavior<TNotification>
    where TNotification : INotification
{
    public async Task Handle(TNotification request, Func<TNotification, CancellationToken, Task> next, CancellationToken cancellationToken)
    {
        await next(request, cancellationToken);
    }
}
