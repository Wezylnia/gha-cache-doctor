# GHA-CACHE009 docker-buildkit-cache-missing

## Summary

Detects `docker/build-push-action` usage and `docker buildx build` commands that do not configure BuildKit layer caching with `cache-from` or `cache-to`.

## Why it matters

Docker layer caching can dramatically speed up image builds in CI. Without `cache-from` and `cache-to`, every CI run rebuilds all layers from scratch. GitHub Actions provides a built-in cache backend (`type=gha`) that stores BuildKit cache layers in the GitHub Actions cache, avoiding the need for a separate registry.

## How to fix

For `docker/build-push-action`, add `cache-from` and `cache-to`:

```yaml
- uses: docker/build-push-action@v6
  with:
    push: true
    tags: user/app:latest
    cache-from: type=gha
    cache-to: type=gha,mode=max
```

For `docker buildx build` CLI commands, add the equivalent flags:

```yaml
- run: docker buildx build --cache-from type=gha --cache-to type=gha,mode=max -t myimage:latest .
```

## Examples

### Bad

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/build-push-action@v6
        with:
          push: true
          tags: user/app:latest
```

### Good

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/setup-buildx-action@v3
      - uses: docker/build-push-action@v6
        with:
          push: true
          tags: user/app:latest
          cache-from: type=gha
          cache-to: type=gha,mode=max
```

## Default severity

`info`

## Detection notes

- Plain `docker build` (without `buildx`) is not reported — it may not use BuildKit and would create false positives.
- The rule reports when neither `cache-from` nor `cache-to` is present. Having just one is enough to suppress the finding.
