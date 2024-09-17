namespace Aptos;

public abstract class PublicKey : Serializable
{
    public bool VerifySignature(string message, Signature signature) =>
        VerifySignature(SigningMessage.Convert(message), signature);

    public abstract bool VerifySignature(byte[] message, Signature signature);

    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}
