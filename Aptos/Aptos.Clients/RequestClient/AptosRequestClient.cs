namespace Aptos;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AptosRequestClient : RequestClient
{
    private readonly HttpClient _httpClient;

    public AptosRequestClient()
    {
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        _httpClient = new HttpClient(handler);
    }

    public override async Task<ClientResponse<Res>> Get<Res>(ClientRequest request)
        where Res : class
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Get, request.Url);

        // Add headers
        if (request.Headers != null)
        {
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        // Add query params
        if (request.QueryParams != null)
        {
            UriBuilder uriBuilder = new(request.Url);
            string query = uriBuilder.Query; // Existing query
            StringBuilder queryBuilder = new(string.IsNullOrEmpty(query) ? "" : query.Substring(1)); // Remove leading '?'

            foreach (KeyValuePair<string, string> queryParam in request.QueryParams)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append('&');
                }
                queryBuilder.Append(
                    $"{Uri.EscapeDataString(queryParam.Key)}={Uri.EscapeDataString(queryParam.Value)}"
                );
            }

            uriBuilder.Query = queryBuilder.ToString();
            httpRequest.RequestUri = uriBuilder.Uri;
        }

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();
            return new ClientResponse<Res>(
                (int)response.StatusCode,
                response.ReasonPhrase ?? "",
                response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Res>(content)! : null,
                !response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<JObject>(content)
                    : null,
                response,
                response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Error making request to {request.Url}: {e.Message}");
        }
    }

    public override async Task<ClientResponse<Res>> Post<Res>(ClientRequest request)
        where Res : class
    {
        HttpRequestMessage httpRequest = new(HttpMethod.Post, request.Url);

        // Add headers
        if (request.Headers != null)
        {
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }
        }

        // Add content
        if (request.Body != null)
        {
            if (request.Body is byte[] bytes)
            {
                httpRequest.Content = new ByteArrayContent(bytes);
            }
            else
            {
                string jsonBody = JsonConvert.SerializeObject(request.Body);
                httpRequest.Content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json"
                );
            }

            if (request.ContentType != null)
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(
                    request.ContentType
                );
        }

        // Add query params
        if (request.QueryParams != null)
        {
            UriBuilder uriBuilder = new(request.Url);
            string query = uriBuilder.Query; // Existing query
            StringBuilder queryBuilder = new(string.IsNullOrEmpty(query) ? "" : query.Substring(1)); // Remove leading '?'

            foreach (KeyValuePair<string, string> queryParam in request.QueryParams)
            {
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Append('&');
                }
                queryBuilder.Append(
                    $"{Uri.EscapeDataString(queryParam.Key)}={Uri.EscapeDataString(queryParam.Value)}"
                );
            }

            uriBuilder.Query = queryBuilder.ToString();
            httpRequest.RequestUri = uriBuilder.Uri;
        }

        try
        {
            HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();
            return new ClientResponse<Res>(
                (int)response.StatusCode,
                response.ReasonPhrase ?? "",
                response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Res>(content)! : null,
                !response.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<JObject>(content)
                    : null,
                response,
                response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
            );
        }
        catch (Exception e)
        {
            throw new Exception($"Error making request to {request.Url}", e);
        }
    }
}
