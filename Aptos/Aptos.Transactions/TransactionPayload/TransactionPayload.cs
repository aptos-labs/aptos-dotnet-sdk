namespace Aptos;

enum TransactionPayloadVariant : uint
{
    Script = 0,
    EntryFunction = 2,
}

public abstract class TransactionPayload : Serializable
{
    public static TransactionPayload Deserialize(Deserializer d)
    {
        TransactionPayloadVariant variant = (TransactionPayloadVariant)d.Uleb128AsU32();
        return variant switch
        {
            TransactionPayloadVariant.Script => TransactionScriptPayload.Deserialize(d),
            TransactionPayloadVariant.EntryFunction => TransactionEntryFunctionPayload.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}