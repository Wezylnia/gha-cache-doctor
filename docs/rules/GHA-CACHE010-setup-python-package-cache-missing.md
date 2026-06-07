# GHA-CACHE010 setup-python-package-cache-missing

## Summary

Detects `actions/setup-python` usage with Poetry or Pipenv dependency installation when the matching cache type (`poetry` or `pipenv`) is not configured, or when `cache: pip` is used but the repository contains `poetry.lock` or `Pipfile.lock`.

## Why it matters

`actions/setup-python` supports built-in dependency caching for pip, pipenv, and Poetry. Using the wrong cache type (e.g., `cache: pip` in a Poetry project) means dependencies are not cached correctly, and each CI run reinstalls them from scratch.

## How to fix

For Poetry projects, use `cache: poetry`:

```yaml
- uses: actions/setup-python@v5
  with:
    python-version: '3.12'
    cache: poetry
```

For Pipenv projects, use `cache: pipenv`:

```yaml
- uses: actions/setup-python@v5
  with:
    python-version: '3.12'
    cache: pipenv
```

## Examples

### Bad (Poetry)

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.12'
          cache: pip
      - run: poetry install
```

### Bad (Pipenv)

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.12'
      - run: pipenv install --deploy
```

### Good (Poetry)

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: '3.12'
          cache: poetry
      - run: poetry install
```

## Default severity

`info`

## Detection notes

- This rule does not duplicate `GHA-CACHE007` (setup-python-pip-cache-missing). GHA-CACHE007 covers pip, and this rule covers Poetry and Pipenv.
- The rule checks for `poetry.lock` or `Pipfile.lock` in the repository before reporting wrong-cache-type findings.
- Python repository hints are required before any findings are reported.
