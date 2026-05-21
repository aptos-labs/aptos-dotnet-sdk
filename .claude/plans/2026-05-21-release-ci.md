# Release CI Revamp Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the push-to-main release workflow with a tag-triggered one that fixes the NuGet version-check bug and creates a GitHub Release with notes from `CHANGELOG.md`.

**Architecture:** Single file replacement — `.github/workflows/check-and-release.yaml`. Two jobs: `validate` (extracts version, asserts tag matches, guards against re-publishing) and `release` (tests, packs, publishes to NuGet, creates GitHub Release with awk-extracted changelog notes). `run-checks.yaml` is untouched.

**Tech Stack:** GitHub Actions, `dotnet` CLI, `xmllint`, `jq`, `curl`, `awk`, `gh` CLI (pre-installed on `ubuntu-latest`)

---

### Task 1: Replace `check-and-release.yaml`

**Files:**
- Modify: `.github/workflows/check-and-release.yaml`

- [ ] **Step 1: Create a feature branch**

```bash
git checkout -b ci/tag-triggered-releases
```

Expected: switched to new branch `ci/tag-triggered-releases`

- [ ] **Step 2: Verify the fixed NuGet check command works locally**

Run these two commands to confirm `jq contains` gives the right answer before embedding it in CI:

```bash
# Should output "false" — definitely not published
PKG_VERSION="99.99.99-beta"
curl -s "https://api.nuget.org/v3-flatcontainer/aptos/index.json" \
  | jq -r --arg v "$PKG_VERSION" '.versions | contains([$v])'

# Should output "true" — pick the first version in the list
PKG_VERSION=$(curl -s "https://api.nuget.org/v3-flatcontainer/aptos/index.json" | jq -r '.versions | first')
echo "Testing with: $PKG_VERSION"
curl -s "https://api.nuget.org/v3-flatcontainer/aptos/index.json" \
  | jq -r --arg v "$PKG_VERSION" '.versions | contains([$v])'
```

Expected output: `false` then `true`

- [ ] **Step 3: Verify the changelog awk extraction works locally**

```bash
awk '/^## Unreleased/{found=1; next} found && /^## /{exit} found{print}' CHANGELOG.md
```

Expected: the text content under `## Unreleased` (breaking changes, features, fixes, security). If output is empty, check that `CHANGELOG.md` has a line starting exactly with `## Unreleased` (no trailing spaces or different casing).

- [ ] **Step 4: Write the new workflow file**

Replace `.github/workflows/check-and-release.yaml` entirely:

