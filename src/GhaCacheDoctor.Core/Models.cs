namespace GhaCacheDoctor.Core;

public sealed record Finding(
    string RuleId,
    Severity Severity,
    string Category,
    string Message,
    string? Recommendation,
    string FilePath,
    int? Line,
    string? JobId,
    string? StepName);

public sealed record ScanResult(
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<WorkflowParseError> ParseErrors,
    IReadOnlyList<SuppressedFinding> SuppressedFindings)
{
    public ScanResult(IReadOnlyList<Finding> findings, IReadOnlyList<WorkflowParseError> parseErrors)
        : this(findings, parseErrors, [])
    {
    }
}

public sealed record SuppressedFinding(
    string RuleId,
    string FilePath,
    int? Line,
    string SuppressionSource,
    Severity Severity,
    string Category,
    string Message);

public sealed record WorkflowParseError(
    string FilePath,
    int? Line,
    string Message);

public sealed record WorkflowDocument(
    string FilePath,
    string? Name,
    IReadOnlyList<WorkflowJob> Jobs);

public sealed record WorkflowJob(
    string Id,
    string? Name,
    IReadOnlyList<WorkflowStep> Steps);

public sealed record WorkflowStep(
    string? Name,
    string? Uses,
    string? Run,
    IReadOnlyDictionary<string, string> With,
    int? Line);

public sealed record WorkflowParseResult(
    WorkflowDocument? Workflow,
    WorkflowParseError? ParseError);

public sealed record RepositoryContext(
    string RootPath,
    IReadOnlyList<string> Files,
    IReadOnlyList<string> LockFiles,
    IReadOnlyList<string> PackageJsonFiles,
    IReadOnlyList<string> CsprojFiles,
    IReadOnlyList<string> SolutionFiles,
    IReadOnlyList<string> Dockerfiles)
{
    public IReadOnlyList<string> GlobalJsonFiles =>
        Files.Where(path => Path.GetFileName(path).Equals("global.json", StringComparison.OrdinalIgnoreCase)).ToArray();

    public IReadOnlyList<string> PythonProjectFiles =>
        Files.Where(path => Path.GetFileName(path).Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase)).ToArray();

    public IReadOnlyList<string> GradleFiles =>
        Files.Where(path =>
            Path.GetFileName(path).Equals("build.gradle", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFileName(path).Equals("build.gradle.kts", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFileName(path).Equals("gradlew", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFileName(path).Equals("gradle.lockfile", StringComparison.OrdinalIgnoreCase)).ToArray();

    public IReadOnlyList<string> ComposeFiles =>
        Files.Where(path =>
            Path.GetFileName(path).Equals("docker-compose.yml", StringComparison.OrdinalIgnoreCase) ||
            Path.GetFileName(path).Equals("compose.yml", StringComparison.OrdinalIgnoreCase)).ToArray();

    public bool HasNodeHints => PackageJsonFiles.Count > 0 || LockFiles.Any(IsNodeLockFile);

    public IReadOnlyList<string> PnpmWorkspaceFiles =>
        Files.Where(IsPnpmWorkspaceFile).ToArray();

    public bool LooksLikeNodeMonorepo =>
        PnpmWorkspaceFiles.Count > 0 ||
        YarnRcFiles.Count > 0 ||
        PackageJsonFiles.Count > 1 ||
        LockFiles.Count(IsNodeLockFile) > 1 ||
        LockFiles.Any(path => IsNodeLockFile(path) && !IsRootPath(path)) ||
        LockFiles.Any(path => IsNodeLockFile(path) && (path.StartsWith("apps/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("packages/", StringComparison.OrdinalIgnoreCase)));

    public IReadOnlyList<string> YarnRcFiles =>
        Files.Where(path => Path.GetFileName(path).Equals(".yarnrc.yml", StringComparison.OrdinalIgnoreCase)).ToArray();

    public bool HasYarnCacheDirectory =>
        Files.Any(path => path.Contains("/.yarn/cache/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(".yarn/cache/", StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> DirectoryPackagesPropsFiles =>
        Files.Where(path => Path.GetFileName(path).Equals("Directory.Packages.props", StringComparison.OrdinalIgnoreCase)).ToArray();

    public static bool IsNodeLockFile(string path)
    {
        var fileName = Path.GetFileName(path);
        return fileName.Equals("package-lock.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("npm-shrinkwrap.json", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("yarn.lock", StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals("pnpm-lock.yaml", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsPnpmWorkspaceFile(string path) =>
        Path.GetFileName(path).Equals("pnpm-workspace.yaml", StringComparison.OrdinalIgnoreCase);

    private static bool IsRootPath(string path) => !path.Contains('/', StringComparison.Ordinal);
}
