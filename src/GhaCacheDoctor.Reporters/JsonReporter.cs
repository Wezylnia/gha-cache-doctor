using System.Text.Json;
using System.Text.Json.Serialization;
using GhaCacheDoctor.Core;

namespace GhaCacheDoctor.Reporters;

public sealed class JsonReporter : IReporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string Render(ScanResult result) => Render(result, showSuppressions: false);

    public string Render(ScanResult result, bool showSuppressions)
    {
        if (showSuppressions)
        {
            return JsonSerializer.Serialize(result, Options) + Environment.NewLine;
        }

        var payload = new { result.Findings, result.ParseErrors };
        return JsonSerializer.Serialize(payload, Options) + Environment.NewLine;
    }
}
