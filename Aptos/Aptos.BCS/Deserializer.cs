namespace Aptos;

using System.Numerics;
using System.Text;

public interface Deserializable<T>
{
    public T Deserialize(Deserializer deserializer);
}

public class Deserializer
{
    private MemoryStream _input;

    private int _length;

    public Deserializer(string data) : this(Hex.FromHexString(data).ToByteArray()) { }
    public Deserializer(byte[] data)
    {
        _input = new MemoryStream(data);
        _length = data.Length;
    }

    public long Remaining() => _length - _input.Position;

    public byte[] Read(int length)
    {
        if (length == 0) return [];
        byte[] value = new byte[length];
        int totalRead = _input.Read(value, 0, length);
        if (totalRead == 0 || totalRead < length)
            throw new ArgumentException("Unexpected end of input. Requested: " + length + ", found: " + totalRead);
        return value;
    }

    public T Deserialize<T>(Deserializable<T> cls) => cls.Deserialize(this);

    public bool Bool()
    {
        byte value = Read(1)[0];
        if (value != 0 && value != 1) throw new ArgumentException("Invalid bool value");
        return value == 1;
    }

    public byte U8() => Read(1)[0];

    public ushort U16() => BitConverter.ToUInt16(Read(2));

    public uint U32() => BitConverter.ToUInt32(Read(4));

    public ulong U64() => BitConverter.ToUInt64(Read(8));

    public BigInteger U128()
    {
        ulong low = U64();
        ulong high = U64();

        // Combine the two 64-bit values into a 128-bit value (little endian)
        return ((BigInteger)high << 64) | low;
    }

    public BigInteger U256()
    {
        BigInteger low = U128();
        BigInteger high = U128();

        // Combine the two 128-bit values into a 256-bit value (little endian)
        return (high << 128) | low;
    }

    public uint Uleb128AsU32()
    {
        uint maxU32 = 0xFFFFFFFF;

        BigInteger value = BigInteger.Zero;
        int shift = 0;

        while (value < maxU32)
        {
            byte b = Read(1)[0];
            value |= (BigInteger)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
        }

        if (value > maxU32)
        {
            throw new ArgumentException("Value is too large to fit in a uint32");
        }

        return (uint)value;
    }

    public string String() => Encoding.UTF8.GetString(Bytes());
    public string? OptionString() => Bool() ? String() : null;

    public T? Option<T>(Func<Deserializer, T> deserializeFunc) where T : class => Bool() ? deserializeFunc(this) : null;

    public byte[] Bytes()
    {
        uint length = Uleb128AsU32();
        return Read((int)length);
    }

    public byte[] FixedBytes(int length) => Read(length);

    public List<T> Vector<T>(Func<Deserializer, T> deserializeFunc)
    {
        uint length = Uleb128AsU32();
        List<T> result = new((int)length);
        for (uint i = 0; i < length; i++) result.Add(deserializeFunc(this));
        return result;
    }

}