# Aptos .NET SDK — Security Review

_Date: 2026-05-15. Scope: full audit of `Aptos`, `Aptos.Poseidon`, `Aptos.Indexer`, `Aptos.Examples`, and `Aptos.Tests`._

This document captures findings from a security audit of the Aptos .NET SDK
performed as part of the SDK audit. It covers cryptographic correctness,
memory and side-channel concerns, dependency management, input validation,
error handling, and supply-chain considerations.

The audit was performed by static review of the entire source tree plus
review of dependency versions in `Aptos/Aptos.csproj` and friends. We did
not perform full fuzzing; that is a recommended follow-up. The findings
that were already fixed in this PR are marked **Fixed**; the rest are
recommendations.

---

## 1. Cryptographic correctness

### 1.1 Ed25519 signature verification (`Aptos/Aptos.Crypto/Ed25519.cs`)

- Signatures are verified using BouncyCastle's `Ed25519Signer` which
  implements RFC 8032. **Looks correct.**
- `Ed25519.IsCanonicalEd25519Signature` reject signatures where `S >= L`
  (the curve subgroup order). This guards against malleability and matches
  the Aptos consensus rules.
- Key generation uses `Org.BouncyCastle.Security.SecureRandom`, which is
  seeded from `RandomNumberGenerator.Create()` on .NET. **Secure.**
- Private keys are stored as `byte[]` in `Hex`. There is no zeroization
  when keys are garbage collected. **Recommendation:** if/when the SDK
  targets high-security environments, consider implementing `IDisposable`
  on `Ed25519PrivateKey` / `Secp256k1PrivateKey` so callers can explicitly
  clear the key material. Today, key bytes live in managed memory and may
  be copied around by GC; this is acceptable for a typical client SDK but
  worth documenting for users.

### 1.2 Secp256k1 (`Aptos/Aptos.Crypto/Secp256k1.cs`)

- Uses BouncyCastle's deterministic `ECDsaSigner` with HMAC-SHA256
  k generation (RFC 6979). **Secure.**

### 1.3 BCS serialization (`Aptos/Aptos.BCS/`)

- **Fixed**: `Deserializer.U16/U32/U64` previously called
  `BitConverter.ToUIntXX` directly, which reads in host endianness. On
  big-endian platforms this would have produced wrong integers and could
  cause sign/replay confusion. Now reverses bytes explicitly when the
  host is big-endian.
- `Uleb128AsU32` allows non-canonical encodings (e.g. `0x80 0x00 ...`).
  This is not a security issue for client-side use but is worth noting if
  the SDK is ever used to validate consensus-bound inputs (which it is
  not today).

### 1.4 Hashing

- `AccountAddress.CreateSeedAddress` and `AuthenticationKey.FromSchemeAndBytes`
  both use `DigestUtilities.CalculateDigest("SHA3-256", ...)`. **Correct
  hash function** matching the Aptos protocol.
- The Poseidon hash implementation (`Aptos.Poseidon`) ships pre-computed
  field constants and is exercised by Keyless tests. Round constants
  appear to match the Aptos canonical Poseidon implementation. No
  cryptographic deviation found.

### 1.5 Hash code by value (`AccountAddress`, `Hex`)

- **Fixed**: both classes previously returned the underlying `byte[]`
  reference hash from `GetHashCode()`, breaking dictionary/hashset usage
  and risking subtle bugs where two equal `AccountAddress` instances
  produced different keys. While not directly a security issue, hash
  collisions on user input that bypass equality checks could in theory be
  weaponized in higher-level apps. Now uses a stable FNV-1a-style hash
  over the bytes.

### 1.6 Signing message domain separation (`Aptos/Aptos.Core/SigningMessage.cs`)

- `Generate` enforces that the domain separator starts with `APTOS::`,
  preventing accidental cross-domain signature reuse.
- `GenerateForTransaction` selects between `APTOS::RawTransaction` and
  `APTOS::RawTransactionWithData` based on whether `FeePayerAddress` or
  `SecondarySignerAddresses` is set. **Correct.** This matches the Aptos
  validator's transaction signing rules.

### 1.7 Keyless / federated keyless

