namespace Aptos.Tests.LedgerClient;

public class AptosLedgerClientTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact(Timeout = 10000)]
    public async Task GetLedgerInfo()
    {

        var client = new AptosClient(new AptosConfig(Networks.Mainnet));

        var ledgerInfo = await client.Block.GetLedgerInfo();
        Assert.Equal(1, ledgerInfo.ChainId);
        Assert.Equal("full_node", ledgerInfo.NodeRole);
        Assert.Equal((ulong)0, ledgerInfo.OldestblockHeight);
    }
}