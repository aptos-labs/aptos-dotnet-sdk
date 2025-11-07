namespace Aptos;

enum TransactionExecutableVariant : uint
{
    Script = 0,
    EntryFunction = 1,
    Empty = 2
}

public abstract class TransactionExecutable : Serializable
{
    public static TransactionExecutable Deserialize(Deserializer d)
    {
        TransactionExecutableVariant variant = (TransactionExecutableVariant)d.Uleb128AsU32();
        return variant switch
        {
            TransactionExecutableVariant.Script => TransactionScriptExecutable.Deserialize(d),
            TransactionExecutableVariant.EntryFunction => TransactionEntryFunctionExecutable.Deserialize(
                d
            ),
            TransactionExecutableVariant.Empty => TransactionEmptyExecutable.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}