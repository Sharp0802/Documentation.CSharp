using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler.Primitives;

public class DeclarationInfo
{
    public DeclarationInfo(
        ISymbol symbol, 
        string title, 
        string assemblyFile, 
        DeclarationKind kind, 
        string declaration, 
        DeclarationInfo[] methods, 
        DeclarationInfo[] events, 
        DeclarationInfo[] properties, 
        DeclarationInfo[] fields)
    {
        Symbol = symbol;
        Title = title;
        Declaration = declaration;
        AssemblyFile = assemblyFile;
        Kind = kind;
        Id = Symbol.GetDocumentationCommentId();
        Documentation = Symbol.GetDocumentationCommentXml(expandIncludes: true);
        IsDeclared = Symbol.DeclaringSyntaxReferences.Length > 0;
        
        Methods = methods;
        Events = events;
        Properties = properties;
        Fields = fields;
    }

    [JsonIgnore]
    public ISymbol? Symbol { get; set; }

    public string Title { get; set; }
    public string AssemblyFile { get; set; }
    public string Declaration { get; set; }
    public DeclarationKind Kind { get; set; }
    public string? Id { get; set; }
    public string? Documentation { get; set; }
    public bool IsDeclared { get; set; }

    public DeclarationInfo[] Methods { get; set; }
    public DeclarationInfo[] Events { get; set; }
    public DeclarationInfo[] Properties { get; set; }
    public DeclarationInfo[] Fields { get; set; }
}
