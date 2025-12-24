namespace OtherMediator.Microsoft.SourceGenerator;

using System.Diagnostics;
using global::Microsoft.CodeAnalysis;
using OtherMediator.Microsoft.SourceGenerator.Templates;

[Generator]
public sealed class OtherMediatorIncrementalGenerator : IIncrementalGenerator
{
    private const string NAMESPACE_ROOT = "OtherMediator";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
//#if DEBUG
//        if (!Debugger.IsAttached)
//        {
//            Debugger.Launch();
//        }
//#endif
        var filter = new FilterSyntaxProvider(NAMESPACE_ROOT);

        var requestHandlers = context.SyntaxProvider
            .CreateSyntaxProvider(FilterSyntaxProvider.IsClassDeclarationSyntax, filter.RequestHandler)
            .Where(symbol => symbol is not null)
            .Collect();

        var notificationHandlers = context.SyntaxProvider
            .CreateSyntaxProvider(FilterSyntaxProvider.IsClassDeclarationSyntax, filter.NotificationHandler)
            .Where(symbol => symbol is not null)
            .Collect();

        var handlers = requestHandlers.Combine(notificationHandlers)
            .Select(static (handlers, _) => handlers.Left.AddRange(handlers.Right));

        context.RegisterSourceOutput(handlers, OtherMediatorStartupTemplate.GetTemplateAction<ITypeSymbol?>(NAMESPACE_ROOT));
        context.RegisterSourceOutput(handlers, OtherMediatorServiceCollectionTemplate.GetTemplateAction<ITypeSymbol?>(NAMESPACE_ROOT));
    }
}
