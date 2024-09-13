namespace Aptos;

using Aptos.Core;
using Aptos.Indexer.GraphQL;

public class FungibleAssetClient(AptosClient client)
{

    private readonly AptosClient _client = client;

    /// <summary>
    /// Gets the paired types for a given value. If the value is a fungible asset object address, it will query
    /// for the paired coin type. 
    /// </summary>
    /// <param name="value">A struct coin type or fungible asset object address.</param>
    /// <returns>An array of paired types.</returns>
    public async Task<List<string>> GetPairedTypes(string value)
    {

        // Struct coin type
        if (value.Contains("::"))
        {
            if (value == Constants.APTOS_COIN_TYPE)
            {
                return [Constants.APTOS_COIN_TYPE, Constants.APTOS_COIN_FA];
            }
            else
            {
                return [value, AccountAddress.CreateObjectAddress(Constants.APTOS_FA_ADDRESS, value).ToStringLong()];
            }
        }
        // Fungible asset object address
        else
        {
            string formattedAddress = AccountAddress.FromString(value).ToStringLong();
            if (formattedAddress == Constants.APTOS_COIN_FA)
            {
                return [Constants.APTOS_COIN_TYPE, Constants.APTOS_COIN_FA];
            }
            else
            {
                try
                {
                    return [formattedAddress, (await _client.View<ResourceStruct>(new("0x1::coin::paired_coin", [formattedAddress]))).ToString()];
                }
                catch
                {
                    // If the paired coin function fails, it means that the coin is not paired.
                    return [formattedAddress];
                }
            }
        }

    }

    /// <inheritdoc cref="GetAccountFungibleAssetBalances(string, int, int, current_fungible_asset_balances_bool_exp?, current_fungible_asset_balances_order_by?)"/>
    public async Task<List<FungibleAssetBalance>> GetAccountFungibleAssetBalances(AccountAddress address, int offset = 0, int limit = 50, current_fungible_asset_balances_bool_exp? where = null, current_fungible_asset_balances_order_by? orderBy = null) => await GetAccountFungibleAssetBalances(address.ToString(), offset, limit, where, orderBy);
    /// <summary>
    /// Gets a paginated list of fungible asset balances of the given address.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the fungible asset balances.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of fungible asset balances.</returns>
    public async Task<List<FungibleAssetBalance>> GetAccountFungibleAssetBalances(string address, int offset = 0, int limit = 50, current_fungible_asset_balances_bool_exp? where = null, current_fungible_asset_balances_order_by? orderBy = null)
    {
        current_fungible_asset_balances_bool_exp condition = new()
        {
            Owner_address = new() { _eq = address }
        };
        if (where != null) condition._and = [where];
        return await GetFungibleAssetBalances(condition, offset, limit, orderBy);
    }

    /// <summary>
    /// Gets a paginated list of fungible asset balances.
    /// </summary>
    /// <param name="where">The condition to filter the fungible asset balances.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of fungible asset balances.</returns>
    public async Task<List<FungibleAssetBalance>> GetFungibleAssetBalances(current_fungible_asset_balances_bool_exp where, int offset = 0, int limit = 50, current_fungible_asset_balances_order_by? orderBy = null)
    {
        var response = await _client.Indexer.Query(async client => await client.GetFungibleAssetBalances.ExecuteAsync(where, offset, limit, orderBy != null ? [orderBy] : []));
        return response.Data.Current_fungible_asset_balances.Select(b =>
        {
            try
            {
                return new FungibleAssetBalance(b);
            }
            catch
            {
                return null;
            }
        }).Where(b => b != null).Cast<FungibleAssetBalance>().ToList();
    }

}