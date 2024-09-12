namespace Aptos;

public class AccountAuthenticatorSingleKey(AnyPublicKey publicKey, AnySignature signature) : AccountAuthenticator
{
    public readonly AnyPublicKey PublicKey = publicKey;

    public readonly AnySignature Signature = signature;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)AccountAuthenticatorVariant.SingleKey);
        PublicKey.Serialize(s);
        Signature.Serialize(s);
    }

    public static new AccountAuthenticatorSingleKey Deserialize(Deserializer d)
    {
        AnyPublicKey publicKey = AnyPublicKey.Deserialize(d);
        AnySignature signature = AnySignature.Deserialize(d);
        return new AccountAuthenticatorSingleKey(publicKey, signature);
    }
}