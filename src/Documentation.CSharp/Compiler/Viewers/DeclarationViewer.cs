using System.Text;
using Documentation.CSharp.Compiler.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Documentation.CSharp.Compiler.Viewers;

public abstract class DeclarationViewer
{
    private static bool IsObject(INamedTypeSymbol symbol)
    {
        return symbol.ContainingNamespace.Name.Equals("System", StringComparison.Ordinal) && symbol.ToDisplayString().Equals("object");
    }

    private static string ViewAttributeWithoutBracket(AttributeData data)
    {
        if (data.AttributeClass is null) return "";

        var builder = new StringBuilder();

        builder.Append(data.AttributeClass.ToDisplayString());

        var ctorArguments = data.ConstructorArguments;
        var namedArguments = data.NamedArguments;
        var hasArguments = ctorArguments.Any() || namedArguments.Any();
        if (hasArguments)
            builder.Append('(');

        var first = true;
        foreach (var constant in data.ConstructorArguments)
        {
            if (!first) builder.Append(", ");
            builder.Append(constant.ToCSharpString());
            first = false;
        }
        foreach (var tuple in data.NamedArguments)
        {
            if (!first) builder.Append(", ");
            builder
                .Append(tuple.Key)
                .Append(" = ")
                .Append(tuple.Value.ToCSharpString());
            first = false;
        }

        if (hasArguments)
            builder.Append(')');

        return builder.ToString();
    }

    public static string ViewAttribute(AttributeData data, string prefix)
    {
        if (data.AttributeClass is null) return "";

        return new StringBuilder()
            .Append('[')
            .Append(prefix)
            .Append(ViewAttributeWithoutBracket(data))
            .Append(']')
            .ToString();
    }

    public static string ViewAttributes(IReadOnlyCollection<AttributeData> attributes, string prefix)
    {
        return string.Join("\n", attributes.Select(attribute => ViewAttribute(attribute, prefix)));
    }

    public static string ViewAttributeInline(IReadOnlyCollection<AttributeData> attributes, string prefix) 
    {
        if (attributes.Count <= 0)
            return "";

        return new StringBuilder()
            .Append('[')
            .Append(prefix)
            .Append(string.Join(", ", attributes.Select(ViewAttributeWithoutBracket)))
            .Append(']')
            .ToString();
    }

