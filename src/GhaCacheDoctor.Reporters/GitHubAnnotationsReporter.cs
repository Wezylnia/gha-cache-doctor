using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class GitHubAnnotationsReporter : IReporter
{
    public string Render(ScanResult result)
    {
        var writer = new StringWriter();

        foreach (var finding in result.Findings)
        {
            var level = finding.Severity switch
            {
                Severity.Error => "error",
                Severity.Warning => "warning",
                _ => "notice"
            };
            var title = $"{finding.RuleId} {finding.Category}";
            var location = AnnotationLocation(finding.FilePath, finding.Line, title);
            var message = string.IsNullOrWhiteSpace(finding.Recommendation)
                ? finding.Message
                : $"{finding.Message} Recommendation: {finding.Recommendation}";

            writer.WriteLine($"::{level} {location}::{EscapeMessage(message)}");
        }

        foreach (var parseError in result.ParseErrors)
        {
            var location = AnnotationLocation(parseError.FilePath, parseError.Line, "parse-error");
            writer.WriteLine($"::error {location}::{EscapeMessage(parseError.Message)}");
        }

        return writer.ToString();
    }

    private static string AnnotationLocation(string filePath, int? line, string title)
    {
        var properties = new List<string>
        {
            $"file={EscapeProperty(filePath)}",
            $"title={EscapeProperty(title)}"
        };

        if (line is not null)
        {
            properties.Add($"line={line}");
        }

        return string.Join(",", properties);
    }

    private static string EscapeProperty(string value) =>
        EscapeMessage(value)
            .Replace(":", "%3A", StringComparison.Ordinal)
            .Replace(",", "%2C", StringComparison.Ordinal);

    private static string EscapeMessage(string value) =>
        value
            .Replace("%", "%25", StringComparison.Ordinal)
            .Replace("\r", "%0D", StringComparison.Ordinal)
            .Replace("\n", "%0A", StringComparison.Ordinal);
}
