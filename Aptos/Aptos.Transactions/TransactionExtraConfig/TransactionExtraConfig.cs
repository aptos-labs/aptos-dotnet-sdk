namespace Aptos;

enum TransactionExtraConfigVariant : uint
{
    V1 = 0,
}

public abstract class TransactionExtraConfig : Serializable
{
    public static TransactionExtraConfig Deserialize(Deserializer d)
    {
        TransactionExtraConfigVariant variant = (TransactionExtraConfigVariant)d.Uleb128AsU32();
        return variant switch
        {
            TransactionExtraConfigVariant.V1 => TransactionExtraConfigV1.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
<<<<<<< Updated upstream
=======

    public abstract bool HasReplayProtectionNonce();
>>>>>>> Stashed changes
}