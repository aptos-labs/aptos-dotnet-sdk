namespace Aptos;

public class AccountAuthenticatorSingleKey : AccountAuthenticator
{
    public readonly PublicKey PublicKey;

    public readonly Signature Signature;

    public AccountAuthenticatorSingleKey(PublicKey publicKey, Signature signature)
    {
        if (!PublicKey.IsSigningKey(PublicKey)) throw new ArgumentException("SingleKey account authenticator only supports signing keys (e.g. Ed25519, Keyless, Secp256k1)");
        PublicKey = publicKey;
        Signature = signature;
    }

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)AccountAuthenticatorVariant.SingleKey);

        s.U32AsUleb128((uint)PublicKey.Type);
        PublicKey.Serialize(s);

        s.U32AsUleb128((uint)Signature.Type);
        Signature.Serialize(s);
    }

    public static new AccountAuthenticatorSingleKey Deserialize(Deserializer d)
    {
        PublicKey publicKey = PublicKey.Deserialize(d);
        Signature signature = Signature.Deserialize(d);
        return new AccountAuthenticatorSingleKey(publicKey, signature);
    }
}