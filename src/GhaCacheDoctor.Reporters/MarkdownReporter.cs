using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class MarkdownReporter : IReporter
{
    public string Render(ScanResult result) =>
        GitHubSummaryReporter.RenderMarkdown(result, "# gha-cache-doctor report");
}
