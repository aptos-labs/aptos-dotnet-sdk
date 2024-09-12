namespace Aptos;

public enum TransactionAuthenticatorVariant : uint
{
    Ed25519 = 0,
    MultiEd25519 = 1,
    MultiAgent = 2,
    FeePayer = 3,
    SingleSender = 4,
}


public abstract class TransactionAuthenticator : Serializable
{
    public static TransactionAuthenticator Deserialize(Deserializer d)
    {
        TransactionAuthenticatorVariant variant = (TransactionAuthenticatorVariant)d.Uleb128AsU32();
        return variant switch
        {
            TransactionAuthenticatorVariant.Ed25519 => TransactionAuthenticatorEd25519.Deserialize(d),
            TransactionAuthenticatorVariant.FeePayer => TransactionAuthenticatorFeePayer.Deserialize(d),
            TransactionAuthenticatorVariant.SingleSender => TransactionAuthenticatorSingleSender.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}