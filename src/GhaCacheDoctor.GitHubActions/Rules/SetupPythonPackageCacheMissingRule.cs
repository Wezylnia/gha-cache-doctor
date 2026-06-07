using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class SetupPythonPackageCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE010";
    public string Title => "setup-python-package-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        if (!RuleHelpers.HasPythonHints(repository))
        {
            return [];
        }

        var findings = new List<Finding>();

        foreach (var job in workflow.Jobs)
        {
            var hasPoetryInstall = HasPoetryInstall(job);
            var hasPipenvInstall = HasPipenvInstall(job);

            if (!hasPoetryInstall && !hasPipenvInstall)
            {
                continue;
            }

            foreach (var step in job.Steps.Where(step => RuleHelpers.IsAction(step, "actions/setup-python")))
            {
                var cacheValue = RuleHelpers.GetWith(step, "cache");

                if (hasPoetryInstall)
                {
                    if (string.IsNullOrEmpty(cacheValue))
                    {
                        findings.Add(MakeFinding(workflow, job, step,
                            "actions/setup-python is used with poetry install but without dependency caching.",
                            "Add `cache: poetry` to the setup-python `with` block."));
                    }
                    else if (cacheValue.Equals("pip", StringComparison.OrdinalIgnoreCase) &&
                             HasPoetryLock(repository))
                    {
                        findings.Add(MakeFinding(workflow, job, step,
                            "actions/setup-python uses `cache: pip`, but the repository contains poetry.lock.",
                            "Use `cache: poetry` instead of `cache: pip` when the project uses Poetry."));
                    }
                }

                if (hasPipenvInstall)
                {
                    if (string.IsNullOrEmpty(cacheValue))
                    {
                        findings.Add(MakeFinding(workflow, job, step,
                            "actions/setup-python is used with pipenv install but without dependency caching.",
                            "Add `cache: pipenv` to the setup-python `with` block."));
                    }
                    else if (cacheValue.Equals("pip", StringComparison.OrdinalIgnoreCase) &&
                             HasPipfileLock(repository))
                    {
                        findings.Add(MakeFinding(workflow, job, step,
                            "actions/setup-python uses `cache: pip`, but the repository contains Pipfile.lock.",
                            "Use `cache: pipenv` instead of `cache: pip` when the project uses Pipenv."));
                    }
                }
            }
        }

        return findings;
    }

    private static Finding MakeFinding(WorkflowDocument workflow, WorkflowJob job, WorkflowStep step, string message, string recommendation) =>
        new("GHA-CACHE010", Severity.Info, "performance", message, recommendation, workflow.FilePath, step.Line, job.Id, step.Name);

    private static bool HasPoetryInstall(WorkflowJob job) =>
        job.Steps.Any(step => ContainsCommand(step.Run, "poetry install"));

    private static bool HasPipenvInstall(WorkflowJob job) =>
        job.Steps.Any(step => ContainsCommand(step.Run, "pipenv install"));

    private static bool HasPoetryLock(RepositoryContext repository) =>
        repository.LockFiles.Any(path => Path.GetFileName(path).Equals("poetry.lock", StringComparison.OrdinalIgnoreCase));

    private static bool HasPipfileLock(RepositoryContext repository) =>
        repository.LockFiles.Any(path => Path.GetFileName(path).Equals("Pipfile.lock", StringComparison.OrdinalIgnoreCase));

    private static bool ContainsCommand(string? run, string command) =>
        run is not null && run.Contains(command, StringComparison.OrdinalIgnoreCase);
}
