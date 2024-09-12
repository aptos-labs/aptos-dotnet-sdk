namespace Aptos.Poseidon;

using System.Numerics;
using System.Text;

public static class BytesHash
{

    private static readonly int _bytesPackedPerScalar = 31;

    private static readonly int _maxNumInputScalars = 16;

    private static readonly int _maxNumInputBytes = (_maxNumInputScalars - 1) * _bytesPackedPerScalar;

    public static BigInteger HashWithLength(string str, int maxSizeBytes) => HashWithLength(Encoding.UTF8.GetBytes(str), maxSizeBytes);
    public static BigInteger HashWithLength(byte[] bytes, int maxSizeBytes)
    {
        if (bytes.Length > maxSizeBytes) throw new ArgumentException($"Input bytes of length {bytes.Length} is longer than {maxSizeBytes} bytes");
        var packed = PadAndPackBytesWithLength(bytes, maxSizeBytes);
        return Hash.PoseidonHash(packed);
    }

    public static List<BigInteger> PadAndPackBytesWithLength(byte[] bytes, int maxSizeBytes)
    {
        if (bytes.Length > maxSizeBytes) throw new ArgumentException($"Input bytes of length {bytes.Length} is longer than {maxSizeBytes} bytes");
        return [.. PadAndPackBytesWithNoLength(bytes, maxSizeBytes), new BigInteger(bytes.Length)];
    }

    public static List<BigInteger> PadAndPackBytesWithNoLength(byte[] bytes, int maxSizeBytes)
    {
        if (bytes.Length > maxSizeBytes) throw new ArgumentException($"Input bytes of length {bytes.Length} is longer than {maxSizeBytes} bytes");
        var paddedStrBytes = PadBytesWithZeros(bytes, maxSizeBytes);
        return PackBytes(paddedStrBytes);
    }

    public static List<BigInteger> PackBytes(byte[] bytes)
    {
        if (bytes.Length > _maxNumInputBytes) throw new ArgumentException($"Can't pack more than {_maxNumInputBytes} bytes");
        return ChunkBytes(bytes, _bytesPackedPerScalar).Select(BytesToBigIntegerLE).ToList();
    }

    public static BigInteger BytesToBigIntegerLE(byte[] bytes)
    {
        var result = BigInteger.Zero;
        for (var i = bytes.Length - 1; i >= 0; i -= 1)
        {
            result = (result << 8) | new BigInteger(bytes[i]);
        }
        return result;
    }

    public static byte[] BigIntegerToBytesLE(BigInteger value, int length)
    {
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = (byte)(value & 0xff);
            value >>= 8;
        }
        return bytes;
    }

    public static byte[][] ChunkBytes(byte[] bytes, int chunkSize)
    {
        List<byte[]> result = [];
        for (int i = 0; i < bytes.Length; i += chunkSize)
        {
            int size = Math.Min(chunkSize, bytes.Length - i);
            byte[] chunk = new byte[size];
            Array.Copy(bytes, i, chunk, 0, size);
            result.Add(chunk);
        }
        return [.. result];
    }

    public static byte[] PadBytesWithZeros(byte[] bytes, int paddedSize)
    {
        if (paddedSize < bytes.Length) throw new ArgumentException($"Padded size {paddedSize} must be greater than or equal to the input array size ({bytes.Length})");
        byte[] paddedBytes = new byte[paddedSize];
        Array.Copy(bytes, paddedBytes, bytes.Length);
        return paddedBytes;
    }

}