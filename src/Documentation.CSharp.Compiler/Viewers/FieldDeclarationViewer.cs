using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Documentation.CSharp.Compiler.Viewers;

public class FieldDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Field;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is VariableDeclaratorSyntax && symbol is IFieldSymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is not VariableDeclaratorSyntax fieldSyntax)
            return null;

        if (semantic.GetDeclaredSymbol(fieldSyntax) is not IFieldSymbol symbol) 
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

        builder
            .Append(symbol.Type.ToDisplayString())
            .Append(' ')
            .Append(symbol.Name);

        if (symbol.IsFixedSizeBuffer)
            builder.Append('[').Append(symbol.FixedSize).Append(']');

        builder.Append(';');

        return builder.ToString();
    }
}
