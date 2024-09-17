using Aptos.Indexer.GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace Aptos.Indexer
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceCollection serviceCollection = new();

            serviceCollection
                .AddAptosIndexerClient()
                .ConfigureHttpClient(client =>
                    client.BaseAddress = new Uri("https://api.mainnet.aptoslabs.com/v1/graphql")
                );

            IServiceProvider services = serviceCollection.BuildServiceProvider();

            IAptosIndexerClient client = services.GetRequiredService<IAptosIndexerClient>();

            Dictionary<string, object?> variables =
                new()
                {
                    {
                        "address",
                        "0x1affcf8d0a23b937c244236486ae43d05f7c821ef7abb0b5ca11cdc2a5aff18d"
                    },
                };

            var result = await client.GetAccountTransactionsCount.ExecuteAsync(
                "0x1affcf8d0a23b937c244236486ae43d05f7c821ef7abb0b5ca11cdc2a5aff18d"
            );
        }
    }
}
