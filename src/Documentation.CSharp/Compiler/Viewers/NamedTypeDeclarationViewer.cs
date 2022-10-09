using System.Text;
using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Documentation.CSharp.Compiler.Viewers;

public class NamedTypeDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Type;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is TypeDeclarationSyntax && symbol is INamedTypeSymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is not TypeDeclarationSyntax typeSyntax) 
            return null;
        
        var symbol = semantic.GetDeclaredSymbol(typeSyntax);
        if (symbol is null) return null;

        var builder = new StringBuilder();

        var attributes = ViewAttributes(symbol.GetAttributes(), "");
        if (!string.IsNullOrEmpty(attributes))
            builder.Append(attributes);

        var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
            builder.Append(accessibility).Append(' ');

        var modifiers = ViewModifiers(typeSyntax);
        if (!string.IsNullOrEmpty(modifiers))
            builder.Append(modifiers).Append(' ');

        if (symbol.IsRefLikeType)
            builder.Append("ref ");
        builder
            .Append(typeSyntax.Keyword.Text)
            .Append(' ')
            .Append(symbol.Name)
            .Append(ViewGenericParameters(symbol));

        var inherits = ViewInherits(symbol);
        if (!string.IsNullOrEmpty(inherits))
            builder.Append(' ').Append(inherits);

        var constraints = ViewGenericParameterConstraints(symbol);
        if (!string.IsNullOrEmpty(constraints))
            builder.Append(' ').Append(constraints);

        return builder.ToString();
    }
}