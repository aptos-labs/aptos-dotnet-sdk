namespace Aptos;

public class TransactionEmptyExecutable() : TransactionExecutable
{
    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionExecutableVariant.Empty);
    }

    public static new TransactionEmptyExecutable Deserialize(Deserializer d) =>
        new();
}
