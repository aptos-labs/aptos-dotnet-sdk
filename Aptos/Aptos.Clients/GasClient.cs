namespace Aptos;

public class GasClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <summary>
    /// Gets the gas price estimation for the current network.
    /// </summary>
    /// <returns>The gas price estimation for the current network.</returns>
    public async Task<GasEstimation> GetGasPriceEstimation()
    {
        AptosResponse<GasEstimation> response = await _client.GetFullNode<GasEstimation>(
            new(path: "estimate_gas_price", originMethod: "getGasPriceEstimation")
        );
        return response.Data;
    }
}
