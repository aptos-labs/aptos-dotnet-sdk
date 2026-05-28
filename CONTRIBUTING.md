# Contributing to the Aptos .NET SDK

## Development setup

**Prerequisites:** .NET 10 SDK, a C# IDE (Rider or VS Code with C# Dev Kit).

```bash
git clone https://github.com/aptos-labs/aptos-dotnet-sdk
cd aptos-dotnet-sdk
dotnet restore
dotnet build --configuration Release
dotnet test --project Aptos.Tests
```

End-to-end tests against devnet are gated by an environment variable:

```bash
DEVNET_E2E=1 dotnet test --project Aptos.Tests
```

## Releasing a new version

Releases are triggered by pushing a signed, annotated tag. CI validates the tag, runs the full test suite, packs the NuGet packages, publishes to NuGet.org, and creates a GitHub Release with changelog notes.

### 1. Prepare the release branch

```bash
git checkout main && git pull
git checkout -b release/vX.Y.Z-beta
```

### 2. Bump the version

Edit `Directory.Build.props` — change `DefaultVersion` to the new version number (without the `-beta` suffix; that is appended by `Package.props`):

```xml
<DefaultVersion>X.Y.Z</DefaultVersion>
```

### 3. Finalize the changelog

In `CHANGELOG.md`, rename `## Unreleased` to `## X.Y.Z-beta (YYYY-MM-DD)` and leave no `## Unreleased` section (CI extracts release notes by matching the versioned heading).

### 4. Open and merge a PR

```bash
git add Directory.Build.props CHANGELOG.md
git commit -m "chore: bump version to X.Y.Z-beta and finalize changelog"
git push -u origin release/vX.Y.Z-beta
gh pr create --title "chore: release vX.Y.Z-beta" --body "..."
```

Wait for CI to go green, then merge.

### 5. Tag the merged commit

```bash
git checkout main && git pull
git tag -a vX.Y.Z-beta -m "Release vX.Y.Z-beta"
git push origin vX.Y.Z-beta
```

Pushing the tag triggers `check-and-release.yaml`, which:

1. Asserts the tag matches the package version in `Directory.Build.props` + `Package.props`
2. Asserts the tagged commit is reachable from `main`
3. Runs `dotnet test`
4. Packs and pushes to NuGet (skipped if the version already exists)
5. Creates a GitHub Release populated with the changelog section for that version

### Version scheme

| Segment | When to increment |
|---------|-------------------|
| patch (`Z`) | Bug fixes, dependency bumps, non-breaking improvements |
| minor (`Y`) | New public API surface or breaking changes |
| major (`X`) | Reserved for a stable 1.0 release |

All releases currently carry the `-beta` suffix. Drop it when the API is considered stable.
