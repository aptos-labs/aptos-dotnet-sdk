namespace Aptos;

/// <summary>
/// The network configuration for an AptosConfig for all endpoints used by the AptosClient.
/// </summary>
/// <param name="name">The name of the network.</param>
/// <param name="nodeUrl">The endpoint for the full node.</param>
/// <param name="indexerUrl">The endpoint for the indexer.</param>
/// <param name="faucetUrl">The endpoint for the faucet.</param>
/// <param name="proverUrl"></param>
/// <param name="pepperUrl"></param>
/// <param name="chainId">The chain ID for the network. If -1, the chain ID is automatically retrieved from the full node.</param>
public class NetworkConfig(
    string name,
    string nodeUrl,
    string indexerUrl,
    string? faucetUrl = null,
    string? proverUrl = null,
    string? pepperUrl = null,
    int chainId = -1
)
{
    public readonly string Name = name;
    public readonly int ChainId = chainId;
    public readonly string NodeUrl = nodeUrl;
    public readonly string IndexerUrl = indexerUrl;
    public readonly string? FaucetUrl = faucetUrl;
    public readonly string? ProverUrl = proverUrl;
    public readonly string? PepperUrl = pepperUrl;
}
