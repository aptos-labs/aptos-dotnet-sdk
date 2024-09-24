namespace Aptos;

/// <summary>
/// A verifying key represents a collection of public keys that can be used to verify signatures
/// or derive authentication keys. This interface is typically implemented to collect public keys
/// for Account Authenticators.
/// </summary>
public interface IVerifyingKey
{
    public AuthenticationKey AuthKey();

    public bool VerifySignature(string message, Signature signature);

    public bool VerifySignature(byte[] message, Signature signature);
}
