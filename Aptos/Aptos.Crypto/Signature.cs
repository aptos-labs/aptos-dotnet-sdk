namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base signatures for anything signed (not specific to signing transactions/messages). This may include all signatures needed for ZK proofs, Certificates, etc.
/// </summary>
public abstract class Signature : Serializable
{
    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}

[JsonConverter(typeof(StringEnumConverter))]
public enum SignatureVariant
{
    [EnumMember(Value = "ed25519")]
    Ed25519 = 0,
    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa = 1,
    [EnumMember(Value = "keyless")]
    Keyless = 3,
}

/// <summary>
/// Signature for results of signing transactions/messages using a authentication scheme (e.g. Ed25519, Keyless, etc.)
/// </summary>
/// <param name="type"> 
/// The type of the signature (e.g. Ed25519, Keyless, etc.)
/// </param>
[JsonConverter(typeof(LegacySignatureConverter))]
public abstract class LegacySignature(SignatureVariant type) : Signature
{
    [JsonProperty("type")]
    public SignatureVariant Type = type;

    [JsonProperty("value")]
    public abstract Hex Value { get; }

}

public abstract class UnifiedSignature : Signature { }

public class LegacySignatureConverter : JsonConverter<LegacySignature>
{
    public override LegacySignature? ReadJson(JsonReader reader, Type objectType, LegacySignature? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        AnyValue? anyValue = JsonConvert.DeserializeObject<AnyValue>(jsonObject.ToString());
        if (anyValue == null) throw new Exception("Invalid LegacySignature shape");

        return type switch
        {
            "ed25519" => new Ed25519Signature(anyValue.Value),
            "secp256k1_ecdsa" => new Secp256k1Signature(anyValue.Value),
            "keyless" => KeylessSignature.Deserialize(new Deserializer(anyValue.Value)),
            _ => throw new Exception($"Unknown signature type: {type}"),
        };
    }

    public override void WriteJson(JsonWriter writer, LegacySignature? value, JsonSerializer serializer)
    {
        if (value == null) writer.WriteNull();
        else
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(JsonConvert.SerializeObject(value!.Type).Replace("\"", ""));
            writer.WritePropertyName("value");
            writer.WriteValue(value!.Value.ToString());
            writer.WriteEndObject();
        }
    }
}