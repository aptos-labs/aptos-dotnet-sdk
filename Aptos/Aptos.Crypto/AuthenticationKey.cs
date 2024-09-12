namespace Aptos;

using Aptos.Schemes;
using Aptos.Exceptions;
using Org.BouncyCastle.Security;

public class AuthenticationKey : Serializable
{
    static readonly int LENGTH = 32;

    public readonly Hex Data;

    public AuthenticationKey(byte[] data)
    {
        if (data.Length != LENGTH) throw new KeyLengthMismatch("AuthenticationKey", LENGTH);
        Data = Hex.FromHexInput(data);
    }

    public AccountAddress DerivedAddress() => AccountAddress.From(Data);

    public byte[] ToByteArray() => Data.ToByteArray();

    public override void Serialize(Serializer s) => s.FixedBytes(Data.ToByteArray());

    public override string ToString() => Data.ToString();

    public static AuthenticationKey Deserialize(Deserializer d) => new(d.FixedBytes(LENGTH));

    public static AuthenticationKey FromSchemeAndBytes(AuthenticationKeyScheme scheme, string bytes) => FromSchemeAndBytes(scheme, Hex.FromHexString(bytes).ToByteArray());
    public static AuthenticationKey FromSchemeAndBytes(AuthenticationKeyScheme scheme, byte[] bytes)
    {
        // Create a new array combining input bytes and the scheme byte
        byte[] hashInput = new byte[bytes.Length + 1];
        Array.Copy(bytes, hashInput, bytes.Length); // Corrected the length argument for Array.Copy
        hashInput[^1] = (byte)scheme;

        // Return new AuthenticationKey with the hash
        return new AuthenticationKey(DigestUtilities.CalculateDigest("SHA3-256", hashInput));
    }

}