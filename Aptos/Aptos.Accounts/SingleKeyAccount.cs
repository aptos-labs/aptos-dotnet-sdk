namespace Aptos;

using Aptos.Schemes;

public class SingleKeyAccount : Account
{
    public readonly PrivateKey PrivateKey;

    private readonly AnyPublicKey _publicKey;
    public override AccountPublicKey PublicKey => _publicKey;

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
        _publicKey = new AnyPublicKey(privateKey.PublicKey());
        _address = address ?? _publicKey.AuthKey().DerivedAddress();
        PrivateKey = privateKey;
    }

    public bool VerifySignature(string message, AnySignature signature) =>
        PublicKey.VerifySignature(message, signature);

    public bool VerifySignature(byte[] message, AnySignature signature) =>
        PublicKey.VerifySignature(message, signature);

    public override Signature SignTransaction(AnyRawTransaction transaction) =>
        Sign(SigningMessage.GenerateForTransaction(transaction));

    public override Signature Sign(byte[] message) => new AnySignature(PrivateKey.Sign(message));

    public override AccountAuthenticator SignWithAuthenticator(byte[] message) =>
        new AccountAuthenticatorSingleKey(_publicKey, (AnySignature)Sign(message));

    public override AccountAuthenticator SignTransactionWithAuthenticator(
        AnyRawTransaction transaction
    ) => new AccountAuthenticatorSingleKey(_publicKey, (AnySignature)SignTransaction(transaction));

    public static new SingleKeyAccount Generate() => Generate(PublicKeyVariant.Ed25519);

    public static SingleKeyAccount Generate(PublicKeyVariant scheme)
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
