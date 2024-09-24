namespace Aptos;

public abstract class PrivateKey : Serializable
{
    public virtual PublicKeySignature Sign(string message) => Sign(SigningMessage.Convert(message));

    public abstract PublicKeySignature Sign(byte[] message);

    public abstract PublicKey PublicKey();

    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}
