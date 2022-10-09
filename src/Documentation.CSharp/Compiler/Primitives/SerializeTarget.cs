namespace Documentation.CSharp.Compiler.Primitives;

public class SerializeTargetHelper
{
    public static SerializeTarget Create(
        string assemblyFile,
        Dictionary<string, DeclarationInfo[]> declarations)
    {
        var target = new SerializeTarget
        {
            AssemblyFile = assemblyFile,
            Declarations = declarations
        };
        
        return target;
    }
}

public class SerializeTarget
{
    public string? AssemblyFile { get; set; }
    public Dictionary<string, DeclarationInfo[]>? Declarations { get; set; }
}