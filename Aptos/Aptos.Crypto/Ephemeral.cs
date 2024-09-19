namespace Aptos;

using Aptos.Exceptions;

public class EphemeralPublicKey : PublicKey
{
    public readonly PublicKey PublicKey;

    public override Hex Value => PublicKey.BcsToHex();

    public EphemeralPublicKey(PublicKey publicKey)
        : base(publicKey.Type)
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

    public override bool VerifySignature(byte[] message, Signature signature) =>
        PublicKey.VerifySignature(message, signature);

    public override byte[] ToByteArray() => PublicKey.BcsToBytes();

    public override string ToString() => PublicKey.ToString();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Type);
        PublicKey.Serialize(s);
    }

    public static new EphemeralPublicKey Deserialize(Deserializer d)
    {
        PublicKeyVariant variant = (PublicKeyVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeyVariant.Ed25519 => new EphemeralPublicKey(Ed25519PublicKey.Deserialize(d)),
            _ => throw new EphemeralKeyVariantUnsupported(variant),
        };
    }
}

public class EphemeralSignature : PublicKeySignature
{
    public readonly PublicKeySignature Signature;

    public override Hex Value => Signature.Value;

    public EphemeralSignature(PublicKeySignature signature)
        : base(signature.Type)
    {
        Signature = signature;
        switch (signature.Type)
        {
            case PublicKeySignatureVariant.Ed25519:
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

    public static new EphemeralSignature Deserialize(Deserializer d)
    {
        PublicKeySignatureVariant variant = (PublicKeySignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            PublicKeySignatureVariant.Ed25519 => new EphemeralSignature(
                Ed25519Signature.Deserialize(d)
            ),
            _ => throw new EphemeralSignatureVariantUnsupported(variant),
        };
    }
}

public enum EphemeralSignatureVariant : uint
{
    ZkProof = 0,
}

public abstract class CertificateSignature : Serializable
{
    public abstract byte[] ToByteArray();

    public override string ToString() => Hex.FromHexInput(ToByteArray()).ToString();
}

public class EphemeralCertificate : Serializable
{
    public readonly CertificateSignature Signature;

    public readonly EphemeralSignatureVariant Variant;

    public EphemeralCertificate(CertificateSignature signature, EphemeralSignatureVariant variant)
    {
        Signature = signature;
        Variant = variant;
    }

    public byte[] ToByteArray() => Signature.ToByteArray();

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)Variant);
        Signature.Serialize(s);
    }

    public static EphemeralCertificate Deserialize(Deserializer d)
    {
        EphemeralSignatureVariant variant = (EphemeralSignatureVariant)d.Uleb128AsU32();
        return variant switch
        {
            EphemeralSignatureVariant.ZkProof => new EphemeralCertificate(
                ZeroKnowledgeSignature.Deserialize(d),
                EphemeralSignatureVariant.ZkProof
            ),
            _ => throw new ArgumentException("Invalid signature variant"),
        };
    }
}

public class ZeroKnowledgeSignature(
    ZkProof proof,
    ulong expHorizonSecs,
    string? extraField = null,
    string? overrideAudVal = null,
    EphemeralSignature? trainingWheelSignature = null
) : CertificateSignature
{
    public readonly ZkProof Proof = proof;

    public readonly ulong ExpHorizonSecs = expHorizonSecs;

    public readonly string? ExtraField = extraField;

    public readonly string? OverrideAudVal = overrideAudVal;

    public readonly EphemeralSignature? TrainingWheelSignature = trainingWheelSignature;

    public override byte[] ToByteArray() => BcsToBytes();

    public override void Serialize(Serializer s)
    {
        Proof.Serialize(s);
        s.U64(ExpHorizonSecs);
        s.OptionString(ExtraField);
        s.OptionString(OverrideAudVal);
        s.Option(TrainingWheelSignature);
    }

    public static ZeroKnowledgeSignature Deserialize(Deserializer d)
    {
        ZkProof proof = ZkProof.Deserialize(d);
        ulong expHorizonSecs = d.U64();
        string? extraField = d.OptionString();
        string? overrideAudVal = d.OptionString();
        EphemeralSignature? trainingWheelSignature = d.Option(EphemeralSignature.Deserialize);
        return new ZeroKnowledgeSignature(
            proof,
            expHorizonSecs,
            extraField,
            overrideAudVal,
            trainingWheelSignature
        );
    }
}
