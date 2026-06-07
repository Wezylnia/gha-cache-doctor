using System.Text.RegularExpressions;

namespace GhaCacheDoctor.Core;

public static class InlineSuppressionParser
{
    private const string DisableNextLinePrefix = "gha-cache-doctor-disable-next-line";
    private const string DisableFilePrefix = "gha-cache-doctor-disable-file";

    public static InlineSuppressions Parse(string filePath, string content)
    {
        var disableNextLine = new Dictionary<int, HashSet<string>>();
        var disableFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = content.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var commentIndex = line.IndexOf('#');
            if (commentIndex < 0)
            {
                continue;
            }

            var comment = line[(commentIndex + 1)..].Trim();

            if (comment.StartsWith(DisableFilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var ruleId = comment[DisableFilePrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(ruleId))
                {
                    disableFile.Add(ruleId);
                }
            }
            else if (comment.StartsWith(DisableNextLinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var ruleId = comment[DisableNextLinePrefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(ruleId))
                {
                    // Find the next non-empty line
                    var nextLine = i + 1;
                    while (nextLine < lines.Length && string.IsNullOrWhiteSpace(lines[nextLine]))
                    {
                        nextLine++;
                    }

                    if (nextLine < lines.Length)
                    {
                        var lineNumber = nextLine + 1; // 1-based
                        if (!disableNextLine.ContainsKey(lineNumber))
                        {
                            disableNextLine[lineNumber] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        disableNextLine[lineNumber].Add(ruleId);
                    }
                }
            }
        }

        return new InlineSuppressions(disableNextLine, disableFile);
    }
}

public sealed record InlineSuppressions(
    IReadOnlyDictionary<int, HashSet<string>> DisableNextLine,
    IReadOnlySet<string> DisableFile)
{
    public bool IsSuppressed(Finding finding)
    {
        // File-level suppression
        if (DisableFile.Contains(finding.RuleId))
        {
            return true;
        }

        // Next-line suppression (only if finding has a line number)
        if (finding.Line is { } line && DisableNextLine.TryGetValue(line, out var ruleIds))
        {
            return ruleIds.Contains(finding.RuleId);
        }

        return false;
    }

    public string GetSuppressionSource(Finding finding)
    {
        if (DisableFile.Contains(finding.RuleId))
        {
            return "inline-file";
        }

        if (finding.Line is { } line && DisableNextLine.TryGetValue(line, out var ruleIds) && ruleIds.Contains(finding.RuleId))
        {
            return "inline-next-line";
        }

        return "unknown";
    }

    public static InlineSuppressions Empty { get; } = new(
        new Dictionary<int, HashSet<string>>(),
        new HashSet<string>(StringComparer.OrdinalIgnoreCase));
}
