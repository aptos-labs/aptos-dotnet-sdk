using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aptos;

[JsonConverter(typeof(StringEnumConverter))]
public enum PrivateKeyVariant : uint
{
    [EnumMember(Value = "ed25519")]
    Ed25519,

    [EnumMember(Value = "secp256k1_ecdsa")]
    Secp256k1Ecdsa,
}

public abstract partial class PrivateKey
{
    /// <summary>
    /// The AIP-80 compliant prefixes for each private key type
    /// </summary>
    /// <remarks>
    /// Read about AIP-80: https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-80.md
    /// </remarks>
    public static readonly Dictionary<PrivateKeyVariant, string> AIP80_PREFIXES =
        new()
        {
            { PrivateKeyVariant.Ed25519, "ed25519-priv-" },
            { PrivateKeyVariant.Secp256k1Ecdsa, "secp256k1-priv-" },
        };

    /// <inheritdoc cref="FormatPrivateKey(string, PrivateKeyVariant)" />
    public static string FormatPrivateKey(byte[] privateKey, PrivateKeyVariant type) =>
        FormatPrivateKey(Hex.FromHexInput(privateKey).ToString(), type);

    /// <summary>
    /// Format a HexInput to an AIP-80 compliant string.
    /// </summary>
    /// <remarks>
    /// Read about AIP-80: https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-80.md
    /// </remarks>
    /// <param name="privateKey">The HexString or byte array format of the private key.</param>
    /// <param name="type">The private key type</param>
    /// <returns>AIP-80 compliant string.</returns>
    public static string FormatPrivateKey(string privateKey, PrivateKeyVariant type)
    {
        string aip80Prefix = AIP80_PREFIXES[type];
        return $"{aip80Prefix}{Hex.FromHexInput(privateKey)}";
    }

    /// <inheritdoc cref="ParseHexInput(string, PrivateKeyVariant, bool?)" />
    public static Hex ParseHexInput(byte[] value, PrivateKeyVariant type, bool? strict = null)
    {
        if (strict == null)
        {
            Console.WriteLine(
                "It is recommended that private keys are parsed as AIP-80 compliant strings instead of byte array (https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-80.md). You can fix the private key by formatting it with `PrivateKey.FormatPrivateKey(privateKey: byte[], type: PrivateKeyVariants)`."
            );
        }
        else if (strict == true)
        {
            throw new ArgumentException(
                "Invalid value while strictly parsing private key. MUST be AIP-80 compliant string, not byte array."
            );
        }
        return ParseHexInput(Hex.FromHexInput(value).ToString(), type, strict);
    }

    /// <summary>
    /// Parse a HexInput that may be a HexString, byte array, or a AIP-80 compliant string to a Hex instance.
    /// </summary>
    /// <param name="value">A HexString, byte array, or AIP-80 compliant string.</param>
    /// <param name="type">The private key type</param>
    /// <param name="strict">If true, the value MUST be compliant with AIP-80. If false, the value MAY be compliant with AIP-80 and ignore warnings.</param>
    /// <returns>A Hex instance.</returns>
    public static Hex ParseHexInput(string value, PrivateKeyVariant type, bool? strict = null)
    {
        Hex data;

        string aip80Prefix = AIP80_PREFIXES[type];
        if (strict != true && !value.StartsWith(aip80Prefix))
        {
            // HexString input
            data = Hex.FromHexInput(value);
            if (strict != false)
            {
                Console.WriteLine(
                    "It is recommended that private keys are AIP-80 compliant (https://github.com/aptos-foundation/AIPs/blob/main/aips/aip-80.md). You can fix the private key by formatting it with `PrivateKey.FormatPrivateKey(privateKey: string, type: PrivateKeyVariant)`."
                );
            }
        }
        else if (value.StartsWith(aip80Prefix))
        {
            // AIP-80 Compliant String input
            string hexValue = value.Split('-')[2];
            data = Hex.FromHexString(hexValue);
        }
        else
        {
            // Should never reach here
            throw new ArgumentException("Invalid HexString input while parsing private key.");
        }

        return data;
    }
}

public abstract partial class PrivateKey(PrivateKeyVariant type) : Serializable
{
    public virtual PublicKeySignature Sign(string message) => Sign(SigningMessage.Convert(message));

    public abstract PublicKeySignature Sign(byte[] message);

    public PrivateKeyVariant Type = type;

    public abstract PublicKey PublicKey();

    public abstract byte[] ToByteArray();

    public virtual string ToHexString() => Hex.FromHexInput(ToByteArray()).ToString();

    public virtual string ToAIP80String() => FormatPrivateKey(ToByteArray(), Type);

    public override string ToString() => ToHexString();
}
