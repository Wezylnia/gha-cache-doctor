# GitHub Action

`gha-cache-doctor` ships a composite GitHub Action wrapper for CI usage.

## Basic Usage

```yaml
name: Cache Doctor

on:
  pull_request:
  push:
    branches: [main]

jobs:
  cache-doctor:
    runs-on: ubuntu-latest
    permissions:
      contents: read
    steps:
      - uses: actions/checkout@v6
      - uses: Wezylnia/gha-cache-doctor@v0.4.0
        with:
          fail-on: warning
```

By default the action writes a Markdown report to the GitHub job summary.

## Inputs

| Input | Default | Description |
| --- | --- | --- |
| `repo` | `.` | Repository root to inspect. |
| `path` | `.github/workflows` | Workflow file or directory to scan. |
| `format` | `github-summary` | `github-summary`, `github-annotations`, `markdown`, `sarif`, `json`, or `text`. |
| `fail-on` | `warning` | Minimum severity that fails the action: `none`, `info`, `warning`, or `error`. |
| `strict` | `false` | Enables stricter rule behavior. |
| `config` | empty | Config file path, or `none` to disable config loading. |
| `include` | empty | Comma-separated rule IDs to include. |
| `exclude` | empty | Comma-separated rule IDs to exclude. |
| `dotnet-version` | `10.0.x` | .NET SDK version used to run the action. |
| `output` | empty | Optional file path where scanner output should be written. |

## Annotation Output

Use `github-annotations` to emit GitHub workflow commands:

```yaml
- uses: Wezylnia/gha-cache-doctor@v0.4.0
  with:
    format: github-annotations
    fail-on: warning
```

## SARIF Output

```yaml
- uses: Wezylnia/gha-cache-doctor@v0.4.0
  with:
    format: sarif
    fail-on: none
    output: gha-cache-doctor.sarif
```
