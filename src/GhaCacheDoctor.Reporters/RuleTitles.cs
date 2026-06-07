namespace GhaCacheDoctor.Reporters;

internal static class RuleTitles
{
    public static string Get(string ruleId) => ruleId switch
    {
        "GHA-CACHE001" => "setup-node-cache-missing",
        "GHA-CACHE002" => "setup-node-cache-dependency-path-missing",
        "GHA-CACHE003" => "actions-cache-key-missing-lockfile-hash",
        "GHA-CACHE004" => "restore-keys-too-broad",
        "GHA-CACHE005" => "install-step-without-cache",
        "GHA-CACHE006" => "gradle-cache-missing",
        "GHA-CACHE007" => "setup-python-pip-cache-missing",
        "GHA-CACHE008" => "dotnet-cache-missing",
        "GHA-CACHE009" => "docker-buildkit-cache-missing",
        "GHA-CACHE010" => "setup-python-package-cache-missing",
        "GHA-CACHE011" => "go-cache-missing",
        "GHA-CACHE012" => "cargo-cache-missing",
        "GHA-CACHE013" => "maven-cache-missing",
        "GHA-CACHE-PARSE" => "workflow-parse-error",
        _ => "cache-rule"
    };
}
