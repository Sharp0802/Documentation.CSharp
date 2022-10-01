using System.CommandLine;
using Microsoft.Extensions.Logging;

namespace Documentation.CSharp.Commands;

public abstract class CommandRegistration
{
    public CommandRegistration(ILogger logger)
    {
        Logger = logger;
    }
    
    protected ILogger Logger { get; }
    
    public abstract void Register(RootCommand root);
}