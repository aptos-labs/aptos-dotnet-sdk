namespace Aptos;

/// <summary>
/// Base signatures for anything signed (not specific to signing transactions/messages). This may include all signatures needed for ZK proofs, Certificates, etc.
/// </summary>
public abstract class Signature : Serializable
{
    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}
