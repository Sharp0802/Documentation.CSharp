using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Documentation.CSharp.Commands;

namespace Documentation.CSharp.Services;

internal class CommandLineService : BackgroundService
{
    public static string[] Args { get; set; } = Array.Empty<string>();
    
    private IHostApplicationLifetime Lifetime { get; }
    private ILogger<CommandLineService> Logger { get; }
    private CommandRegistrar CommandRegistrar { get; }

    public CommandLineService(
        IHostApplicationLifetime lifetime,
        ILogger<CommandLineService> logger,
        CommandRegistrar commandRegistrar)
    {
        Lifetime = lifetime;
        Logger = logger;
        CommandRegistrar = commandRegistrar;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Args.Length <= 0)
        {
            Logger.Log(LogLevel.Critical, "Cannot load given arguments. Terminate program...");
            Lifetime.StopApplication();
        }
        else
        {
            var root = new RootCommand("a documentation generator for C#.");
            
        }
    }
}
