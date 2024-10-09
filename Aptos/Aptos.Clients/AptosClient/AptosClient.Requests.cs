namespace Aptos;

using System.Reflection;
using Aptos.Exceptions;
using Newtonsoft.Json.Linq;

public abstract class BaseAptosRequest
{
    public abstract string? Path { get; set; }

    public abstract dynamic? Body { get; set; }

    public abstract string? ContentType { get; set; }

    public abstract Dictionary<string, string>? QueryParams { get; set; }

    public abstract string? OriginMethod { get; set; }
}

public class AptosRequest(
    string url,
    HttpMethod method,
    string? path = null,
    dynamic? body = null,
    string? contentType = null,
    Dictionary<string, string>? queryParams = null,
    string? originMethod = null
) : BaseAptosRequest
{
    public string Url { get; set; } = url;
    public override string? Path { get; set; } = path;
    public override dynamic? Body { get; set; } = body;
    public override string? ContentType { get; set; } = contentType;
    public override Dictionary<string, string>? QueryParams { get; set; } = queryParams;
    public override string? OriginMethod { get; set; } = originMethod;
    public HttpMethod Method { get; set; } = method;

    public AptosRequest(string url, PostAptosRequest postAptosRequest)
        : this(url, HttpMethod.Post, postAptosRequest) { }

    public AptosRequest(string url, GetAptosRequest getAptosRequest)
        : this(url, HttpMethod.Get, getAptosRequest) { }

    public AptosRequest(string url, HttpMethod method, BaseAptosRequest request)
        : this(
            url,
            method,
            request.Path,
            (object?)request.Body,
            request.ContentType,
            request.QueryParams,
            request.OriginMethod
        ) { }
}

public class AptosResponse<Res>(
    int status,
    string statusText,
    Res data,
    string url,
    Dictionary<string, string> headers,
    AptosRequest? request = null
)
    where Res : class
{
    public readonly int Status = status;

    public readonly string StatusText = statusText;

    public readonly Res Data = data;

    public readonly string Url = url;

    public readonly Dictionary<string, string> Headers = headers;

    public readonly AptosRequest? Request = request;
}

public partial class AptosClient
{
    public async Task<AptosResponse<Res>> Request<Res>(ApiType type, AptosRequest request)
        where Res : class
    {
        AptosRequest aptosRequest = request;

        aptosRequest.Url = request.Path != null ? $"{request.Url}/{request.Path}" : request.Url;

        ClientRequest clientRequest =
            new(
                aptosRequest.Url,
                aptosRequest.Method,
                aptosRequest.Body,
                aptosRequest.ContentType,
                aptosRequest.QueryParams,
                new Dictionary<string, string>(Config.Headers)
            );

        // Add default headers
        clientRequest.Headers ??= [];
        clientRequest.Headers.Add(
            "x-aptos-client",
            $"aptos-dotnet-sdk/{Assembly.GetExecutingAssembly().GetName().Version}"
        );
        if (request.OriginMethod != null)
            clientRequest.Headers.Add("x-aptos-dotnet-sdk-origin-method", request.OriginMethod);

        ClientResponse<Res> clientResponse = await ClientRequest<Res>(clientRequest);

        AptosResponse<JObject> errorResponse =
            new(
                clientResponse.Status,
                clientResponse.StatusText,
                clientResponse.Error ?? new JObject(),
                aptosRequest.Url,
                clientResponse.Headers ?? [],
                aptosRequest
            );

        // Handle case for `401 Unauthorized` responses (e.g. provided API key is invalid)
        if (clientResponse.Status == 401)
            throw new ApiException(type, request, errorResponse);

        if (type == ApiType.Indexer)
        {
            // TODO: Add case handler for Indexer calls
        }
        else if (type == ApiType.Prover || type == ApiType.Pepper)
        {
            if (clientResponse.Status >= 400)
            {
                throw new ApiException(type, request, errorResponse);
            }
        }

        // Handle non-200 responses
        if (clientResponse.Status < 200 || clientResponse.Status >= 300)
            throw new ApiException(type, request, errorResponse);

        return new(
            clientResponse.Status,
            clientResponse.StatusText,
            clientResponse.Data!,
            aptosRequest.Url,
            clientResponse.Headers ?? [],
            aptosRequest
        );
    }

    private async Task<ClientResponse<Res>> ClientRequest<Res>(ClientRequest request)
        where Res : class
    {
        string method = request.Method.Method;
        return method switch
        {
            "GET" => await Config.RequestClient.Get<Res>(request),
            "POST" => await Config.RequestClient.Post<Res>(request),
            _ => throw new NotImplementedException(),
        };
    }
}
