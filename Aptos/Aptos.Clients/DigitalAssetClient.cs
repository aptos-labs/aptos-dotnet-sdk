namespace Aptos;

using Aptos.Indexer.GraphQL;

public class DigitalAssetClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <inheritdoc cref="GetAccountDigitalAssetOwnerships(string, int, int, current_token_ownerships_v2_bool_exp?, current_token_ownerships_v2_order_by?)"/>
    public async Task<List<DigitalAssetOwnership>> GetAccountDigitalAssetOwnerships(
        AccountAddress address,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    ) => await GetAccountDigitalAssetOwnerships(address.ToString(), offset, limit, where, orderBy);

    /// <summary>
    /// Gets the digital asset ownerships of an account. Digital assets are a representation of NFTs on Aptos.
    /// </summary>
    /// <param name="address">The address of the account.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="where">The condition to filter the digital asset ownerships.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of digital asset ownerships.</returns>
    public async Task<List<DigitalAssetOwnership>> GetAccountDigitalAssetOwnerships(
        string address,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_bool_exp? where = null,
        current_token_ownerships_v2_order_by? orderBy = null
    )
    {
        current_token_ownerships_v2_bool_exp condition =
            new() { Owner_address = new() { _eq = address } };
        if (where != null)
            condition._and = [where];
        return await GetDigitalAssetOwnerships(condition, offset, limit, orderBy);
    }

    /// <summary>
    /// Gets the digital asset ownerships of the account. Digital assets are a representation of NFTs on Aptos.
    /// </summary>
    /// <param name="where">The condition to filter the digital asset ownerships.</param>
    /// <param name="offset">The offset of the query.</param>
    /// <param name="limit">The item limit of the query.</param>
    /// <param name="orderBy">The order by condition of the query.</param>
    /// <returns>A list of digital asset ownerships.</returns>
    public async Task<List<DigitalAssetOwnership>> GetDigitalAssetOwnerships(
        current_token_ownerships_v2_bool_exp where,
        int offset = 0,
        int limit = 50,
        current_token_ownerships_v2_order_by? orderBy = null
    )
    {
        var response = await _client.Indexer.Query(async client =>
            await client.GetDigitalAssetOwnerships.ExecuteAsync(
                where,
                offset,
                limit,
                orderBy != null ? [orderBy] : []
            )
        );
        return response
            .Data.Current_token_ownerships_v2.Select(b =>
            {
                try
                {
                    return new DigitalAssetOwnership(b);
                }
                catch
                {
                    return null;
                }
            })
            .Where(b => b != null)
            .Cast<DigitalAssetOwnership>()
            .ToList();
    }
}
