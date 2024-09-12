using System.Numerics;
using System.Security.Cryptography;
using Aptos.Core;
using Aptos.Poseidon;

namespace Aptos;

public class EphemeralKeyPair : Serializable
{
    public static readonly int BLINDER_LENGTH = 31;

    /// <summary>
    /// The default expiry duration for an ephemeral key pair is 14 days.
    /// </summary>
    public static readonly int DEFAULT_EXPIRY_DURATION = 60 * 60 * 24 * 14;

    /// <summary>
    /// The private key is used to sign transactions. This private key is not tied to any account on the chain as it is
    /// ephemeral (not permanent) in nature.
    /// </summary>
    private PrivateKey _privateKey;

    /// <summary>
    /// A public key used to verify transactions. This public key is not tied to any account on the chain as it is
    /// ephemeral (not permanent) in nature.
    /// </summary>
    public EphemeralPublicKey PublicKey;

    public readonly byte[] Blinder;

    /// <summary>
    /// A timestamp in seconds indicating when the ephemeral key pair will expire. After expiry, a new
    /// EphemeralKeyPair must be generated and a new JWT needs to be created.
    /// </summary>
    public readonly ulong ExpiryTimestamp;

    /// <summary>
    /// The value passed to the IdP when the user logs in. 
    /// </summary>
    public readonly string Nonce;

    public EphemeralKeyPair(PrivateKey privateKey, ulong? expiryTimestamp = null, byte[]? blinder = null)
    {
        _privateKey = privateKey;
        if (privateKey.PublicKey() is LegacyAccountPublicKey publicKey)
        {
            PublicKey = new EphemeralPublicKey(publicKey);
        }
        else throw new ArgumentException("Invalid PrivateKey passed to EphemeralKeyPair. Expected LegacyAccountPublicKey.");

        // By default, the expiry timestamp is 14 days from now.
        ExpiryTimestamp = expiryTimestamp ?? (ulong)Utilities.FloorToWholeHour(DateTime.Now.ToUnixTimestamp() + DEFAULT_EXPIRY_DURATION);
        Blinder = blinder ?? GenerateBlinder();

        // Compute nonce
        var fields = BytesHash.PadAndPackBytesWithLength(PublicKey.BcsToBytes(), 93);
        fields.Add(new BigInteger(ExpiryTimestamp));
        fields.Add(BytesHash.BytesToBigIntegerLE(Blinder));
        Nonce = Hash.PoseidonHash(fields).ToString();
    }

    public bool IsExpired() => (ulong)DateTime.Now.ToUnixTimestamp() > ExpiryTimestamp;

    public EphemeralSignature Sign(byte[] data)
    {
        if (IsExpired()) throw new Exception("EphemeralKeyPair is expired");
        var signature = _privateKey.Sign(data);
        if (signature is LegacySignature legacySignature)
        {
            return new EphemeralSignature(legacySignature);
        }
        throw new ArgumentException("Invalid PrivateKey passed to EphemeralKeyPair. Expecting a legacy private key.");
    }

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)PublicKey.Type);
        s.Bytes(_privateKey.ToByteArray());
        s.U64(ExpiryTimestamp);
        s.Bytes(Blinder);
    }


    public static EphemeralKeyPair Deserialize(Deserializer d)
    {
        PublicKeyVariant variant = (PublicKeyVariant)d.Uleb128AsU32();
        var privateKey = variant switch
        {
            PublicKeyVariant.Ed25519 => Ed25519PrivateKey.Deserialize(d),
            _ => throw new ArgumentException($"Unsupported public key scheme for types {variant}"),
        };
        var expiryTimestamp = d.U64();
        var blinder = d.Bytes();
        return new EphemeralKeyPair(privateKey, expiryTimestamp, blinder);
    }

    public static EphemeralKeyPair Generate()
    {
        var privateKey = Ed25519PrivateKey.Generate();
        return new EphemeralKeyPair(privateKey);
    }

    private byte[] GenerateBlinder()
    {
        byte[] blinder = new byte[BLINDER_LENGTH];
        RandomNumberGenerator.Create().GetBytes(blinder);
        return blinder;
    }

}