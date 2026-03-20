namespace Aptos.Tests.TransactionClient;

public class AptosTransactionClientTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact(Timeout = 10000)]
    public async Task GetTransactionByVersion_Ed25519Signature_NoExceptions()
    {
        var aptosClient = new AptosClient(new AptosConfig(Networks.Testnet));
        var txn = await aptosClient.Transaction.GetTransactionByVersion("6694134266");
        Assert.NotNull(txn);
    }
}
