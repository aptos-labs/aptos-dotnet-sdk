namespace Aptos;

using Aptos.Schemes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(AccountSignatureConverter))]
public abstract class AccountSignature(SigningScheme type)
{
    [JsonProperty("type")]
    public SigningScheme Type = type;
}

public class AccountSignatureConverter : JsonConverter<AccountSignature>
{
    static readonly JsonSerializerSettings SpecifiedSubclassConversion =
        new()
        {
            ContractResolver = new SubclassSpecifiedConcreteClassConverter<AccountSignature>(),
        };

    public override AccountSignature? ReadJson(
        JsonReader reader,
        Type objectType,
        AccountSignature? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        return type switch
        {
            "ed25519_signature" => JsonConvert.DeserializeObject<AccountEd25519Signature>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "single_key_signature" => JsonConvert.DeserializeObject<AccountSingleKeySignature>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "multi_key_signature" => JsonConvert.DeserializeObject<AccountMultiKeySignature>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            _ => throw new Exception($"Unknown account signature type: {type}"),
        };
    }

    public override void WriteJson(
        JsonWriter writer,
        AccountSignature? value,
        JsonSerializer serializer
    ) => serializer.Serialize(writer, value);
}

public class AccountEd25519Signature(Hex publicKey, Hex signature)
    : AccountSignature(SigningScheme.Ed25519)
{
    [JsonProperty("public_key")]
    public Hex PublicKey = publicKey;

    [JsonProperty("signature")]
    public Hex Signature = signature;
}

public class AccountSingleKeySignature(ILegacyPublicKey publicKey, LegacySignature signature)
    : AccountSignature(SigningScheme.SingleKey)
{
    [JsonProperty("public_key")]
    public ILegacyPublicKey PublicKey = publicKey;

    [JsonProperty("signature")]
    public LegacySignature Signature = signature;
}

public class AccountMultiKeySignature(
    List<ILegacyPublicKey> publicKeys,
    List<LegacySignature> signatures,
    byte signaturesRequired
) : AccountSignature(SigningScheme.MultiKey)
{
    public class IndexedAccountSignature(byte index, LegacySignature signature)
    {
        [JsonProperty("index")]
        public byte Index = index;

        [JsonProperty("signature")]
        public LegacySignature Signature = signature;
    }

    [JsonProperty("public_keys")]
    public List<ILegacyPublicKey> PublicKeys = publicKeys;

    [JsonProperty("signatures")]
    public List<IndexedAccountSignature> Signatures = signatures
        .Select((sig, i) => new IndexedAccountSignature((byte)i, sig))
        .ToList();

    [JsonProperty("signatures_required")]
    public byte SignaturesRequired = signaturesRequired;
}