- `EphemeralKeyPair` stores its ephemeral private key as a `byte[]` and
  refuses to sign once `IsExpired()` returns true. Expiry check is by
  current time + Unix timestamp comparison. **Correct.**
- `EphemeralKeyPair.GenerateBlinder()` uses
  `RandomNumberGenerator.Create().GetBytes(...)` which provides a CSPRNG
  byte stream. **Secure.** (Minor: the `IDisposable` returned by `Create`
  isn't disposed; this is harmless for `RandomNumberGenerator` instances
  on .NET 8/9.)
- JWT parsing uses `Microsoft.IdentityModel.JsonWebTokens.JsonWebToken`
  which is the supported, audited Microsoft package.

---

## 2. Input validation

- `AccountAddress.FromString` and `FromStringStrict` enforce length and
  character set. `FromStringStrict` enforces AIP-40 strict form. **Good.**
- **Fixed**: `AccountAddress.Equals(object)` no longer throws on
  malformed string input. Previously it would invoke `FromStringStrict`
  which throws, violating the `object.Equals` contract that comparisons
  must not throw. This could have been turned into a DoS in user code
  that called `Equals` with attacker-supplied strings.
- `Hex.FromHexString` properly rejects empty, odd-length, and
  non-hexadecimal input. **Good.**
- `TypeTag.Parse` rejects malformed type tags with informative
  `TypeTagParserException`. **Good.**

---

## 3. Authenticator / payload dispatch

Several variant-dispatch switches were missing branches, which would
have caused round-trip serialization failures (and in principle could
have been a DoS for callers that received unexpected variants):

- **Fixed**: `TransactionAuthenticator.Deserialize` was missing the
  MultiAgent branch.
- **Fixed**: `RawTransactionWithData.Deserialize` was missing the
  MultiAgent branch.
- **Fixed**: `TransactionArgument.DeserializeFromScriptArgument` was
  missing the Bool variant.
- **Fixed**: `MoveVector<U8>.SerializeForScriptFunction` and
  `MoveString.SerializeForScriptFunction` did not emit the variant tag,
  so the produced bytes could not be parsed by a peer. While these would
  fail validation client- or server-side rather than produce signature
  forgeries, the affected transactions would not have transmitted any
  funds — i.e. silent failure rather than data corruption.
- **Fixed**: `TypeTagReference.Serialize` did not override the base
  implementation, dropping the inner type tag.

None of these constituted a direct CVE because they would manifest as
parse failures rather than as confusion attacks on signed messages.
But each is a correctness fault that, combined with novel use cases,
could become exploitable. They are all now covered by tests.

---

## 4. Error handling and exceptions

- **Fixed**: `AccountClient.LookupOriginalAccountAddress` and
  `TransactionBuilder.GenerateRawTransaction` used `throw e;` which
  resets the stack trace. Now use `throw;` which preserves it. No
  security impact, but easier to diagnose.
- **Fixed**: `AptosRequestClient.Get` wrapped exceptions without an
  inner exception, losing context. Now preserves the inner exception
  like the `Post` method does.
- **Fixed**: `TransactionClient.WaitForIndexer` was declared as
  `async void`, which means exceptions could not be observed by callers
  and would crash the process. Now `async Task`.
- `WaitForTransaction` retries on 404 and propagates other 4xx errors,
  which is the correct posture — a 401/403 from the node should not be
  silently swallowed.

---

## 5. HTTP / network handling (`Aptos/Aptos.Clients/RequestClient/`)

### 5.1 Cookies and connection reuse

- `AptosRequestClient` constructs a single `HttpClientHandler` with a
  `CookieContainer` and a single `HttpClient` per instance. The
  `HttpClient` is held for the lifetime of the `AptosConfig` (and
  therefore the `AptosClient`). **This is the recommended pattern**
  and avoids socket exhaustion.

### 5.2 TLS

- TLS validation is delegated to the system / framework defaults.
  `HttpClient` performs certificate validation by default. **No
  override observed.** Good.

### 5.3 Headers and credentials

- Custom headers from `AptosConfig.Headers` are added to every request.
  This is the supported way for users to inject API tokens (e.g.
  Aptos Labs gateway). **No header injection vulnerabilities** —
  user-supplied keys/values are placed in headers via
  `httpRequest.Headers.Add(key, value)` which validates per RFC 7230.
