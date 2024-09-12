namespace Aptos;

using System.Text.Json;
using Aptos.Exceptions;
using Aptos.Indexer.GraphQL;

public class TokenOwnership(decimal amount, DigitalAssetData currentTokenData, bool? isSoulboundV2, AccountAddress ownerAddress, decimal propertyVersionV1, string storageId, JsonElement? tokenPropertiesMutatedV1) : DigitalAssetOwnership(amount, currentTokenData, isSoulboundV2, ownerAddress, propertyVersionV1, storageId, tokenPropertiesMutatedV1) { }

public class DigitalAssetOwnership(decimal amount, DigitalAssetData currentTokenData, bool? isSoulboundV2, AccountAddress ownerAddress, decimal propertyVersionV1, string storageId, JsonElement? tokenPropertiesMutatedV1)
{

    public DigitalAssetOwnership(IGetDigitalAssetOwnerships_Current_token_ownerships_v2 ownership) : this(
        ownership.Amount,
        ownership.Current_token_data != null ? new DigitalAssetData(ownership.Current_token_data) : throw new UnexpectedResponseException($"Expected DigitalAssetOwnership({ownership.Owner_address}) to have current token data"),
        ownership.Is_soulbound_v2,
        AccountAddress.From(ownership.Owner_address),
        ownership.Property_version_v1,
        ownership.Storage_id,
        ownership.Token_properties_mutated_v1
    )
    { }

    public decimal Amount = amount;

    public DigitalAssetData CurrentTokenData = currentTokenData;

    public bool? IsSoulbound = isSoulboundV2;

    public AccountAddress OwnerAddress = ownerAddress;

    public decimal PropertyVersion = propertyVersionV1;

    public string StorageId = storageId;

    public JsonElement? TokenProperties = tokenPropertiesMutatedV1;

    public TokenOwnership ToTokenOwnership() => new(Amount, CurrentTokenData, IsSoulbound, OwnerAddress, PropertyVersion, StorageId, TokenProperties);

}

public class DigitalAssetData(string description, bool? isFungibleV2, decimal? largestPropertyVersionV1, string tokenDataId, string tokenName, JsonElement? tokenProperties, string tokenStandard, string tokenUri, CollectionData currentCollection, CdnAssetUris? cdnAssetUris)
{

    public DigitalAssetData(IDigitalAssetData data) : this(
        data.Description,
        data.Is_fungible_v2,
        data.Largest_property_version_v1,
        data.Token_data_id,
        data.Token_name,
        data.Token_properties,
        data.Token_standard,
        data.Token_uri,
        data.Current_collection != null ? new CollectionData(data.Current_collection) : throw new UnexpectedResponseException($"Expected DigitalAssetOwnership({data.Token_data_id}) to have collection data"),
        data.Cdn_asset_uris != null ? new CdnAssetUris(data.Cdn_asset_uris) : null
    )
    { }

    public string Description = description;

    public bool? IsFungible = isFungibleV2;

    public decimal? LargestPropertyVersion = largestPropertyVersionV1;

    public string TokenDataId = tokenDataId;

    public string TokenName = tokenName;

    public JsonElement? TokenProperties = tokenProperties;

    public string TokenStandard = tokenStandard;

    public string TokenUri = tokenUri;

    public CollectionData? CurrentCollection = currentCollection;

    public CdnAssetUris? CdnAssetUris = cdnAssetUris;

}