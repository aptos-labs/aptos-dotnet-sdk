namespace Aptos.Tests.LedgerClient;

public class AptosAccountClientTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact(Timeout = 10000)]
    public async Task GetCoinBalance()
    {
        var client = new AptosClient(new AptosConfig(Networks.Mainnet));
        Assert.True(
            (
                await client.Account.GetCoinBalance(
                    "0xa35066a9238fc9d2f4f53e2ec6a0aba4ff949ba7eaa43b748ad9f0fb60add3c2",
                    "0x1::aptos_coin::AptosCoin"
                )
            )?.Amount > 0
        );
        Assert.True(
            (
                await client.Account.GetCoinBalance(
                    "0xa35066a9238fc9d2f4f53e2ec6a0aba4ff949ba7eaa43b748ad9f0fb60add3c2",
                    "0xa"
                )
            )?.Amount > 0
        );
        Assert.True(
            (
                await client.Account.GetCoinBalance(
                    "0x66cb05df2d855fbae92cdb2dfac9a0b29c969a03998fa817735d27391b52b189",
                    "0xa"
                )
            )?.Amount > 0
        );
        Assert.True(
            (
                await client.Account.GetCoinBalance(
                    "0x66cb05df2d855fbae92cdb2dfac9a0b29c969a03998fa817735d27391b52b189",
                    "0x1::aptos_coin::AptosCoin"
                )
            )?.Amount > 0
        );
        Assert.Null(
            await client.Account.GetCoinBalance(
                "0x66cb05df2d855fbae92cdb2dfac9a0b29c969a03998fa817735d27391b52b189",
                "0x123a"
            )
        );
    }
}
