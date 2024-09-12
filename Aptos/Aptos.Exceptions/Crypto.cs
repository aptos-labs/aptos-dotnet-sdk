namespace Aptos.Exceptions
{
    public class EphemeralSignatureVariantUnsupported(SignatureVariant variant) : BaseException($"Ephemeral signature variant {variant} is not supported") { }
    public class EphemeralKeyVariantUnsupported(PublicKeyVariant variant) : BaseException($"Ephemeral key variant {variant} is not supported") { }

    public class KeyLengthMismatch(string name, int length) : BaseException($"{name} length should be {length}") { }

    public class InvalidDerivationPath(string path) : BaseException($"Invalid derivation path: {path}") { }

}
