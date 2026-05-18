using Aptos.Exceptions;

namespace Aptos;

/// <summary>
/// Instantiate a new instance of the AptosConfig class. This class is used to configure the AptosClient, its dependencies, and endpoints. See the <see cref="Networks"/> class
/// for a predefined list of networks.
/// </summary>
/// <param name="networkConfig">The endpoints and chain ID for the network. If none are provided, Devnet is used.</param>
/// <param name="headers">Default headers to be added to all requests.</param>
/// <param name="requestClient">The request client used to make HTTP requests. If none is provided, a default client is used. The default client honours <paramref name="httpTimeout"/>.</param>
/// <param name="httpTimeout">
/// The per-request timeout for the default HTTP client. Defaults to 30 seconds.
/// Pass <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable.
/// Ignored if a custom <paramref name="requestClient"/> is provided.
/// </param>
public class AptosConfig(
    NetworkConfig? networkConfig = null,
    Dictionary<string, string>? headers = null,
    RequestClient? requestClient = null,
    TimeSpan? httpTimeout = null
)
{
    /// <summary>
    /// The default per-request HTTP timeout used by the <see cref="AptosRequestClient"/>.
    /// 30 seconds is short enough for interactive apps (Unity/Godot games)
    /// to fail fast on flaky networks rather than appear hung for the
    /// 100-second default <see cref="HttpClient"/> timeout.
    /// </summary>
    public static readonly TimeSpan DefaultHttpTimeout = TimeSpan.FromSeconds(30);

    public readonly NetworkConfig NetworkConfig = networkConfig ?? Networks.Devnet;

    public readonly TimeSpan HttpTimeout = httpTimeout ?? DefaultHttpTimeout;

    public readonly RequestClient RequestClient =
        requestClient ?? new AptosRequestClient(httpTimeout ?? DefaultHttpTimeout);

    public readonly Dictionary<string, string> Headers = headers ?? [];

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

        if (url == null)
            throw new ConfigException(
                $"No endpoint found for API type {apiType} on the current network {NetworkConfig.Name}"
            );

        return url;
    }
}
