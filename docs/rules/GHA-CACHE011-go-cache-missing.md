# GHA-CACHE011 go-cache-missing

## Summary

Detects `actions/setup-go` usage without Go module caching when Go commands like `go test`, `go build`, or `go mod download` are present.

## Why it matters

Go module downloads can be slow, especially in CI environments. `actions/setup-go` supports built-in caching with `cache: true`, which caches both the module download cache (`GOMODCACHE`) and the build cache (`GOCACHE`). Without it, every CI run downloads all dependencies from scratch.

## How to fix

Enable setup-go caching with `cache: true`:

```yaml
- uses: actions/setup-go@v5
  with:
    go-version: '1.22'
    cache: true
```

Or use `actions/cache` manually:

```yaml
- uses: actions/cache@v4
  with:
    path: |
      ~/go/pkg/mod
      ~/.cache/go-build
    key: ${{ runner.os }}-go-${{ hashFiles('**/go.sum') }}
```

## Examples

### Bad

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-go@v5
        with:
          go-version: '1.22'
      - run: go test ./...
```

### Good

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-go@v5
        with:
          go-version: '1.22'
          cache: true
      - run: go test ./...
```

## Default severity

`info`
