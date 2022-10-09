using System.Diagnostics;
using System.Runtime.InteropServices;
using Documentation.CSharp.Compiler;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Project = Microsoft.Build.Evaluation.Project;

namespace Documentation.CSharp.Helpers;

public class CompilerHelper
{
    public CompilerHelper(string? configuration, string? platform)
    {
        Configuration = configuration;
        Platform = platform;
    }

    private ILogger Logger { get; } = new Logging.ConsoleLogger(typeof(CompilerHelper));
    
    private string? Configuration { get; }
    private string? Platform { get; }

    private VisualStudioInstance VisualStudio { get; set; } = null!;


    private static string CompilerPath =>
        Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? "", "Documentation.CSharp.Compiler.dll");
    
    public void Initialize()
    {
        VisualStudio = MSBuildLocator.RegisterDefaults();
        Logger.LogInformation("Use build tool: {msbuild}", VisualStudio.MSBuildPath);
    }

    public async Task BuildSolutionAsync(string path)
    {
        var sln = SolutionFile.Parse(path);
        var projects = sln.ProjectsInOrder;

        foreach (var element in projects)
        {
            await BuildProjectAsync(element.AbsolutePath);
        }
    }

    public async Task BuildProjectAsync(string path)
    {
        var project = Project.FromFile(path, new ProjectOptions());

        if (Configuration is not null)
            project.SetGlobalProperty("Configuration", Configuration);
        if (Platform is not null)
            project.SetGlobalProperty("Platform", Platform);
        project.ReevaluateIfNecessary();
        
        var required = project.GetPropertyValue("DocumentRequired") is "true";
        if (!required)
        {
            Logger.LogWarning("Project is not document-required.");
            return;
        }
            
        project.SetProperty("GenerateDocumentationFile", "true");
        project.AddItem("Reference", "Documentation.CSharp.Compiler", new []
        {
            new KeyValuePair<string, string>("HintPath", CompilerPath),
            new KeyValuePair<string, string>("OutputItemType", "Analyzer"),
            new KeyValuePair<string, string>("ReferenceOutputAssembly", "false")
        });
        project.ReevaluateIfNecessary();

        var output = Path.ChangeExtension(project.FullPath, ".g.csproj");
        try
        {
            project.Save(output);

            using var workspace = MSBuildWorkspace.Create();
            var roslynProject = await workspace.OpenProjectAsync(output, new ConsoleLogger(LoggerVerbosity.Normal));

            var compilation = await roslynProject.GetCompilationAsync();
            if (compilation is null) throw new ExternalException("Failed to get compilation.");

            DocumentationCompiler.Execute(new FileInfo(output), compilation);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to generate documentation file.");
        }
        finally
        {
            if (File.Exists(output))
                File.Delete(output);
        }
    }
}