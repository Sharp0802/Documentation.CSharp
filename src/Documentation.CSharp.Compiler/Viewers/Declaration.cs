using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Documentation.CSharp.Compiler.Viewers;

public static class Declaration
{
    static Declaration()
    {
        Viewers = typeof(Declaration).Assembly.DefinedTypes
            .Where(t => t.IsSubclassOf(typeof(DeclarationViewer)))
            .Select(Activator.CreateInstance)
            .Cast<DeclarationViewer>()
            .ToArray();
    }

    private static DeclarationViewer[] Viewers { get; }

    public static bool IsSupported(SyntaxNode syntax, ISymbol symbol)
    {
        return Viewers.Any(v => v.IsSupported(syntax, symbol));
    }

    public static DeclarationInfo? View(SemanticModel semantic, SyntaxNode syntax)
    {
        var symbol = semantic.GetDeclaredSymbol(syntax);
        if (symbol is null) return null;

        var viewer = Viewers.FirstOrDefault(v => v.IsSupported(syntax, symbol));
        if (viewer is null) return null;

        var decl = viewer.View(semantic, syntax);
        if (decl is null) return null;

        return new DeclarationInfo(symbol, $"{symbol.Name}{DeclarationViewer.ViewGenericParameters(symbol)}", viewer.Kind, decl);
    }
}
