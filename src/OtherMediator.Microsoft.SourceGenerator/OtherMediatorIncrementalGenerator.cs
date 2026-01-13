namespace OtherMediator.Microsoft.SourceGenerator;

using System.Collections.Immutable;

using global::Microsoft.CodeAnalysis;
using global::Microsoft.CodeAnalysis.CSharp.Syntax;
using OtherMediator.Microsoft.SourceGenerator.Templates;

[Generator]
public class MediatorSourceGenerator : IIncrementalGenerator
{
    private const string RequestHandlerInterface = "OtherMediator.Contracts.IRequestHandler<TRequest, TResponse>";
    private const string NotificationHandlerInterface = "OtherMediator.Contracts.INotificationHandler<TNotification>";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
//#if DEBUG
//        if (!System.Diagnostics.Debugger.IsAttached)
//        {
//            System.Diagnostics.Debugger.Launch();
//        }
//#endif

        var requestHandlers = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: TransformToRequestHandler)
            .Where(static handler => handler.HasValue)
            .Select(static (handler, _) => handler!.Value)
            .Collect();

        var notificationHandlers = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: TransformToNotificationHandler)
            .Where(static notification => notification.HasValue)
            .Select(static (notification, _) => notification!.Value)
            .Collect()
            .Select(static (notifications, _) => GroupNotifications(notifications));

        var combinedForExtensions = requestHandlers.Combine(notificationHandlers);
        var combinedForOpenTelemetry = requestHandlers.Combine(notificationHandlers);

        context.RegisterSourceOutput(requestHandlers, GenerateRequestHandlerSource);
        context.RegisterSourceOutput(notificationHandlers, GenerateNotificationHandlerSource);
        context.RegisterSourceOutput(requestHandlers, GenerateMicrosoftContainerSource);
        context.RegisterSourceOutput(combinedForExtensions, GenerateMediatorExtensionSource);
        context.RegisterSourceOutput(combinedForOpenTelemetry, GenerateOpenTelemetrySource);
    }

    private static (ITypeSymbol Request, ITypeSymbol Response, ITypeSymbol Handler)? TransformToRequestHandler(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        try
        {
            var classSymbol = (ITypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)!;

            foreach (var interfaceSymbol in classSymbol.AllInterfaces)
            {
                if (interfaceSymbol.IsGenericType &&
                    interfaceSymbol.OriginalDefinition.ToDisplayString() == RequestHandlerInterface)
                {
                    return (
                        Request: interfaceSymbol.TypeArguments[0],
                        Response: interfaceSymbol.TypeArguments[1],
                        Handler: classSymbol
                    );
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static (ITypeSymbol Notification, ITypeSymbol Handler)? TransformToNotificationHandler(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        try
        {
            var classSymbol = (ITypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(ctx.Node)!;

            foreach (var interfaceSymbol in classSymbol.AllInterfaces)
            {
                if (interfaceSymbol.IsGenericType &&
                    interfaceSymbol.OriginalDefinition.ToDisplayString() == NotificationHandlerInterface)
                {
                    return (
                        Notification: interfaceSymbol.TypeArguments[0],
                        Handler: classSymbol
                    );
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static ImmutableArray<(ITypeSymbol Notification, ImmutableArray<ITypeSymbol> Handlers)> GroupNotifications(ImmutableArray<(ITypeSymbol Notification, ITypeSymbol Handler)> items)
    {
        var grouped = items
            .GroupBy(
                item => item.Notification,
                item => item.Handler,
                (notification, handlers) => (notification, handlers.ToImmutableArray()))
            .ToImmutableArray();

        return grouped;
    }

    private static void GenerateRequestHandlerSource(
        SourceProductionContext ctx,
        ImmutableArray<(ITypeSymbol Request, ITypeSymbol Response, ITypeSymbol Handler)> handlers)
    {
        if (handlers.IsEmpty)
            return;

        try
        {
            SenderSourceGenerationTemplate.GenerateMediatorSource(ctx, handlers, "OtherMediator");
        }
        catch (Exception ex)
        {
            ReportError(ctx, "OMG001", "Error generating request mediator", ex.Message);
        }
    }

    private static void GenerateNotificationHandlerSource(
        SourceProductionContext ctx,
        ImmutableArray<(ITypeSymbol Notification, ImmutableArray<ITypeSymbol> Handlers)> notifications)
    {
        if (notifications.IsEmpty)
            return;

        try
        {
            NotificationSourceGenerationTemplate.GenerateNotificationSource(ctx, notifications, "OtherMediator");
        }
        catch (Exception ex)
        {
            ReportError(ctx, "OMG002", "Error generating notification mediator", ex.Message);
        }
    }

    private static void GenerateMicrosoftContainerSource(
        SourceProductionContext ctx,
        ImmutableArray<(ITypeSymbol Request, ITypeSymbol Response, ITypeSymbol Handler)> handlers)
    {
        if (handlers.IsEmpty)
            return;

        try
        {
            MicrosoftContainerTemplate.GenerateMicrosoftContainerSource(ctx, handlers, "OtherMediator");
        }
        catch (Exception ex)
        {
            ReportError(ctx, "OMG003", "Error generating Microsoft container", ex.Message);
        }
    }

    private static void GenerateMediatorExtensionSource(
        SourceProductionContext ctx,
        (ImmutableArray<(ITypeSymbol Request, ITypeSymbol Response, ITypeSymbol Handler)> Requests,
         ImmutableArray<(ITypeSymbol Notification, ImmutableArray<ITypeSymbol> Handlers)> Notifications) data)
    {
        var (requestHandlers, notificationHandlers) = data;

        if (requestHandlers.IsEmpty && notificationHandlers.IsEmpty)
            return;

        try
        {
            MediatorExtensionTemplate.GenerateMediatorExtensionSource(
                ctx,
                requestHandlers,
                notificationHandlers,
                "OtherMediator");
        }
        catch (Exception ex)
        {
            ReportError(ctx, "OMG004", "Error generating mediator extensions", ex.Message);
        }
    }

    private static void GenerateOpenTelemetrySource(
        SourceProductionContext ctx,
        (ImmutableArray<(ITypeSymbol Request, ITypeSymbol Response, ITypeSymbol Handler)> Requests,
         ImmutableArray<(ITypeSymbol Notification, ImmutableArray<ITypeSymbol> Handlers)> Notifications) data)
    {
        var (requestHandlers, notificationHandlers) = data;

        if (requestHandlers.IsEmpty && notificationHandlers.IsEmpty)
            return;

        try
        {
            OpenTelemetryExtensionsTemplate.GenerateOpenTelemetrySource(
                ctx,
                requestHandlers,
                notificationHandlers,
                "OtherMediator");
        }
        catch (Exception ex)
        {
            ReportError(ctx, "OMG005", "Error generating OpenTelemetry extensions", ex.Message);
        }
    }

    private static void ReportError(
        SourceProductionContext ctx,
        string id,
        string title,
        string message)
    {
        ctx.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id,
                title,
                $"Error: {message}",
                "OtherMediator",
                DiagnosticSeverity.Error,
                true),
            Location.None));
    }
}
