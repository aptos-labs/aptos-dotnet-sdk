namespace Aptos;

using Aptos.Core;
using OneOf;

public static class TransactionBuilder
{
    #region GetAuthenticator

    public static AccountAuthenticator GetAuthenticatorForSimulation(
        OneOf<PublicKey, IVerifyingKey> publicOrVerifyingKey
    )
    {
        Ed25519Signature invalidSignature = new(new byte[64]);

        return publicOrVerifyingKey.Match<AccountAuthenticator>(
            publicKey =>
            {
                if (publicKey is Ed25519PublicKey ed25519PublicKey)
                    return new AccountAuthenticatorEd25519(ed25519PublicKey, invalidSignature);

                if (publicKey is KeylessPublicKey keylessPublicKey)
                    return new AccountAuthenticatorSingleKey(
                        keylessPublicKey,
                        Keyless.GetSimulationSignature()
                    );
                if (publicKey is FederatedKeylessPublicKey federatedKeylessPublicKey)
                    return new AccountAuthenticatorSingleKey(
                        federatedKeylessPublicKey,
                        Keyless.GetSimulationSignature()
                    );

                return new AccountAuthenticatorSingleKey(publicKey, invalidSignature);
            },
            verifyingKey =>
            {
                if (verifyingKey is SingleKey singleKey)
                    return new AccountAuthenticatorSingleKey(singleKey.PublicKey, invalidSignature);

                if (verifyingKey is MultiKey multiKey)
                    return new AccountAuthenticatorMultiKey(
                        multiKey,
                        MultiKey.GetSimulationSignature(multiKey)
                    );

                throw new ArgumentException(
                    $"{verifyingKey.GetType().Name} is not supported for simulation."
                );
            }
        );
    }

    #endregion

    #region GenerateSignedTransaction

    public static SignedTransaction GeneratedSignedTransaction(SubmitTransactionData data)
    {
        TransactionAuthenticator? transactionAuthenticator = null;
        if (data.Transaction.FeePayerAddress != null)
        {
            if (data.FeePayerAuthenticator == null)
                throw new ArgumentException(
                    "FeePayerAuthenticator is required for transactions that have a FeePayer address"
                );
            transactionAuthenticator = new TransactionAuthenticatorFeePayer(
                data.SenderAuthenticator,
                data.Transaction.SecondarySignerAddresses ?? [],
                data.AdditionalSignersAuthenticators ?? [],
                (data.Transaction.FeePayerAddress, data.FeePayerAuthenticator)
            );
        }
        else if (data.Transaction.SecondarySignerAddresses != null)
        {
            // Make sure that there are enough additional authenticators for the secondary signers addresses
            if (
                data.Transaction.SecondarySignerAddresses?.Count > 0
                && data.AdditionalSignersAuthenticators == null
            )
                throw new ArgumentException(
                    "AdditionalSignersAuthenticators is required for transactions that have secondary signer addresses"
                );
            transactionAuthenticator = new TransactionAuthenticatorMultiAgent(
                data.SenderAuthenticator,
                data.Transaction.SecondarySignerAddresses ?? [],
                data.AdditionalSignersAuthenticators ?? []
            );
        }
        else if (data.SenderAuthenticator is AccountAuthenticatorEd25519 ed25519Authenticator)
        {
            transactionAuthenticator = new TransactionAuthenticatorEd25519(
                ed25519Authenticator.PublicKey,
                ed25519Authenticator.Signature
            );
        }
        else if (data.SenderAuthenticator is AccountAuthenticatorSingleKey singleKeyAuthenticator)
        {
            transactionAuthenticator = new TransactionAuthenticatorSingleSender(
                singleKeyAuthenticator
            );
        }
        else if (data.SenderAuthenticator is AccountAuthenticatorMultiKey multiKeyAuthenticator)
        {
            transactionAuthenticator = new TransactionAuthenticatorSingleSender(
                multiKeyAuthenticator
            );
        }

        if (transactionAuthenticator == null)
            throw new ArgumentException("Invalid authentication scheme");

        return new SignedTransaction(data.Transaction.RawTransaction, transactionAuthenticator);
    }

