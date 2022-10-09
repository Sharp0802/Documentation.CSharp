using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler.Viewers;

public static class Declaration
{
    static Declaration()
    {
        Viewers = new DeclarationViewer[]
        {
            new DelegateDeclarationViewer(),
            new EventDeclarationViewer(),
            new FieldDeclarationViewer(),
            new MethodDeclarationViewer(),
            new NamedTypeDeclarationViewer(),
            new PropertyDeclarationViewer()
        };
    }

    private static DeclarationViewer[] Viewers { get; }

    private static bool HasComment(ISymbol symbol)
    {
        return !string.IsNullOrEmpty(symbol.GetDocumentationCommentXml());
    }

    public static bool IsSupported(SyntaxNode syntax, ISymbol symbol)
    {
        return Viewers.Any(v => v.IsSupported(syntax, symbol));
    }

    public static DeclarationInfo? View(string assemblyFile, SemanticModel semantic, SyntaxNode syntax)
    {
        var symbol = semantic.GetDeclaredSymbol(syntax);
        if (symbol is null) return null;

        var viewer = Viewers.FirstOrDefault(v => v.IsSupported(syntax, symbol));
        if (viewer is null) return null;

        var decl = viewer.View(semantic, syntax);
        if (decl is null) return null;

        var methods = new List<DeclarationInfo>();
        var events = new List<DeclarationInfo>();
        var properties = new List<DeclarationInfo>();
        var fields = new List<DeclarationInfo>();
        
        if (symbol is INamedTypeSymbol type)
        {
            foreach (var child in type.GetMembers()
                         .Where(HasComment)
                         .Select(member => member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax())
                         .Where(childSyntax => childSyntax is not null)
                         .Select(childSyntax => View(assemblyFile, semantic, childSyntax!))
                         .Where(child => child is not null))
            {
                (child!.Kind switch
                {
                    DeclarationKind.Method => methods,
                    DeclarationKind.Event => events,
                    DeclarationKind.Property => properties,
                    DeclarationKind.Field => fields,
                    _ => null
                })?.Add(child);
            }
        }

        return DeclarationInfoHelper.Create(
            symbol, 
            $"{symbol.Name}{DeclarationViewer.ViewGenericParameters(symbol)}", 
            assemblyFile, 
            viewer.Kind, 
            decl, 
            methods.ToArray(), 
            events.ToArray(), 
            properties.ToArray(), 
            fields.ToArray());
    }
}
