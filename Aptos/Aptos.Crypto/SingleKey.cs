namespace Aptos;

using Aptos.Schemes;

public class AnyPublicKey : UnifiedAccountPublicKey
{
    public readonly PublicKey PublicKey;

    public readonly PublicKeyVariant Type;

    public AnyPublicKey(PublicKey publicKey)
    {
        if (publicKey is LegacyPublicKey legacyPublicKey)
        {
            PublicKey = legacyPublicKey;
            Type = legacyPublicKey.Type;
        }
        else if (publicKey is LegacyAccountPublicKey legacyAccountPublicKey)
        {
            PublicKey = legacyAccountPublicKey;
            Type = legacyAccountPublicKey.Type;
        }
        else
        {
            throw new ArgumentException("Invalid public key type");
        }
    }

    public override bool VerifySignature(byte[] message, Signature signature) =>
        PublicKey.VerifySignature(
            message,
            signature is AnySignature anySignature ? anySignature.Signature : signature
        );

    public override AuthenticationKey AuthKey() =>
        AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.SingleKey, BcsToBytes());

    public override byte[] ToByteArray() => PublicKey.ToByteArray();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Type);
        PublicKey.Serialize(s);
    }

    public static AnyPublicKey Deserialize(Deserializer d)
    {
        PublicKeyVariant variant = (PublicKeyVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeyVariant.Ed25519 => new AnyPublicKey(Ed25519PublicKey.Deserialize(d)),
            PublicKeyVariant.Secp256k1Ecdsa => new AnyPublicKey(Secp256k1PublicKey.Deserialize(d)),
            PublicKeyVariant.Keyless => new AnyPublicKey(KeylessPublicKey.Deserialize(d)),
            _ => throw new ArgumentException("Invalid public key variant"),
        };
    }
}

public class AnySignature(LegacySignature signature) : UnifiedSignature
{
    public readonly Signature Signature = signature;

    public readonly SignatureVariant Type = signature.Type;

    public AnySignature(Signature signature)
        : this(
            signature is LegacySignature accountSignature
                ? accountSignature
                : throw new ArgumentException("Invalid signature type")
        ) { }

    public override byte[] ToByteArray() => Signature.ToByteArray();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Type);
        Signature.Serialize(s);
    }

    public static AnySignature Deserialize(Deserializer d)
    {
        SignatureVariant variant = (SignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            SignatureVariant.Ed25519 => new AnySignature(Ed25519Signature.Deserialize(d)),
            SignatureVariant.Secp256k1Ecdsa => new AnySignature(Secp256k1Signature.Deserialize(d)),
            SignatureVariant.Keyless => new AnySignature(KeylessSignature.Deserialize(d)),
            _ => throw new ArgumentException("Invalid signature variant"),
        };
    }
}
