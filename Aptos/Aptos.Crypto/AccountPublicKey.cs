namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

public abstract class AccountPublicKey : PublicKey
{
    public abstract AuthenticationKey AuthKey();
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PublicKeyVariant : uint
{
    [EnumMember(Value = "ed25519")]
    Ed25519,

    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa,

    [EnumMember(Value = "secp256r1_ecdsa")]
    Secp256r1Ecdsa,

    [EnumMember(Value = "keyless")]
    Keyless,
}

[JsonConverter(typeof(LegacyPublicKeyConverter))]
public interface ILegacyPublicKey
{
    [JsonProperty("type")]
    public PublicKeyVariant Type { get; }

    [JsonProperty("value")]
    public Hex Value { get; }
}

public abstract class LegacyPublicKey(PublicKeyVariant type) : PublicKey, ILegacyPublicKey
{
    private readonly PublicKeyVariant _type = type;
    public PublicKeyVariant Type => _type;

    public abstract Hex Value { get; }
}

public class LegacyPublicKeyConverter : JsonConverter<ILegacyPublicKey>
{
    public override ILegacyPublicKey? ReadJson(
        JsonReader reader,
        Type objectType,
        ILegacyPublicKey? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        AnyValue? anyValue = JsonConvert.DeserializeObject<AnyValue>(jsonObject.ToString());
        if (anyValue == null)
            throw new Exception("Invalid public key shape");

        Deserializer deserializer = new(anyValue.Value);
        deserializer.Uleb128AsU32();

        return type switch
        {
            "ed25519" => new Ed25519PublicKey(anyValue.Value),
            "secp256k1_ecdsa" => new Secp256k1PublicKey(anyValue.Value),
            "keyless" => KeylessPublicKey.Deserialize(new Deserializer(anyValue.Value)),
            _ => throw new Exception($"Unknown public key type: {type}"),
        };
    }

    public override void WriteJson(
        JsonWriter writer,
        ILegacyPublicKey? value,
        JsonSerializer serializer
    )
    {
        if (value == null)
            writer.WriteNull();
        else
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(JsonConvert.SerializeObject(value.Type).Replace("\"", ""));
            writer.WritePropertyName("value");
            writer.WriteValue(value.Value.ToString());
            writer.WriteEndObject();
        }
    }
}

public abstract class UnifiedAccountPublicKey : AccountPublicKey { }

public abstract class LegacyAccountPublicKey(PublicKeyVariant type)
    : AccountPublicKey,
        ILegacyPublicKey
{
    private readonly PublicKeyVariant _type = type;
    public PublicKeyVariant Type => _type;

    public abstract Hex Value { get; }
}
