namespace Aptos;

public class ChainId(byte chainId) : Serializable
{
    public readonly byte Value = chainId;

    public override void Serialize(Serializer s) => s.U8(Value);

    public static ChainId Deserialize(Deserializer d) => new(d.U8());
}
