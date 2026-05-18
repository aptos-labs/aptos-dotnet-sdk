namespace Aptos.Tests.NetworksTests;

using Aptos.Exceptions;

public class HttpTimeoutTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void AptosConfig_DefaultHttpTimeout_Is30Seconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(30), AptosConfig.DefaultHttpTimeout);
    }

    [Fact]
    public void AptosConfig_DefaultsToDefaultHttpTimeout()
    {
        var config = new AptosConfig();
        Assert.Equal(AptosConfig.DefaultHttpTimeout, config.HttpTimeout);
    }

    [Fact]
    public void AptosConfig_AcceptsCustomTimeout()
    {
        var custom = TimeSpan.FromSeconds(5);
        var config = new AptosConfig(httpTimeout: custom);
        Assert.Equal(custom, config.HttpTimeout);
    }

    [Fact]
    public void AptosConfig_AcceptsInfiniteTimeout()
    {
        var config = new AptosConfig(httpTimeout: System.Threading.Timeout.InfiniteTimeSpan);
        Assert.Equal(System.Threading.Timeout.InfiniteTimeSpan, config.HttpTimeout);
    }

    [Fact]
    public void AptosConfig_CustomRequestClient_IgnoresTimeoutArg()
    {
        // If a custom request client is supplied, the timeout argument is
        // not used (caller is in charge of their own client).
        var custom = new AptosRequestClient(TimeSpan.FromSeconds(99));
        var config = new AptosConfig(requestClient: custom, httpTimeout: TimeSpan.FromSeconds(1));
        Assert.Same(custom, config.RequestClient);
    }

    [Fact]
    public async Task AptosClient_ShortTimeout_FailsFastOnUnreachableHost()
    {
        // Point at a non-routable address to guarantee the request never
        // completes, then verify the SDK errors out around the timeout
        // window rather than blocking for 100s+.
        var unreachable = new NetworkConfig(
            "unreachable",
            // 192.0.2.0/24 is reserved (RFC 5737) — packets are black-holed.
            "https://192.0.2.1/v1",
            "https://192.0.2.1/v1/graphql",
            null,
            null,
            null,
            42
        );
        var config = new AptosConfig(unreachable, httpTimeout: TimeSpan.FromMilliseconds(500));
        var client = new AptosClient(config);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<Exception>(() => client.Block.GetLedgerInfo());
        stopwatch.Stop();

        // 500ms timeout + some slack. The previous default would have
        // taken ~100 seconds.
        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(15),
            $"Expected failure within ~500ms timeout, but waited {stopwatch.Elapsed.TotalSeconds:F1}s"
        );
    }
}
