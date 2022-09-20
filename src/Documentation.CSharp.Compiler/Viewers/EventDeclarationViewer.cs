using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Documentation.CSharp.Compiler.Viewers;

public class EventDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Event;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is VariableDeclaratorSyntax or EventDeclarationSyntax && symbol is IEventSymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is EventDeclarationSyntax eventSyntax)
        {
            if (semantic.GetDeclaredSymbol(eventSyntax) is not IEventSymbol symbol)
                return null;

            var builder = new StringBuilder();

            var attributes = ViewAttributes(symbol.GetAttributes(), "");
            if (!string.IsNullOrEmpty(attributes))
                builder.Append(attributes).Append('\n');

            var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
            if (!string.IsNullOrEmpty(accessibility))
                builder.Append(accessibility).Append(' ');

            var modifier = ViewModifiers(eventSyntax);
            if (!string.IsNullOrEmpty(modifier))
                builder.Append(modifier).Append(' ');

            return builder
                .Append("event ")
                .Append(symbol.Type.ToDisplayString())
                .Append(' ')
                .Append(symbol.Name)
                .Append(" { add; remove; }")
                .ToString();
        }
        else if (syntax is VariableDeclaratorSyntax fieldSyntax)
        {
            if (semantic.GetDeclaredSymbol(fieldSyntax) is not IEventSymbol symbol)
                return null;

            var builder = new StringBuilder();

            var attributes = ViewAttributes(symbol.GetAttributes(), "");
            if (!string.IsNullOrEmpty(attributes))
                builder.Append(attributes).Append('\n');

            var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
            if (!string.IsNullOrEmpty(accessibility))
                builder.Append(accessibility).Append(' ');

            var modifier = ViewModifiers(symbol);
            if (!string.IsNullOrEmpty(modifier))
                builder.Append(modifier).Append(' ');

            return builder
                .Append("event ")
                .Append(symbol.Type.ToDisplayString())
                .Append(' ')
                .Append(symbol.Name)
                .Append(" { add; remove; }")
                .ToString();
        }
        else
        {
            return null;
        }
    }
}
