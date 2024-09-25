namespace Aptos;

using Aptos.Core;
using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;

public class ANSClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <inheritdoc cref="GetAnsName(AccountAddress)"/>
    public async Task<string?> GetAnsName(string address) =>
        await GetAnsName(AccountAddress.From(address));

    /// <summary>
    /// Gets the registered ANS name of the given address. This includes the subdomain of the name.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <returns>The ANS name of the given address.</returns>
    public async Task<string?> GetAnsName(AccountAddress address)
    {
        string? routerAddress = Ans.GetRouterAddress(_client.Config.NetworkConfig);
        if (routerAddress == null)
            throw new ANSUnsupportedNetwork();

        var response = (
            await _client.Indexer.Query(async client =>
                await client.GetNames.ExecuteAsync(
                    0,
                    1,
                    new()
                    {
                        Registered_address = new() { _eq = address.ToString() },
                        Is_active = new() { _eq = true },
                    },
                    [new() { Is_primary = Order_by.Desc }]
                )
            )
        ).Data.Current_aptos_names.ElementAtOrDefault(0);

        // Join the names removing any null values
        var name = new List<string?> { response?.Subdomain, response?.Domain }
            .Where(s => s != null && s != "")
            .ToList();
        return name.Count > 0 ? string.Join(".", name) + ".apt" : null;
    }

    /// <inheritdoc cref="GetAnsAddress(string, string?)"/>
    public async Task<AccountAddress?> GetAnsAddress(string name)
    {
        var (domain, subdomain) = Ans.ParseAnsName(name);
        return await GetAnsAddress(domain, subdomain);
    }

    /// <summary>
    /// Gets the ANS address of the given domain and subdomain. If the network is not supported by the ANS resolver, an exception will be thrown.
    /// </summary>
    /// <param name="domain">The domain of the ANS name.</param>
    /// <param name="subdomain">The subdomain of the ANS name.</param>
    /// <returns>The ANS address of the given domain and subdomain.</returns>
    public async Task<AccountAddress?> GetAnsAddress(string domain, string? subdomain = null)
    {
        string? routerAddress = Ans.GetRouterAddress(_client.Config.NetworkConfig);
        if (routerAddress == null)
            throw new ANSUnsupportedNetwork();

        var response = await _client.Contract.View(
            new(
                function: $"{routerAddress}::router::get_target_addr",
                functionArguments: [domain, subdomain]
            )
        );

        return Utilities.UnwrapOption<AccountAddress>(response[0]);
    }
}
