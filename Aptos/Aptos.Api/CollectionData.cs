namespace Aptos;

using Aptos.Indexer.GraphQL;

public class CollectionData(
    string collectionId,
    string collectionName,
    AccountAddress creatorAddress,
    decimal currentSupply,
    string description,
    decimal? maxSupply,
    bool? mutableDescription,
    bool? mutableUri,
    string? tableHandleV1,
    string tokenStandard,
    string uri,
    object? collectionProperties,
    CdnAssetUris? cdnAssetUris
)
{
    public CollectionData(ICollectionData currentCollection)
        : this(
            currentCollection.Collection_id,
            currentCollection.Collection_name,
            AccountAddress.From(currentCollection.Creator_address),
            currentCollection.Current_supply,
            currentCollection.Description,
            currentCollection.Max_supply,
            currentCollection.Mutable_description,
            currentCollection.Mutable_uri,
            currentCollection.Table_handle_v1,
            currentCollection.Token_standard,
            currentCollection.Uri,
            currentCollection.Collection_properties,
            currentCollection.Cdn_asset_uris != null
                ? new CdnAssetUris(currentCollection.Cdn_asset_uris)
                : null
        ) { }

    public string CollectionId = collectionId;

    public string CollectionName = collectionName;

    public AccountAddress CreatorAddress = creatorAddress;

    public decimal CurrentSupply = currentSupply;

    public string Description = description;

    public decimal? MaxSupply = maxSupply;

    public bool? MutableDescription = mutableDescription;

    public bool? MutableUri = mutableUri;

    public string? TableHandleV1 = tableHandleV1;

    public string TokenStandard = tokenStandard;

    public string Uri = uri;

    public object? CollectionProperties = collectionProperties;

    public CdnAssetUris? CdnAssetUris = cdnAssetUris;
}
