namespace Aptos;

public class AccountAuthenticatorEd25519(Ed25519PublicKey publicKey, Ed25519Signature signature) : AccountAuthenticator
{
    public readonly Ed25519PublicKey PublicKey = publicKey;

    public readonly Ed25519Signature Signature = signature;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)AccountAuthenticatorVariant.Ed25519);
        PublicKey.Serialize(s);
        Signature.Serialize(s);
    }

    public static new AccountAuthenticatorEd25519 Deserialize(Deserializer d)
    {
        Ed25519PublicKey publicKey = Ed25519PublicKey.Deserialize(d);
        Ed25519Signature signature = Ed25519Signature.Deserialize(d);
        return new AccountAuthenticatorEd25519(publicKey, signature);
    }
}