using Documentation.CSharp.Compiler.Rendering;
using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler.Primitives;

public static class DeclarationInfoHelper
{
    public static DeclarationInfo Create(
        Compilation compilation,
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
        var decl = new DeclarationInfo
        {
            Title = title,
            Declaration = declaration,
            AssemblyFile = assemblyFile,
            Kind = kind,
            Id = symbol.GetDocumentationCommentId(),
            Documentation = DocumentationRenderer.Render(
                compilation, 
                symbol.GetDocumentationCommentXml(expandIncludes: true) ?? ""),
            IsDeclared = symbol.DeclaringSyntaxReferences.Length > 0,
            
            Methods = methods,
            Events = events,
            Properties = properties,
            Fields = fields
        };

        return decl;
    }
}

public class DeclarationInfo
{
    public string? Title { get; set; }
    public string? AssemblyFile { get; set; }
    public string? Declaration { get; set; }
    public DeclarationKind Kind { get; set; }
    public string? Id { get; set; }
    public string? Documentation { get; set; }
    public bool IsDeclared { get; set; }

    public DeclarationInfo[]? Methods { get; set; }
    public DeclarationInfo[]? Events { get; set; }
    public DeclarationInfo[]? Properties { get; set; }
    public DeclarationInfo[]? Fields { get; set; }
}
