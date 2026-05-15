namespace Aptos;

using Aptos.Core;
using Aptos.Exceptions;
using Newtonsoft.Json;

[JsonConverter(typeof(HexConverter))]
public class Hex(byte[] data)
{
    private readonly byte[] _data = data;

    public byte[] ToByteArray() => _data;

    public string ToStringWithoutPrefix() =>
        BitConverter.ToString(_data).Replace("-", string.Empty).ToLower();

    public override string ToString() => $"0x{ToStringWithoutPrefix()}";

    public override bool Equals(object? obj)
    {
        if (obj is Hex hex)
        {
            return _data.SequenceEqual(hex._data);
        }
        if (obj is byte[] bytes)
        {
            return _data.SequenceEqual(bytes);
        }
        if (obj is string str)
        {
            try
            {
                return _data.SequenceEqual(FromHexString(str)._data);
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Hash by value so that two Hex instances containing the same bytes
        // produce the same hash code. The previous implementation hashed the
        // underlying byte array by reference which broke dictionary / hashset
        // usage of Hex (and AccountAddress which wraps Hex semantics).
        unchecked
        {
            int hash = (int)2166136261u;
            foreach (byte b in _data)
                hash = (hash ^ b) * 16777619;
            return hash;
        }
    }

    public static Hex FromHexString(string str)
    {
        string input = str;

        if (input.StartsWith("0x"))
        {
            input = input[2..];
        }

        if (input.Length == 0)
        {
            throw new HexException("Hex string is empty.", HexInvalidReason.TooShort);
        }

        if (input.Length % 2 != 0)
        {
            throw new HexException(
                "Hex string must be an even number of hex characters.",
                HexInvalidReason.InvalidLength
            );
        }

        try
        {
            return new Hex(Utilities.HexStringToBytes(input));
        }
        catch
        {
            throw new HexException(
                "Hex string contains invalid hex characters.",
                HexInvalidReason.InvalidCharacters
            );
        }
    }

    public static Hex FromHexInput(byte[] bytes) => new(bytes);

    public static Hex FromHexInput(string input) => FromHexString(input);

    public static bool IsValid(string str)
    {
        try
        {
            FromHexString(str);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class HexConverter : JsonConverter<Hex>
{
    public override Hex? ReadJson(
        JsonReader reader,
        Type objectType,
        Hex? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        string? value = (string?)reader.Value;
        return value != null ? Hex.FromHexString(value) : null;
    }

    public override void WriteJson(JsonWriter writer, Hex? value, JsonSerializer serializer) =>
        writer.WriteValue(value?.ToString());
}
