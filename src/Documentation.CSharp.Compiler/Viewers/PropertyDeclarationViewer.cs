using Documentation.CSharp.Compiler.Extensions;
using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace Documentation.CSharp.Compiler.Viewers;

public delegate Build Build(string s);

public class PropertyDeclarationViewer : DeclarationViewer
{
    public override DeclarationKind Kind => DeclarationKind.Property;

    public override bool IsSupported(SyntaxNode syntax, ISymbol symbol) => syntax is PropertyDeclarationSyntax && symbol is IPropertySymbol;

    public override string? View(SemanticModel semantic, SyntaxNode syntax)
    {
        if (syntax is not PropertyDeclarationSyntax propertySyntax)
            return null;
        if (semantic.GetDeclaredSymbol(propertySyntax) is not IPropertySymbol symbol)
            return null;

        var builder = new StringBuilder();

        var attributes = ViewAttributes(symbol.GetAttributes(), "");
        if (!string.IsNullOrEmpty(attributes))
            builder.Append(attributes).Append('\n');

        var accessibility = ViewAccessibility(symbol.DeclaredAccessibility);
        if (!string.IsNullOrEmpty(accessibility))
            builder.Append(accessibility).Append(' ');

        var modifiers = ViewModifiers(propertySyntax);
        if (!string.IsNullOrEmpty(modifiers))
            builder.Append(modifiers).Append(' ');

        if (symbol.ReturnsByRefReadonly)
            builder.Append("ref readonly ");
        else if (symbol.ReturnsByRef)
            builder.Append("ref ");

        builder
            .Append(symbol.Type.ToDisplayString())
            .Append(' ');

        if (symbol.IsIndexer)
            builder.Append("this").Append(ViewParameters(semantic, symbol.Parameters, "[]"));
        else
            builder.Append(symbol.Name);

        var getterAttr = symbol.GetMethod?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty;
        var getterRetAttr = symbol.GetMethod?.GetReturnTypeAttributes() ?? ImmutableArray<AttributeData>.Empty;
        var setterAttr = symbol.SetMethod?.GetAttributes() ?? ImmutableArray<AttributeData>.Empty;

        var multiline = 
            getterAttr is { Length: > 0 } || 
            getterRetAttr is { Length: > 0} || 
            setterAttr is { Length: > 0 };

        if (multiline)
        {
            builder
                .Append("\n{\n")
                .Append(ViewAttributes(getterAttr, "").Indent())
                .Append('\n')
                .Append(ViewAttributes(getterRetAttr, "return: ").Indent()
                .Append('\n'))
                .Append("\tget;\n")
                .Append(ViewAttributes(setterAttr, "").Indent())
                .Append('\n');
            if (!symbol.IsReadOnly)
            {
                builder.Append("\n\t");
                if (symbol.GetMethod!.DeclaredAccessibility != symbol.SetMethod!.DeclaredAccessibility)
                    builder.Append(ViewAccessibility(symbol.SetMethod!.DeclaredAccessibility)).Append(' ');
                builder.Append("set;\n");
            }
            builder.Append('}');
        }
        else
        {
            if (symbol.IsReadOnly)
            {
                builder.Append(" { get; }");
            }
            else if (symbol.GetMethod!.DeclaredAccessibility != symbol.SetMethod!.DeclaredAccessibility)
            {
                builder
                    .Append(" { get; ")
                    .Append(ViewAccessibility(symbol.SetMethod.DeclaredAccessibility))
                    .Append(" set; }");
            }
            else
            {
                builder.Append(" { get; set; }");
            }
        }

        return builder.ToString();
    }
}
