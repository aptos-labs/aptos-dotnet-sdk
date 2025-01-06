namespace Aptos;

using Aptos.Exceptions;
using NBitcoin;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

public static class Secp256k1
{
    public static readonly ECDomainParameters DOMAIN_PARAMS = new(
        SecNamedCurves.GetByName("secp256k1")
    );

    public static readonly BigInteger HALF_CURVE_ORDER = CustomNamedCurves
        .GetByName("secp256k1")
        .Curve.Order.ShiftRight(1);
}

public class Secp256k1PublicKey : PublicKey
{
    static readonly int LENGTH = 65;

    private readonly Hex _key;

    public override Hex Value => _key;

    public Secp256k1PublicKey(string publicKey)
        : this(Hex.FromHexInput(publicKey).ToByteArray()) { }

    public Secp256k1PublicKey(byte[] publicKey)
        : base(PublicKeyVariant.Secp256k1Ecdsa)
    {
        if (publicKey.Length != LENGTH)
            throw new KeyLengthMismatch("Secp256k1PublicKey", LENGTH);
        _key = new(publicKey);
    }

    public override void Serialize(Serializer s) => s.Bytes(_key.ToByteArray());

    public override byte[] ToByteArray() => _key.ToByteArray();

    public override bool VerifySignature(byte[] message, Signature signature)
    {
        // Hash the message
        byte[] hash = DigestUtilities.CalculateDigest("SHA3-256", message);

        // Initialize the signer with the public key
        ECPublicKeyParameters publicKeyParams = new(
            Secp256k1.DOMAIN_PARAMS.Curve.DecodePoint(ToByteArray()),
            Secp256k1.DOMAIN_PARAMS
        );
        ECDsaSigner signer = new();
        signer.Init(false, publicKeyParams);

        // Extract the r and s values from the signature
        byte[] signatureBytes = signature.ToByteArray();
        BigInteger r = new(1, signatureBytes[..32]);
        BigInteger s = new(1, signatureBytes[32..]);

        return signer.VerifySignature(hash, r, s);
    }

    public static new Secp256k1PublicKey Deserialize(Deserializer d) => new(d.Bytes());
}

public class Secp256k1PrivateKey : PrivateKey
{
    static readonly int LENGTH = 32;

    private readonly Hex _key;

    public Secp256k1PrivateKey(string privateKey, bool? strict = null)
        : this(ParseHexInput(privateKey, PrivateKeyVariant.Secp256k1Ecdsa, strict)) { }

    public Secp256k1PrivateKey(byte[] privateKey)
        : this(ParseHexInput(privateKey, PrivateKeyVariant.Secp256k1Ecdsa)) { }

    internal Secp256k1PrivateKey(Hex privateKey)
        : base(PrivateKeyVariant.Secp256k1Ecdsa)
    {
        if (privateKey.ToByteArray().Length != LENGTH)
            throw new KeyLengthMismatch("Secp256k1PrivateKey", LENGTH);
        _key = new(privateKey.ToByteArray());
    }

    public override PublicKey PublicKey()
    {
        // Load curve parameters
        X9ECParameters curve = ECNamedCurveTable.GetByName("secp256k1");
        ECDomainParameters domainParams = new(curve);

        // Create private key parameters
        BigInteger privateKeyInt = new(1, ToByteArray());
        ECPrivateKeyParameters privateKeyParams = new(privateKeyInt, Secp256k1.DOMAIN_PARAMS);

        // Derive public key from private key parameters
        ECPoint q = domainParams.G.Multiply(privateKeyParams.D);
        ECPublicKeyParameters publicKey = new(q, domainParams);

        return new Secp256k1PublicKey(publicKey.Q.GetEncoded(false));
    }

    public override PublicKeySignature Sign(byte[] message)
    {
        // Hash the message
        byte[] hash = DigestUtilities.CalculateDigest("SHA3-256", message);

        // Create the ECDsaSigner
        ECDsaSigner signer = new(new HMacDsaKCalculator(new Sha256Digest()));
        ECPrivateKeyParameters privateKeyParams = new(
            new BigInteger(1, ToByteArray()),
            Secp256k1.DOMAIN_PARAMS
        );
        signer.Init(true, privateKeyParams);

        // Generate the signature
        BigInteger[] signatureComponents = signer.GenerateSignature(hash);

        // Ensure the s component is in the lower half of the order (low S value)
        BigInteger r = signatureComponents[0];
        BigInteger s = signatureComponents[1];
        if (s.CompareTo(Secp256k1.HALF_CURVE_ORDER) > 0)
        {
            s = Secp256k1.DOMAIN_PARAMS.N.Subtract(s);
        }

        return new Secp256k1Signature([.. r.ToByteArrayUnsigned(), .. s.ToByteArrayUnsigned()]);
    }

    public override byte[] ToByteArray() => _key.ToByteArray();

    public override void Serialize(Serializer s) => s.Bytes(_key.ToByteArray());

    public static Secp256k1PrivateKey Generate()
    {
        SecureRandom secureRandom = new();
        ECKeyGenerationParameters keyParams = new(Secp256k1.DOMAIN_PARAMS, secureRandom);

        ECKeyPairGenerator keyPairGenerator = new("ECDSA");
        keyPairGenerator.Init(keyParams);

        return new Secp256k1PrivateKey(
            (
                (ECPrivateKeyParameters)keyPairGenerator.GenerateKeyPair().Private
            ).D.ToByteArrayUnsigned()
        );
    }

    public static Secp256k1PrivateKey FromDerivationPath(string path, string mnemonic)
    {
        if (!HdKey.IsValidBip44Path(path))
            throw new InvalidDerivationPath(path);
        ExtKey masterKey = new Mnemonic(mnemonic).DeriveExtKey().Derive(KeyPath.Parse(path));
        return new Secp256k1PrivateKey(masterKey.PrivateKey.ToBytes());
    }

    public static Secp256k1PrivateKey Deserialize(Deserializer d) => new(d.Bytes());
}

public class Secp256k1Signature : PublicKeySignature
{
    static readonly int LENGTH = 64;

    private readonly Hex _value;

    public override Hex Value => _value;

    public Secp256k1Signature(string signature)
        : this(Hex.FromHexInput(signature).ToByteArray()) { }

    public Secp256k1Signature(byte[] signature)
        : base(PublicKeySignatureVariant.Secp256k1Ecdsa)
    {
        if (signature.Length != LENGTH)
            throw new KeyLengthMismatch("Secp256k1Signature", LENGTH);
        _value = new(signature);
    }

    public override byte[] ToByteArray() => _value.ToByteArray();

    public override void Serialize(Serializer s) => s.Bytes(_value.ToByteArray());

    public static new Secp256k1Signature Deserialize(Deserializer d) => new(d.Bytes());
}
