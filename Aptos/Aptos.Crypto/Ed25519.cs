namespace Aptos;

using Aptos.Exceptions;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

public static class Ed25519
{
    public static readonly byte[] L = [0xed, 0xd3, 0xf5, 0x5c, 0x1a, 0x63, 0x12, 0x58, 0xd6, 0x9c, 0xf7, 0xa2, 0xde, 0xf9, 0xde, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10];
    public static bool IsCanonicalEd25519Signature(Signature signature)
    {
        byte[] s = signature.ToByteArray().Skip(32).ToArray();
        for (int i = L.Length - 1; i >= 0; i -= 1)
        {
            if (s[i] < L[i]) return true;
            if (s[i] > L[i]) return false;
        }

        // As this stage S == L which implies a non-canonical S.
        return false;
    }
}

public class Ed25519PublicKey : PublicKey
{
    static readonly int LENGTH = 32;

    private readonly Hex _key;

    public override Hex Value => _key;

    public Ed25519PublicKey(string publicKey) : this(Hex.FromHexInput(publicKey).ToByteArray()) { }
    public Ed25519PublicKey(byte[] publicKey) : base(PublicKeyVariant.Ed25519)
    {
        if (publicKey.Length != LENGTH) throw new KeyLengthMismatch("Ed25519PublicKey", LENGTH);
        _key = new(publicKey);
    }

    public override byte[] ToByteArray() => _key.ToByteArray();

    public override bool VerifySignature(byte[] message, Signature signature)
    {
        if (!Ed25519.IsCanonicalEd25519Signature(signature)) return false;

        byte[] signatureBytes = signature.ToByteArray();
        byte[] publicKeyBytes = ToByteArray();

        Ed25519Signer signer = new();
        signer.Init(false, new Ed25519PublicKeyParameters(publicKeyBytes, 0));
        signer.BlockUpdate(message, 0, message.Length);
        return signer.VerifySignature(signatureBytes);
    }

    public override void Serialize(Serializer s) => s.Bytes(_key.ToByteArray());

    public static new Ed25519PublicKey Deserialize(Deserializer d) => new(d.Bytes());

}

public class Ed25519PrivateKey : PrivateKey
{
    static readonly int LENGTH = 32;
    static readonly string SLIP_0010_SEED = "ed25519 seed";

    static readonly uint HARDENED_OFFSET = 0x80000000;

    private readonly Hex _key;

    public Ed25519PrivateKey(string privateKey) : this(Hex.FromHexInput(privateKey).ToByteArray()) { }
    public Ed25519PrivateKey(byte[] privateKey)
    {
        if (privateKey.Length != LENGTH) throw new KeyLengthMismatch("Ed25519PrivateKey", LENGTH);
        _key = new(privateKey);
    }

    public override PublicKey PublicKey() => new Ed25519PublicKey(new Ed25519PrivateKeyParameters(_key.ToByteArray(), 0).GeneratePublicKey().GetEncoded());

    public override Signature Sign(string message) => Sign(SigningMessage.Convert(message));
    public override Signature Sign(byte[] message)
    {
        Ed25519Signer signer = new();
        signer.Init(true, new Ed25519PrivateKeyParameters(_key.ToByteArray(), 0));
        signer.BlockUpdate(message, 0, message.Length);
        return new Ed25519Signature(signer.GenerateSignature());
    }

    public override byte[] ToByteArray() => _key.ToByteArray();

    public override void Serialize(Serializer s) => s.Bytes(_key.ToByteArray());

    public static Ed25519PrivateKey Generate()
    {
        Ed25519KeyPairGenerator keyPairGenerator = new();
        keyPairGenerator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
        AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();
        return new Ed25519PrivateKey(((Ed25519PrivateKeyParameters)keyPair.Private).GetEncoded());
    }

    public static Ed25519PrivateKey FromDerivationPath(string path, string mnemonic)
    {
        if (!HdKey.IsValidHardenedPath(path)) throw new InvalidDerivationPath(path);
        HdKey.DerivedKeys derivedKeys = HdKey.DeriveKey(SLIP_0010_SEED, HdKey.MnemonicToSeed(mnemonic));

        IEnumerable<uint> segments = HdKey.SplitPath(path).Select(uint.Parse);

        // Derive the child key based on the path
        HdKey.DerivedKeys keys = segments.Aggregate(
            derivedKeys,
            (parentKeys, segment) => HdKey.CKDPriv(parentKeys, segment + HARDENED_OFFSET)
        );

        return new Ed25519PrivateKey(keys.PrivateKey);
    }

    public static Ed25519PrivateKey Deserialize(Deserializer d) => new(d.Bytes());
}

public class Ed25519Signature : Signature
{

    static readonly int LENGTH = 64;

    private readonly Hex _value;

    public override Hex Value => _value;

    public Ed25519Signature(string signature) : this(Hex.FromHexInput(signature).ToByteArray()) { }
    public Ed25519Signature(byte[] signature) : base(SignatureVariant.Ed25519)
    {
        if (signature.Length != LENGTH) throw new KeyLengthMismatch("Ed25519Signature", LENGTH);
        _value = new(signature);
    }

    public override byte[] ToByteArray() => _value.ToByteArray();

    public override void Serialize(Serializer s) => s.Bytes(_value.ToByteArray());

    public static new Ed25519Signature Deserialize(Deserializer d) => new(d.Bytes());

}