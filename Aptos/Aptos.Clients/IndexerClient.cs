namespace Aptos;

using System.Reflection;
using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;
using Aptos.Indexer.Scalars;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using StrawberryShake;

public class IndexerClient
{
    private readonly AptosClient _client;

    private readonly IAptosIndexerClient _indexerClient;
    public IAptosIndexerClient Build => _indexerClient;

    private readonly string _baseUrl;

    public IndexerClient(AptosClient client)
    {
        _client = client;
        _baseUrl = _client.Config.GetRequestUrl(ApiType.Indexer);

        ServiceCollection serviceCollection = new();

        serviceCollection
            .AddSerializer<NumericSerializer>()
            .AddSerializer<TimestampSerializer>()
            .AddSerializer<BigIntSerializer>()
            .AddSerializer<JsonbSerializer>()
            .AddAptosIndexerClient()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(_baseUrl);
                client.DefaultRequestHeaders.Add(
                    "x-aptos-client",
                    $"aptos-dotnet-sdk/{Assembly.GetExecutingAssembly().GetName().Version}"
                );
                foreach (var header in _client.Config.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            });

        IServiceProvider services = serviceCollection.BuildServiceProvider();

        _indexerClient = services.GetRequiredService<IAptosIndexerClient>();
    }

    public async Task<AptosResponse<Res>> Query<Res>(
        Func<IAptosIndexerClient, Task<IOperationResult<Res>>> query
    )
        where Res : class
    {
        var result = await query(_indexerClient);

        var request = new AptosRequest(_baseUrl, HttpMethod.Post);
        try
        {
            result.EnsureNoErrors();
            return new AptosResponse<Res>(200, "", result.Data!, request.Url, [], request);
        }
        catch (GraphQLClientException exception)
        {
            throw new ApiException(
                ApiType.Indexer,
                request,
                new AptosResponse<object>(
                    200,
                    exception.Message,
                    JObject.FromObject(new { errors = exception.Errors }),
                    _baseUrl,
                    []
                )
            );
        }
    }

    public async Task<AptosResponse<Res>> Query<Res>(
        string query,
        Dictionary<string, object>? variables = null,
        string? originMethod = null
    )
        where Res : class =>
        await _client.PostIndexer<Res>(
            new(
                originMethod: originMethod ?? "queryIndexer",
                path: "",
                body: new { query, variables }
            )
        );
}
