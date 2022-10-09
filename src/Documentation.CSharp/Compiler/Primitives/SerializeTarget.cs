namespace Documentation.CSharp.Compiler.Primitives;

public class SerializeTarget
{
    public SerializeTarget(
        string assemblyFile, 
        Dictionary<string, DeclarationInfo[]> declarations)
    {
        AssemblyFile = assemblyFile;
        Declarations = declarations;
    }

    public string AssemblyFile { get; set; }
    public Dictionary<string, DeclarationInfo[]> Declarations { get; set; }
}