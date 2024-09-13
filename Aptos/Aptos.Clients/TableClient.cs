
namespace Aptos;

public class TableClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    public async Task<T> GetItem<T>(string handle, (dynamic keyType, dynamic valueType, dynamic key) request, ulong? ledgerVersion = null) where T : class
    {
        Dictionary<string, string> queryParams = [];
        if (ledgerVersion != null) { queryParams.Add("ledger_version", ledgerVersion.ToString()!); }

        var response = await _client.PostFullNode<T>(new(
            path: $"tables/{handle}/item",
            body: new
            {
                key_type = request.keyType,
                value_type = request.valueType,
                key = request.key
            },
            queryParams: queryParams,
            originMethod: "getTableItem"
        ));

        return response.Data;
    }

}