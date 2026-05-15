namespace Aptos.Tests.E2E;

using System.Numerics;

/// <summary>
/// Helpers shared by all devnet E2E tests.
/// </summary>
public static class DevnetE2E
{
    /// <summary>
    /// The trait name used to identify E2E tests. Tests carrying this trait
    /// are skipped by default and only run when DEVNET_E2E=1 (or "true") is
    /// set in the environment. They make real HTTP calls against the public
    /// Aptos devnet endpoints.
    /// </summary>
    public const string Category = "Category";

    public const string E2E = "E2E";

    /// <summary>
    /// Set of environment variables that, when set, enable E2E tests. Devs
    /// can opt in locally via `DEVNET_E2E=1 dotnet test`, and CI can flip it
    /// on its main branch only.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            var v = Environment.GetEnvironmentVariable("DEVNET_E2E");
            return !string.IsNullOrEmpty(v) && (v == "1" || v.ToLowerInvariant() == "true");
        }
    }

    public static AptosClient NewClient() => new(new AptosConfig(Networks.Devnet));

    public static async Task FundOrSkip(AptosClient client, AccountAddress address, ulong amount)
    {
        try
        {
            await client.Faucet.FundAccount(address, amount);
        }
        catch (Exception e)
        {
            throw new SkipException(
                $"Devnet faucet failed (this is common for devnet); skipping. Inner: {e.Message}"
            );
        }
    }
}

/// <summary>
/// A SkipException is raised when a test cannot proceed because a precondition
/// outside the SDK's control failed (devnet down, faucet down, etc). The test
/// runner treats it as a skipped result rather than a failure when used in the
/// dynamic `Skip` attribute below.
/// </summary>
public class SkipException(string reason) : Exception(reason);

/// <summary>
/// Marks a test that should only run when the DEVNET_E2E env var is set.
/// </summary>
public sealed class DevnetE2EFactAttribute : FactAttribute
{
    public DevnetE2EFactAttribute()
    {
        if (!DevnetE2E.IsEnabled)
        {
            Skip = "Set DEVNET_E2E=1 to run E2E tests against Aptos devnet.";
        }
    }
}
