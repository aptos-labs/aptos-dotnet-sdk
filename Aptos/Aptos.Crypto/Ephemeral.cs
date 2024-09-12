namespace Aptos;

using Aptos.Exceptions;

public class EphemeralPublicKey : LegacyPublicKey
{

    public readonly LegacyAccountPublicKey PublicKey;

    public override Hex Value => PublicKey.BcsToHex();

    public EphemeralPublicKey(LegacyAccountPublicKey publicKey) : base(publicKey.Type)
    {
        PublicKey = publicKey;
        switch (publicKey.Type)
        {
            case PublicKeyVariant.Ed25519:
                break;
            default:
                throw new EphemeralKeyVariantUnsupported(publicKey.Type);
        }
    }

    public override bool VerifySignature(byte[] message, Signature signature) => PublicKey.VerifySignature(message, signature);

    public override byte[] ToByteArray() => PublicKey.BcsToBytes();

    public override string ToString() => PublicKey.ToString();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Type);
        PublicKey.Serialize(s);
    }

    public static EphemeralPublicKey Deserialize(Deserializer d)
    {
        PublicKeyVariant variant = (PublicKeyVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeyVariant.Ed25519 => new EphemeralPublicKey(Ed25519PublicKey.Deserialize(d)),
            _ => throw new EphemeralKeyVariantUnsupported(variant),
        };
    }
}

public class EphemeralSignature : Signature
{
    public readonly Signature Signature;

    public readonly SignatureVariant Type;

    public EphemeralSignature(LegacySignature signature)
    {
        Signature = signature;
        switch (signature.Type)
        {
            case SignatureVariant.Ed25519:
                break;
            default:
                throw new EphemeralSignatureVariantUnsupported(signature.Type);
        }
    }

    public override byte[] ToByteArray() => Signature.BcsToBytes();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Type);
        Signature.Serialize(s);
    }

    public static EphemeralSignature Deserialize(Deserializer d)
    {
        SignatureVariant variant = (SignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            SignatureVariant.Ed25519 => new EphemeralSignature(Ed25519Signature.Deserialize(d)),
            _ => throw new EphemeralSignatureVariantUnsupported(variant),
        };
    }
}