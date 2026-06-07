# Changelog

All notable changes to this project will be documented in this file.

The project follows preview releases until the first stable `1.0.0`.

## 0.9.0 - 2026-06-07

### Added

- `GHA-CACHE011` go-cache-missing rule: detects `actions/setup-go` without Go module caching.
- `GHA-CACHE012` cargo-cache-missing rule: detects Cargo commands without Rust dependency caching.
- `GHA-CACHE013` maven-cache-missing rule: detects Maven commands without Maven dependency caching.
- SARIF `partialFingerprints` for GitHub Code Scanning alert tracking.
- Machine-readable JSON schemas for output, baseline, and configuration.

### Changed

- Package version updated to `0.9.0`.

## 0.8.0 - 2026-06-07

### Added

- Stable SHA-256 baseline fingerprints for reliable matching across line changes.
- `--prune-baseline` to remove stale baseline entries.
- `--show-suppressions` to make suppressed findings visible in text output.
- `SuppressedFinding` model with suppression source tracking (baseline, inline-file, inline-next-line).

### Changed

- Package version updated to `0.8.0`.

## 0.7.0 - 2026-06-07

### Added

- Baseline file support via `--baseline <path>` and `baseline` config field.
- Baseline generation via `--write-baseline <path>`.
- Inline suppression comments: `# gha-cache-doctor-disable-next-line` and `# gha-cache-doctor-disable-file`.

### Changed

- Package version updated to `0.7.0`.

## 0.6.0 - 2026-06-07

### Added

- `GHA-CACHE010` setup-python-package-cache-missing rule: detects Poetry and Pipenv cache misconfiguration on `actions/setup-python`.
- Yarn `.yarnrc.yml` and `.yarn/cache` workspace context detection.
- `Directory.Packages.props` context detection.

### Changed

- `LooksLikeNodeMonorepo` now includes `.yarnrc.yml` as a monorepo signal.
- `GHA-CACHE002` recommendation text now includes concrete glob patterns (`**/package-lock.json`, `**/pnpm-lock.yaml`, `**/yarn.lock`).
- Package version updated to `0.6.0`.

## 0.5.0 - 2026-06-07

### Added

- `GHA-CACHE008` dotnet-cache-missing rule: detects .NET NuGet restore commands without configured NuGet package caching.
- `GHA-CACHE009` docker-buildkit-cache-missing rule: detects `docker/build-push-action` and `docker buildx build` without BuildKit layer caching.

### Changed

- Package version updated to `0.5.0`.

## 0.4.0 - 2026-06-07

### Added

- SARIF 2.1.0 output through `--format sarif`.
- SARIF rule metadata, source locations, and parser error results.
- Code scanning documentation for GitHub Actions SARIF upload.
- `output` support in the composite action for SARIF and other file-based workflows.

### Changed

- Package version updated to `0.4.0`.
- README header and release metadata presentation cleaned up.

## 0.3.0 - 2026-06-07

### Added

- Markdown report output through `--format markdown`.
- GitHub workflow annotation output through `--format github-annotations`.
- Official composite GitHub Action wrapper with job summary support.
- `GHA-CACHE006` Gradle cache opportunity rule with docs, samples, and unit tests.
- `GHA-CACHE007` setup-python pip cache rule with docs and unit tests.

### Changed

- Package version updated to `0.3.0`.
- Test dependencies updated consistently across all test projects.

## 0.2.0 - 2026-06-05

### Added

- `.gha-cache-doctor.yml` and `.gha-cache-doctor.yaml` configuration loading from the repository root.
- `--config <path|none>` CLI option.
- Config support for workflow path, output format, fail threshold, strict mode, included rules, excluded rules, and per-rule severity overrides.
- Unit tests for config loading, CLI precedence, config disabling, and severity override behavior.
- Configuration reference documentation.

### Changed

- CLI-provided values now explicitly take precedence over config-file values.
- Package version updated to `0.2.0`.

## 0.1.0 - 2026-06-05

### Added

- Strict-mode behavior for broader restore-key and install-step cache checks.
- Repository context detection for `global.json`, `pyproject.toml`, Gradle files, and Docker Compose files.
- Direct unit coverage for `GHA-CACHE004` and `GHA-CACHE005`.
- Direct text and JSON reporter tests.

### Changed

- Promoted the package version from `0.1.0-preview.1` to `0.1.0`.
- Updated project status, roadmap, and install documentation for the polished MVP release.

## 0.1.0-preview.1 - 2026-05-31

### Added

- Initial .NET 10 CLI.
- GitHub Actions workflow discovery for `.github/workflows/*.yml` and `.github/workflows/*.yaml`.
- YAML parsing for workflow jobs and steps.
- Repository context detection for lockfiles and project files.
- Text and JSON report output.
- `scan` command with `--path`, `--format`, `--fail-on`, `--include`, `--exclude`, and `--strict`.
- Initial cache analysis rules:
  - `GHA-CACHE001` setup-node-cache-missing
  - `GHA-CACHE002` setup-node-cache-dependency-path-missing
  - `GHA-CACHE003` actions-cache-key-missing-lockfile-hash
  - `GHA-CACHE004` restore-keys-too-broad
  - `GHA-CACHE005` install-step-without-cache
- Sample good and bad workflows.
- Rule documentation.
- Unit tests for parser, repository context, rules, CLI, and reporters.

### Known Gaps At 0.1.0

- SARIF output is not implemented yet.
- GitHub annotation output is not implemented yet.
- Official GitHub Action wrapper is not implemented yet.
- Configuration file support is not implemented yet.
- Auto-fix support is not implemented yet.
