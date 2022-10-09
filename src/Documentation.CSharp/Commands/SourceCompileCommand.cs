using System.CommandLine;
using System.CommandLine.Parsing;
using Documentation.CSharp.Helpers;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Documentation.CSharp.Commands;

public class SourceCompileCommand : CommandRegistration
{
    public SourceCompileCommand(ILogger logger) : base(logger)
    {
    }
    
    private static void FileInfoValidator(OptionResult result)
    {
        var file = result.GetValueOrDefault<FileInfo?>();
        if (file is null || !file.Exists)
            result.ErrorMessage = "File does not exist.";
    }

    private static void BuildableValidator(OptionResult result)
    {
        var file = result.GetValueOrDefault<FileInfo?>();
        if (file?.Extension is not (".sln" or ".csproj"))
            result.ErrorMessage = "File is not buildable. File must be Project Solution or C# Project";
    }
    
    public override void Register(RootCommand root)
    {
        var targetOption = new Option<FileInfo?>(
            name: "--target",
            description: "The .sln or .csproj to build documentation.");
        targetOption.AddAlias("-t");
        targetOption.AddValidator(FileInfoValidator);
        targetOption.AddValidator(BuildableValidator);

        var settingsOption = new Option<FileInfo?>(
            name: "--settings",
            description: "The configuration file.");
        settingsOption.AddAlias("-s");
        settingsOption.AddValidator(FileInfoValidator);

        var configOption = new Option<string?>(
            name: "--configuration",
            description: "The build configuration. Usually one of 'Debug' or 'Release'. Can be inherited from the configuration file.");
        configOption.AddAlias("-c");
        configOption.AddCompletions("Debug", "Release");

        var platformOption = new Option<string?>(
            name: "--platform",
            description: "The build target platform. Usually one of 'x64' or 'x86'. Can be inherited from the configuration file.");
        platformOption.AddAlias("-p");
        platformOption.AddCompletions("x64", "x86");

        var build = new Command("build", "Build xml documentation for .sln or .csproj")
        {
            targetOption,
            settingsOption,
            configOption,
            platformOption
        };
        build.AddAlias("b");
        
        build.SetHandler(async (target, settings, config, platform) =>
        {
            if (target is null)
            {
                Logger.LogCritical("Cannot load given build target.");
                return;
            }
            
            Environment.SetEnvironmentVariable("DCSOutputPath", target.DirectoryName, EnvironmentVariableTarget.User);

            try
            {
                var workspace = new CompilerHelper(config, platform);
                workspace.Initialize();
                if (string.Equals(".sln", target.Extension))
                    await workspace.BuildSolutionAsync(target.FullName);
                else if (string.Equals(".csproj", target.Extension)) 
                    await workspace.BuildProjectAsync(target.FullName);
            }
            finally
            {
                Environment.SetEnvironmentVariable("DCSOutputPath", null, EnvironmentVariableTarget.User);
            }
            
        }, targetOption, settingsOption, configOption, platformOption);

        root.AddCommand(build);
    }
}