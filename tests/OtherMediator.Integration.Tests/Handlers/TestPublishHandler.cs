namespace OtherMediator.Integration.Tests.Handlers;

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
