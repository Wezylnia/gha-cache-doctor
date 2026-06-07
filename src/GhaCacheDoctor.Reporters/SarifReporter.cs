using System.Text.Json;
using System.Text.Json.Serialization;
using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class SarifReporter : IReporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public string Render(ScanResult result)
    {
        var ruleIds = result.Findings
            .Select(finding => finding.RuleId)
            .Concat(result.ParseErrors.Count > 0 ? ["GHA-CACHE-PARSE"] : [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var sarif = new Dictionary<string, object?>
        {
            ["version"] = "2.1.0",
            ["$schema"] = "https://json.schemastore.org/sarif-2.1.0.json",
            ["runs"] = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "gha-cache-doctor",
                            informationUri = "https://github.com/Wezylnia/gha-cache-doctor",
                            rules = ruleIds.Select(ruleId => new
                            {
                                id = ruleId,
                                name = RuleTitles.Get(ruleId),
                                shortDescription = new { text = RuleTitles.Get(ruleId) },
                                defaultConfiguration = new { level = DefaultLevel(result.Findings, ruleId) },
                                properties = new { tags = new[] { "github-actions", "cache" } }
                            }).ToArray()
                        }
                    },
                    results = result.Findings.Select(ToResult).Concat(result.ParseErrors.Select(ToParseErrorResult)).ToArray()
                }
            }
        };

        return JsonSerializer.Serialize(sarif, Options) + Environment.NewLine;
    }

    private static object ToResult(Finding finding) => new
    {
        ruleId = finding.RuleId,
        level = Level(finding.Severity),
        message = new
        {
            text = string.IsNullOrWhiteSpace(finding.Recommendation)
                ? finding.Message
                : $"{finding.Message} Recommendation: {finding.Recommendation}"
        },
        locations = new[]
        {
            new
            {
                physicalLocation = new
                {
                    artifactLocation = new { uri = NormalizeUri(finding.FilePath) },
                    region = finding.Line is null ? null : new { startLine = finding.Line }
                }
            }
        },
        properties = new
        {
            category = finding.Category,
            jobId = finding.JobId,
            stepName = finding.StepName
        }
    };

    private static object ToParseErrorResult(WorkflowParseError parseError) => new
    {
        ruleId = "GHA-CACHE-PARSE",
        level = "error",
        message = new { text = parseError.Message },
        locations = new[]
        {
            new
            {
                physicalLocation = new
                {
                    artifactLocation = new { uri = NormalizeUri(parseError.FilePath) },
                    region = parseError.Line is null ? null : new { startLine = parseError.Line }
                }
            }
        },
        properties = new { category = "parser" }
    };

    private static string DefaultLevel(IReadOnlyList<Finding> findings, string ruleId) =>
        ruleId == "GHA-CACHE-PARSE"
            ? "error"
            : Level(findings.First(finding => finding.RuleId == ruleId).Severity);

    private static string Level(Severity severity) => severity switch
    {
        Severity.Error => "error",
        Severity.Warning => "warning",
        _ => "note"
    };

    private static string NormalizeUri(string path) => path.Replace('\\', '/');
}
