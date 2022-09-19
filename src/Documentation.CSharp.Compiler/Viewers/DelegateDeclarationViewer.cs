using System.Text;
using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Documentation.CSharp.Compiler.Viewers;

public class DelegateDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Delegate;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is DelegateDeclarationSyntax && symbol is INamedTypeSymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is not DelegateDeclarationSyntax delegateSyntax) 
            return null;
        if (semantic.GetDeclaredSymbol(delegateSyntax) is not INamedTypeSymbol symbol)
            return null;

        var builder = new StringBuilder();

        var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
            builder.Append(accessibility).Append(' ');

        var modifiers = ViewModifiers(delegateSyntax);
        if (!string.IsNullOrEmpty(modifiers))
            builder.Append(modifiers).Append(' ');

        builder
            .Append("delegate ")
            .Append(delegateSyntax.ReturnType.ToString())
            .Append(' ')
            .Append(symbol.Name)
            .Append(ViewGenericParameters(symbol))
            .Append(ViewParameters(semantic, delegateSyntax.ParameterList.Parameters, "()"));

        var constraints = ViewGenericParameterConstraints(symbol);
        if (!string.IsNullOrEmpty(constraints))
            builder.Append(' ').Append(constraints);

        builder.Append(';');

        return builder.ToString();
    }
}