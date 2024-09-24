namespace Aptos;

public class AccountAuthenticatorSingleKey(PublicKey publicKey, PublicKeySignature signature)
    : AccountAuthenticator
{
    public readonly PublicKey PublicKey = publicKey;

    public readonly PublicKeySignature Signature = signature;

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
        PublicKeySignature signature = PublicKeySignature.Deserialize(d);
        return new AccountAuthenticatorSingleKey(publicKey, signature);
    }
}
