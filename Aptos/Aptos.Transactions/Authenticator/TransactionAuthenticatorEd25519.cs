namespace Aptos;

public class TransactionAuthenticatorEd25519(Ed25519PublicKey publicKey, Ed25519Signature signature)
    : TransactionAuthenticator
{
    public readonly Ed25519PublicKey PublicKey = publicKey;

    public readonly Ed25519Signature Signature = signature;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionAuthenticatorVariant.Ed25519);
        PublicKey.Serialize(s);
        Signature.Serialize(s);
    }

    public static new TransactionAuthenticatorEd25519 Deserialize(Deserializer d)
    {
        Ed25519PublicKey publicKey = Ed25519PublicKey.Deserialize(d);
        Ed25519Signature signature = Ed25519Signature.Deserialize(d);
        return new TransactionAuthenticatorEd25519(publicKey, signature);
    }
}
