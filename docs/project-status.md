# Project Status

This public status document tracks user-facing progress from the current MVP toward the next release.

## Current Version Target

Current release: `1.0.0`

Goal: first stable release. All documented CLI, JSON, SARIF, configuration, baseline, suppression, and cache rule behavior is locked down.

## Completed In v1.0.0

- Lock down v1 stable contract tests for JSON, SARIF, baseline prune, inline suppression, and CLI exit codes.
- Stabilize suppression source names (`inline-file`, `inline-next-line`, `baseline`).
- Refresh all documentation, project status, roadmap, package metadata, and install examples.

## Completed In v0.9.0

- Add `GHA-CACHE011` go-cache-missing, `GHA-CACHE012` cargo-cache-missing, `GHA-CACHE013` maven-cache-missing rules.
- Add SARIF `partialFingerprints`.
- Add machine-readable JSON schemas.

## Completed In v0.8.0

- Add stable SHA-256 baseline fingerprints.
- Add `--prune-baseline` mode.
- Add `--show-suppressions` and `SuppressedFinding` model.

## Completed In v0.7.0

- Add baseline suppression, generation, and inline suppression comments.

## Completed In v0.6.0

- Improve workspace context detection (`.yarnrc.yml`, `.yarn/cache`, `Directory.Packages.props`).
- Strengthen monorepo cache dependency path recommendations.
- Add `GHA-CACHE010` setup-python-package-cache-missing rule.

- Add `GHA-CACHE008` dotnet-cache-missing rule.
- Add `GHA-CACHE009` docker-buildkit-cache-missing rule.
- Add rule docs, samples, and unit tests for each new rule.

## Completed In v0.1.0

- .NET 10 target framework.
- Local CLI.
- GitHub Actions workflow scanning.
- Text and JSON output.
- Include/exclude filtering.
- `--fail-on` exit-code behavior, including `--fail-on none`.
- Strict mode behavior for selected rules.
- Repository context detection for Node, .NET, Python, Gradle, Dockerfile, and Docker Compose hints.
- Initial cache rules.
- Sample good and bad workflows.
- README usage documentation.
- Changelog, contributing, security, code of conduct, roadmap, and release checklist.
- Rule, parser, repository context, reporter, and CLI tests.

## Completed In v0.2.0

- Add `.gha-cache-doctor.yml` configuration support.
- Add rule disabling and severity overrides.
- Add workflow path, format, fail threshold, strict mode, include, exclude, and severity override config fields.
- Add config validation errors with helpful CLI output.
- Document config examples.
- Add config behavior tests.

## Completed In v0.3.0

- Add Markdown report output.
- Add GitHub job summary output.
- Add GitHub workflow annotation output.
- Add official composite GitHub Action wrapper.
- Add Gradle and setup-python pip cache opportunity rules.
- Update test dependencies consistently across all test projects.
- Document CI usage for the action and output formats.

## Completed In v0.4.0

- Add SARIF 2.1.0 output.
- Add SARIF rule metadata, source locations, and parser error results.
- Add GitHub code scanning documentation.
- Add action `output` support for file-based workflows.
- Clean up README release/license metadata.

## Repository Protection

The repository is configured so public contribution should flow through pull requests:

- `main` is protected.
- The `build` GitHub Actions check is required before merge.
- Pull requests require at least one approving review.
- Code-owner review is required.
- `CODEOWNERS` assigns all files to `@Wezylnia`.
- Stale approvals are dismissed after new commits are pushed.
- The most recent push must be approved by someone other than the pusher.
- Conversation resolution is required before merge.
- Force pushes and branch deletion are disabled.
- Administrators are not locked out, so the maintainer keeps emergency bypass ability.
- GitHub Copilot is configured for automatic pull request review on pushes to PRs targeting `main`.

## Next Release

Future releases will add non-breaking improvements while keeping the v1.0 public API stable.

## Validation Commands

```bash
dotnet restore
dotnet build GhaCacheDoctor.slnx --configuration Release
dotnet test GhaCacheDoctor.slnx --configuration Release --no-build
dotnet pack src/GhaCacheDoctor.Cli --configuration Release --no-build
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --fail-on none
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format json --fail-on none
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format markdown --fail-on none
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format sarif --fail-on none
dotnet run --project src/GhaCacheDoctor.Cli -- scan --path samples/github-actions/bad --format github-annotations --fail-on none
```
