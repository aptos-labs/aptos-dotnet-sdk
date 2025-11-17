namespace Aptos;

enum InnerTransactionPayloadVariant : uint
{
    V1 = 0,
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

    public static InnerTransactionPayload FromLegacy(
        TransactionPayload payload,
        TransactionExtraConfig extraConfig
    )
    {
        return payload switch
        {
            TransactionScriptPayload scriptPayload => new InnerTransactionPayloadV1(
                new TransactionScriptExecutable(scriptPayload.Script),
                extraConfig
            ),
            TransactionEntryFunctionPayload entryFunctionPayload => new InnerTransactionPayloadV1(
                new TransactionEntryFunctionExecutable(entryFunctionPayload.Function),
                extraConfig
            ),
            TransactionInnerPayload innerPayload => innerPayload.InnerPayload,
            _ => throw new ArgumentException($"Invalid payload: {payload.GetType().Name}"),
        };
    }
}
