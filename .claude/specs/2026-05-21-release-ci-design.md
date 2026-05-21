# Release CI Design

**Date:** 2026-05-21
**Scope:** Revamp `.github/workflows/check-and-release.yaml`

## Goals

1. Switch the release trigger from push-to-main to an explicit git tag push.
2. Fix the NuGet version-check bug (currently compares against the *latest* published version instead of checking whether the specific version is in the published list).
3. Add a GitHub Release with release notes extracted from `CHANGELOG.md`.

## Non-Goals

- Changing how the version number is set (still `Directory.Build.props` + `Package.props`).
- Automating CHANGELOG.md updates.
- Adding a manual approval gate (GitHub Environment protection rules).

## Trigger

```yaml
on:
  push:
    tags: ['v*']
```

A release is initiated by pushing a tag whose name equals `v` + the computed `PackageVersion` (e.g., `v0.0.17-beta`). The `run-checks.yaml` workflow continues to handle tests and formatting checks on PRs and main-branch pushes — no changes there.

## Job: `validate`

**Permissions:** `contents: read`

Steps:

1. **Extract version** — use `xmllint` to read `DefaultVersion` from `Directory.Build.props` and `PackageVersion` template from `Package.props`, then substitute to produce the full NuGet version string (e.g., `0.0.17-beta`).

2. **Assert tag matches version** — fail fast if `github.ref_name` != `v${PackageVersion}`. Catches the common mistake of tagging before bumping the version.

3. **NuGet guard (fixed)** — fetch `https://api.nuget.org/v3-flatcontainer/aptos/index.json` and check whether the specific version appears anywhere in `.versions[]`:
   ```bash
   jq -r --arg v "$PKG_VERSION" '.versions | contains([$v])'
   ```
   If `true`, set `is_published=true` and the `release` job is skipped.

**Outputs:** `version`, `is_published`

## Job: `release`

**Permissions:** `contents: write`
**Condition:** `needs.validate.outputs.is_published == 'false'`

Steps in order:

1. Checkout + Setup .NET (restore + build).
2. Run tests — `dotnet test` to verify the tagged commit before publishing.
3. Pack — `dotnet pack --configuration Release --no-build --output ./artifacts`.
4. Publish to NuGet — `dotnet nuget push ./artifacts/*.nupkg` using `NUGET_API_KEY` secret.
5. Extract changelog — `awk` pulls everything under `## Unreleased` down to the next `##` heading in `CHANGELOG.md`.
6. Create GitHub Release — `gh release create "v${PKG_VERSION}"` against the existing tag, with extracted notes as the body and `.nupkg` files attached as assets.

**Note:** The git-tag creation step present in the current workflow is removed — the tag already exists (it triggered the workflow).

## Secrets Required

| Secret | Purpose |
|--------|---------|
| `NUGET_API_KEY` | Publish packages to NuGet (already configured) |
| `GITHUB_TOKEN` | Create GitHub Release (auto-provided by Actions) |

## Release Workflow (developer steps)

1. Bump `DefaultVersion` in `Directory.Build.props`.
2. Update `## Unreleased` section in `CHANGELOG.md` with release notes.
3. Merge to `main`.
4. Push the tag: `git tag v0.0.18-beta && git push origin v0.0.18-beta`.
5. CI validates, tests, publishes to NuGet, and creates a GitHub Release automatically.
