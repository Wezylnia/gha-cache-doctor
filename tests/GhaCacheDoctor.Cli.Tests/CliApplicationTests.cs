using GhaCacheDoctor.Cli;

namespace GhaCacheDoctor.Cli.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public void ScanReturnsOneWhenFailOnThresholdMatchesFinding()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = new CliApplication(output, error).Run(["scan", "--repo", directory.Path, "--fail-on", "info"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("GHA-CACHE001", output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public void ScanSupportsJsonOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "json"]);

        Assert.Equal(0, exitCode);
        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Equal("GHA-CACHE003", document.RootElement.GetProperty("findings")[0].GetProperty("ruleId").GetString());
    }

    [Fact]
    public void ScanSupportsGitHubSummaryOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "github-summary"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("# gha-cache-doctor summary", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanSupportsMarkdownOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "markdown"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("# gha-cache-doctor report", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanSupportsSarifOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "sarif"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("\"version\": \"2.1.0\"", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanSupportsGitHubAnnotationsOutput()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--format", "github-annotations"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("::warning", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanReturnsOneWhenFailOnWarningMatchesWarningFinding()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);

        var exitCode = new CliApplication(new StringWriter(), new StringWriter()).Run(["scan", "--repo", directory.Path, "--fail-on", "warning"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public void ScanAcceptsFailOnNone()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);

        var exitCode = new CliApplication(new StringWriter(), new StringWriter()).Run(["scan", "--repo", directory.Path, "--fail-on", "none"]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public void ScanHonorsIncludeRuleFilter()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
                  - run: npm ci
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--include", "GHA-CACHE003"]);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("GHA-CACHE001", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanHonorsExcludeRuleFilter()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--exclude", "GHA-CACHE003"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("No cache issues found." + Environment.NewLine, output.ToString());
    }

    [Fact]
    public void ScanLoadsDefaultConfigFile()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            exclude:
              - GHA-CACHE003
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(0, exitCode);
        Assert.Equal("No cache issues found." + Environment.NewLine, output.ToString());
    }

    [Fact]
    public void ScanLoadsGitHubSummaryFormatFromConfigFile()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            format: github-summary
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(0, exitCode);
        Assert.Contains("# gha-cache-doctor summary", output.ToString());
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanLoadsSarifFormatFromConfigFile()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            format: sarif
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(0, exitCode);
        Assert.Contains("\"version\": \"2.1.0\"", output.ToString());
    }

    [Fact]
    public void ScanAppliesConfigSeverityOverrideBeforeFailOn()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            failOn: warning
            severity:
              GHA-CACHE005: warning
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - run: dotnet restore
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(1, exitCode);
        Assert.Contains("[warning] GHA-CACHE005", output.ToString());
    }

    [Fact]
    public void ScanCliExcludeOverridesConfigInclude()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            include:
              - GHA-CACHE003
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--exclude", "GHA-CACHE003"]);

        Assert.Equal(0, exitCode);
        Assert.Equal("No cache issues found." + Environment.NewLine, output.ToString());
    }

    [Fact]
    public void ScanConfigNoneDisablesDefaultConfigFile()
    {
        using var directory = new TempDirectory();
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            exclude:
              - GHA-CACHE003
            """);
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--config", "none"]);

        Assert.Equal(0, exitCode);
        Assert.Contains("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void ScanReturnsThreeForParseErrors()
    {
        using var directory = new TempDirectory();
        directory.Write(".github/workflows/ci.yml", "jobs: [");
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path]);

        Assert.Equal(3, exitCode);
        Assert.Contains("parse-error", output.ToString());
    }

    [Fact]
    public void BaselineSuppressesMatchingFinding()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        directory.Write(
            ".gha-cache-doctor-baseline.json",
            """
            {
              "version": 1,
              "findings": [
                {
                  "ruleId": "GHA-CACHE001",
                  "filePath": ".github/workflows/ci.yml",
                  "jobId": "test",
                  "stepName": null,
                  "message": "actions/setup-node is used without dependency caching."
                },
                {
                  "ruleId": "GHA-CACHE005",
                  "filePath": ".github/workflows/ci.yml",
                  "jobId": "test",
                  "stepName": null,
                  "message": "This job installs dependencies but does not configure a matching dependency cache."
                }
              ]
            }
            """);
        var output = new StringWriter();
        var error = new StringWriter();

        var exitCode = new CliApplication(output, error).Run(["scan", "--repo", directory.Path, "--baseline", ".gha-cache-doctor-baseline.json", "--fail-on", "info"]);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("GHA-CACHE001", output.ToString());
    }

    [Fact]
    public void BaselineDoesNotSuppressNonMatchingFinding()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        directory.Write(
            ".gha-cache-doctor-baseline.json",
            """
            {
              "version": 1,
              "findings": [
                {
                  "ruleId": "GHA-CACHE003",
                  "filePath": ".github/workflows/other.yml",
                  "jobId": "build",
                  "stepName": null,
                  "message": "other message"
                }
              ]
            }
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--baseline", ".gha-cache-doctor-baseline.json", "--fail-on", "info"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("GHA-CACHE001", output.ToString());
    }

    [Fact]
    public void BaselineParseErrorsRemain()
    {
        using var directory = new TempDirectory();
        directory.Write(".github/workflows/ci.yml", "jobs: [");
        directory.Write(
            ".gha-cache-doctor-baseline.json",
            """
            { "version": 1, "findings": [] }
            """);

        var exitCode = new CliApplication(new StringWriter(), new StringWriter()).Run(["scan", "--repo", directory.Path, "--baseline", ".gha-cache-doctor-baseline.json"]);

        Assert.Equal(3, exitCode);
    }

    [Fact]
    public void BaselineNoneDisablesConfigBaseline()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        directory.Write(
            ".gha-cache-doctor.yml",
            """
            baseline: .gha-cache-doctor-baseline.json
            """);
        directory.Write(
            ".gha-cache-doctor-baseline.json",
            """
            {
              "version": 1,
              "findings": [
                {
                  "ruleId": "GHA-CACHE001",
                  "filePath": ".github/workflows/ci.yml",
                  "jobId": "test",
                  "stepName": null,
                  "message": "actions/setup-node is used without dependency caching."
                }
              ]
            }
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--baseline", "none", "--fail-on", "info"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("GHA-CACHE001", output.ToString());
    }

    [Fact]
    public void WriteBaselineCreatesValidJsonFile()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);

        var error = new StringWriter();
        var exitCode = new CliApplication(new StringWriter(), error).Run(["scan", "--repo", directory.Path, "--write-baseline", "output-baseline.json", "--fail-on", "none"]);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.True(File.Exists(System.IO.Path.Combine(directory.Path, "output-baseline.json")));
        var content = File.ReadAllText(System.IO.Path.Combine(directory.Path, "output-baseline.json"));
        Assert.Contains("GHA-CACHE001", content);
        Assert.Contains("\"version\":1", content);
    }

    [Fact]
    public void InlineSuppressionDisableNextLineWorks()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            jobs:
              test:
                steps:
                  # gha-cache-doctor-disable-next-line GHA-CACHE003
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--fail-on", "info"]);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void InlineSuppressionDisableFileWorks()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            # gha-cache-doctor-disable-file GHA-CACHE003
            jobs:
              test:
                steps:
                  - uses: actions/cache@v4
                    with:
                      path: ~/.npm
                      key: npm-cache
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--fail-on", "info"]);

        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("GHA-CACHE003", output.ToString());
    }

    [Fact]
    public void InlineSuppressionUnrelatedRuleIsNotSuppressed()
    {
        using var directory = new TempDirectory();
        directory.Write("package-lock.json", "{}");
        directory.Write(
            ".github/workflows/ci.yml",
            """
            # gha-cache-doctor-disable-file GHA-CACHE003
            jobs:
              test:
                steps:
                  - uses: actions/setup-node@v4
                  - run: npm ci
            """);
        var output = new StringWriter();

        var exitCode = new CliApplication(output, new StringWriter()).Run(["scan", "--repo", directory.Path, "--fail-on", "info"]);

        Assert.Equal(1, exitCode);
        Assert.Contains("GHA-CACHE001", output.ToString());
    }
}

internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gha-cache-doctor-" + Guid.NewGuid());
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public string Write(string relativePath, string contents)
    {
        var filePath = System.IO.Path.Combine(Path, relativePath);
        var parent = System.IO.Path.GetDirectoryName(filePath);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }

        File.WriteAllText(filePath, contents);
        return filePath;
    }

    public void Dispose()
    {
        Directory.Delete(Path, recursive: true);
    }
}
