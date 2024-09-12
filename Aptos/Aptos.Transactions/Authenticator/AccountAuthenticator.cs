namespace Aptos;

public enum AccountAuthenticatorVariant : uint
{
    Ed25519 = 0,
    MultiEd25519 = 1,
    SingleKey = 2,
    MultiKey = 3,
}


public abstract class AccountAuthenticator : Serializable
{
    public static AccountAuthenticator Deserialize(Deserializer d)
    {
        AccountAuthenticatorVariant variant = (AccountAuthenticatorVariant)d.Uleb128AsU32();
        return variant switch
        {
            AccountAuthenticatorVariant.Ed25519 => AccountAuthenticatorEd25519.Deserialize(d),
            AccountAuthenticatorVariant.SingleKey => AccountAuthenticatorSingleKey.Deserialize(d),
            AccountAuthenticatorVariant.MultiKey => AccountAuthenticatorMultiKey.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}