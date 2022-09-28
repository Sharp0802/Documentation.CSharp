namespace Documentation.CSharp.Web.Primitives;

public class TreeNode
{
    public TreeNode(object? data, string text, TreeNode[] children)
    {
        Data = data;
        Text = text;
        Children = children;
    }

    public object? Data { get; }
    public string Text { get; }
    public TreeNode[] Children { get; }
}
