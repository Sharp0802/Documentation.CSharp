using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Documentation.CSharp.Compiler;

namespace Documentation.CSharp;

public abstract class AA<T> where T : unmanaged
{
        
}

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine(RuntimeEnvironment.GetRuntimeDirectory());

        var src = new SourceGenerator();
        src.WriteTypeInfo(typeof(AA<>));
        Console.WriteLine(src.ToString());
        
        /*
        var docsOpt = new Option<FileInfo?>(
            name: "--xml",
            description: "The xml documentation file generated with msbuild option 'GenerateDocumentationFile'.");

        var dllOpt = new Option<FileInfo?>(
            name: "--dll",
            description: "The managed dll compiled from your C# project.");

        var root = new RootCommand("Documentation generator for C#.");
        root.AddOption(dllOpt);
        root.AddOption(docsOpt);
        
        root.SetHandler(async (dllFile, xmlFile) =>
        {
            if (xmlFile is null) throw new ArgumentNullException(nameof(xmlFile));
            if (dllFile is null) throw new ArgumentNullException(nameof(dllFile));
            
            await Parse(dllFile, xmlFile);
        }, dllOpt, docsOpt);

        return root.InvokeAsync(args);
        */
    }

    public static async Task Parse(FileInfo[] targetDll, FileInfo xmlFile)
    {   
        string content;
        {
            await using var file = new FileStream(xmlFile.FullName, FileMode.Open);
            using var reader = new StreamReader(file);
            content = await reader.ReadToEndAsync();
        }
        
        var doc = XDocument.Parse(content);
        var root = doc.Root;
        if (root is null) 
            throw new InvalidOperationException("Invalid xml data detected. The documentation xml must contain root element");

        var resolver = new PathAssemblyResolver(Array.Empty<string>());
        using (var mlc = new MetadataLoadContext(resolver))
        {
        }
    }
}