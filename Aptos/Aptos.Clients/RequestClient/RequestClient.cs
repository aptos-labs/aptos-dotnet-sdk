namespace Aptos;

using Newtonsoft.Json.Linq;

public class ClientRequest(
    string url,
    HttpMethod method,
    object? body = null,
    string? contentType = null,
    Dictionary<string, string>? queryParams = null,
    Dictionary<string, string>? headers = null
)
{
    public string Url = url;

    public HttpMethod Method = method;

    public object? Body = body;

    public string? ContentType = contentType;

    public Dictionary<string, string>? QueryParams = queryParams;

    public Dictionary<string, string>? Headers = headers;
}

public class ClientResponse<Res>(
    int status,
    string statusText,
    Res? data,
    object? error = null,
    HttpResponseMessage? response = null,
    Dictionary<string, string>? headers = null
)
{
    public int Status = status;
    public string StatusText = statusText;
    public Res? Data = data;

    /// <summary>
    /// The error object returned by the API. This can be a JObject or a string.
    /// </summary>
    public object? Error = error;
    public HttpResponseMessage? Response = response;
    public Dictionary<string, string>? Headers = headers ?? [];
}

public abstract class RequestClient
{
    public abstract Task<ClientResponse<Res>> Post<Res>(ClientRequest request)
        where Res : class;
    public abstract Task<ClientResponse<Res>> Get<Res>(ClientRequest request)
        where Res : class;
}