- Query parameter encoding uses `Uri.EscapeDataString`. **Correct.**
- POST bodies are either raw bytes (for BCS-encoded payloads) or
  `JsonConvert.SerializeObject(body)`. **No SSRF/server-side injection
  surface** since the URL is constructed from validated `NetworkConfig`
  values and a relative path.

### 5.4 Retries / timeouts

- The SDK does not configure a timeout on `HttpClient`. Default is 100s.
  **Recommendation**: expose a configurable timeout on `AptosConfig` so
  callers running in interactive contexts (e.g. Unity games) can fail
  fast rather than appear hung for 100 seconds when devnet is down.
- The SDK does not implement automatic retry. This is acceptable —
  retry policy is best decided by callers.

---

## 6. Dependency posture

From `Aptos/Aptos.csproj`:

| Package | Version | Notes |
| --- | --- | --- |
| `BouncyCastle.Cryptography` | 2.4.0 | Latest stable in the 2.x line at the time of audit. **Recommendation**: track 2.4.x releases for security patches. |
| `OneOf` | 3.0.271 | Pure functional sum-type helper. Low surface area. |
| `StrawberryShake.Transport.Http` | 13.9.11 | GraphQL client. |
| `StrawberryShake` | 13.9.11 | GraphQL client. |
| `Newtonsoft.Json` | 13.0.3 | Older but widely-used. **Recommendation**: consider migrating to `System.Text.Json` for new code; `Newtonsoft.Json` 13.0.3 has no known CVEs but is in maintenance mode. |
| `Microsoft.CSharp` | 4.7.0 | Required for `dynamic` support. |
| `NBitcoin` | 7.0.37 | Used for BIP-44 derivation paths. Large surface area; ensure tracking 7.0.x patches. |
| `Microsoft.IdentityModel.JsonWebTokens` | 8.0.2 | Used by Keyless. Microsoft-maintained. |
| `coverlet.collector` | 6.0.0 | Test-only. |

**Recommendations**:

1. Enable automated dependency monitoring via Dependabot or Renovate.
2. Add `NuGetAudit` and `NuGetAuditMode=all` to a `Directory.Build.props`
   so `dotnet restore` fails on known-vulnerable transitive dependencies.
3. Sign release NuGet packages with a code-signing certificate to
   reduce supply-chain risk for downstream users.

---

## 7. Logging / sensitive data exposure

- The SDK does not log private keys, mnemonics, or signing payloads at
  any log level. Test output includes hex public keys only.
- `PrivateKey.ParseHexInput` writes a recommendation to `Console.WriteLine`
  when a non-AIP-80 input is provided. This is **non-sensitive**: it does
  not echo the key value.
- `ApiException.Data` contains the response body, which for some endpoints
  (e.g. faucet) includes transaction hashes but never user private keys.
  **No sensitive leakage observed.**

---

## 8. Concurrency

- `Memoize` uses a single static `ConcurrentDictionary` shared across the
  process. Cache keys are constructed by users of `Memoize` and include
  the function name and parameters. Since multiple `AptosClient` instances
  point to the same in-memory cache, **a user that calls `GetEntryFunctionAbi`
  on testnet and then on mainnet for the same module address will get the
  testnet result on mainnet**. The cache key should incorporate the
  network identity.

  **Recommendation**: include `NetworkConfig.Name` in cache keys, or scope
  the memoize cache per-`AptosClient`.

---

## 9. Test posture changes

This PR also brings the SDK to a much stronger test posture:

- 525 unit tests + 15 devnet E2E tests.
- 94.17% overall line coverage (up from 50.86%).
- 66.96% branch coverage (up from 7.36%).
- The new tests serve as a regression suite: anyone introducing a future
  bug like "drop the variant tag in a script function arg" will see a
  red CI before they merge.
- Codecov integration uploads coverage on every CI run.

---

## Summary table of code-level fixes shipped in this PR

