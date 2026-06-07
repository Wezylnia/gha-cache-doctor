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

    public string Render(ScanResult result, bool showSuppressions)
    {
        var output = Render(result);
        if (!showSuppressions || result.SuppressedFindings.Count == 0)
        {
            return output;
        }

        var writer = new StringWriter();
        writer.Write(output);
        writer.WriteLine("Suppressed findings:");
        writer.WriteLine();

        foreach (var group in result.SuppressedFindings.GroupBy(s => s.FilePath).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine(group.Key);
            foreach (var sf in group)
            {
                writer.WriteLine($"  [{sf.Severity.ToString().ToLowerInvariant()}] {sf.RuleId} ({sf.SuppressionSource})");
                if (sf.Line is not null)
                {
                    writer.WriteLine($"  Line: {sf.Line}");
                }

                writer.WriteLine($"  {sf.Message}");
                writer.WriteLine();
            }
        }

        return writer.ToString();
    }
}
