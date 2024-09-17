namespace Aptos;

using Aptos.Schemes;

public class SingleKeyAccount : Account
{

    public readonly PrivateKey PrivateKey;

    private readonly PublicKey _publicKey;
    public override PublicKey PublicKey => _publicKey;

    private readonly AccountAddress _address;
    public override AccountAddress Address => _address;

    public override SigningScheme SigningScheme => SigningScheme.SingleKey;

    public override AuthenticationKey AuthKey()
    {
        Serializer s = new();
        s.U32AsUleb128((uint)_publicKey.Type);
        PublicKey.Serialize(s);
        return AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.SingleKey, s.ToBytes());
    }

    public SingleKeyAccount(PrivateKey privateKey) : this(privateKey, (AccountAddress?)null) { }
    public SingleKeyAccount(PrivateKey privateKey, byte[]? address = null) : this(privateKey, address != null ? AccountAddress.From(address) : null) { }
    public SingleKeyAccount(PrivateKey privateKey, string? address = null) : this(privateKey, address != null ? AccountAddress.From(address) : null) { }
    public SingleKeyAccount(PrivateKey privateKey, AccountAddress? address = null)
    {
        if (!PublicKey.IsSigningKey(privateKey.PublicKey())) throw new ArgumentException("SingleKeyAccount only supports signing keys (e.g. Ed25519, Keyless, Secp256k1)");
        _publicKey = privateKey.PublicKey();
        _address = address ?? AuthKey().DerivedAddress();
        PrivateKey = privateKey;
    }

    public bool VerifySignature(string message, Signature signature) => PublicKey.VerifySignature(message, signature);
    public bool VerifySignature(byte[] message, Signature signature) => PublicKey.VerifySignature(message, signature);

    public override Signature SignTransaction(AnyRawTransaction transaction) => Sign(SigningMessage.GenerateForTransaction(transaction));

    public override Signature Sign(byte[] message) => PrivateKey.Sign(message);

    public override AccountAuthenticator SignWithAuthenticator(byte[] message) => new AccountAuthenticatorSingleKey(_publicKey, Sign(message));

    public override AccountAuthenticator SignTransactionWithAuthenticator(AnyRawTransaction transaction) => new AccountAuthenticatorSingleKey(_publicKey, SignTransaction(transaction));

    public new static SingleKeyAccount Generate() => Generate(PublicKeyVariant.Ed25519);
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

    public static SingleKeyAccount FromDerivationPath(PublicKeyVariant scheme, string path, string mnemnoic)
    {
        PrivateKey privateKey = scheme switch
        {
            PublicKeyVariant.Ed25519 => Ed25519PrivateKey.FromDerivationPath(path, mnemnoic),
            PublicKeyVariant.Secp256k1Ecdsa => Secp256k1PrivateKey.FromDerivationPath(path, mnemnoic),
            _ => throw new ArgumentException($"Unsupported public key scheme for types {scheme}"),
        };
        return new SingleKeyAccount(privateKey);
    }

}