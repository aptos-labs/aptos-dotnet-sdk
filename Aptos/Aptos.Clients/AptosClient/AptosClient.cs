namespace Aptos;

/// <inheritdoc cref="AptosClient(AptosConfig?)"/>
public partial class AptosClient
{
    public readonly AptosConfig Config;

    public readonly TransactionClient Transaction;

    public readonly BlockClient Block;

    public readonly FaucetClient Faucet;

    public readonly AccountClient Account;

    public readonly ContractClient Contract;

    public readonly GasClient Gas;

    public readonly IndexerClient Indexer;

    public readonly ANSClient Ans;

    public readonly FungibleAssetClient FungibleAsset;

    public readonly DigitalAssetClient DigitalAsset;

    public readonly KeylessClient Keyless;

    public readonly TableClient Table;

    /// <summary>
    /// Creates an instance of the AptosClient with a given <see cref="AptosConfig"/>.
    /// This client is used to interface with the Aptos blockchain and perform various operations such
    /// as querying the blockchain, submitting transactions, and interacting with the Aptos network.
    /// </summary>
    /// <remarks>
    /// By default, the client will use Devnet as the network configuration.
    /// </remarks>
    public AptosClient(AptosConfig? config = null)
    {
        Config = config ?? new AptosConfig();
        Transaction = new(this);
        Block = new(this);
        Faucet = new(this);
        Account = new(this);
        Contract = new(this);
        Gas = new(this);
        Indexer = new(this);
        Ans = new(this);
        FungibleAsset = new(this);
        DigitalAsset = new(this);
        Keyless = new(this);
        Table = new(this);
    }

    #region Contract

    /// <inheritdoc cref="ContractClient.View(GenerateViewFunctionPayloadData, ulong?)"/>
    public async Task<List<object>> View(
        GenerateViewFunctionPayloadData data,
        ulong? ledgerVersion = null
    ) => await Contract.View(data, ledgerVersion);

    /// <inheritdoc cref="ContractClient.View{T}(GenerateViewFunctionPayloadData, ulong?)"/>
    public async Task<T> View<T>(GenerateViewFunctionPayloadData data, ulong? ledgerVersion = null)
        where T : class => await Contract.View<T>(data, ledgerVersion);

    #endregion

    #region Transaction

    /// <inheritdoc cref="TransactionClient.SignAndSubmitTransaction(Aptos.Account, AnyRawTransaction, AccountAuthenticator?, List{AccountAuthenticator}?)"/>
    public async Task<PendingTransactionResponse> SignAndSubmitTransaction(
        Account signer,
        AnyRawTransaction transaction,
        AccountAuthenticator? feePayerAuthenticator = null,
        List<AccountAuthenticator>? additionalSignersAuthenticators = null
    ) =>
        await Transaction.SignAndSubmitTransaction(
            signer,
            transaction,
            feePayerAuthenticator,
            additionalSignersAuthenticators
        );

    #endregion

    #region Block

    /// <inheritdoc cref="BlockClient.GetLedgerInfo()"/>
    public async Task<LedgerInfo> GetLedgerInfo() => await Block.GetLedgerInfo();

    #endregion

    #region Faucet

    /// <inheritdoc cref="FaucetClient.FundAccount(string, ulong)"/>
    public async Task<UserTransactionResponse> FundAccount(string address, ulong amount) =>
        await Faucet.FundAccount(address, amount);

    /// <inheritdoc cref="FaucetClient.FundAccount(AccountAddress, ulong)"/>
    public async Task<UserTransactionResponse> FundAccount(AccountAddress address, ulong amount) =>
        await Faucet.FundAccount(address, amount);

    #endregion

    #region Account

    /// <inheritdoc cref="AccountClient.GetModule(string, string, ulong?)"/>
    public async Task<MoveModuleBytecode> GetModule(
        string address,
        string moduleName,
        ulong? ledgerVersion = null
    ) => await Account.GetModule(address, moduleName, ledgerVersion);

    /// <inheritdoc cref="AccountClient.GetModule(AccountAddress, string, ulong?)"/>
    public async Task<MoveModuleBytecode> GetModule(
        AccountAddress address,
        string moduleName,
        ulong? ledgerVersion = null
    ) => await Account.GetModule(address, moduleName, ledgerVersion);

    /// <inheritdoc cref="AccountClient.GetResource(string, string, ulong?)"/>
    public async Task<MoveResource> GetResource(
        string address,
        string resourceType,
        ulong? ledgerVersion = null
    ) => await Account.GetResource(address, resourceType, ledgerVersion);

    /// <inheritdoc cref="AccountClient.GetResource(AccountAddress, string, ulong?)"/>
    public async Task<MoveResource> GetResource(
        AccountAddress address,
        string resourceType,
        ulong? ledgerVersion = null
    ) => await Account.GetResource(address, resourceType, ledgerVersion);

    /// <inheritdoc cref="AccountClient.GetResources(string, int?, int?)"/>
    public async Task<List<MoveResource>> GetResources(
        string address,
        int? start = null,
        int? limit = null
    ) => await Account.GetResources(address, start, limit);

    /// <inheritdoc cref="AccountClient.GetResources(AccountAddress, int?, int?)"/>
    public async Task<List<MoveResource>> GetResources(
        AccountAddress address,
        int? start = null,
        int? limit = null
    ) => await Account.GetResources(address, start, limit);

    #endregion
}
