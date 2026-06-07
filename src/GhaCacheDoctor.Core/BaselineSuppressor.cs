using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GhaCacheDoctor.Core;

public sealed record BaselineDocument(
    int Version,
    List<BaselineEntry> Findings)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static BaselineDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Baseline file not found: {path}");
        }

        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<BaselineDocument>(json, Options);
        return document ?? new BaselineDocument(1, []);
    }

    public void Save(string path)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(this, Options);
        File.WriteAllText(path, json);
    }

    public static BaselineDocument FromFindings(IReadOnlyList<Finding> findings) =>
        new(1, findings.Select(BaselineEntry.FromFinding).ToList());
}

public sealed record BaselineEntry(
    string RuleId,
    string FilePath,
    string? JobId,
    string? StepName,
    string Message,
    string? Fingerprint = null)
{
    public static BaselineEntry FromFinding(Finding finding) =>
        new(finding.RuleId, NormalizePath(finding.FilePath), finding.JobId, finding.StepName, finding.Message,
            ComputeFingerprint(finding));

    public bool Matches(Finding finding)
    {
        // If this entry has a fingerprint, match by fingerprint only
        if (!string.IsNullOrWhiteSpace(Fingerprint))
        {
            return Fingerprint.Equals(ComputeFingerprint(finding), StringComparison.Ordinal);
        }

        // Legacy matching for entries without fingerprint
        return RuleId.Equals(finding.RuleId, StringComparison.OrdinalIgnoreCase) &&
            PathsMatch(FilePath, finding.FilePath) &&
            (JobId ?? "") == (finding.JobId ?? "") &&
            (StepName ?? "") == (finding.StepName ?? "") &&
            Message.Equals(finding.Message, StringComparison.Ordinal);
    }

    public static string ComputeFingerprint(Finding finding)
    {
        var input = string.Join("|",
            finding.RuleId.ToLowerInvariant(),
            NormalizePath(finding.FilePath),
            (finding.JobId ?? "").ToLowerInvariant(),
            (finding.StepName ?? "").ToLowerInvariant(),
            finding.Category.ToLowerInvariant(),
            finding.Message.ToLowerInvariant());

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }

    private static bool PathsMatch(string baselinePath, string findingPath)
    {
        var normalizedBaseline = NormalizePath(baselinePath);
        var normalizedFinding = NormalizePath(findingPath);

        if (normalizedBaseline.Equals(normalizedFinding, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Allow relative baseline path to match absolute finding path
        return normalizedFinding.EndsWith("/" + normalizedBaseline, StringComparison.OrdinalIgnoreCase) ||
               normalizedFinding.EndsWith("\\" + normalizedBaseline.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizePath(string path) => path.Replace('\\', '/');
}

public static class BaselineSuppressor
{
    public static (ScanResult Result, List<SuppressedFinding> Suppressed) ApplyWithTracking(ScanResult result, string baselinePath)
    {
        var baseline = BaselineDocument.Load(baselinePath);
        var entries = baseline.Findings ?? [];

        var remainingFindings = new List<Finding>();
        var suppressed = new List<SuppressedFinding>();
        foreach (var finding in result.Findings)
        {
            if (entries.Any(entry => entry.Matches(finding)))
            {
                suppressed.Add(new SuppressedFinding(finding.RuleId, finding.FilePath, finding.Line, "baseline", finding.Severity, finding.Category, finding.Message));
                continue;
            }

            remainingFindings.Add(finding);
        }

        var mergedSuppressed = result.SuppressedFindings
            .Concat(suppressed)
            .ToArray();
        return (new ScanResult(remainingFindings, result.ParseErrors, mergedSuppressed), suppressed);
    }

    public static ScanResult Apply(ScanResult result, string baselinePath)
    {
        var (scanResult, _) = ApplyWithTracking(result, baselinePath);
        return scanResult;
    }

    public static void Prune(ScanResult result, string baselinePath)
    {
        var baseline = BaselineDocument.Load(baselinePath);
        var entries = baseline.Findings ?? [];

        // Create backup
        var backupPath = baselinePath + ".bak";
        File.Copy(baselinePath, backupPath, overwrite: true);

        // Keep only entries that still match current findings
        var pruned = entries
            .Where(entry => result.Findings.Any(f => entry.Matches(f)))
            .ToList();

        var prunedDocument = new BaselineDocument(baseline.Version, pruned);
        prunedDocument.Save(baselinePath);
    }
}
