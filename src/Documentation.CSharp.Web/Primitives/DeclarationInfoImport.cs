namespace Documentation.CSharp.Web.Primitives;

public class DeclarationInfoImport
{
    public string? Title { get; set; }
    public string? Declaration { get; set; }
    public int Kind { get; set; }
    public string? Id { get; set; }
    public string? Documentation { get; set; }
    public bool IsDeclared { get; set; }

    public DeclarationInfoImport[] Methods { get; set; } = null!;
    public DeclarationInfoImport[] Events { get; set; } = null!;
    public DeclarationInfoImport[] Properties { get; set; } = null!;
    public DeclarationInfoImport[] Fields { get; set; } = null!;
}
