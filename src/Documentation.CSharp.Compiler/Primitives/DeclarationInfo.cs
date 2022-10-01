using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Documentation.CSharp.Compiler.Primitives;

public class DeclarationInfo
{
    public DeclarationInfo(ISymbol symbol, string name, DeclarationKind kind, string declaration)
    {
        Title = name;
        Declaration = declaration;
        Kind = kind;
        Symbol = symbol;
        Id = Symbol.GetDocumentationCommentId();
        Documentation = Symbol.GetDocumentationCommentXml(expandIncludes: true);
        IsDeclared = Symbol.DeclaringSyntaxReferences.Length > 0;
    }

    [JsonIgnore]
    public ISymbol Symbol { get; }

    public string Title { get; }
    public string Declaration { get; }
    public DeclarationKind Kind { get; }
    public string? Id { get; }
    public string? Documentation { get; }
    public bool IsDeclared { get; }

    public List<DeclarationInfo> Methods { get; } = new();
    public List<DeclarationInfo> Events { get; } = new();
    public List<DeclarationInfo> Properties { get; } = new();
    public List<DeclarationInfo> Fields { get; } = new();
}
