using System.Collections.Concurrent;
using System.Text.Json;
using Documentation.CSharp.Compiler.Primitives;
using Documentation.CSharp.Compiler.Viewers;
using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler;

public static class DocumentationCompiler
{
    public static void Execute(FileInfo file, Compilation compilation)
    {
        var globalList = new ConcurrentDictionary<string, ConcurrentBag<DeclarationInfo>>();
        Parallel.ForEach(
            compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodesAndSelf()),
            node =>
            {
                var semantic = compilation.GetSemanticModel(node.SyntaxTree);
                var symbol = semantic.GetDeclaredSymbol(node);

                if (symbol is not INamedTypeSymbol) return;
                if (!Declaration.IsSupported(node, symbol)) return;

                var decl = Declaration.View(file.Name, compilation, semantic, node);
                if (decl is null) return;
                globalList.AddOrUpdate(symbol.ContainingNamespace.ToDisplayString(),
                    _ => new ConcurrentBag<DeclarationInfo> { decl },
                    (_, bag) =>
                    {
                        bag.Add(decl);
                        return bag;
                    });
            });

        var target = SerializeTargetHelper.Create(
            file.Name, 
            globalList.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()));
            
        var dir = Path.Combine(file.DirectoryName ?? "", $"{compilation.AssemblyName}.json");
        using var stream = new FileStream(dir, FileMode.Create);
        JsonSerializer.Serialize(stream, target, typeof(SerializeTarget), PrimitiveSerializingContext.Default);
    }
}