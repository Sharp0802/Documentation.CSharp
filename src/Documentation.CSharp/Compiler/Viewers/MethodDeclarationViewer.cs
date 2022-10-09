using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Documentation.CSharp.Compiler.Viewers;

public class MethodDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Method;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is MethodDeclarationSyntax && symbol is IMethodSymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is not MethodDeclarationSyntax methodSyntax)
            return null;
        if (semantic.GetDeclaredSymbol(syntax) is not IMethodSymbol symbol)
            return null;

        var builder = new StringBuilder();

        var attributes = ViewAttributes(symbol.GetAttributes(), "");
        if (!string.IsNullOrEmpty(attributes))
            builder.Append(attributes).Append('\n');

        var retAttrs = ViewAttributes(symbol.GetReturnTypeAttributes(), "return: ");
        if (!string.IsNullOrEmpty(retAttrs))
            builder.Append(retAttrs).Append('\n');

        var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
            builder.Append(accessibility).Append(' ');

        var modifiers = ViewModifiers(methodSyntax);
        if (!string.IsNullOrEmpty(modifiers))
            builder.Append(modifiers).Append(' ');

        builder
            .Append(symbol.ReturnsByRefReadonly ? "ref readonly " : (symbol.ReturnsByRef ? "ref " : ""))
            .Append(symbol.ReturnsVoid ? "void" : symbol.ReturnType.ToDisplayString())
            .Append(' ')
            .Append(symbol.Name)
            .Append(ViewGenericParameters(symbol))
            .Append(ViewParameters(semantic, methodSyntax.ParameterList.Parameters, "()"));

        var constraints = ViewGenericParameterConstraints(symbol);
        if (!string.IsNullOrEmpty(constraints))
            builder.Append(' ').Append(constraints);

        return builder.ToString();
    }
}
