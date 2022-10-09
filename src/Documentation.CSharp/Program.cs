using System.CommandLine;
using Documentation.CSharp.Commands;

namespace Documentation.CSharp;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var root = new RootCommand("a documentation generator for C#.");
        CommandRegistrar.ApplyCommands(root);
        await root.InvokeAsync(args);
    }
}