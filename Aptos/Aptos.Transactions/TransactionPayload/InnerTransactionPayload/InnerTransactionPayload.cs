namespace Aptos;

enum InnerTransactionPayloadVariant : uint
{
    V1 = 0
}

public abstract class InnerTransactionPayload : Serializable
{
    public static InnerTransactionPayload Deserialize(Deserializer d)
    {
        InnerTransactionPayloadVariant variant = (InnerTransactionPayloadVariant)d.Uleb128AsU32();
        return variant switch
        {
            InnerTransactionPayloadVariant.V1 => InnerTransactionPayloadV1.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}