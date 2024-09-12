using Aptos.Exceptions;

namespace Aptos;

/// <summary>
/// Instantiate a new instance of the AptosConfig class. This class is used to configure the AptosClient, its dependencies, and endpoints. See the <see cref="Networks"/> class
/// for a predefined list of networks. 
/// </summary>
/// <param name="networkConfig">The endpoints and chain ID for the network. If none are provided, Devnet is used.</param>
/// <param name="requestClient">The request client used to make HTTP requests. If none is provided, a default client is used.</param>
public class AptosConfig(NetworkConfig? networkConfig = null, RequestClient? requestClient = null)
{
    public readonly NetworkConfig NetworkConfig = networkConfig ?? Networks.Devnet;

    public readonly RequestClient RequestClient = requestClient ?? new AptosRequestClient();

    public string GetRequestUrl(ApiType apiType)
    {
        string? url = apiType switch
        {
            ApiType.FullNode => NetworkConfig.NodeUrl,
            ApiType.Indexer => NetworkConfig.IndexerUrl,
            ApiType.Faucet => NetworkConfig.FaucetUrl,
            ApiType.Prover => NetworkConfig.ProverUrl,
            ApiType.Pepper => NetworkConfig.PepperUrl,
            _ => throw new ConfigException($"Invalid API type {apiType}"),
        };

        if (url == null) throw new ConfigException($"No endpoint found for API type {apiType} on the current network {NetworkConfig.Name}");

        return url;
    }
}