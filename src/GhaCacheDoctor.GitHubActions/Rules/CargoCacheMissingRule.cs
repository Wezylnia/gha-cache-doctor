using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.GitHubActions.Rules;

public sealed class CargoCacheMissingRule : IRule
{
    public string Id => "GHA-CACHE012";
    public string Title => "cargo-cache-missing";
    public Severity DefaultSeverity => Severity.Info;
    public string Category => "performance";

    private static readonly string[] CargoCommands = ["cargo build", "cargo test", "cargo check", "cargo clippy"];

    public IReadOnlyList<Finding> Analyze(WorkflowDocument workflow, RepositoryContext repository, bool strictMode = false)
    {
        var findings = new List<Finding>();
        foreach (var job in workflow.Jobs)
        {
            for (var i = 0; i < job.Steps.Count; i++)
            {
                var step = job.Steps[i];
                if (!IsCargoCommand(step))
                {
                    continue;
                }

                if (HasRustCacheBefore(job.Steps.Take(i)))
                {
                    continue;
                }

                findings.Add(new Finding(
                    Id,
                    DefaultSeverity,
                    Category,
                    "This job runs Cargo commands without Rust dependency caching.",
                    "Add Rust dependency caching before Cargo commands, for example `Swatinem/rust-cache` or actions/cache for `~/.cargo/registry`, `~/.cargo/git`, and `target`.",
                    workflow.FilePath,
                    step.Line,
                    job.Id,
                    step.Name));
                break;
            }
        }

        return findings;
    }

    private static bool IsCargoCommand(WorkflowStep step) =>
        step.Run is not null && CargoCommands.Any(cmd => step.Run.Contains(cmd, StringComparison.OrdinalIgnoreCase)) &&
        !step.Run.Contains("cargo --version", StringComparison.OrdinalIgnoreCase);

    private static bool HasRustCacheBefore(IEnumerable<WorkflowStep> steps) =>
        steps.Any(step =>
            RuleHelpers.IsAction(step, "Swatinem/rust-cache") ||
            (RuleHelpers.IsAction(step, "actions/cache") && IsCargoCachePath(RuleHelpers.GetWith(step, "path"))));

    private static bool IsCargoCachePath(string? path) =>
        path is not null && (
            path.Contains("~/.cargo/registry", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("~/.cargo/git", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/target", StringComparison.OrdinalIgnoreCase));
}
