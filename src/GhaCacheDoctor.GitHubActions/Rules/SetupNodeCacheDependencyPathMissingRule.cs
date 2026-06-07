using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class SetupNodeCacheDependencyPathMissingRule : IRule
{
    public string Id => "GHA-CACHE002";
    public string Title => "setup-node-cache-dependency-path-missing";
    public Severity DefaultSeverity => Severity.Warning;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        if (!repository.LooksLikeNodeMonorepo)
        {
            return [];
        }

        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            foreach (var step in job.Steps.Where(step =>
                RuleHelpers.IsAction(step, "actions/setup-node") &&
                RuleHelpers.HasWith(step, "cache") &&
                !RuleHelpers.HasWith(step, "cache-dependency-path")))
            {
                var recommendation = GetRecommendation(repository, job, step);
                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This repository looks like a monorepo, but setup-node cache has no cache-dependency-path.",
                    recommendation,
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
            }
        }

        return findings;
    }

    private static string GetRecommendation(RepositoryContext repository, WorkflowJob job, WorkflowStep step)
    {
        var cacheKind = RuleHelpers.GetWith(step, "cache") ?? RuleHelpers.DetectNodeCacheKind(repository, job);

        if (cacheKind.Equals("pnpm", StringComparison.OrdinalIgnoreCase) ||
            repository.PnpmWorkspaceFiles.Count > 0)
        {
            return "Set `cache-dependency-path: '**/pnpm-lock.yaml'` so the cache key reflects all workspace lockfiles.";
        }

        if (cacheKind.Equals("yarn", StringComparison.OrdinalIgnoreCase))
        {
            return "Set `cache-dependency-path: '**/yarn.lock'` so the cache key reflects all workspace lockfiles.";
        }

        return "Set `cache-dependency-path: '**/package-lock.json'` so the cache key reflects all workspace lockfiles.";
    }
}
