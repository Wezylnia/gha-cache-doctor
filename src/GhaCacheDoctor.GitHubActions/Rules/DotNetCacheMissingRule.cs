using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class DotNetCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE008";
    public string Title => "dotnet-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    private static readonly string[] RestoreCommands = ["dotnet restore"];
    private static readonly string[] BuildCommands = ["dotnet build"];
    private static readonly string[] TestCommands = ["dotnet test"];
    private static readonly string[] PublishCommands = ["dotnet publish"];

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            for (var stepIndex = 0; stepIndex < job.Steps.Count; stepIndex++)
            {
                var step = job.Steps[stepIndex];
                if (!IsNuGetRestoreSignal(step))
                {
                    continue;
                }

                if (HasNuGetCacheBefore(job.Steps.Take(stepIndex)))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This job runs a .NET command that restores NuGet packages without configured NuGet caching.",
                    "Cache the NuGet package folder before restore, for example actions/cache with path `~/.nuget/packages` and a key based on `hashFiles('**/*.csproj', '**/packages.lock.json', '**/global.json')`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
                break;
            }
        }

        return findings;
    }

    private static bool IsNuGetRestoreSignal(WorkflowStep step)
    {
        if (string.IsNullOrWhiteSpace(step.Run))
        {
            return false;
        }

        var run = step.Run;

        // dotnet restore (always triggers restore)
        if (ContainsCommand(run, RestoreCommands))
        {
            return true;
        }

        // dotnet build without --no-restore
        if (ContainsCommand(run, BuildCommands) && !run.Contains("--no-restore", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // dotnet test without --no-restore
        if (ContainsCommand(run, TestCommands) && !run.Contains("--no-restore", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // dotnet publish without --no-restore
        if (ContainsCommand(run, PublishCommands) && !run.Contains("--no-restore", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool ContainsCommand(string run, string[] commands)
    {
        return commands.Any(cmd => run.Contains(cmd, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasNuGetCacheBefore(IEnumerable<WorkflowStep> previousSteps)
    {
        return previousSteps.Any(step =>
            RuleHelpers.IsAction(step, "actions/cache") && IsNuGetCachePath(RuleHelpers.GetWith(step, "path")));
    }

    private static bool IsNuGetCachePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return path.Contains("~/.nuget/packages", StringComparison.OrdinalIgnoreCase) ||
               path.Contains(".nuget/packages", StringComparison.OrdinalIgnoreCase) ||
               (path.Contains("NUGET_PACKAGES", StringComparison.OrdinalIgnoreCase) &&
                path.Contains("env", StringComparison.OrdinalIgnoreCase));
    }
}
