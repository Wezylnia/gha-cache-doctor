# GHA-CACHE008 dotnet-cache-missing

## Summary

Detects .NET commands that restore NuGet packages (such as `dotnet restore`, `dotnet build`, `dotnet test`, `dotnet publish`) when no NuGet cache is configured earlier in the same job.

## Why it matters

Without NuGet package caching, every CI run must download all packages from NuGet.org or a private feed. This adds minutes to each build and wastes runner bandwidth. A small `actions/cache` step before restore can reduce restore times from minutes to seconds.

## How to fix

Add an `actions/cache` step before NuGet restore that caches `~/.nuget/packages` with a key that includes lockfile hashes:

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/packages.lock.json', '**/global.json') }}
    restore-keys: ${{ runner.os }}-nuget-
```

If you use a custom NuGet packages directory via `NUGET_PACKAGES`, use that path instead:

```yaml
- uses: actions/cache@v4
  with:
    path: ${{ env.NUGET_PACKAGES }}
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj', '**/packages.lock.json') }}
```

## Examples

### Bad

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet restore
      - run: dotnet build --no-restore
```

### Good

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
          restore-keys: ${{ runner.os }}-nuget-
      - run: dotnet restore
      - run: dotnet build --no-restore
```

## Default severity

`info`

## Detection notes

- `dotnet build --no-restore` and `dotnet test --no-restore` do not trigger this rule.
- `actions/setup-dotnet` by itself is not treated as cache configuration.
- The rule checks for `actions/cache` with a NuGet-related path (`~/.nuget/packages`, `.nuget/packages`, or `${{ env.NUGET_PACKAGES }}`).
