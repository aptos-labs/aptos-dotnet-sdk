namespace Aptos;

public class TransactionInnerPayload(InnerTransactionPayload innerPayload) : TransactionPayload
{
    public readonly InnerTransactionPayload InnerPayload = innerPayload;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionPayloadVariant.InnerPayload);
        InnerPayload.Serialize(s);
    }

    public static new TransactionInnerPayload Deserialize(Deserializer d) =>
        new(InnerTransactionPayload.Deserialize(d));
}
