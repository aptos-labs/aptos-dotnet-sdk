namespace Aptos;

using Aptos.Indexer.GraphQL;

public class CdnAssetUris(string? cdnImageUri, string? cdnJsonUri, string? cdnAnimationUri)
{
    public CdnAssetUris(ICdnAssetUris cdnAssetUris)
        : this(
            cdnAssetUris.Cdn_image_uri,
            cdnAssetUris.Cdn_json_uri,
            cdnAssetUris.Cdn_animation_uri
        ) { }

    public string? CdnImageUri = cdnImageUri;

    public string? CdnJsonUri = cdnJsonUri;

    public string? CdnAnimationUri = cdnAnimationUri;
}
