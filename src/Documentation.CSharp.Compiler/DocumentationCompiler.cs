using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Documentation.CSharp.Compiler.Primitives;
using Documentation.CSharp.Compiler.Viewers;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Documentation.CSharp.Compiler;

[Generator]
public class DocumentationCompiler : ISourceGenerator
{
    private SyntaxReceiver SyntaxReceiver { get; } = new();

    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public void Initialize(GeneratorInitializationContext context)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var name = new AssemblyName(args.Name);
            var loadedAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(a => a.GetName().FullName == name.FullName);
            if (loadedAssembly != null)
                return loadedAssembly;

            using var resourceStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"Documentation.CSharp.Compiler.{name.Name}.dll");
            if (resourceStream == null)
                return null;

            using var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);
            return Assembly.Load(memoryStream.ToArray());
        };

        context.RegisterForSyntaxNotifications(() => SyntaxReceiver);
    }

    public static bool HasComment(ISymbol symbol)
    {
        return !string.IsNullOrEmpty(symbol.GetDocumentationCommentXml());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var globalList = new Dictionary<INamespaceSymbol?, List<DeclarationInfo>>(SymbolEqualityComparer.Default);
        foreach (var node in SyntaxReceiver.SyntaxNodes)
        {
            var semantic = context.Compilation.GetSemanticModel(node.SyntaxTree);
            var symbol = semantic.GetDeclaredSymbol(node);

            if (symbol is null) continue;
            if (!Declaration.IsSupported(node, symbol)) continue;

            if (symbol is INamedTypeSymbol typeSymbol)
            {
                var methods = new List<DeclarationInfo>();
                var events = new List<DeclarationInfo>();
                var properties = new List<DeclarationInfo>();
                var fields = new List<DeclarationInfo>();

                foreach (var member in typeSymbol.GetMembers())
                {
                    if (!HasComment(member)) continue;
                    var syntax = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                    if (syntax is null) continue;
                    var decl = Declaration.View(semantic, syntax);
                    if (decl is not null)
                    {
                        (decl.Kind switch
                        {
                            DeclarationKind.Method => methods,
                            DeclarationKind.Event => events,
                            DeclarationKind.Property => properties,
                            DeclarationKind.Field => fields,
                            _ => null
                        })?.Add(decl);
                    }
                }

                if (methods.Count > 0 || events.Count > 0 || properties.Count > 0 || fields.Count > 0 || HasComment(symbol))
                {
                    var decl = Declaration.View(semantic, node)!;
                    decl.Methods.AddRange(methods);
                    decl.Events.AddRange(events);
                    decl.Properties.AddRange(properties);
                    decl.Fields.AddRange(fields);

                    if (globalList.ContainsKey(symbol.ContainingNamespace))
                        globalList[symbol.ContainingNamespace].Add(decl);
                    else
                        globalList.Add(symbol.ContainingNamespace, new List<DeclarationInfo>{ decl });
                }
            }
        }

        var main = context.Compilation.SyntaxTrees.First(x => x.HasCompilationUnitRoot);
        var dir = Path.GetDirectoryName(main.FilePath);
        dir = Path.Combine(dir, "../docs.json");

        var data = JsonConvert.SerializeObject(globalList, Formatting.Indented);

        using var file = new FileStream(dir, FileMode.Create);
        using var writer = new StreamWriter(file);
        writer.Write(data);
    }
}