namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum PublicKeySignatureVariant
{
    [EnumMember(Value = "ed25519")]
    Ed25519 = 0,

    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa = 1,

    [EnumMember(Value = "keyless")]
    Keyless = 3,
}

/// <summary>
/// Cryptographic signatures typically using a private key to sign a message or transaction.
/// </summary>
[JsonConverter(typeof(PublicKeySignatureConverter))]
public abstract class PublicKeySignature(PublicKeySignatureVariant type) : Signature
{
    [JsonProperty("type")]
    public PublicKeySignatureVariant Type = type;

    [JsonProperty("value")]
    public abstract Hex Value { get; }

    public abstract override byte[] ToByteArray();

    public static PublicKeySignature Deserialize(Deserializer d)
    {
        PublicKeySignatureVariant variant = (PublicKeySignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeySignatureVariant.Ed25519 => Ed25519Signature.Deserialize(d),
            PublicKeySignatureVariant.Secp256k1Ecdsa => Secp256k1Signature.Deserialize(d),
            PublicKeySignatureVariant.Keyless => KeylessSignature.Deserialize(d),
            _ => throw new ArgumentException("Invalid signature variant"),
        };
    }
}

public class PublicKeySignatureConverter : JsonConverter<PublicKeySignature>
{
    public override PublicKeySignature? ReadJson(
        JsonReader reader,
        Type objectType,
        PublicKeySignature? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        AnyValue? anyValue = JsonConvert.DeserializeObject<AnyValue>(jsonObject.ToString());
        if (anyValue == null)
            throw new Exception("Invalid Signature shape");

        return type switch
        {
            "ed25519" => new Ed25519Signature(anyValue.Value),
            "secp256k1_ecdsa" => new Secp256k1Signature(anyValue.Value),
            "keyless" => KeylessSignature.Deserialize(new Deserializer(anyValue.Value)),
            _ => throw new Exception($"Unknown signature type: {type}"),
        };
    }

    public override void WriteJson(
        JsonWriter writer,
        PublicKeySignature? value,
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
