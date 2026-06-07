# GHA-CACHE013 maven-cache-missing

## Summary

Detects Maven commands (`mvn test`, `mvn verify`, `mvn package`, `mvn install`, and their `./mvnw` equivalents) that run without Maven dependency caching configured earlier in the same job.

## Why it matters

Maven downloads dependencies to `~/.m2/repository` on every clean build. Without caching, CI runs waste time and bandwidth re-downloading the same artifacts. `actions/setup-java` supports built-in Maven caching with `cache: maven`.

## How to fix

Use `actions/setup-java` with `cache: maven`:

```yaml
- uses: actions/setup-java@v4
  with:
    distribution: 'temurin'
    java-version: '21'
    cache: maven
```

Or use `actions/cache` manually:

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.m2/repository
    key: ${{ runner.os }}-maven-${{ hashFiles('**/pom.xml') }}
```

## Examples

### Bad

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: mvn test
```

### Good

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-java@v4
        with:
          distribution: 'temurin'
          java-version: '21'
          cache: maven
      - run: mvn test
```

## Default severity

`info`

## Detection notes

- `mvn --version` does not trigger this rule.
- Gradle commands are handled separately by `GHA-CACHE006`.
- `cache: gradle` on `setup-java` does not count as Maven cache.
