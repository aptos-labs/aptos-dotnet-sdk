namespace Aptos;

public class BlockClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <summary>
    /// Gets the ledger information of the blockchain.
    /// </summary>
    /// <returns>The ledger information of the blockchain.</returns>
    public async Task<LedgerInfo> GetLedgerInfo()
    {
        AptosResponse<LedgerInfo> response = await _client.GetFullNode<LedgerInfo>(new(originMethod: "getLedgerInfo"));
        return response.Data;
    }

}