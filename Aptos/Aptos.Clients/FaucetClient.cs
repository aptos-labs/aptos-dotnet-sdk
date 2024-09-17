namespace Aptos;

using Aptos.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class FaucetClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <inheritdoc cref="FundAccount(string, ulong)"/>
    public async Task<UserTransactionResponse> FundAccount(string address, ulong amount) =>
        await FundAccount(AccountAddress.From(address), amount);

    /// <summary>
    /// Funds an account with the given amount of coins using the faucet. If the network is not supported by the faucet, an exception will be thrown.
    /// </summary>
    /// <param name="address">The address of the account to fund.</param>
    /// <param name="amount">The amount of APT coins to fund.</param>
    /// <returns>The transaction response of the funded account.</returns>
    /// <exception cref="FaucetException">Unable to fund the account.</exception>
    /// <exception cref="Exception">If the response from the faucet is not in the expected format.</exception>
    public async Task<UserTransactionResponse> FundAccount(AccountAddress address, ulong amount)
    {
        AptosResponse<JObject> response = await _client.PostFaucet<JObject>(
            new(
                body: new { address = address.ToString(), amount },
                path: "fund",
                originMethod: "fundAccount"
            )
        );

        string? txnHash = JsonConvert
            .DeserializeObject<string[]>(response.Data["txn_hashes"]?.ToString() ?? "[]")
            ?.FirstOrDefault();
        if (txnHash == null)
            throw new FaucetException(
                "No transaction hash returned from faucet. The faucet may be down at the moment."
            );

        CommittedTransactionResponse txnResponse = await _client.Transaction.WaitForTransaction(
            txnHash
        );

        if (txnResponse is not UserTransactionResponse)
            throw new Exception("Unexpected transaction response from faucet");

        return (UserTransactionResponse)txnResponse;
    }
}
