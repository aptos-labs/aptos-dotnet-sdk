namespace Aptos.Tests.TransactionClient;

using Aptos.Tests.E2E;

public class AptosTransactionClientTests(ITestOutputHelper output) : BaseTests(output)
{
    // Pre-existing network-bound test; gated behind DEVNET_E2E so the unit
    // test suite remains deterministic.
    [DevnetE2EFact(Timeout = 10000)]
    public async Task GetTransactionByVersion_Ed25519Signature_NoExceptions()
    {
        var aptosClient = new AptosClient(new AptosConfig(Networks.Testnet));
        var txn = await aptosClient.Transaction.GetTransactionByVersion("6694134266");
        Assert.NotNull(txn);
    }
}
