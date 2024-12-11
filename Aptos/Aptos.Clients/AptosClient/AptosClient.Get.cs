namespace Aptos;

public class GetAptosRequest(
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
    public Task<AptosResponse<Res>> Get<Res>(ApiType type, GetAptosRequest request)
        where Res : class => Request<Res>(type, new(Config.GetRequestUrl(type), request));

    public Task<AptosResponse<Res>> GetFullNode<Res>(GetAptosRequest request)
        where Res : class => Get<Res>(ApiType.FullNode, request);

    public Task<AptosResponse<Res>> GetFaucet<Res>(GetAptosRequest request)
        where Res : class => Get<Res>(ApiType.Faucet, request);

    public async Task<List<Res>> GetFullNodeWithPagination<Res>(GetAptosRequest request)
        where Res : class
    {
        List<Res> temp = [];
        string? cursor;
        var requestParams = request.QueryParams;
        do
        {
            var response = await GetFullNode<List<Res>>(
                new(
                    path: request.Path,
                    body: request.Body,
                    contentType: request.ContentType,
                    originMethod: request.OriginMethod,
                    queryParams: requestParams
                )
            );

            // Get the cursor from the response headers
            cursor = response.Headers.TryGetValue("x-aptos-cursor", out string? value)
                ? value
                : null;

            // Add the results to the list
            temp.AddRange(response.Data);

            // Add the cursor to the request params if it exists
            requestParams ??= [];
            if (cursor != null)
                requestParams["cursor"] = cursor;
        } while (cursor != null);
        return temp;
    }
}
