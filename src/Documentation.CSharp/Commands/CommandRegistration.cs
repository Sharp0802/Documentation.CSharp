using System.CommandLine;
using Documentation.CSharp.Logging;
using Microsoft.Extensions.Logging;

namespace Documentation.CSharp.Commands;

public abstract class CommandRegistration
{
    protected CommandRegistration(ILogger logger)
    {
        Logger = logger;
    }

    protected ILogger Logger { get; }
    
    public abstract void Register(RootCommand root);

    public static IEnumerable<CommandRegistration> GetRegistrationTypes()
    {
        var logger = new ConsoleLogger(typeof(CommandRegistration));
        return new[]
        {
            new SourceCompileCommand(logger)
        };
    }
}