```yaml
name: "Check and Release NuGet Packages"

on:
  push:
    tags: ['v*']

permissions:
  contents: read

jobs:
  validate:
    name: Validate Version and Check NuGet
    runs-on: ubuntu-latest
    permissions:
      contents: read
    outputs:
      version: ${{ steps.version.outputs.version }}
      is_published: ${{ steps.nuget_exists.outputs.exists }}
    steps:
      - name: Checkout Code
        uses: actions/checkout@34e114876b0b11c390a56381ad16ebd13914f8d5 # v4
        with:
          persist-credentials: false

      - name: Setup .NET
        uses: ./.github/actions/setup-dotnet

      - name: Install xmllint
        run: sudo apt-get install -y libxml2-utils
        shell: bash

      - name: Extract Version
        id: version
        run: |
          DEFAULT_VERSION=$(xmllint --xpath "string(//*[local-name()='DefaultVersion'])" Directory.Build.props)
          VERSION_TEMPLATE=$(xmllint --xpath "string(//*[local-name()='PackageVersion'])" Package.props)
          VERSION=${VERSION_TEMPLATE/'$(DefaultVersion)'/$DEFAULT_VERSION}
          echo "version=$VERSION" >> $GITHUB_OUTPUT
        shell: bash

      - name: Assert tag matches package version
        env:
          PKG_VERSION: ${{ steps.version.outputs.version }}
        run: |
          EXPECTED="v${PKG_VERSION}"
          ACTUAL="${{ github.ref_name }}"
          if [ "$ACTUAL" != "$EXPECTED" ]; then
            echo "ERROR: tag '${ACTUAL}' does not match expected tag '${EXPECTED}'"
            exit 1
          fi
          echo "Tag matches package version: ${PKG_VERSION}"
        shell: bash

      - name: Check If NuGet Package Exists
        id: nuget_exists
        shell: bash
        env:
          PKG_VERSION: ${{ steps.version.outputs.version }}
        run: |
          EXISTS=$(curl -s "https://api.nuget.org/v3-flatcontainer/aptos/index.json" \
            | jq -r --arg v "$PKG_VERSION" '.versions | contains([$v])')
          if [ "$EXISTS" = "true" ]; then
            echo "Package already published: $PKG_VERSION"
            echo "exists=true" >> $GITHUB_OUTPUT
          else
            echo "Package not yet published: $PKG_VERSION"
            echo "exists=false" >> $GITHUB_OUTPUT
          fi

  release:
    name: Pack and Publish Packages
    needs: validate
    runs-on: ubuntu-latest
    permissions:
      contents: write
    if: ${{ needs.validate.outputs.is_published == 'false' }}
    steps:
      - name: Checkout Code
        uses: actions/checkout@34e114876b0b11c390a56381ad16ebd13914f8d5 # v4

      - name: Setup .NET
        uses: ./.github/actions/setup-dotnet

      - name: Run Tests
        run: dotnet test --no-restore --verbosity normal
        working-directory: ./Aptos.Tests
        shell: bash

      - name: Pack Packages
        run: dotnet pack --configuration Release --no-build --output ./artifacts
        shell: bash

      - name: Publish to NuGet
        run: |
          dotnet nuget push ./artifacts/*.nupkg \
            --source https://api.nuget.org/v3/index.json \
            --api-key ${{ secrets.NUGET_API_KEY }}
        shell: bash

      - name: Extract Changelog Notes
        run: |
          awk '/^## Unreleased/{found=1; next} found && /^## /{exit} found{print}' \
            CHANGELOG.md > /tmp/release-notes.md
          echo "--- Release notes preview ---"
          cat /tmp/release-notes.md
        shell: bash

      - name: Create GitHub Release
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          PKG_VERSION: ${{ needs.validate.outputs.version }}
        run: |
          gh release create "v${PKG_VERSION}" \
            --title "v${PKG_VERSION}" \
            --notes-file /tmp/release-notes.md \
            ./artifacts/*.nupkg
        shell: bash
```

- [ ] **Step 5: Validate YAML syntax**

```bash
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/check-and-release.yaml'))" && echo "YAML valid"
```

Expected: `YAML valid`

- [ ] **Step 6: Commit**

```bash
git add .github/workflows/check-and-release.yaml
git commit -m "ci: revamp release workflow — tag trigger, fixed NuGet check, GitHub Release"
```

---

### Task 2: Validate with actionlint

**Files:**
- No changes expected (fix any issues actionlint reports)

- [ ] **Step 1: Install actionlint if not present**

```bash
which actionlint || brew install actionlint
```

Expected: path to actionlint binary, or a successful brew install

- [ ] **Step 2: Run actionlint on the new workflow**

```bash
actionlint .github/workflows/check-and-release.yaml
```

Expected: no output (a clean run produces no output). If there are errors, fix them in the workflow file and re-run until clean.

- [ ] **Step 3: Commit any fixes** (only if actionlint required changes)

```bash
git add .github/workflows/check-and-release.yaml
git commit -m "ci: fix actionlint warnings in release workflow"
```

---

### Developer release instructions (update README or CONTRIBUTING if one exists)

After the CI is merged, a release is performed by:

```bash
# 1. Bump DefaultVersion in Directory.Build.props, e.g. 0.0.17 → 0.0.18
# 2. Update ## Unreleased in CHANGELOG.md with notes for this release
# 3. Merge to main
# 4. Tag and push (tag must match the full NuGet PackageVersion):
git tag v0.0.18-beta
git push origin v0.0.18-beta
# CI validates, tests, publishes to NuGet, and creates the GitHub Release automatically.
```
