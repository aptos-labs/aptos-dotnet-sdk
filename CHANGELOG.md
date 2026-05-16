# Aptos .NET SDK Changelog

## Unreleased

### Breaking

- breaking: `Hex.ToByteArray()` now returns a defensive copy of the underlying bytes on every call. Mutating the returned array no longer mutates the `Hex` instance. Two consecutive calls return distinct array instances.
- breaking: `AccountAddress.Data` is now a property (returning a defensive copy) instead of a public `readonly byte[]` field. Source-compatible for read access (`addr.Data[i]`, `addr.Data.Length`, etc.), but external mutation of the returned array no longer affects the `AccountAddress` instance. This makes `AccountAddress` and `Hex` safe to use as `Dictionary` / `HashSet` keys.

### Features

- feat: `PrivateKey` types (`Ed25519PrivateKey`, `Secp256k1PrivateKey`) now implement `IDisposable`. `Dispose()` zeros the underlying key bytes and subsequent calls to `Sign` / `PublicKey` / `ToByteArray` / `Serialize` throw `ObjectDisposedException`. Use `using var key = Ed25519PrivateKey.Generate();` to scrub keys when they go out of scope.
- feat: `AptosConfig` now accepts an optional `httpTimeout: TimeSpan` parameter. The default per-request HTTP timeout drops from `HttpClient`'s 100s to 30s so interactive apps fail fast on flaky networks. Pass `Timeout.InfiniteTimeSpan` to opt out.
- feat: `AptosRequestClient` exposes a new constructor that accepts a `TimeSpan` HTTP timeout.
- feat: Increase default max gas amount by 10x (200000 -> 2000000).
- feat: `Memoize` cache keys are now scoped by `NetworkConfig.Name`, so devnet / testnet / mainnet clients running in the same process no longer share cached module ABIs.
- feat: Add devnet end-to-end test suite gated by the `DEVNET_E2E=1` environment variable. Covers transfers signed by Ed25519 / SingleKey (ed25519 & secp256k1) / MultiKey (2-of-3), sponsored fee-payer transactions, simulation, view functions, faucet funding, account / module / resource lookups, gas estimation, and chain-id auto-fetch.
- feat: Add Codecov integration. CI uploads cobertura coverage reports on every run; `codecov.yml` sets a 90% project / patch target with `Aptos.Indexer/` excluded as auto-generated.
- feat: Add fuzz tests for the BCS `Deserializer`. 1,000 random inputs per public deserializer entry point, plus targeted DoS regression cases (uleb128 with all continuation bits set, oversized `Bytes()` length prefixes).
- feat: Test count grows from 317 → 560 deterministic offline tests + 18 gated devnet E2E tests. Overall line coverage 50.86% → 94.19%.

### Fixes

- fix: `AccountAddress.GetHashCode()` and `Hex.GetHashCode()` now hash by byte value rather than by reference. Previously two `Equal` instances produced different hash codes, making them unsafe as `Dictionary` / `HashSet` keys.
- fix: `AccountAddress.Equals(object)` no longer throws when given a malformed string; returns `false` instead. Honours the `object.Equals` contract.
- fix: `BCS Deserializer.U16/U32/U64` now read little-endian on every platform regardless of host endianness, matching the BCS specification and the SDK's `Serializer`.
- fix: `TransactionAuthenticator.Deserialize` now dispatches to the `MultiAgent` variant. Previously a serialised multi-agent authenticator could not be round-tripped.
- fix: `RawTransactionWithData.Deserialize` now dispatches to the `MultiAgent` variant. Previously a serialised `MultiAgentRawTransaction` could not be round-tripped through the base dispatcher.
- fix: `TransactionArgument.DeserializeFromScriptArgument` now handles the `Bool` variant. Previously a `Bool` script-function argument could not be round-tripped.
- fix: `MoveVector<U8>.SerializeForScriptFunction` now emits the `U8Vector` variant tag and a single length prefix. Previously the encoding was malformed (variant byte missing, length prefix duplicated).
- fix: `MoveString.SerializeForScriptFunction` now emits the `U8Vector` variant tag.
- fix: `TypeTagReference.Serialize` now writes the inner type tag after the variant byte. Previously only the variant byte was written, dropping the inner tag.
- fix: `Secp256k1PrivateKey.Generate()` and `Secp256k1PrivateKey.Sign()` now zero-pad the BouncyCastle `BigInteger` representation to 32 bytes (and 64 bytes for signatures). Without padding, ~1 in 256 generated keys and ~1 in 128 signatures were rejected with `KeyLengthMismatch` because `BigInteger.ToByteArrayUnsigned()` strips leading zero bytes.
- fix: `TransactionClient.WaitForIndexer` is now `async Task` instead of `async void` so callers can await it and observe its exceptions.
- fix: `AptosRequestClient.Get` now preserves the original exception as the inner exception when wrapping request errors (matches the `Post` implementation).
- fix: `AccountClient.LookupOriginalAccountAddress` and `TransactionBuilder.GenerateRawTransaction` use `throw;` rather than `throw e;` to preserve the original stack trace on rethrow.

### Security

- security: Enable `NuGetAudit` (`mode=all`, `level=low`) in `Directory.Build.props` so `dotnet restore` fails on any known-vulnerable direct or transitive dependency.
- security: Pin patched versions of six previously-flagged transitive dependencies: `System.Text.Json` 8.0.5 (was 8.0.0/8.0.4/6.0.7), `System.Net.Http` 4.3.4 (was 4.3.0), `System.Text.RegularExpressions` 4.3.1 (was 4.3.0). Resolves GHSA-8g4q-xg66-9fp4, GHSA-hh2w-p6rv-4g7w, GHSA-7jgj-8wvc-jh57, GHSA-cmhx-cq75-c4mj.
- security: Add `SECURITY.md` documenting the private vulnerability disclosure flow (GitHub private advisories or `security@aptoslabs.com`), scope, acknowledgement / patch timelines, and supported versions.
- security: Add `SECURITY_REVIEW.md` capturing the results of the audit performed against this release.
- security: Add `.github/dependabot.yml` for weekly NuGet and GitHub Actions updates.

## 0.0.17-beta

- feat: Add support for Orderless transactions
- fix: Fixed address deserialization for addresses without padding

## 0.0.16-beta

- breaking: Remove support for .NET 6 and 7, add support for .NET 9
- breaking: Default `ToString` implementation for `PrivateKey` classes to return AIP-80 compliant string representation of the key.

## 0.0.15-beta
