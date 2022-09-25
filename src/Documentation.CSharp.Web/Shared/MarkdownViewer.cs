using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Prism;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Documentation.CSharp.Web.Shared;

public class MarkdownViewer : ComponentBase
{
    [Inject]
    private ILogger<MarkdownViewer> Logger { get; set; } = null!;

    [Parameter]
    public string? Data { get; set; }

    private MarkupString? Body { get; set; }


    private MarkdownPipeline Pipeline { get; } = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .UseEmojiAndSmiley()
        .UsePrism()
        .Build();


    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Data is null) return;

        Logger.LogInformation($"attempt to load markdown. : {Data}");

        var document = Markdown.Parse(Data, Pipeline);
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

        if (yamlBlock is not null)
        {
            var yaml = Data.Substring(yamlBlock.Span.Start, yamlBlock.Span.End);
            Logger.LogInformation(yaml);
        }

        Body = (MarkupString)document.ToHtml(Pipeline);

        Logger.LogInformation("markdown loaded.");
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);
        if (Body is not null)
            builder.AddContent(0, Body);
    }
}
