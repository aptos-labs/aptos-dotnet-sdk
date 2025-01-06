namespace Aptos;

using Aptos.Core;
using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AccountClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    ///<inheritdoc cref = "GetInfo(string)" />
    public async Task<AccountData> GetInfo(AccountAddress address) =>
        await GetInfo(address.ToString());

    /// <summary>
    /// Gets the account information of the given address. This includes the sequence number and authentication key.
    /// </summary>
    /// <param name="address"> The address of the account.</param>
    /// <returns> The account information of the given address.</returns>
    public async Task<AccountData> GetInfo(string address)
    {
        AptosResponse<AccountData> response = await _client.GetFullNode<AccountData>(
            new(path: $"accounts/{address}", originMethod: "getInfo")
        );
        return response.Data;
    }

    /// <inheritdoc cref="GetModule(string, string, ulong?)"/>
    public async Task<MoveModuleBytecode> GetModule(
        AccountAddress address,
        string moduleName,
        ulong? ledgerVersion = null
    ) => await GetModule(address.ToString(), moduleName, ledgerVersion);

    /// <summary>
    /// Gets the Move module of the given address and module name.
    ///
    /// The function will be cached every time it is called with the same parameters to avoid unnecessary network requests.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="ledgerVersion">The ledger version to use for the module call.</param>
    /// <returns>The Move bytecode and ABI.</returns>
    public async Task<MoveModuleBytecode> GetModule(
        string address,
        string moduleName,
        ulong? ledgerVersion = null
    )
    {
        async Task<MoveModuleBytecode> GetModuleInner(
            string address,
            string moduleName,
            ulong? ledgerVersion = null
        )
        {
            Dictionary<string, string> queryParams = [];
            if (ledgerVersion != null)
            {
                queryParams.Add("ledger_version", ledgerVersion!.ToString()!);
            }
            return (
                await _client.GetFullNode<MoveModuleBytecode>(
                    new(
                        path: $"accounts/{address}/module/{moduleName}",
                        originMethod: "getModule",
                        queryParams: queryParams
                    )
                )
            ).Data;
        }

        if (ledgerVersion != null)
            return await GetModuleInner(address, moduleName, ledgerVersion);

        return await Memoize.MemoAsync(
            async () => await GetModuleInner(address, moduleName),
            $"module-{address}-{moduleName}",
            1000 * 60 * 5 // 5 minutes
        )();
    }

    /// <inheritdoc cref="GetTransactionCount(string)"/>
    public async Task<int> GetTransactionCount(AccountAddress address) =>
        await GetTransactionCount(address.ToString());

    /// <summary>
    /// Gets the number of transactions committed for the given address.
    /// </summary>
    /// <param name="address"> The address of the account.</param>
    /// <returns> The number of transactions committed for the given address.</returns>
    public async Task<int> GetTransactionCount(string address)
    {
        var result = await _client.Indexer.Query(async client =>
            await client.GetAccountTransactionsCount.ExecuteAsync(address)
        );
        return result.Data?.Account_transactions_aggregate?.Aggregate?.Count ?? 0;
    }

    /// <inheritdoc cref="GetResource(string, string, ulong?)"/>
    public async Task<MoveResource> GetResource(
        AccountAddress address,
        string resourceType,
        ulong? ledgerVersion = null
    ) => await GetResource(address.ToString(), resourceType, ledgerVersion);

    /// <inheritdoc cref="GetResource(string, string, ulong?)"/>
    public async Task<MoveResource> GetResource(
        string address,
        string resourceType,
        ulong? ledgerVersion = null
    ) => await GetResource<MoveResource>(address, resourceType, ledgerVersion);

    /// <summary>
    /// Gets the resource of the given address and resource type.
    /// </summary>
    /// <typeparam name="T"> The return type of the resource. Should typically be <see cref="MoveResource"/>.</typeparam>
    /// <param name="address">The address of the account.</param>
    /// <param name="resourceType">The resource type.</param>
    /// <param name="ledgerVersion">The ledger version to use for the resource call.</param>
    /// <returns>The resource of the given address and resource type.</returns>
    public async Task<T> GetResource<T>(
        string address,
        string resourceType,
        ulong? ledgerVersion = null
    )
        where T : class
    {
        Dictionary<string, string> queryParams = [];
        if (ledgerVersion != null)
        {
            queryParams.Add("ledger_version", ledgerVersion.ToString()!);
        }
        var response = await _client.GetFullNode<MoveResource>(
            new(
                path: $"accounts/{AccountAddress.From(address)}/resource/{resourceType}",
                originMethod: "getResource",
                queryParams: queryParams
            )
        );
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(response.Data.Data))!;
    }

    /// <inheritdoc cref="GetResources(string, int?, int?)"/>
    public async Task<List<MoveResource>> GetResources(
        AccountAddress address,
        int? start = null,
        int? limit = null
    ) => await GetResources(address.ToString(), start, limit);

    /// <summary>
    /// Gets a paginated list of resources of the given address.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="start">The start index of the resources to return.</param>
    /// <param name="limit">The limit of the resources to return.</param>
    /// <returns>The resources of the given address.</returns>
    public async Task<List<MoveResource>> GetResources(
        string address,
        int? start = null,
        int? limit = null
    )
    {
        Dictionary<string, string> queryParams = [];
        if (start != null)
        {
            queryParams.Add("start", start.ToString()!);
        }
        if (limit != null)
        {
            queryParams.Add("limit", limit.ToString()!);
        }
        var response = await _client.GetFullNodeWithPagination<MoveResource>(
            new(
                originMethod: "getResources",
                path: $"accounts/{address}/resources",
                queryParams: queryParams
            )
        );
        return response;
    }

    /// <inheritdoc cref="GetCoinBalances(string, List{string}?, int, int, current_fungible_asset_balances_bool_exp?, current_fungible_asset_balances_order_by?)"/>
    public async Task<List<CoinBalance>> GetCoinBalances(
        AccountAddress address,
        List<string>? types = null,
        int offset = 0,
        int limit = 50,
        current_fungible_asset_balances_bool_exp? where = null,
        current_fungible_asset_balances_order_by? orderBy = null
    ) => await GetCoinBalances(address.ToString(), types, offset, limit, where, orderBy);

    /// <summary>
    /// Gets a paginated list of coin balances of the given address.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="types">The types of the coin balances to return.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the coin balances.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of coin balances.</returns>
    public async Task<List<CoinBalance>> GetCoinBalances(
        string address,
        List<string>? types = null,
        int offset = 0,
        int limit = 50,
        current_fungible_asset_balances_bool_exp? where = null,
        current_fungible_asset_balances_order_by? orderBy = null
    )
    {
        current_fungible_asset_balances_bool_exp condition = new();
        if (types != null)
            condition.Asset_type = new() { _in = types };
        if (where != null)
            condition._and = [where];
        bool hasCondition = where != null || types != null;
        return (
            await _client.FungibleAsset.GetAccountFungibleAssetBalances(
                address,
                offset,
                limit,
                hasCondition ? condition : null,
                orderBy
            )
        )
            .Select(e => e.ToCoinBalance())
            .ToList();
    }

    /// <inheritdoc cref="GetCoinBalance(string, string?, int, int, current_fungible_asset_balances_bool_exp?, current_fungible_asset_balances_order_by?)"/>
    public async Task<CoinBalance?> GetCoinBalance(
        AccountAddress address,
        string type = "0xa",
        int offset = 0,
        int limit = 50,
        current_fungible_asset_balances_bool_exp? where = null,
        current_fungible_asset_balances_order_by? orderBy = null
    ) => await GetCoinBalance(address.ToString(), type, offset, limit, where, orderBy);

    /// <summary>
    /// Gets the coin balance of the given address and coin type. When providing the types, its important to include both the Fungible Asset address
    /// and the coin type if they both exist for the asset.
    /// <br/>
    /// The default will be "0xa" and "0x1::aptos_coin::AptosCoin".
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="type">The type of the coin.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the coin balances.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>The coin balance of the given address and coin type.</returns>
    public async Task<CoinBalance?> GetCoinBalance(
        string address,
        string type = "0xa",
        int offset = 0,
        int limit = 50,
        current_fungible_asset_balances_bool_exp? where = null,
        current_fungible_asset_balances_order_by? orderBy = null
    ) =>
        (
            await GetCoinBalances(
                address,
                await _client.FungibleAsset.GetPairedTypes(type),
                offset,
                limit,
                where,
                orderBy
            )
        ).FirstOrDefault();

    /// <inheritdoc cref="GetTokenOwnerships(string, List{string}?, int, int, current_token_ownerships_v2_bool_exp?, current_token_ownerships_v2_order_by?)"/>
    public async Task<List<TokenOwnership>> GetTokenOwnerships(
        AccountAddress address,
        List<string>? collectionIds = null,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    ) => await GetTokenOwnerships(address.ToString(), collectionIds, offset, limit, where, orderBy);

    /// <summary>
    /// Gets a paginated list of token ownerships of the given address.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="collectionIds">The collection ids of the token ownerships to return.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the token ownerships.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of token ownerships.</returns>
    public async Task<List<TokenOwnership>> GetTokenOwnerships(
        string address,
        List<string>? collectionIds = null,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    )
    {
        current_token_ownerships_v2_bool_exp condition = new();
        if (collectionIds != null)
            condition.Current_token_data = new() { Collection_id = new() { _in = collectionIds } };
        if (where != null)
            condition._and = [where];
        bool hasCondition = where != null || collectionIds != null;
        return (
            await _client.DigitalAsset.GetAccountDigitalAssetOwnerships(
                address,
                offset,
                limit,
                hasCondition ? condition : null,
                orderBy
            )
        )
            .Select(e => e.ToTokenOwnership())
            .ToList();
    }

    /// <inheritdoc cref="GetTokenOwnership(string, string, int, int, current_token_ownerships_v2_bool_exp?, current_token_ownerships_v2_order_by?)"/>
    public async Task<TokenOwnership?> GetTokenOwnership(
        AccountAddress address,
        string tokenId,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    ) => await GetTokenOwnership(address.ToString(), tokenId, offset, limit, where, orderBy);

    /// <summary>
    /// Gets the token ownership of the given address and token id.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="tokenId">The token id of the token ownership to return.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the token ownerships.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>The token ownership of the given address and token id.</returns>
    public async Task<TokenOwnership?> GetTokenOwnership(
        string address,
        string tokenId,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    )
    {
        current_token_ownerships_v2_bool_exp condition = new()
        {
            Token_data_id = new() { _eq = tokenId },
        };
        if (where != null)
            condition._and = [where];
        return (
            await _client.DigitalAsset.GetAccountDigitalAssetOwnerships(
                address,
                offset,
                limit,
                condition,
                orderBy
            )
        )
            .Select(e => e.ToTokenOwnership())
            .FirstOrDefault();
    }

    public async Task<AccountAddress> LookupOriginalAccountAddress(string authenticationKey)
    {
        var resource = await GetResource<OriginatingAddress>(
            "0x1",
            "0x1::account::OriginatingAddress"
        );

        try
        {
            var originalAddress = await _client.Table.GetItem<string>(
                handle: resource.AddressMap.handle,
                ("address", "address", AccountAddress.From(authenticationKey).ToString())
            );
            return AccountAddress.From(originalAddress);
        }
        catch (Exception e)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (
                e is ApiException apex
                && (apex.Data as JObject)["error_code"]?.ToString() == "table_item_not_found"
            )
            {
                return AccountAddress.From(authenticationKey);
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            throw e;
        }
    }
}