    public static string ViewAccessibility(Accessibility accessibility)
    {
        return accessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Public => "public",
            Accessibility.NotApplicable or _ => ""
        };
    }

    public static string ViewModifiers(IFieldSymbol field)
    {
        var list = new List<string>();

        if (field.IsConst)
            list.Add("const");
        if (field.IsStatic)
            list.Add("static");
        if (field.IsVolatile)
            list.Add("volatile");
        if (field.IsExtern)
            list.Add("extern");
        if (field.IsAbstract)
            list.Add("abstract");
        if (field.IsVirtual)
            list.Add("virtual");
        if (field.IsSealed)
            list.Add("sealed");
        if (field.IsReadOnly)
            list.Add("readonly");
        if (field.IsFixedSizeBuffer)
            list.Add("fixed");

        return string.Join(" ", list);
    }

    public static string ViewModifiers(IEventSymbol @event)
    {
        var list = new List<string>();

        if (@event.IsStatic)
            list.Add("static");
        if (@event.IsExtern)
            list.Add("extern");
        if (@event.IsAbstract)
            list.Add("abstract");
        if (@event.IsVirtual)
            list.Add("virtual");
        if (@event.IsSealed)
            list.Add("sealed");

        return string.Join(" ", list);
    }

    public static string ViewModifiers(MemberDeclarationSyntax syntax)
    {
        return string.Join(" ", syntax.Modifiers.Where(token => 
        !token.Text.Equals("async", StringComparison.Ordinal) &&
        !token.Text.Equals("public", StringComparison.Ordinal) &&
        !token.Text.Equals("internal", StringComparison.Ordinal) &&
        !token.Text.Equals("protected", StringComparison.Ordinal) &&
        !token.Text.Equals("private", StringComparison.Ordinal)));
    }

    public static string ViewGenericParameters(ISymbol symbol)
    {
        if (symbol is not (IMethodSymbol { IsGenericMethod: true } or INamedTypeSymbol { IsGenericType: true }))
            return "";
        
        var builder = new StringBuilder();
        
        builder.Append('<');

        var first = true;

#pragma warning disable CS8509
        var parameters = symbol switch
#pragma warning restore CS8509
        {
            IMethodSymbol method => method.TypeParameters,
            INamedTypeSymbol type => type.TypeParameters
        };
        
        foreach (var parameter in parameters)
        {
            if (!first) builder.Append(", ");
            
            builder.Append(parameter.Variance switch
            {
                VarianceKind.In => "in ",
                VarianceKind.Out => "out ",
                VarianceKind.None or _ => ""
            });

            builder.Append(parameter.ToDisplayString());

            first = false;
        }

        builder.Append('>');

        return builder.ToString();
    }

    public static string ViewGenericParameterConstraints(ISymbol symbol)
    {
        if (symbol is not (IMethodSymbol { IsGenericMethod: true } or INamedTypeSymbol { IsGenericType: true }))
            return "";
        
        var constraints = new Queue<string>();
        
#pragma warning disable CS8509
        var parameters = symbol switch
#pragma warning restore CS8509
        {
            IMethodSymbol method => method.TypeParameters,
            INamedTypeSymbol type => type.TypeParameters
        };
        
        foreach (var parameter in parameters)
        {
            var local = new Queue<string>();

            foreach (var constraint in parameter.ConstraintTypes) 
                local.Enqueue(constraint.ToDisplayString());

            if (parameter.HasUnmanagedTypeConstraint)
                local.Enqueue("unmanaged");
            else if (parameter.HasConstructorConstraint)
                local.Enqueue("new()");
            else if (parameter.HasNotNullConstraint)
                local.Enqueue("notnull");
            else if (parameter.HasValueTypeConstraint)
                local.Enqueue("struct");
            else if (parameter.HasReferenceTypeConstraint)
                local.Enqueue(parameter.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated
                    ? "class?"
                    : "class");

            if (local.Count > 0)
                constraints.Enqueue($"where {parameter.ToDisplayString()} : {string.Join(", ", local)}");
        }

        return string.Join(" ", constraints);
    }

    public static string ViewInherits(ITypeSymbol symbol)
    {
        var builder = new StringBuilder();
        
        var @base = symbol.BaseType;
        if (@base is not null && IsObject(@base))
            @base = null;

        var interfaces = symbol.Interfaces;

        if (@base is null && interfaces.Length <= 0) return "";

        builder.Append(": ");

        var first = true;
        if (@base is not null && !IsObject(@base))
        {
            builder.Append(@base.ToDisplayString());
            first = false;
        }

        foreach (var @interface in interfaces)
        {
            if (!first) builder.Append(", ");
            builder.Append(@interface.ToDisplayString());
            first = false;
        }

        return builder.ToString();
    }

    public static string ViewParameters(SemanticModel semantic, SeparatedSyntaxList<ParameterSyntax> parameters, string brackets)
    {
        return new StringBuilder()
            .Append(brackets[0])
            .Append(string.Join(", ", parameters.Select(s => ViewParameter(semantic, s))))
            .Append(brackets[1])
            .ToString();
    }

    public static string ViewParameters(SemanticModel semantic, IReadOnlyCollection<IParameterSymbol> parameters, string brackets)
    {
        return new StringBuilder()
            .Append(brackets[0])
            .Append(string.Join(", ", parameters.Select(s => ViewParameter(semantic, s))))
            .Append(brackets[1])
            .ToString();
    }

    public static string ViewParameter(SemanticModel semantic, ParameterSyntax syntax)
    {
        var symbol = semantic.GetDeclaredSymbol(syntax);
        if (symbol is null) return "";

        var builder = new StringBuilder();

        var attributes = ViewAttributeInline(symbol.GetAttributes(), "");
        if (!string.IsNullOrEmpty(attributes))
            builder.Append(attributes).Append(' ');

        if (symbol.IsThis)
            builder.Append("this ");
        if (symbol.IsParams)
            builder.Append("params ");

        builder
            .Append(symbol.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => ""
            })
            .Append(symbol.Type.ToDisplayString())
            .Append(' ')
            .Append(syntax.Identifier.Text);

        if (symbol.HasExplicitDefaultValue)
        {
            builder
                .Append(' ')
                .Append(syntax.Default);
        }

        return builder.ToString();
    }

    public static string ViewParameter(SemanticModel semantic, IParameterSymbol symbol)
    {
        if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ParameterSyntax syntax)
            return "";
        return ViewParameter(semantic, syntax);
    }

    public abstract DeclarationKind Kind { get; }

    public abstract bool IsSupported(SyntaxNode syntax, ISymbol symbol);

    public abstract string? View(SemanticModel semantic, SyntaxNode syntax);
}