    public static SignedTransaction GenerateSignedTransactionForSimulation(
        SimulateTransactionData data
    )
    {
        AccountAuthenticator senderAuthenticator = GetAuthenticatorForSimulation(
            data.SignerPublicKey
        );

        if (data.Transaction.FeePayerAddress != null)
        {
            if (data.FeePayerPublicKey == null)
                throw new ArgumentException(
                    "FeePayerPublicKey is required for transactions that have a FeePayer address"
                );

            FeePayerRawTransaction transactionToSign = new(
                data.Transaction.RawTransaction,
                data.Transaction.SecondarySignerAddresses ?? [],
                data.Transaction.FeePayerAddress
            );

            List<AccountAuthenticator> secondaryAccountAuthenticators =
                data.SecondarySignersPublicKeys?.Select(GetAuthenticatorForSimulation).ToList()
                ?? [];

            AccountAuthenticator feePayerAuthenticator = GetAuthenticatorForSimulation(
                (OneOf<PublicKey, IVerifyingKey>)data.FeePayerPublicKey
            );

            TransactionAuthenticatorFeePayer feePayerTransactionAuthenticator = new(
                senderAuthenticator,
                data.Transaction.SecondarySignerAddresses ?? [],
                secondaryAccountAuthenticators,
                (data.Transaction.FeePayerAddress, feePayerAuthenticator)
            );

            return new SignedTransaction(
                transactionToSign.RawTransaction,
                feePayerTransactionAuthenticator
            );
        }

        if (data.Transaction.SecondarySignerAddresses != null)
        {
            if (
                data.Transaction.SecondarySignerAddresses.Count > 0
                && data.SecondarySignersPublicKeys == null
            )
                throw new ArgumentException(
                    "SecondarySignersPublicKeys is required for transactions that have secondary signer addresses"
                );

            MultiAgentRawTransaction transactionToSign = new(
                data.Transaction.RawTransaction,
                data.Transaction.SecondarySignerAddresses
            );

            List<AccountAuthenticator> secondaryAccountAuthenticators =
                data.SecondarySignersPublicKeys?.Select(GetAuthenticatorForSimulation).ToList()
                ?? [];

            TransactionAuthenticatorMultiAgent multiAgentTransactionAuthenticator = new(
                senderAuthenticator,
                data.Transaction.SecondarySignerAddresses,
                secondaryAccountAuthenticators
            );
            return new SignedTransaction(
                transactionToSign.RawTransaction,
                multiAgentTransactionAuthenticator
            );
        }

        TransactionAuthenticator transactionAuthenticator = senderAuthenticator switch
        {
            AccountAuthenticatorEd25519 ed25519Authenticator => new TransactionAuthenticatorEd25519(
                ed25519Authenticator.PublicKey,
                ed25519Authenticator.Signature
            ),
            _ => throw new ArgumentException("Invalid authentication scheme"),
        };

        return new SignedTransaction(data.Transaction.RawTransaction, transactionAuthenticator);
    }

    #endregion

    #region GenerateTransaction

    public class GenerateTransactionOptions(
        ulong? maxGasAmount = null,
        ulong? gasUnitPrice = null,
        ulong? expireTimestamp = null,
        ulong? accountSequenceNumber = null,
        TransactionExtraConfig? extraConfig = null
    )
    {
        public ulong? MaxGasAmount = maxGasAmount;
        public ulong? GasUnitPrice = gasUnitPrice;
        public ulong? ExpireTimestamp = expireTimestamp;
        public ulong? AccountSequenceNumber = accountSequenceNumber;
        public TransactionExtraConfig? ExtraConfig = extraConfig;
    }

    public static async Task<SimpleTransaction> GenerateTransaction(
        AptosClient client,
        AccountAddress sender,
        GenerateEntryFunctionPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    )
    {
        var payload = await GenerateTransactionPayload(client, data);
        return await BuildRawTransaction(client, payload, sender, withFeePayer, options);
    }

    public static async Task<SimpleTransaction> GenerateTransaction(
        AptosClient client,
        AccountAddress sender,
        GenerateScriptPayloadData data,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    )
    {
        var payload = GenerateTransactionPayload(client, data);
        return await BuildRawTransaction(client, payload, sender, withFeePayer, options);
    }

    public static async Task<MultiAgentTransaction> GenerateTransaction(
        AptosClient client,
        AccountAddress sender,
        GenerateEntryFunctionPayloadData data,
        List<AccountAddress> secondarySignerAddresses,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    )
    {
        var payload = await GenerateTransactionPayload(client, data);
        return await BuildRawTransaction(
            client,
            payload,
            sender,
            secondarySignerAddresses,
            withFeePayer,
            options
        );
    }

    #endregion

    #region GenerateTransactionPayload

    public static TransactionScriptPayload GenerateTransactionPayload(
        AptosClient _,
        GenerateScriptPayloadData data
    ) =>
        new(
            new(
                data.Bytecode,
                Utilities.StandardizeTypeTags(data.TypeArguments ?? []),
                data.FunctionArguments ?? []
            )
        );

    public static TransactionPayload GenerateTransactionPayload(
        AptosClient client,
        GenerateMultisigPayloadData data
    ) => throw new NotImplementedException();

    public static async Task<TransactionEntryFunctionPayload> GenerateTransactionPayload(
        AptosClient client,
        GenerateEntryFunctionPayloadData data
    )
    {
        (string moduleAddress, string moduleName, string functionName) =
            Utilities.ParseFunctionParts(data.Function);

        EntryFunctionAbi functionAbi =
            data.Abi
            ?? await Memoize.MemoAsync(
                async () =>
                    await client.Contract.GetEntryFunctionAbi(
                        moduleAddress,
                        moduleName,
                        functionName
                    ),
                $"entry-function-abi-{moduleAddress}-{moduleName}-{functionName}",
                1000 * 60 * 5 // 5 minutes
            )();

        EntryFunction payload = EntryFunction.Generate(
            data.Function,
            data.FunctionArguments ?? [],
            data.TypeArguments ?? [],
            functionAbi
        );
        return new TransactionEntryFunctionPayload(payload);
    }

