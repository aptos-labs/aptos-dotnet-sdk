namespace Aptos;

using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;

public class CoinBalance(
    decimal? amount,
    string assetType,
    bool isFrozen,
    bool? isPrimary,
    AccountAddress ownerAddress,
    string storageId,
    string? tokenStandard,
    FungibleAssetMetadata metadata
)
    : FungibleAssetBalance(
        amount,
        assetType,
        isFrozen,
        isPrimary,
        ownerAddress,
        storageId,
        tokenStandard,
        metadata
    ) { }

public class FungibleAssetBalance(
    decimal? amount,
    string assetType,
    bool isFrozen,
    bool? isPrimary,
    AccountAddress ownerAddress,
    string storageId,
    string? tokenStandard,
    FungibleAssetMetadata metadata
)
{
    public FungibleAssetBalance(IGetFungibleAssetBalances_Current_fungible_asset_balances balance)
        : this(
            balance.Amount,
            balance.Asset_type
                ?? throw new UnexpectedResponseException(
                    $"Expected FungibleAssetBalance({balance.Owner_address}) to have asset type"
                ),
            balance.Is_frozen,
            balance.Is_primary,
            AccountAddress.From(balance.Owner_address),
            balance.Storage_id,
            balance.Token_standard,
            balance.Metadata != null
                ? new FungibleAssetMetadata(balance.Metadata)
                : throw new UnexpectedResponseException(
                    $"Expected FungibleAssetBalance({balance.Asset_type}) to have metadata"
                )
        ) { }

    public decimal? Amount = amount;
    public string AssetType = assetType;
    public bool IsFrozen = isFrozen;
    public bool? IsPrimary = isPrimary;
    public AccountAddress OwnerAddress = ownerAddress;
    public string StorageId = storageId;
    public string? TokenStandard = tokenStandard;
    public FungibleAssetMetadata Metadata = metadata;

    public CoinBalance ToCoinBalance() =>
        new(
            Amount,
            AssetType,
            IsFrozen,
            IsPrimary,
            OwnerAddress,
            StorageId,
            TokenStandard,
            Metadata
        );
}

public class FungibleAssetMetadata(
    string symbol,
    string name,
    int decimals,
    string creatorAddress,
    string? projectUri,
    string? iconUri
)
{
    public FungibleAssetMetadata(IFungibleAssetMetadata metadata)
        : this(
            metadata.Symbol,
            metadata.Name,
            metadata.Decimals,
            metadata.Creator_address,
            metadata.Project_uri,
            metadata.Icon_uri
        ) { }

    public string Symbol = symbol;
    public string Name = name;
    public int Decimals = decimals;
    public string CreatorAddress = creatorAddress;
    public string? ProjectUri = projectUri;
    public string? IconUri = iconUri;
}
