namespace Aptos.Tests.LedgerClient;

using Aptos.Tests.E2E;

public class AptosLedgerClientTests(ITestOutputHelper output) : BaseTests(output)
{
    // Pre-existing network-bound test; gated behind DEVNET_E2E so the unit
    // test suite remains deterministic.
    [DevnetE2EFact(Timeout = 10000)]
    public async Task GetLedgerInfo()
    {
        var client = new AptosClient(new AptosConfig(Networks.Mainnet));

        var ledgerInfo = await client.Block.GetLedgerInfo();
        Assert.Equal(1, ledgerInfo.ChainId);
        Assert.Equal("full_node", ledgerInfo.NodeRole);
        Assert.Equal((ulong)0, ledgerInfo.OldestblockHeight);
    }
}
