namespace Aptos;

using System.Text;
using Aptos.Core;
using Aptos.Exceptions;
using Aptos.Schemes;
using Newtonsoft.Json;
using Org.BouncyCastle.Security;

public partial class AccountAddress
{
    public static AccountAddress CreateSeedAddress(AccountAddress creatorAddress, string seed, DeriveScheme scheme) => CreateSeedAddress(creatorAddress, Encoding.UTF8.GetBytes(seed), scheme);
    public static AccountAddress CreateSeedAddress(AccountAddress creatorAddress, byte[] seed, DeriveScheme scheme)
    {
        var creatorBytes = creatorAddress.BcsToBytes();

        var bytes = new byte[creatorBytes.Length + seed.Length + 1];
        creatorBytes.CopyTo(bytes, 0);
        seed.CopyTo(bytes, creatorBytes.Length);
        bytes[^1] = (byte)scheme;

        return new AccountAddress(DigestUtilities.CalculateDigest("SHA3-256", bytes));
    }


    public static AccountAddress CreateObjectAddress(AccountAddress creatorAddress, string seed) => CreateObjectAddress(creatorAddress, Encoding.UTF8.GetBytes(seed));
    public static AccountAddress CreateObjectAddress(AccountAddress creatorAddress, byte[] seed) => CreateSeedAddress(creatorAddress, seed, DeriveScheme.DeriveObjectAddressFromObject);

    public static AccountAddress CreateResourceAddress(AccountAddress creatorAddress, string seed) => CreateResourceAddress(creatorAddress, Encoding.UTF8.GetBytes(seed));
    public static AccountAddress CreateResourceAddress(AccountAddress creatorAddress, byte[] seed) => CreateSeedAddress(creatorAddress, seed, DeriveScheme.DeriveResourceAccountAddress);

}

[JsonConverter(typeof(AccountAddressConverter))]
public partial class AccountAddress : TransactionArgument
{
    public static AccountAddress ZERO { get; } = new(new byte[LENGTH]);

    public static readonly int LENGTH = 32;

    public static readonly int LONG_LENGTH = 64;

    public readonly byte[] Data;

    public AccountAddress(byte[] data)
    {
        if (!data.Length.Equals(LENGTH)) throw new AccountAddressParsingException($"AccountAddress data should be exactly {LENGTH} bytes long but got {data.Length} bytes", AccountAddressInvalidReason.IncorrectNumberOfBytes);
        Data = data;
    }

    public bool IsSpecial() => Data.Take(Data.Length - 1).All(b => b == 0) && Data[^1] < 16;


    public string ToStringWithoutPrefix()
    {
        string hex = Hex.FromHexInput(Data).ToStringWithoutPrefix();
        return IsSpecial() ? hex[^1].ToString() : hex;
    }

    public string ToStringLongWithoutPrefix() => Hex.FromHexInput(Data).ToStringWithoutPrefix();

    public string ToStringLong() => $"0x{ToStringLongWithoutPrefix()}";

    public override string ToString() => $"0x{ToStringWithoutPrefix()}";

    public byte[] ToByteArray() => Data;

    public override void Serialize(Serializer s) => s.FixedBytes(Data);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.Address);
        s.FixedBytes(Data);
    }

    public static AccountAddress Deserialize(Deserializer d) => new(d.FixedBytes(LENGTH));

    public static AccountAddress FromString(string str)
    {
        string parsedInput = str;

        if (parsedInput.StartsWith("0x")) parsedInput = parsedInput[2..];

        if (parsedInput.Length == 0) throw new AccountAddressParsingException("Hex string is too short, must be 1 to 64 chars long, excluding the leading 0x.", AccountAddressInvalidReason.TooShort);

        if (parsedInput.Length > 64) throw new AccountAddressParsingException("Hex string is too long, must be 1 to 64 chars long, excluding the leading 0x.", AccountAddressInvalidReason.TooLong);

        byte[] addressBytes;
        try
        {
            addressBytes = Utilities.HexStringToBytes(parsedInput.PadLeft(64, '0'));
        }
        catch
        {
            throw new AccountAddressParsingException("Hex string contains invalid hex characters.", AccountAddressInvalidReason.InvalidHexChars);
        }

        return new AccountAddress(addressBytes);
    }

    public static AccountAddress FromStringStrict(string str)
    {
        if (!str.StartsWith("0x")) throw new AccountAddressParsingException("Hex string must start with 0x", AccountAddressInvalidReason.LeadingZeroXRequired);

        AccountAddress address = FromString(str);

        if (str.Length != LONG_LENGTH + 2)
        {
            if (!address.IsSpecial())
            {
                throw new AccountAddressParsingException($"The given hex string {str} is not a special address, it must be represented as 0x + 64 chars.", AccountAddressInvalidReason.LongFormRequiredUnlessSpecial);
            }
            else if (str.Length != 3)
            {
                throw new AccountAddressParsingException($"The given hex string {str} is a special address not in LONG form, it must be 0x0 to 0xf without padding zeroes.", AccountAddressInvalidReason.InvalidPaddingZeroes);
            }
        }

        return address;
    }

    public static AccountAddress From(string str) => FromString(str);
    public static AccountAddress From(byte[] bytes) => new(bytes);
    public static AccountAddress From(Hex hex) => new(hex.ToByteArray());
    public static AccountAddress From(AccountAddress address) => address;

    public static AccountAddress FromStrict(string str) => FromStringStrict(str);
    public static AccountAddress FromStrict(byte[] bytes) => new(bytes);
    public static AccountAddress FromStrict(Hex hex) => new(hex.ToByteArray());
    public static AccountAddress FromStrict(AccountAddress address) => address;

    public static bool IsValid(string str, bool? strict)
    {
        try
        {
            if (strict == true)
            {
                FromStringStrict(str);
            }
            else
            {
                FromString(str);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is AccountAddress address) return Data.SequenceEqual(address.Data);
        if (obj is byte[] bytes) return Data.SequenceEqual(bytes);
        if (obj is string str) return FromStringStrict(str).Data.SequenceEqual(Data);
        return base.Equals(obj);
    }

    public override int GetHashCode() => Data.GetHashCode();
}

public class AccountAddressConverter : JsonConverter<AccountAddress>
{

    public override AccountAddress? ReadJson(JsonReader reader, Type objectType, AccountAddress? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string? value = (string?)reader.Value;
        return value != null ? AccountAddress.FromString(value) : null;
    }

    public override void WriteJson(JsonWriter writer, AccountAddress? value, JsonSerializer serializer) => writer.WriteValue(value?.ToString());

}