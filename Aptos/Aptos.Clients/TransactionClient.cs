namespace Aptos;

using Aptos.Core;
using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;
using static Aptos.TransactionBuilder;

public class TransactionClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    /// <summary>
    /// Helper method to sign a transaction with an account.
    /// </summary>
    /// <param name="signer">The account to sign the transaction with.</param>
    /// <param name="transaction">The transaction to sign.</param>
    /// <returns>The authenticator with the signed transaction and public key.</returns>
    public AccountAuthenticator SignTransaction(Account signer, AnyRawTransaction transaction) =>
        signer.SignTransactionWithAuthenticator(transaction);

    /// <summary>
    /// Submits a transaction to the blockchain.
    /// </summary>
    /// <param name="data">The transaction data to submit.</param>
    /// <returns>The pending transaction response.</returns>
    public async Task<PendingTransactionResponse> SubmitTransaction(SubmitTransactionData data)
    {
        AptosResponse<PendingTransactionResponse> response =
            await _client.PostFullNode<PendingTransactionResponse>(
                new(
                    body: GeneratedSignedTransaction(data).BcsToBytes(),
                    contentType: MimeType.BCS_SIGNED_TRANSACTION,
                    path: "transactions",
                    originMethod: "POST"
                )
            );
        return response.Data;
    }

    /// <summary>
    /// Signs and submits a transaction to the blockchain. You can build a transaction payload by using the <see cref="TransactionClient.Build(AccountAddress, GenerateEntryFunctionPayloadData, bool, GenerateTransactionOptions?)"/> method.
    /// </summary>
    /// <param name="signer">The account to sign the transaction with.</param>
    /// <param name="transaction">The transaction to sign and submit.</param>
    /// <param name="feePayerAuthenticator">If the transaction has a fee payer, you can provide an authenticator for it here.</param>
    /// <param name="additionalSignersAuthenticators">If the transaction has additional signers, you can provide authenticators for them here.</param>
    /// <returns> The pending transaction response.</returns>
    public async Task<PendingTransactionResponse> SignAndSubmitTransaction(
        Account signer,
        AnyRawTransaction transaction,
        AccountAuthenticator? feePayerAuthenticator = null,
        List<AccountAuthenticator>? additionalSignersAuthenticators = null
    )
    {
        AccountAuthenticator signedTransactionAccountAuthenticator = SignTransaction(
            signer,
            transaction
        );
        return await SubmitTransaction(
            new SubmitTransactionData(
                transaction,
                signedTransactionAccountAuthenticator,
                feePayerAuthenticator,
                additionalSignersAuthenticators
            )
        );
    }

    /// <summary>
    /// Gets a transaction data by hash.
    /// </summary>
    /// <param name="hash">The hash of the transaction.</param>
    /// <returns>The transaction data.</returns>
    public async Task<TransactionResponse> GetTransactionByHash(string hash)
    {
        AptosResponse<TransactionResponse> response =
            await _client.GetFullNode<TransactionResponse>(
                new(path: $"transactions/by_hash/{hash}", originMethod: "getTransactionByHash")
            );
        return response.Data;
    }

    /// <summary>
    /// Gets a transaction data by version.
    /// </summary>
    /// <param name="version">The version of the transaction.</param>
    /// <returns>The transaction data.</returns>
    public async Task<TransactionResponse> GetTransactionByVersion(string version)
    {
        AptosResponse<TransactionResponse> response =
            await _client.GetFullNode<TransactionResponse>(
                new(
                    path: $"transactions/by_version/{version}",
                    originMethod: "getTransactionByVersion"
                )
            );
        return response.Data;
    }

    /// <summary>
    /// Gets a paginated list of transactions.
    /// </summary>
    /// <param name="start">The start index of the transactions to return.</param>
    /// <param name="limit">The limit of the transactions to return.</param>
    /// <returns>A list of transactions.</returns>
    public async Task<TransactionResponse[]> GetTransactions(int? start = null, int? limit = null)
    {
        Dictionary<string, string> queryParams = [];
        if (start != null)
        {
            queryParams["start"] = ((int)start).ToString();
        }
        if (limit != null)
        {
            queryParams["limit"] = ((int)limit).ToString();
        }
        List<TransactionResponse> response =
            await _client.GetFullNodeWithPagination<TransactionResponse>(
                new(originMethod: "getTransactions", path: "transactions", queryParams: queryParams)
            );
        return [.. response];
    }

    /// <summary>
    /// Waits for a transaction to be committed. Its recommended to use <see cref="WaitForTransaction(PendingTransactionResponse, WaitForTransactionOptions?)"/> instead.
    /// </summary>
    /// <param name="hash">The hash of the transaction to wait for.</param>
    /// <returns>The transaction response.</returns>
    public async Task<TransactionResponse> LongWaitForTransaction(string hash)
    {
        AptosResponse<TransactionResponse> response =
            await _client.GetFullNode<TransactionResponse>(
                new(
                    path: $"transactions/wait_by_hash/{hash}",
                    originMethod: "longWaitForTransaction"
                )
            );
        return response.Data;
    }

    public class WaitForTransactionOptions(
        ulong timeoutSecs,
        ulong interval,
        bool? checkSuccess = null,
        bool? waitForIndexer = null
    )
    {
        public ulong TimeoutSecs = timeoutSecs;
        public ulong Interval = interval;
        public bool? CheckSuccess = checkSuccess;
        public bool? WaitForIndexer = waitForIndexer;
    }

    /// <summary>
    /// Waits for a transaction to be committed using polling.
    /// </summary>
    /// <param name="pendingTransaction">The pending transaction to wait for.</param>
    /// <param name="options">The options for waiting for the transaction.</param>
    /// <returns>The transaction response.</returns>
    public async Task<CommittedTransactionResponse> WaitForTransaction(
        PendingTransactionResponse pendingTransaction,
        WaitForTransactionOptions? options = null
    ) => await WaitForTransaction(pendingTransaction.Hash, options);

    public async Task<CommittedTransactionResponse> WaitForTransaction(
        Hex hash,
        WaitForTransactionOptions? options = null
    ) => await WaitForTransaction(hash.ToString(), options);

    public async Task<CommittedTransactionResponse> WaitForTransaction(
        string hash,
        WaitForTransactionOptions? options = null
    )
    {
        ulong timeoutSecs = options?.TimeoutSecs ?? Constants.DEFAULT_TIMEOUT_SECS;
        bool checkSuccess = options?.CheckSuccess ?? true;

        bool isPending = true;
        double timeElapsed = 0;
        TransactionResponse? lastTxn = null;
        Exception? lastException = null;

        double backoffIntervalMs = 200;
        double backoffMultiplier = 1.5;

        void HandleApiError(Exception e)
        {
            if (e is ApiException apex)
            {
                lastException = apex;
                // If the exception is a bad request error, rethrow the exception.
                if (apex.Status != 404 && apex.Status >= 400 && apex.Status < 500)
                    throw lastException;
            }
            else
            {
                // If the exception is not an AptosException, throw it because it is unexpected.
                throw e;
            }
        }

        try
        {
            lastTxn = await GetTransactionByHash(hash);
            isPending = lastTxn.Type == TransactionResponseType.Pending;
        }
        catch (Exception e)
        {
            HandleApiError(e);
        }

        // If the transaction is still pending, wait for it to be committed by doing a long wait.
        if (isPending)
        {
            DateTime startTime = DateTime.Now;
            try
            {
                lastTxn = await LongWaitForTransaction(hash);
                isPending = lastTxn.Type == TransactionResponseType.Pending;
            }
            catch (Exception e)
            {
                HandleApiError(e);
            }
            timeElapsed = (DateTime.Now - startTime).TotalSeconds;
        }

        // Now we do polling to see if the transaction is still pending.
        while (isPending)
        {
            if (timeElapsed >= timeoutSecs)
                break;
            try
            {
                lastTxn = await GetTransactionByHash(hash);
                isPending = lastTxn.Type == TransactionResponseType.Pending;
                if (!isPending)
                    break;
            }
            catch (Exception e)
            {
                HandleApiError(e);
            }
            await Task.Delay((int)backoffIntervalMs);
            timeElapsed += backoffIntervalMs / 1000;
            backoffIntervalMs *= backoffMultiplier;
        }

        // If the transaction is still undefined, throw an error.
        if (lastTxn == null)
        {
            if (lastException != null)
                throw lastException;
            throw new WaitForTransactionException(
                $"Transaction {hash} is still undefined after waiting for {timeoutSecs} seconds",
                lastTxn
            );
        }

        // If the transaction is still pending, throw an error.
        if (lastTxn.Type == TransactionResponseType.Pending)
        {
            throw new WaitForTransactionException(
                $"Transaction {hash} is still pending after waiting for {timeoutSecs} seconds",
                lastTxn
            );
        }

        if (checkSuccess && !((CommittedTransactionResponse)lastTxn).Success)
        {
            throw new FailedTransactionException(
                $"Transaction {hash} failed after waiting for {timeoutSecs} seconds",
                (CommittedTransactionResponse)lastTxn
            );
        }

        return (CommittedTransactionResponse)lastTxn;
    }

    public async void WaitForIndexer(long? minimumLedgerVersion, string processorType)
    {
        int timeoutMilliseconds = 3000; // 3 seconds
        long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long indexerVersion = -1;

        while (indexerVersion < minimumLedgerVersion)
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime > timeoutMilliseconds)
                throw new Exception("waitForLastSuccessIndexerVersionSync timeout");

            processor_status_bool_exp condition =
                new() { _and = [new() { Processor = new() { _eq = processorType } }] };

            var status = await _client.Indexer.Query(async client =>
                await client.GetProcessorStatus.ExecuteAsync(condition)
            );
            var lastSuccessVersion = status
                .Data.Processor_status.FirstOrDefault()
                ?.Last_success_version;
            if (lastSuccessVersion != null)
                indexerVersion = (long)lastSuccessVersion;

            if (indexerVersion >= minimumLedgerVersion)
                break;

            await Task.Delay(200);
        }
    }

    public async Task<SimpleTransaction> Build(
        Account sender,
        GenerateEntryFunctionPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    ) => await Build(sender.Address, data, withFeePayer, options);

    public async Task<SimpleTransaction> Build(
        string sender,
        GenerateEntryFunctionPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    ) => await Build(AccountAddress.From(sender), data, withFeePayer, options);

    public async Task<SimpleTransaction> Build(
        AccountAddress sender,
        GenerateEntryFunctionPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    ) => await GenerateTransaction(_client, sender, data, withFeePayer, options);

    public async Task<SimpleTransaction> Build(
        AccountAddress sender,
        GenerateScriptPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    ) => await GenerateTransaction(_client, sender, data, withFeePayer, options);

    public async Task<MultiAgentTransaction> Build(
        AccountAddress sender,
        GenerateEntryFunctionPayloadData data,
        List<AccountAddress> secondarySignerAddresses,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    ) =>
        await GenerateTransaction(
            _client,
            sender,
            data,
            secondarySignerAddresses,
            withFeePayer,
            options
        );

    /// <summary>
    /// Simulates a transaction without submitting it to the blockchain.
    /// </summary>
    /// <param name="data">The transaction data to simulate.</param>
    /// <returns>The simulated transaction responses.</returns>
    public async Task<List<UserTransactionResponse>> Simulate(SimulateTransactionData data)
    {
        var signedTransaction = GenerateSignedTransactionForSimulation(data);

        Dictionary<string, string> queryParams = [];
        if (data.Options?.EstimateGasUnitPrice != null)
        {
            queryParams.Add(
                "estimate_gas_unit_price",
                data.Options.EstimateGasUnitPrice.ToString()
            );
        }
        if (data.Options?.EstimateMaxGasAmount != null)
        {
            queryParams.Add(
                "estimate_max_gas_amount",
                data.Options.EstimateMaxGasAmount.ToString()
            );
        }
        if (data.Options?.EstimatePrioritizedGasUnitPrice != null)
        {
            queryParams.Add(
                "estimate_prioritized_gas_unit_price",
                data.Options.EstimatePrioritizedGasUnitPrice.ToString()
            );
        }

        var response = await _client.PostFullNode<List<UserTransactionResponse>>(
            new(
                path: "transactions/simulate",
                originMethod: "simulateTransaction",
                body: signedTransaction.BcsToBytes(),
                contentType: MimeType.BCS_SIGNED_TRANSACTION,
                queryParams: queryParams
            )
        );
        return response.Data;
    }
}
