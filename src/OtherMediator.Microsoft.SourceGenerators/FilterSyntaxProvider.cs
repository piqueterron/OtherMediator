namespace OtherMediator.Microsoft.SourceGenerator;

using System.Threading;
using global::Microsoft.CodeAnalysis;
using global::Microsoft.CodeAnalysis.CSharp.Syntax;

public class FilterSyntaxProvider
{
    private readonly string _namespace;

    public FilterSyntaxProvider(string @namespace)
    {
        _namespace = @namespace;
    }

    public static bool IsClassDeclarationSyntax(SyntaxNode node, CancellationToken _) => node is ClassDeclarationSyntax;

    public ITypeSymbol? RequestHandler(GeneratorSyntaxContext context, CancellationToken token)
        => SearchOriginalDefinition($"{_namespace}.Contracts.IRequestHandler<TRequest, TResponse>", context, token);

    public ITypeSymbol? NotificationHandler(GeneratorSyntaxContext context, CancellationToken token)
        => SearchOriginalDefinition($"{_namespace}.Contracts.INotificationHandler<TNotification>", context, token);

    private static ITypeSymbol? SearchOriginalDefinition(string definition, GeneratorSyntaxContext context, CancellationToken token)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration, token);

        if (symbol is not ITypeSymbol typeSymbol)
        {
            return null;
        }

        var implementsRequestHandler = typeSymbol.AllInterfaces.Any(@interface =>
            @interface.OriginalDefinition.ToDisplayString() == definition);

        return implementsRequestHandler ? typeSymbol : null;
    }
}
