# Code Scanning

`gha-cache-doctor` can emit SARIF 2.1.0 for GitHub code scanning.

## CLI

```bash
gha-cache-doctor scan --format sarif --fail-on none > gha-cache-doctor.sarif
```

## GitHub Actions

```yaml
name: Cache Doctor Code Scanning

on:
  pull_request:
  push:
    branches: [main]

jobs:
  scan:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      security-events: write
    steps:
      - uses: actions/checkout@v6
      - uses: Wezylnia/gha-cache-doctor@v0.4.0
        with:
          format: sarif
          fail-on: none
          output: gha-cache-doctor.sarif
      - uses: github/codeql-action/upload-sarif@v4
        with:
          sarif_file: gha-cache-doctor.sarif
```

`fail-on: none` lets the upload step run even when findings are present. Raise the threshold separately if you want the workflow to fail on warnings or errors.