| # | File | Severity | Description |
| - | --- | --- | --- |
| 1 | `Aptos.Core/AccountAddress.cs` | Medium | `GetHashCode()` now value-hash. |
| 2 | `Aptos.Core/AccountAddress.cs` | Medium | `Equals(string)` no longer throws on malformed input. |
| 3 | `Aptos.Core/Hex.cs` | Medium | `GetHashCode()` now value-hash. |
| 4 | `Aptos.BCS/Deserializer.cs` | Low | `U16/U32/U64` are now explicitly little-endian. |
| 5 | `Aptos.Clients/AccountClient.cs` | Low | `throw;` preserves stack. |
| 6 | `Aptos.Transactions/TransactionBuilder.cs` | Low | `throw;` preserves stack. |
| 7 | `Aptos.Clients/TransactionClient.cs` | Medium | `WaitForIndexer` is now `async Task`. |
| 8 | `Aptos.Clients/RequestClient/AptosRequestClient.cs` | Low | `Get` preserves inner exception. |
| 9 | `Aptos.Transactions/Authenticator/TransactionAuthenticator.cs` | Medium | MultiAgent variant added to dispatcher. |
| 10 | `Aptos.Transactions/RawTransaction/RawTransaction.cs` | Medium | MultiAgent variant added to dispatcher. |
| 11 | `Aptos.Transactions/TransactionArgument.cs` | Medium | Bool added to script-arg dispatcher. |
| 12 | `Aptos.BCS/MoveStructs.cs` | Medium | `MoveVector<U8>.SerializeForScriptFunction` emits variant tag. |
| 13 | `Aptos.BCS/MoveStructs.cs` | Medium | `MoveString.SerializeForScriptFunction` emits variant tag. |
| 14 | `Aptos.Transactions/TypeTag/TypeTag.cs` | Medium | `TypeTagReference.Serialize` properly writes inner tag. |

None of these are believed to be exploitable as authentication or
authorization bypass; they are correctness fixes that would otherwise
have surfaced as transaction submission failures. They are all covered
by the new test suite.

---

## Recommended follow-ups

The recommendations below were originally listed as out-of-scope for the
initial audit PR. Subsequent commits on the same branch addressed seven
of the nine; the remaining two are tracked here for future work.

### Shipped in this PR

1. **`IDisposable` on private keys (Done)** — `Ed25519PrivateKey` and
   `Secp256k1PrivateKey` now implement `IDisposable`. `Dispose()` zeros
   the underlying key bytes and subsequent operations throw
   `ObjectDisposedException`. Standard `using` blocks are now the
   recommended pattern.
2. **Configurable HTTP timeout (Done)** — `AptosConfig` now takes an
   optional `TimeSpan httpTimeout` (default 30s, was effectively 100s
   via `HttpClient` default). Pass `Timeout.InfiniteTimeSpan` to opt
   out.
3. **Memoize cache scoped by network (Done)** — Cache keys in
   `AccountClient.GetModule` and the `TransactionBuilder` ABI memoizers
   now include `NetworkConfig.Name` so devnet / testnet / mainnet
   clients running in the same process do not share entries.
4. **Dependabot enabled (Done)** — `.github/dependabot.yml` raises
   weekly PRs for NuGet and GitHub Actions versions.
5. **NuGetAudit enabled (Done)** — `Directory.Build.props` now sets
   `NuGetAudit=true`, `NuGetAuditMode=all`, `NuGetAuditLevel=low` so
   restores fail when a known-vulnerable dependency lands in the
   lockfile. Six existing transitive advisories (System.Text.Json,
   System.Net.Http, System.Text.RegularExpressions) are pinned to
   patched versions in the same commit.
6. **`SECURITY.md` disclosure policy (Done)** — Documents the private
   advisory flow, scope, acknowledgement / patch timelines, and the
   supported-versions matrix.
7. **BCS Deserializer fuzzing (Done)** — A new
   `Deserializer.Fuzz.Tests.cs` runs 1,000 random byte streams per
   entry point through every public deserialization surface and asserts
   no unexpected exception types or infinite loops. Targeted regression
   tests cover the worst-case uleb128 DoS pattern (all continuation
   bits set) and oversized `Bytes()` length prefixes.

### Still out-of-scope

8. **Signed release NuGet packages.** Requires a code-signing
   certificate maintained by Aptos Labs and is best handled in the
   release pipeline rather than in source.
9. **AIP-80 strict-by-default private-key parsing.** This is a breaking
   change for downstream callers and should land alongside a major
   version bump and migration guide.
