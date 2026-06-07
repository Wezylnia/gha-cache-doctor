using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class TextReporter : IReporter
{
    public string Render(ScanResult result)
    {
        if (result.Findings.Count == 0 && result.ParseErrors.Count == 0)
        {
            return "No cache issues found." + Environment.NewLine;
        }

        var writer = new StringWriter();
        foreach (var group in result.Findings.GroupBy(finding => finding.FilePath).OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine(group.Key);
            writer.WriteLine();

            foreach (var finding in group)
            {
                writer.WriteLine($"[{finding.Severity.ToString().ToLowerInvariant()}] {finding.RuleId} {RuleTitles.Get(finding.RuleId)}");
                if (!string.IsNullOrWhiteSpace(finding.JobId))
                {
                    writer.WriteLine($"Job: {finding.JobId}");
                }

                if (!string.IsNullOrWhiteSpace(finding.StepName))
                {
                    writer.WriteLine($"Step: {finding.StepName}");
                }

                if (finding.Line is not null)
                {
                    writer.WriteLine($"Line: {finding.Line}");
                }

                writer.WriteLine(finding.Message);
                if (!string.IsNullOrWhiteSpace(finding.Recommendation))
                {
                    writer.WriteLine($"Recommendation: {finding.Recommendation}");
                }

                writer.WriteLine();
            }
        }

        foreach (var parseError in result.ParseErrors)
        {
            writer.WriteLine($"[error] parse-error {parseError.FilePath}");
            if (parseError.Line is not null)
            {
                writer.WriteLine($"Line: {parseError.Line}");
            }

            writer.WriteLine(parseError.Message);
            writer.WriteLine();
        }

        return writer.ToString();
    }
}
