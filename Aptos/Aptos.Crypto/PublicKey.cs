namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum PublicKeyVariant : uint
{
    [EnumMember(Value = "ed25519")]
    Ed25519 = 0,
    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa = 1,
    [EnumMember(Value = "keyless")]
    Keyless = 3,
    // Unified public key variants (these do not exist on the blockchain)
    [EnumMember(Value = "multikey")]
    MultiKey = 99,
}

[JsonConverter(typeof(PublicKeyConverter))]
public abstract class PublicKey(PublicKeyVariant type) : Serializable
{
    [JsonProperty("type")]
    public PublicKeyVariant Type = type;

    [JsonProperty("value")]
    public abstract Hex Value { get; }

    public bool VerifySignature(string message, Signature signature) => VerifySignature(SigningMessage.Convert(message), signature);

    public abstract bool VerifySignature(byte[] message, Signature signature);

    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();

    public static PublicKey Deserialize(Deserializer d)
    {
        PublicKeyVariant variant = (PublicKeyVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeyVariant.Ed25519 => new Ed25519PublicKey(d.Bytes()),
            PublicKeyVariant.Secp256k1Ecdsa => new Secp256k1PublicKey(d.Bytes()),
            PublicKeyVariant.Keyless => KeylessPublicKey.Deserialize(d),
            PublicKeyVariant.MultiKey => MultiKey.Deserialize(d),
            _ => throw new ArgumentException("Invalid public key variant"),
        };
    }

    public static bool IsSigningKey(PublicKey publicKey) => publicKey is not MultiKey;

}

public class PublicKeyConverter : JsonConverter<PublicKey>
{
    public override PublicKey? ReadJson(JsonReader reader, Type objectType, PublicKey? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        AnyValue? anyValue = JsonConvert.DeserializeObject<AnyValue>(jsonObject.ToString());
        if (anyValue == null) throw new Exception("Invalid public key shape");

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

    public override void WriteJson(JsonWriter writer, PublicKey? value, JsonSerializer serializer)
    {
        if (value == null) writer.WriteNull();
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