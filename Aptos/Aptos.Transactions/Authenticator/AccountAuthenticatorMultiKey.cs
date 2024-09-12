namespace Aptos;

public class AccountAuthenticatorMultiKey(MultiKey publicKey, MultiKeySignature signature) : AccountAuthenticator
{
    public readonly MultiKey PublicKey = publicKey;

    public readonly MultiKeySignature Signature = signature;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)AccountAuthenticatorVariant.MultiKey);
        PublicKey.Serialize(s);
        Signature.Serialize(s);
    }

    public static new AccountAuthenticatorMultiKey Deserialize(Deserializer d)
    {
        MultiKey publicKey = MultiKey.Deserialize(d);
        MultiKeySignature signature = MultiKeySignature.Deserialize(d);
        return new AccountAuthenticatorMultiKey(publicKey, signature);
    }
}