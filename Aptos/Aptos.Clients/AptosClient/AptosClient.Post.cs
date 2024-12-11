namespace Aptos;

public class PostAptosRequest(
    string? path = null,
    object? body = null,
    string? contentType = null,
    Dictionary<string, string>? queryParams = null,
    string? originMethod = null
) : BaseAptosRequest
{
    public override string? Path { get; set; } = path;
    public override object? Body { get; set; } = body;
    public override string? ContentType { get; set; } = contentType;
    public override Dictionary<string, string>? QueryParams { get; set; } = queryParams;
    public override string? OriginMethod { get; set; } = originMethod;
}

public partial class AptosClient
{
    public Task<AptosResponse<Res>> Post<Res>(ApiType type, PostAptosRequest request)
        where Res : class => Request<Res>(type, new(Config.GetRequestUrl(type), request));

    public Task<AptosResponse<Res>> PostFullNode<Res>(PostAptosRequest request)
        where Res : class => Post<Res>(ApiType.FullNode, request);

    public Task<AptosResponse<Res>> PostIndexer<Res>(PostAptosRequest request)
        where Res : class => Post<Res>(ApiType.Indexer, request);

    public Task<AptosResponse<Res>> PostFaucet<Res>(PostAptosRequest request)
        where Res : class => Post<Res>(ApiType.Faucet, request);

    public Task<AptosResponse<Res>> PostProver<Res>(PostAptosRequest request)
        where Res : class => Post<Res>(ApiType.Prover, request);

    public Task<AptosResponse<Res>> PostPepper<Res>(PostAptosRequest request)
        where Res : class => Post<Res>(ApiType.Pepper, request);
}
