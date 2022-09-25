namespace Documentation.CSharp.Web.Primitives;

public class TreeNode
{
    public TreeNode(string? data, string text, TreeNode[] children)
    {
        Data = data;
        Text = text;
        Children = children;
    }

    public string? Data { get; }
    public string Text { get; }
    public TreeNode[] Children { get; }
}
