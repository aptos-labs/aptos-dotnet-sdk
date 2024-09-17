namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum SignatureVariant
{
    [EnumMember(Value = "ed25519")]
    Ed25519 = 0,
    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa = 1,
    [EnumMember(Value = "keyless")]
    Keyless = 3,
    // Unified Signatures variants (these do not exist on the blockchain)
    [EnumMember(Value = "multikey")]
    MultiKey = 99,
}


/// <summary>
/// Base signatures for anything signed (not specific to signing transactions/messages). This may include all signatures needed for ZK proofs, Certificates, etc.
/// </summary>
[JsonConverter(typeof(SignatureConverter))]
public abstract class Signature(SignatureVariant type) : Serializable
{
    [JsonProperty("type")]
    public SignatureVariant Type = type;

    [JsonProperty("value")]
    public abstract Hex Value { get; }

    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();

    public static Signature Deserialize(Deserializer d)
    {
        SignatureVariant variant = (SignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            SignatureVariant.Ed25519 => Ed25519Signature.Deserialize(d),
            SignatureVariant.Secp256k1Ecdsa => Secp256k1Signature.Deserialize(d),
            SignatureVariant.Keyless => KeylessSignature.Deserialize(d),
            SignatureVariant.MultiKey => MultiKeySignature.Deserialize(d),
            _ => throw new ArgumentException("Invalid signature variant"),
        };
    }
}


public class SignatureConverter : JsonConverter<Signature>
{
    public override Signature? ReadJson(JsonReader reader, Type objectType, Signature? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        AnyValue? anyValue = JsonConvert.DeserializeObject<AnyValue>(jsonObject.ToString());
        if (anyValue == null) throw new Exception("Invalid Signature shape");

        return type switch
        {
            "ed25519" => new Ed25519Signature(anyValue.Value),
            "secp256k1_ecdsa" => new Secp256k1Signature(anyValue.Value),
            "keyless" => KeylessSignature.Deserialize(new Deserializer(anyValue.Value)),
            _ => throw new Exception($"Unknown signature type: {type}"),
        };
    }

    public override void WriteJson(JsonWriter writer, Signature? value, JsonSerializer serializer)
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