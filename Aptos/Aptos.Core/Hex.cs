namespace Aptos;

using Aptos.Core;
using Aptos.Exceptions;
using Newtonsoft.Json;

/// <summary>
/// An immutable wrapper around a byte sequence that supports hex parsing /
/// formatting and value-based equality / hashing.
///
/// <para>
/// The bytes are defensively copied on construction and on every read so
/// that <see cref="Hex"/> can safely be used as a key in
/// <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> /
/// <see cref="System.Collections.Generic.HashSet{T}"/>: external mutation
/// of the input or returned arrays cannot perturb the instance's hash code
/// or equality. Internal callers that need direct (non-copying) access to
/// the underlying bytes use <see cref="GetUnsafeByteArrayReference"/>.
/// </para>
/// </summary>
[JsonConverter(typeof(HexConverter))]
public class Hex
{
    private readonly byte[] _data;

    public Hex(byte[] data)
    {
        // Defensive copy on input so the caller cannot mutate the bytes
        // backing this Hex after construction.
        _data = (byte[])data.Clone();
    }

    /// <summary>
    /// Returns a <em>copy</em> of the underlying bytes. Mutating the
    /// returned array is safe and will not affect this Hex instance.
    /// </summary>
    public byte[] ToByteArray() => (byte[])_data.Clone();

    /// <summary>
    /// Internal-only: returns the raw byte array without copying. Used by
    /// trusted call sites (e.g. <see cref="PrivateKey.Dispose"/>) where the
    /// caller is responsible for not mutating the returned reference.
    ///
    /// <para>This intentionally does <strong>not</strong> have a public
    /// counterpart — public consumers must use <see cref="ToByteArray"/>
    /// which preserves immutability.</para>
    /// </summary>
    internal byte[] GetUnsafeByteArrayReference() => _data;

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
            catch (HexException)
            {
                // Malformed hex input is treated as not equal. Other
                // exception types are intentionally not caught so genuine
                // bugs (NullReferenceException, OutOfMemoryException, etc.)
                // surface to the caller.
                return false;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        // Hash by value so that two Hex instances containing the same bytes
        // produce the same hash code. Combined with the defensive copies in
        // the constructor and ToByteArray, this guarantees that a Hex used
        // as a Dictionary / HashSet key never has its hash drift even if
        // the source array is later mutated by the caller.
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
        catch (HexException)
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
