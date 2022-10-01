using System.Collections.Immutable;
using System.CommandLine;

namespace Documentation.CSharp.Commands;

public class CommandRegistrar
{
    public CommandRegistrar()
    {
        CommandRegistrations = typeof(CommandRegistrar).Assembly
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CommandRegistration)))
            .Select(Activator.CreateInstance)
            .Cast<CommandRegistration>()
            .ToImmutableArray();
    }
    
    private ImmutableArray<CommandRegistration> CommandRegistrations { get; }

    public void ApplyCommands(RootCommand root)
    {
        foreach (var registration in CommandRegistrations)
            registration.Register(root);
    }
}