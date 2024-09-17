namespace Aptos;

using System.Numerics;
using System.Runtime.Serialization;
using System.Text;

public abstract class Serializable
{
    public abstract void Serialize(Serializer s);

    public byte[] BcsToBytes()
    {
        Serializer serializer = new();
        serializer.Serialize(this);
        return serializer.ToBytes();
    }

    public Hex BcsToHex() => new(BcsToBytes());
}

public class Serializer
{
    private MemoryStream _output;

    public Serializer()
    {
        _output = new MemoryStream();
    }

    public void Serialize(bool v) => Bool(v);

    public void Serialize(byte v) => U8(v);

    public void Serialize(ushort v) => U16(v);

    public void Serialize(uint v) => U32(v);

    public void Serialize(ulong v) => U64(v);

    public void Serialize(Serializable v) => v.Serialize(this);

    public void Serialize<T>(List<T> v)
        where T : Serializable => Vector(v);

    public void Bool(bool v)
    {
        U8(v ? (byte)1 : (byte)0);
    }

    public void U8(byte v)
    {
        _output.WriteByte(v);
    }

    public void U16(ushort v)
    {
        byte[] ub = BitConverter.GetBytes(v);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(ub);
        _output.Write(ub, 0, ub.Length);
    }

    public void U32(uint v)
    {
        byte[] ub = BitConverter.GetBytes(v);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(ub);
        _output.Write(ub, 0, ub.Length);
    }

    public void U64(ulong v)
    {
        byte[] ub = BitConverter.GetBytes(v);
        if (!BitConverter.IsLittleEndian)
            Array.Reverse(ub);
        _output.Write(ub, 0, ub.Length);
    }

    public void U128(BigInteger v)
    {
        // Ensure the BigInteger is unsigned
        if (v.Sign == -1)
            throw new SerializationException("Invalid value for an unsigned int128");

        // This is already little-endian
        byte[] content = v.ToByteArray(isUnsigned: true, isBigEndian: false);

        // BigInteger.toByteArray() may add a most-significant zero
        // byte for signing purpose: ignore it.
        if (!(content.Length <= 16 || content[0] == 0))
            throw new SerializationException("Invalid value for an unsigned int128");

        // Ensure we're padded to 16
        _output.Write(content);
        if (content.Length != 16)
            _output.Write(new byte[16 - content.Length]);
    }

    public void U256(BigInteger v)
    {
        // Ensure the BigInteger is unsigned
        if (v.Sign == -1)
            throw new SerializationException("Invalid value for an unsigned int256");

        // This is already little-endian
        byte[] content = v.ToByteArray(isUnsigned: true, isBigEndian: false);

        // BigInteger.toByteArray() may add a most-significant zero
        // byte for signing purpose: ignore it.
        if (!(content.Length <= 32 || content[0] == 0))
            throw new SerializationException("Invalid value for an unsigned int256");

        // Ensure we're padded to 32
        _output.Write(content);
        if (content.Length != 32)
            _output.Write(new byte[32 - content.Length]);
    }

    public void U32AsUleb128(uint v)
    {
        while (v >= 0x80)
        {
            // Write 7 (lowest) bits of data and set the 8th bit to 1.
            byte b = (byte)(v & 0x7f);
            _output.WriteByte((byte)(b | 0x80));
            v >>= 7;
        }

        // Write the remaining bits of data and set the highest bit to 0
        _output.WriteByte((byte)(v & 0x7f));
    }

    public void String(string v) => Bytes(Encoding.UTF8.GetBytes(v));

    public void OptionString(string? v)
    {
        if (v == null)
        {
            U32AsUleb128(0);
        }
        else
        {
            U32AsUleb128(1);
            String(v);
        }
    }

    public void Option<T>(T? v)
        where T : Serializable
    {
        if (v == null)
        {
            Bool(false);
        }
        else
        {
            Bool(true);
            v.Serialize(this);
        }
    }

    public void Bytes(byte[] v)
    {
        // Write the length of the bytes array
        U32AsUleb128((uint)v.Length);
        // Copy the bytes to the rest of the array
        _output.Write(v);
    }

    public void FixedBytes(byte[] v) => _output.Write(v);

    public void Vector<T>(T[] v)
        where T : Serializable => Vector(v.ToList());

    public void Vector<T>(List<T> v)
        where T : Serializable
    {
        // Write the length of the vector
        U32AsUleb128((uint)v.Count);
        // Serialize each element of the vector
        v.ForEach(e => e.Serialize(this));
    }

    public void Vector<T>(T[] v, Action<T> serializerFunc) => Vector(v.ToList(), serializerFunc);

    public void Vector<T>(List<T> v, Action<T> serializerFunc)
    {
        // Write the length of the vector
        U32AsUleb128((uint)v.Count);
        // Serialize each element of the vector
        v.ForEach(e => serializerFunc(e));
    }

    public byte[] ToBytes()
    {
        return _output.ToArray();
    }

    public void Reset()
    {
        _output = new MemoryStream();
    }
}
