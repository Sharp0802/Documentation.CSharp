using System.Xml;
using Microsoft.CodeAnalysis;

namespace Documentation.CSharp.Compiler.Rendering;

public static class DocumentationRenderer
{
    public static string Render(Compilation compilation, string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return document.DocumentElement is null 
            ? string.Empty
            : Render(compilation, document.DocumentElement);
    }
    
    private static string Render(Compilation compilation, XmlElement element)
    {
        // TODO : Impl rendering method.
        
        switch (element.Name)
        {
            case "summary":
            case "remarks":
                
            case "returns":
            case "typeparam":
            case "typeparamref":
            case "param":
            case "paramref":
            case "exception":
            case "value":
                
            case "para":
            case "list":
            case "c":
            case "code":
            case "example":
                
            case "inheritdoc":
                
            case "see":
            case "seealso":
                
                break;
        }

        throw new NotImplementedException();
    }
}