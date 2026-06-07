using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class GoCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE011";
    public string Title => "go-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    private static readonly string[] GoCommands = ["go test", "go build", "go mod download", "go list"];

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            var setupGoIndex = -1;
            for (var i = 0; i < job.Steps.Count; i++)
            {
                if (RuleHelpers.IsAction(job.Steps[i], "actions/setup-go"))
                {
                    setupGoIndex = i;
                    break;
                }
            }

            if (setupGoIndex < 0)
            {
                continue;
            }

            var setupGoStep = job.Steps[setupGoIndex];
            if (RuleHelpers.GetWith(setupGoStep, "cache")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
            {
                continue;
            }

            for (var i = setupGoIndex + 1; i < job.Steps.Count; i++)
            {
                var step = job.Steps[i];
                if (!IsGoCommand(step))
                {
                    continue;
                }

                if (HasGoCacheBefore(job.Steps.Take(i)))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This job uses actions/setup-go and runs Go commands without Go module caching.",
                    "Enable setup-go caching with `cache: true`, or cache `~/go/pkg/mod` and `~/.cache/go-build` with a key based on `hashFiles('**/go.sum')`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
                break;
            }
        }

        return findings;
    }

    private static bool IsGoCommand(WorkflowStep step) =>
        step.Run is not null && GoCommands.Any(cmd => step.Run.Contains(cmd, StringComparison.OrdinalIgnoreCase));

    private static bool HasGoCacheBefore(IEnumerable<WorkflowStep> steps) =>
        steps.Any(step => RuleHelpers.IsAction(step, "actions/cache") && IsGoCachePath(RuleHelpers.GetWith(step, "path")));

    private static bool IsGoCachePath(string? path) =>
        path is not null && (
            path.Contains("~/go/pkg/mod", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("~/.cache/go-build", StringComparison.OrdinalIgnoreCase) ||
            (path.Contains("GOMODCACHE", StringComparison.OrdinalIgnoreCase) && path.Contains("env", StringComparison.OrdinalIgnoreCase)) ||
            (path.Contains("GOCACHE", StringComparison.OrdinalIgnoreCase) && path.Contains("env", StringComparison.OrdinalIgnoreCase)));
}
