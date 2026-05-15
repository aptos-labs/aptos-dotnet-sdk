# Security Policy

This document covers how to report security issues for the
[`aptos-labs/aptos-dotnet-sdk`](https://github.com/aptos-labs/aptos-dotnet-sdk)
repository. See [`SECURITY_REVIEW.md`](./SECURITY_REVIEW.md) for the
results of the most recent internal audit.

## Reporting a Vulnerability

**Please do not file public GitHub issues for security vulnerabilities.**
Public reports tip off attackers before downstream users (Unity / Godot
games, server-side .NET integrations, mobile apps) have a chance to
patch.

To report a vulnerability:

1. Open a private security advisory through GitHub's reporting flow:
   [`Report a vulnerability`](https://github.com/aptos-labs/aptos-dotnet-sdk/security/advisories/new).
   This creates a private channel between you and the Aptos Labs
   maintainers and is the preferred reporting mechanism.
2. If you cannot use GitHub's private advisory flow, email
   **security@aptoslabs.com**. Encrypt the report with the Aptos Labs
   security PGP key if it contains exploit details — see
   [aptoslabs.com/security](https://aptoslabs.com/security).

When you report, please include as much of the following as you can:

- A clear description of the issue and the affected SDK component
  (file path, function or class name, version).
- The shortest reproduction you can produce, ideally a self-contained
  test or script.
- The impact you believe it has (signing forgery, key disclosure,
  denial of service, denial of submission, etc.).
- Any suggested fix or mitigation.

We will acknowledge receipt within **2 business days**, agree on a
target patch timeline within **5 business days**, and aim to ship a
fix and coordinated disclosure within **90 days** of the original
report. We'll keep you informed throughout.

## What is in scope

In scope:

- The `Aptos`, `Aptos.Indexer`, and `Aptos.Poseidon` packages.
- The `Aptos.Examples` reference application, where the example code
  itself implements a security-sensitive flow.
- Build / CI configuration that could leak secrets or compromise the
  release pipeline (`.github/workflows`, `Directory.Build.props`,
  `Package.props`, `codecov.yml`, etc.).

Out of scope:

- Issues in services that the SDK consumes (the Aptos full nodes, the
  faucet, the indexer GraphQL gateway) — report those to the relevant
  Aptos repository.
- Issues in third-party dependencies, unless the SDK is using the
  dependency in an unsafe way. (For known transitive advisories,
  Dependabot will raise PRs and NuGetAudit will fail the build.)
- Performance or readability concerns that don't have a security
  impact — please open a regular GitHub issue or pull request for
  those.

## Supported versions

We patch security issues in the latest minor of the `Aptos` NuGet
package. Older versions are not patched; downstream users are
encouraged to track the latest release.

| Version | Supported |
| --- | --- |
| Latest minor (`Aptos.0.0.x`) | Yes |
| Older versions | No |

## Disclosure process

1. Reporter submits a private advisory.
2. Maintainers triage and acknowledge.
3. Maintainers prepare a fix on a private branch / fork.
4. CVE is requested where appropriate.
5. Fix is released. Reporter is credited in the release notes unless
   they opt out.
6. Advisory is made public.

Thank you for helping keep the Aptos .NET SDK and its users safe.
