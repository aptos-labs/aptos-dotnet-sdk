namespace Aptos;

using Newtonsoft.Json;

public abstract class Proof : Serializable
{
    public override string ToString() => Hex.FromHexInput(BcsToBytes()).ToString();
}

[JsonConverter(typeof(G1BytesConverter))]
public class G1Bytes : Serializable
{
    public byte[] Data;

    public G1Bytes(string data)
        : this(Hex.FromHexInput(data).ToByteArray()) { }

    public G1Bytes(byte[] data)
    {
        Data = data;
        if (Data.Length != 32)
            throw new ArgumentException("Invalid G1 bytes length");
    }

    public override void Serialize(Serializer s) => s.FixedBytes(Data);

    public static G1Bytes Deserialize(Deserializer d) => new(d.FixedBytes(32));
}

public class G1BytesConverter : JsonConverter<G1Bytes>
{
    public override bool CanWrite => false;

    public override G1Bytes? ReadJson(
        JsonReader reader,
        Type objectType,
        G1Bytes? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.String)
        {
            var hexString = (string)reader.Value!;
            return new G1Bytes(hexString);
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, G1Bytes? value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}

[JsonConverter(typeof(G2BytesConverter))]
public class G2Bytes : Serializable
{
    public byte[] Data;

    public G2Bytes(string data)
        : this(Hex.FromHexInput(data).ToByteArray()) { }

    public G2Bytes(byte[] data)
    {
        Data = Hex.FromHexInput(data).ToByteArray();
        if (Data.Length != 64)
            throw new ArgumentException("Invalid G2 bytes length");
    }

    public override void Serialize(Serializer s) => s.FixedBytes(Data);

    public static G2Bytes Deserialize(Deserializer d) => new(d.FixedBytes(64));
}

public class G2BytesConverter : JsonConverter<G2Bytes>
{
    public override bool CanWrite => false;

    public override G2Bytes? ReadJson(
        JsonReader reader,
        Type objectType,
        G2Bytes? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.String)
        {
            var hexString = (string)reader.Value!;
            return new G2Bytes(hexString);
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, G2Bytes? value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}

public class Groth16Zkp : Proof
{
    [JsonProperty("a")]
    public G1Bytes A;

    [JsonProperty("b")]
    public G2Bytes B;

    [JsonProperty("c")]
    public G1Bytes C;

    [JsonConstructor]
    public Groth16Zkp(string a, string b, string c)
        : this(new G1Bytes(a), new G2Bytes(b), new G1Bytes(c)) { }

    public Groth16Zkp(byte[] a, byte[] b, byte[] c)
        : this(new G1Bytes(a), new G2Bytes(b), new G1Bytes(c)) { }

    public Groth16Zkp(G1Bytes a, G2Bytes b, G1Bytes c)
    {
        A = a;
        B = b;
        C = c;
    }

    public override void Serialize(Serializer s)
    {
        A.Serialize(s);
        B.Serialize(s);
        C.Serialize(s);
    }

    public static Groth16Zkp Deserialize(Deserializer d) =>
        new(G1Bytes.Deserialize(d), G2Bytes.Deserialize(d), G1Bytes.Deserialize(d));
}

public enum ZkpVariant
{
    Groth16 = 0,
}

public class ZkProof : Serializable
{
    public readonly Proof Proof;

    public readonly ZkpVariant Variant;

    public ZkProof(Proof proof, ZkpVariant variant)
    {
        Proof = proof;
        Variant = variant;
    }

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Variant);
        Proof.Serialize(s);
    }

    public static ZkProof Deserialize(Deserializer d)
    {
        ZkpVariant variant = (ZkpVariant)d.Uleb128AsU32();
        return variant switch
        {
            ZkpVariant.Groth16 => new ZkProof(Groth16Zkp.Deserialize(d), ZkpVariant.Groth16),
            _ => throw new ArgumentException("Invalid proof variant"),
        };
    }
}
