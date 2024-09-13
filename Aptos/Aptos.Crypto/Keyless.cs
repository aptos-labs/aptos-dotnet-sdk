namespace Aptos;

using System.Numerics;
using Aptos.Poseidon;
using Aptos.Schemes;
using Microsoft.IdentityModel.JsonWebTokens;

public static class Keyless
{

    const int EPK_HORIZON_SECS = 10000000;
    const int MAX_AUD_VAL_BYTES = 120;
    const int MAX_UID_KEY_BYTES = 30;
    const int MAX_UID_VAL_BYTES = 330;
    const int MAX_ISS_VAL_BYTES = 120;
    const int MAX_EXTRA_FIELD_BYTES = 350;
    const int MAX_JWT_HEADER_B64_BYTES = 300;
    const int MAX_COMMITED_EPK_BYTES = 93;

    public static byte[] ComputeIdCommitment(string jwt, string pepper, string uidKey = "sub") => ComputeIdCommitment(jwt, Hex.FromHexInput(pepper).ToByteArray(), uidKey);
    public static byte[] ComputeIdCommitment(string jwt, byte[] pepper, string uidKey = "sub")
    {
        JsonWebToken token = new(jwt);
        var aud = token.GetClaim("aud").Value;
        var uidVal = token.GetClaim(uidKey).Value;
        return ComputeIdCommitment(uidKey, uidVal, aud, pepper);
    }

    public static byte[] ComputeIdCommitment(string uidKey, string uidVal, string aud, string pepper) => ComputeIdCommitment(uidKey, uidVal, aud, Hex.FromHexInput(pepper).ToByteArray());
    public static byte[] ComputeIdCommitment(string uidKey, string uidVal, string aud, byte[] pepper)
    {
        List<BigInteger> fields = [
            BytesHash.BytesToBigIntegerLE(pepper),
            BytesHash.HashWithLength(aud, MAX_AUD_VAL_BYTES),
            BytesHash.HashWithLength(uidVal, MAX_UID_VAL_BYTES),
            BytesHash.HashWithLength(uidKey, MAX_UID_KEY_BYTES),
        ];

        return BytesHash.BigIntegerToBytesLE(Hash.PoseidonHash(fields), KeylessPublicKey.ID_COMMITMENT_LENGTH);
    }

}

public class KeylessPublicKey : LegacyAccountPublicKey
{
    public static readonly int ID_COMMITMENT_LENGTH = 32;

    public readonly string Iss;

    private readonly byte[] IdCommitment;

    public override Hex Value => Hex.FromHexInput(BcsToBytes());

    public KeylessPublicKey(string iss, string uidKey, string uidVal, string aud, string pepper) : this(iss, Keyless.ComputeIdCommitment(uidKey, uidVal, aud, pepper)) { }
    public KeylessPublicKey(string iss, string uidKey, string uidVal, string aud, byte[] pepper) : this(iss, Keyless.ComputeIdCommitment(uidKey, uidVal, aud, pepper)) { }
    public KeylessPublicKey(string iss, string idCommitment) : this(iss, Hex.FromHexInput(idCommitment).ToByteArray()) { }
    public KeylessPublicKey(string iss, byte[] idCommitment) : base(PublicKeyVariant.Keyless)
    {
        if (idCommitment.Length != ID_COMMITMENT_LENGTH) throw new Exceptions.KeyLengthMismatch("KeylessPublicKey", ID_COMMITMENT_LENGTH);
        Iss = iss;
        IdCommitment = idCommitment;
    }

    public override AuthenticationKey AuthKey()
    {
        Serializer s = new();
        s.U32AsUleb128((uint)Type);
        s.FixedBytes(BcsToBytes());
        return AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.SingleKey, s.ToBytes());
    }

    public override bool VerifySignature(byte[] message, Signature signature) => throw new NotImplementedException();

    public override byte[] ToByteArray() => BcsToBytes();

    public override void Serialize(Serializer s)
    {
        s.String(Iss);
        s.Bytes(IdCommitment);
    }

    public static KeylessPublicKey Deserialize(Deserializer d)
    {
        string iss = d.String();
        byte[] idCommitment = d.Bytes();
        return new KeylessPublicKey(iss, idCommitment);
    }

    public static KeylessPublicKey FromJwt(string jwt, string pepper, string uidKey = "sub") => new(new JsonWebToken(jwt).Issuer, Keyless.ComputeIdCommitment(jwt, pepper, uidKey));
    public static KeylessPublicKey FromJwt(string jwt, byte[] pepper, string uidKey = "sub") => new(new JsonWebToken(jwt).Issuer, Keyless.ComputeIdCommitment(jwt, pepper, uidKey));

}

public class KeylessSignature : LegacySignature
{

    public readonly EphemeralCertificate EphemeralCertificate;

    public readonly string JwtHeader;

    public readonly ulong ExpiryDateSecs;

    public readonly EphemeralPublicKey EphemeralPublicKey;

    public readonly EphemeralSignature EphemeralSignature;

    public override Hex Value => BcsToHex();

    public KeylessSignature(EphemeralCertificate ephemeralCertificate, string jwtHeader, ulong expiryDateSecs, EphemeralPublicKey ephemeralPublicKey, EphemeralSignature ephemeralSignature) : base(SignatureVariant.Keyless)
    {
        EphemeralCertificate = ephemeralCertificate;
        JwtHeader = jwtHeader;
        ExpiryDateSecs = expiryDateSecs;
        EphemeralPublicKey = ephemeralPublicKey;
        EphemeralSignature = ephemeralSignature;
    }

    public override byte[] ToByteArray() => BcsToBytes();

    public override void Serialize(Serializer s)
    {
        EphemeralCertificate.Serialize(s);
        s.String(JwtHeader);
        s.U64(ExpiryDateSecs);
        EphemeralPublicKey.Serialize(s);
        EphemeralSignature.Serialize(s);
    }

    public static KeylessSignature Deserialize(Deserializer d)
    {
        EphemeralCertificate ephemeralCertificate = EphemeralCertificate.Deserialize(d);
        string jwtHeader = d.String();
        ulong expiryDateSecs = d.U64();
        EphemeralPublicKey ephemeralPublicKey = EphemeralPublicKey.Deserialize(d);
        EphemeralSignature ephemeralSignature = EphemeralSignature.Deserialize(d);
        return new KeylessSignature(ephemeralCertificate, jwtHeader, expiryDateSecs, ephemeralPublicKey, ephemeralSignature);
    }
}

public enum EphemeralSignatureVariant : uint
{
    ZkProof = 0,
}

public class EphemeralCertificate : Signature
{
    public readonly Signature Signature;

    public readonly EphemeralSignatureVariant Variant;

    public EphemeralCertificate(Signature signature, EphemeralSignatureVariant variant)
    {
        Signature = signature;
        Variant = variant;
    }

    public override byte[] ToByteArray() => Signature.ToByteArray();

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
            EphemeralSignatureVariant.ZkProof => new EphemeralCertificate(ZeroKnowledgeSignature.Deserialize(d), EphemeralSignatureVariant.ZkProof),
            _ => throw new ArgumentException("Invalid signature variant"),
        };
    }

}

public class ZeroKnowledgeSignature(ZkProof proof, ulong expHorizonSecs, string? extraField, string? overrideAudVal, EphemeralSignature? trainingWheelSignature) : Signature
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
        return new ZeroKnowledgeSignature(proof, expHorizonSecs, extraField, overrideAudVal, trainingWheelSignature);
    }
}