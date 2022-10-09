using System.Text;

namespace Documentation.CSharp.Compiler.Extensions;

internal static class StringExtension
{
    public static string Indent(this string src, int cnt = 1)
    {
        return src
            .Split('\n')
            .Aggregate(new StringBuilder(), (builder, s) => builder.Append('\t', cnt).AppendLine(s))
            .ToString();
    }
}
