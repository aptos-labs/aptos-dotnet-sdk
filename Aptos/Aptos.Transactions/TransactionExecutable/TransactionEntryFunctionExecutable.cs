namespace Aptos;

public class TransactionEntryFunctionExecutable(EntryFunction function) : TransactionExecutable
{
    public readonly EntryFunction Function = function;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionExecutableVariant.EntryFunction);
        Function.Serialize(s);
    }

    public static new TransactionEntryFunctionExecutable Deserialize(Deserializer d) =>
        new(EntryFunction.Deserialize(d));
}
