using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class DockerBuildKitCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE009";
    public string Title => "docker-buildkit-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            foreach (var step in job.Steps)
            {
                if (IsDockerBuildPushActionWithoutCache(step))
                {
                    findings.Add(new Finding(
                        Id,
                        DefaultSeverity,
                        Category,
                        "docker/build-push-action is used without BuildKit layer caching (cache-from or cache-to).",
                        "Add BuildKit layer caching, for example `cache-from: type=gha` and `cache-to: type=gha,mode=max` for docker/build-push-action.",
                        workflow.FilePath,
                        step.Line,
                        job.Id,
                        step.Name));
                }
                else if (IsDockerBuildXWithoutCache(step))
                {
                    findings.Add(new Finding(
                        Id,
                        DefaultSeverity,
                        Category,
                        "docker buildx build is used without BuildKit cache flags (--cache-from or --cache-to).",
                        "Add BuildKit layer caching, for example `--cache-from type=gha --cache-to type=gha,mode=max`.",
                        workflow.FilePath,
                        step.Line,
                        job.Id,
                        step.Name));
                }
            }
        }

        return findings;
    }

    private static bool IsDockerBuildPushActionWithoutCache(WorkflowStep step)
    {
        if (!RuleHelpers.IsAction(step, "docker/build-push-action"))
        {
            return false;
        }

        var hasCacheFrom = step.With.ContainsKey("cache-from");
        var hasCacheTo = step.With.ContainsKey("cache-to");

        return !hasCacheFrom && !hasCacheTo;
    }

    private static bool IsDockerBuildXWithoutCache(WorkflowStep step)
    {
        if (string.IsNullOrWhiteSpace(step.Run))
        {
            return false;
        }

        var run = step.Run;

        // Must contain "docker buildx build" (not plain docker build)
        if (!run.Contains("buildx build", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var hasCacheFrom = run.Contains("--cache-from", StringComparison.OrdinalIgnoreCase);
        var hasCacheTo = run.Contains("--cache-to", StringComparison.OrdinalIgnoreCase);

        return !hasCacheFrom && !hasCacheTo;
    }
}
