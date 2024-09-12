namespace Aptos;

using Newtonsoft.Json.Linq;

public class ClientRequest(string url, HttpMethod method, dynamic? body = null, string? contentType = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? headers = null)
{

    public string Url = url;

    public HttpMethod Method = method;

    public dynamic? Body = body;

    public string? ContentType = contentType;

    public Dictionary<string, string>? QueryParams = queryParams;

    public Dictionary<string, string>? Headers = headers;

}

public class ClientResponse<Res>(int status, string statusText, Res? data, JObject? error = null, HttpResponseMessage? response = null, Dictionary<string, string>? headers = null)
{
    public int Status = status;
    public string StatusText = statusText;
    public Res? Data = data;
    public JObject? Error = error;
    public HttpResponseMessage? Response = response;
    public Dictionary<string, string>? Headers = headers ?? [];
}


public abstract class RequestClient
{
    abstract public Task<ClientResponse<Res>> Post<Res>(ClientRequest request) where Res : class;
    abstract public Task<ClientResponse<Res>> Get<Res>(ClientRequest request) where Res : class;
}