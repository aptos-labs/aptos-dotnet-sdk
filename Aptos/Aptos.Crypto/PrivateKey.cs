namespace Aptos;

public abstract class PrivateKey : Serializable
{
    public abstract Signature Sign(byte[] message);

    public abstract Signature Sign(string message);

    public abstract PublicKey PublicKey();

    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}