    public static async Task<EntryFunction> GenerateViewFunctionPayload(
        AptosClient client,
        GenerateViewFunctionPayloadData data
    )
    {
        (string moduleAddress, string moduleName, string functionName) =
            Utilities.ParseFunctionParts(data.Function);

        ViewFunctionAbi functionAbi =
            data.Abi
            ?? await Memoize.MemoAsync(
                async () =>
                    await client.Contract.GetViewFunctionAbi(
                        moduleAddress,
                        moduleName,
                        functionName
                    ),
                $"view-function-abi-{moduleAddress}-{moduleName}-{functionName}",
                1000 * 60 * 5 // 5 minutes
            )();

        data.Abi = functionAbi;

        return EntryFunction.Generate(
            data.Function,
            data.FunctionArguments ?? [],
            data.TypeArguments ?? [],
            functionAbi
        );
    }

    #endregion

    #region BuildRawTransaction

    public static async Task<MultiAgentTransaction> BuildRawTransaction(
        AptosClient client,
        TransactionPayload payload,
        AccountAddress sender,
        List<AccountAddress> secondarySignerAddresses,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    )
    {
        AccountAddress? feePayerAddress = withFeePayer ? AccountAddress.ZERO : null;
        RawTransaction rawTxn = await GenerateRawTransaction(
            client,
            sender,
            payload,
            feePayerAddress,
            options
        );
        return new MultiAgentTransaction(rawTxn, secondarySignerAddresses, feePayerAddress);
    }

    public static async Task<SimpleTransaction> BuildRawTransaction(
        AptosClient client,
        TransactionPayload payload,
        AccountAddress sender,
        bool withFeePayer = false,
        GenerateTransactionOptions? options = null
    )
    {
        AccountAddress? feePayerAddress = withFeePayer ? AccountAddress.ZERO : null;
        RawTransaction rawTxn = await GenerateRawTransaction(
            client,
            sender,
            payload,
            withFeePayer ? AccountAddress.ZERO : null,
            options
        );
        return new SimpleTransaction(rawTxn, feePayerAddress);
    }

    internal static bool CanSkipSequenceNumberFetch(GenerateTransactionOptions? options)
    {
        return options?.AccountSequenceNumber != null || options?.ExtraConfig?.HasReplayProtectionNonce() == true;
    }

    public static async Task<RawTransaction> GenerateRawTransaction(
        AptosClient client,
        AccountAddress sender,
        TransactionPayload payload,
        AccountAddress? feePayerAddress,
        GenerateTransactionOptions? options = null
    )
    {
        async Task<byte> GetChainId()
        {
            if (client.Config.NetworkConfig.ChainId == -1)
                return (await client.Block.GetLedgerInfo()).ChainId;
            return (byte)client.Config.NetworkConfig.ChainId;
        }

        async Task<ulong> GetGasUnitPrice()
        {
            if (options?.GasUnitPrice != null)
                return options.GasUnitPrice.Value;
            return (await client.Gas.GetGasPriceEstimation()).GasEstimate;
        }

        async Task<ulong> GetSequenceNumber()
        {
            if (CanSkipSequenceNumberFetch(options))
                return options?.AccountSequenceNumber ?? 0;

            try
            {
                return (await client.Account.GetInfo(sender)).SequenceNumber;
            }
            catch (Exception e)
            {
                // Check if is sponsored transaction to honor AIP-52 (https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-52.md)
                //
                // Handle sponsored transaction generation with the option that the main signer has not been created on chain
                // If the main signer has not been created, assign sequence number 0.
                if (
                    feePayerAddress != null
                    && AccountAddress.From(feePayerAddress).Equals(AccountAddress.ZERO)
                )
                {
                    return 0;
                }
                throw e;
            }
        }

        var (chainIdTask, gasUnitPriceTask, sequenceNumberTask) = (
            GetChainId(),
            GetGasUnitPrice(),
            GetSequenceNumber()
        );
        await Task.WhenAll(chainIdTask, gasUnitPriceTask, sequenceNumberTask);

        ChainId chainId = new(await chainIdTask);
        ulong sequenceNumber = await sequenceNumberTask;
        ulong gasUnitPrice = await gasUnitPriceTask;
        if (options?.ExtraConfig != null)
        {
            payload = TransactionInnerPayload.FromLegacy(payload, options.ExtraConfig);
        }

        return new RawTransaction(
            sender: AccountAddress.From(sender),
            sequenceNumber,
            payload: payload,
            maxGasAmount: options?.MaxGasAmount ?? Constants.DEFAULT_MAX_GAS_AMOUNT,
            gasUnitPrice,
            expirationTimestampSecs: options?.ExpireTimestamp
                ?? (ulong)DateTime.Now.ToUnixTimestamp() + Constants.DEFAULT_TXN_EXP_SEC,
            chainId
        );
    }

    #endregion
}
