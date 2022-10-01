using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Documentation.CSharp.Commands;

public class SourceCompileCommand : CommandRegistration
{
    public SourceCompileCommand(ILogger logger) : base(logger) {}
    
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
            if (string.Equals(".sln", target.Extension))
            {
                var msbuild = MSBuildWorkspace.Create();

                var sln = await msbuild.OpenSolutionAsync(target.FullName, new ConsoleLogger(LoggerVerbosity.Detailed));

                var leaves = sln.Projects.ToList();
                foreach (var project in sln.Projects)
                {
                    for (var i = 0; i < leaves.Count; ++i)
                    {
                        if (project.ProjectReferences.Any(reference => reference.ProjectId == leaves[i].Id))
                            leaves.RemoveAt(i);
                    }
                }

                if (leaves.Count <= 0)
                    Logger.Log(LogLevel.Warning, "No documentated assembly founded. No document will be builded.");
                else
                    Logger.Log(LogLevel.Information, $"{leaves.Count} documented project(s) found.");

                foreach (var leaf in leaves)
                {
                    if (!leaf.TryGetCompilation(out var compilation))
                        continue;
                }
            }
            else if (string.Equals(".csproj", target.Extension))
            {
                var msbuild = MSBuildWorkspace.Create();

                var csproj = await msbuild.OpenProjectAsync(target.FullName, new ConsoleLogger(LoggerVerbosity.Detailed));
                if (!csproj.TryGetCompilation(out var compilation))
                    ; // TODO : handle failed
            }
        }, targetOption, settingsOption, configOption, platformOption);

        root.AddCommand(build);
    }
}