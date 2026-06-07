# Rule Catalog

`gha-cache-doctor` rules focus on GitHub Actions cache correctness, performance, and maintainability.

| Rule | Description |
| --- | --- |
| [`GHA-CACHE001`](GHA-CACHE001-setup-node-cache-missing.md) | Detects `actions/setup-node` without dependency caching when installs are present. |
| [`GHA-CACHE002`](GHA-CACHE002-setup-node-cache-dependency-path-missing.md) | Detects missing `cache-dependency-path` in likely monorepos. |
| [`GHA-CACHE003`](GHA-CACHE003-actions-cache-key-missing-lockfile-hash.md) | Detects dependency cache keys that do not include lockfile hashes. |
| [`GHA-CACHE004`](GHA-CACHE004-restore-keys-too-broad.md) | Detects overly broad restore keys. |
| [`GHA-CACHE005`](GHA-CACHE005-install-step-without-cache.md) | Detects install steps that appear to run without a matching cache. |
| [`GHA-CACHE006`](GHA-CACHE006-gradle-cache-missing.md) | Detects Gradle build or test jobs that run before Gradle dependency caching is configured. |
| [`GHA-CACHE007`](GHA-CACHE007-setup-python-pip-cache-missing.md) | Detects `actions/setup-python` without pip dependency caching when Python installs are present. |
| [`GHA-CACHE008`](GHA-CACHE008-dotnet-cache-missing.md) | Detects .NET NuGet restore commands without configured NuGet package caching. |
| [`GHA-CACHE009`](GHA-CACHE009-docker-buildkit-cache-missing.md) | Detects `docker/build-push-action` and `docker buildx build` without BuildKit layer caching. |
| [`GHA-CACHE010`](GHA-CACHE010-setup-python-package-cache-missing.md) | Detects `actions/setup-python` with Poetry or Pipenv installs but without the matching cache type. |
| [`GHA-CACHE011`](GHA-CACHE011-go-cache-missing.md) | Detects `actions/setup-go` without Go module caching. |
| [`GHA-CACHE012`](GHA-CACHE012-cargo-cache-missing.md) | Detects Cargo commands without Rust dependency caching. |
| [`GHA-CACHE013`](GHA-CACHE013-maven-cache-missing.md) | Detects Maven commands without Maven dependency caching. |

New rules should include focused tests, a rule document, and a README table update.

For package-manager-specific key examples, see the [Cache Key Cookbook](../cache-key-cookbook.md).
