namespace Aptos;

public class TransactionEntryFunctionPayload(EntryFunction function) : TransactionPayload
{
    public readonly EntryFunction Function = function;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionPayloadVariant.EntryFunction);
        Function.Serialize(s);
    }

    public static new TransactionEntryFunctionPayload Deserialize(Deserializer d) => new(EntryFunction.Deserialize(d));
}