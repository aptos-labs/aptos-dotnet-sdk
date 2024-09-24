using Aptos.Schemes;

namespace Aptos;

public partial class SingleKey(PublicKey publicKey) : Serializable, IVerifyingKey
{
    public readonly PublicKey PublicKey = publicKey;

    public AuthenticationKey AuthKey() =>
        AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.SingleKey, BcsToBytes());

    public bool VerifySignature(string message, Signature signature) =>
        VerifySignature(SigningMessage.Convert(message), signature);

    public bool VerifySignature(byte[] message, Signature signature) =>
        PublicKey.VerifySignature(message, signature);

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)PublicKey.Type);
        PublicKey.Serialize(s);
    }

    public static SingleKey Deserialize(Deserializer d) => new(PublicKey.Deserialize(d));
}
