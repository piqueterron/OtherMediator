namespace OtherMediator.Benchmarks.Extensions;

using Microsoft.Extensions.DependencyInjection;
using OtherMediator.Benchmarks.Harness;
using OtherMediator.Contracts;

public static class WarmUpExtensions
{
    public static void WarmUpDefault(IServiceProvider serviceProvider)
    {
        var simpleHandler = serviceProvider.GetRequiredService<IRequestHandler<SimpleRequest, SimpleResponse>>();
        var complexHandler = serviceProvider.GetRequiredService<IRequestHandler<ComplexRequest, ComplexResponse>>();
        var notificationHandlers = serviceProvider.GetServices<INotificationHandler<SimpleNotification>>();

        var simpleBehaviors = serviceProvider.GetServices<IPipelineBehavior<SimpleRequest, SimpleResponse>>();
        var complexBehaviors = serviceProvider.GetServices<IPipelineBehavior<ComplexRequest, ComplexResponse>>();
        var notificationBehaviors = serviceProvider.GetServices<IPipelineBehavior<SimpleNotification>>();

        WarmMediator.WarmRequestHandlers(simpleHandler, simpleBehaviors);
        WarmMediator.WarmRequestHandlers(complexHandler, complexBehaviors);

        foreach (var notificationHandler in notificationHandlers)
        {
            WarmMediator.WarmNotificationHandlers(notificationHandler, notificationBehaviors);
        }
    }
}
