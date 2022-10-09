using System.CommandLine;

namespace Documentation.CSharp.Commands;

public class CommandRegistrar
{
    static CommandRegistrar()
    {
        CommandRegistrations = CommandRegistration.GetRegistrationTypes();
    }

    private static IEnumerable<CommandRegistration> CommandRegistrations { get; }

    public static void ApplyCommands(RootCommand root)
    {
        foreach (var registration in CommandRegistrations)
            registration.Register(root);
    }
}