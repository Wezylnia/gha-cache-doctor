using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class MavenCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE013";
    public string Title => "maven-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    private static readonly string[] MavenCommands = ["mvn test", "mvn verify", "mvn package", "mvn install",
        "./mvnw test", "./mvnw verify", "./mvnw package", "./mvnw install"];

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            for (var i = 0; i < job.Steps.Count; i++)
            {
                var step = job.Steps[i];
                if (!IsMavenCommand(step))
                {
                    continue;
                }

                if (HasMavenCacheBefore(job.Steps.Take(i)))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This job runs Maven commands without Maven dependency caching.",
                    "Add Maven dependency caching before Maven commands, for example `actions/setup-java` with `cache: maven` or actions/cache for `~/.m2/repository`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
                break;
            }
        }

        return findings;
    }

    private static bool IsMavenCommand(WorkflowStep step) =>
        step.Run is not null && MavenCommands.Any(cmd => step.Run.Contains(cmd, StringComparison.OrdinalIgnoreCase));

    private static bool HasMavenCacheBefore(IEnumerable<WorkflowStep> steps) =>
        steps.Any(step =>
            (RuleHelpers.IsAction(step, "actions/setup-java") &&
                RuleHelpers.GetWith(step, "cache")?.Equals("maven", StringComparison.OrdinalIgnoreCase) == true) ||
            (RuleHelpers.IsAction(step, "actions/cache") && IsMavenCachePath(RuleHelpers.GetWith(step, "path"))));

    private static bool IsMavenCachePath(string? path) =>
        path is not null && path.Contains("~/.m2/repository", StringComparison.OrdinalIgnoreCase);
}
