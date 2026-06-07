# GHA-CACHE012 cargo-cache-missing

## Summary

Detects Rust Cargo commands (`cargo build`, `cargo test`, `cargo check`, `cargo clippy`) that run without Rust dependency caching configured earlier in the same job.

## Why it matters

Cargo downloads and compiles dependencies on every clean build. In CI, this can add minutes to each run. Tools like `Swatinem/rust-cache` or manual `actions/cache` for `~/.cargo/registry`, `~/.cargo/git`, and `target` can dramatically reduce build times.

## How to fix

Use `Swatinem/rust-cache` (recommended):

```yaml
- uses: Swatinem/rust-cache@v2
```

Or use `actions/cache` manually:

```yaml
- uses: actions/cache@v4
  with:
    path: |
      ~/.cargo/registry
      ~/.cargo/git
      target
    key: ${{ runner.os }}-cargo-${{ hashFiles('**/Cargo.lock') }}
```

## Examples

### Bad

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: cargo test
```

### Good

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: Swatinem/rust-cache@v2
      - run: cargo test
```

## Default severity

`info`

## Detection notes

- `cargo --version` does not trigger this rule.
- `Swatinem/rust-cache` or an `actions/cache` step with cargo-related paths before the Cargo command suppresses the finding.
