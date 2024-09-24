using Aptos.Schemes;

namespace Aptos;

public class SingleKeyAccount : Account
{
    public readonly PrivateKey PrivateKey;

    private readonly SingleKey _verifyingKey;
    public override IVerifyingKey VerifyingKey => _verifyingKey;

    public PublicKey PublicKey => _verifyingKey.PublicKey;

    private readonly AccountAddress _address;
    public override AccountAddress Address => _address;

    public override SigningScheme SigningScheme => SigningScheme.SingleKey;

    public SingleKeyAccount(PrivateKey privateKey)
        : this(privateKey, (AccountAddress?)null) { }

    public SingleKeyAccount(PrivateKey privateKey, byte[]? address = null)
        : this(privateKey, address != null ? AccountAddress.From(address) : null) { }

    public SingleKeyAccount(PrivateKey privateKey, string? address = null)
        : this(privateKey, address != null ? AccountAddress.From(address) : null) { }

    public SingleKeyAccount(PrivateKey privateKey, AccountAddress? address = null)
    {
        _verifyingKey = new SingleKey(privateKey.PublicKey());
        _address = address ?? _verifyingKey.AuthKey().DerivedAddress();
        PrivateKey = privateKey;
    }

    public bool VerifySignature(string message, Signature signature) =>
        _verifyingKey.VerifySignature(message, signature);

    public bool VerifySignature(byte[] message, Signature signature) =>
        _verifyingKey.VerifySignature(message, signature);

    public override Signature Sign(byte[] message) => PrivateKey.Sign(message);

    public override AccountAuthenticator SignWithAuthenticator(byte[] message) =>
        new AccountAuthenticatorSingleKey(
            _verifyingKey.PublicKey,
            (PublicKeySignature)Sign(message)
        );

    public static SingleKeyAccount Generate(PublicKeyVariant scheme = PublicKeyVariant.Ed25519)
    {
        PrivateKey privateKey = scheme switch
        {
            PublicKeyVariant.Ed25519 => Ed25519PrivateKey.Generate(),
            PublicKeyVariant.Secp256k1Ecdsa => Secp256k1PrivateKey.Generate(),
            _ => throw new ArgumentException($"Unsupported public key scheme for types {scheme}"),
        };
        return new SingleKeyAccount(privateKey);
    }

    public static SingleKeyAccount FromDerivationPath(
        PublicKeyVariant scheme,
        string path,
        string mnemnoic
    )
    {
        PrivateKey privateKey = scheme switch
        {
            PublicKeyVariant.Ed25519 => Ed25519PrivateKey.FromDerivationPath(path, mnemnoic),
            PublicKeyVariant.Secp256k1Ecdsa => Secp256k1PrivateKey.FromDerivationPath(
                path,
                mnemnoic
            ),
            _ => throw new ArgumentException($"Unsupported public key scheme for types {scheme}"),
        };
        return new SingleKeyAccount(privateKey);
    }
}
