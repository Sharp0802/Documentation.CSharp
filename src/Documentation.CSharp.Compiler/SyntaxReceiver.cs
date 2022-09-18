using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler;

public class SyntaxReceiver : ISyntaxReceiver
{
    public SyntaxNode[] SyntaxNodes => _syntaxNodeList.ToArray();
    
    private readonly List<SyntaxNode> _syntaxNodeList = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        _syntaxNodeList.Add(syntaxNode);
    }
}