namespace Aptos.Tests.BCS;

using System.Numerics;

public sealed class SerializerTests
{
    private static byte[] ExpectedBytes(string hex) => Hex.FromHexString(hex).ToByteArray();

    [Theory]
    [InlineData("0x0", "0x0000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("0x1", "0x0000000000000000000000000000000000000000000000000000000000000001")]
    [InlineData(
        "0x123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
        "0x0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF"
    )]
    [InlineData(
        "0xA123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
        "0xA123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF"
    )]
    public void SerializeAddress(string value, string expectedBytes)
    {
        Assert.Equal(ExpectedBytes(expectedBytes), AccountAddress.From(value).BcsToBytes());
    }

    [Theory]
    [InlineData("false", "0x00")]
    [InlineData("true", "0x01")]
    public void SerializeBool(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Serialize(bool.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x00")]
    [InlineData("1", "0x01")]
    [InlineData("255", "0xFF")]
    public void SerializeU8(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Serialize(byte.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x0000")]
    [InlineData("1", "0x0100")]
    [InlineData("255", "0xFF00")]
    [InlineData("256", "0x0001")]
    [InlineData("65535", "0xFFFF")]
    public void SerializeU16(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Serialize(ushort.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x00000000")]
    [InlineData("1", "0x01000000")]
    [InlineData("255", "0xFF000000")]
    [InlineData("256", "0x00010000")]
    [InlineData("65535", "0xFFFF0000")]
    [InlineData("65536", "0x00000100")]
    [InlineData("16777215", "0xFFFFFF00")]
    [InlineData("16777216", "0x00000001")]
    [InlineData("4294967295", "0xFFFFFFFF")]
    public void SerializeU32(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Serialize(uint.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x0000000000000000")]
    [InlineData("1", "0x0100000000000000")]
    [InlineData("255", "0xFF00000000000000")]
    [InlineData("256", "0x0001000000000000")]
    [InlineData("65535", "0xFFFF000000000000")]
    [InlineData("65536", "0x0000010000000000")]
    [InlineData("16777215", "0xFFFFFF0000000000")]
    [InlineData("16777216", "0x0000000100000000")]
    [InlineData("4294967295", "0xFFFFFFFF00000000")]
    [InlineData("4294967296", "0x0000000001000000")]
    [InlineData("18446744073709551615", "0xFFFFFFFFFFFFFFFF")]
    public void SerializeU64(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Serialize(ulong.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x00000000000000000000000000000000")]
    [InlineData("1", "0x01000000000000000000000000000000")]
    [InlineData("255", "0xFF000000000000000000000000000000")]
    [InlineData("256", "0x00010000000000000000000000000000")]
    [InlineData("65535", "0xFFFF0000000000000000000000000000")]
    [InlineData("65536", "0x00000100000000000000000000000000")]
    [InlineData("4294967295", "0xFFFFFFFF000000000000000000000000")]
    [InlineData("4294967296", "0x00000000010000000000000000000000")]
    [InlineData("340282366920938463463374607431768211455", "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")]
    public void SerializeU128(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.U128(BigInteger.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    public static TheoryData<string, string> SerializeU256Data =>
        new()
        {
            { "0", "0x0000000000000000000000000000000000000000000000000000000000000000" },
            { "1", "0x0100000000000000000000000000000000000000000000000000000000000000" },
            { "255", "0xFF00000000000000000000000000000000000000000000000000000000000000" },
            { "256", "0x0001000000000000000000000000000000000000000000000000000000000000" },
            {
                "115792089237316195423570985008687907853269984665640564039457584007913129639935",
                "0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"
            },
        };

    [Theory]
    [MemberData(nameof(SerializeU256Data))]
    public void SerializeU256(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.U256(BigInteger.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0", "0x00")]
    [InlineData("1", "0x01")]
    [InlineData("127", "0x7F")]
    [InlineData("128", "0x8001")]
    [InlineData("240", "0xF001")]
    [InlineData("255", "0xFF01")]
    [InlineData("65535", "0xFFFF03")]
    [InlineData("16777215", "0xFFFFFF07")]
    [InlineData("4294967295", "0xFFFFFFFF0F")]
    public void SerializeUleb128(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.U32AsUleb128(uint.Parse(value));
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("0x00", "0x00")]
    [InlineData("0x7F", "0x7F")]
    [InlineData("0x0102", "0x0102")]
    public void SerializeFixedBytes(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.FixedBytes(Hex.FromHexString(value).ToByteArray());
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    public static TheoryData<string, string> SerializeBytesData =>
        new()
        {
            { "0x00", "0x0100" },
            { "0x7F", "0x017F" },
            { "0x0102", "0x020102" },
            {
                "0x0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCD",
                "0x7F0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCD"
            },
            {
                "0x0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF",
                "0x80010123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF"
            },
        };

    [Theory]
    [MemberData(nameof(SerializeBytesData))]
    public void SerializeBytes(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.Bytes(Hex.FromHexString(value).ToByteArray());
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("", "0x00")]
    [InlineData("A", "0x0141")]
    [InlineData("a", "0x0161")]
    [InlineData("abc", "0x03616263")]
    [InlineData("abcd", "0x0461626364")]
    [InlineData("1234abcd", "0x083132333461626364")]
    [InlineData("😀🚀", "0x08F09F9880F09F9A80")]
    public void SerializeString(string value, string expectedBytes)
    {
        var s = new Serializer();
        s.String(value);
        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }

    [Theory]
    [InlineData("bool", "[]", "0x00")]
    [InlineData("bool", "[true]", "0x0101")]
    [InlineData("bool", "[false,true]", "0x020001")]
    [InlineData("u8", "[]", "0x00")]
    [InlineData("u8", "[0]", "0x0100")]
    [InlineData("u8", "[1,3]", "0x020103")]
    [InlineData("u16", "[]", "0x00")]
    [InlineData("u16", "[0]", "0x010000")]
    [InlineData("u16", "[1,3]", "0x0201000300")]
    [InlineData("u32", "[]", "0x00")]
    [InlineData("u32", "[0]", "0x0100000000")]
    [InlineData("u32", "[1,3]", "0x020100000003000000")]
    [InlineData("u64", "[]", "0x00")]
    [InlineData("u64", "[0]", "0x010000000000000000")]
    [InlineData("u64", "[1,3]", "0x0201000000000000000300000000000000")]
    [InlineData("u128", "[]", "0x00")]
    [InlineData("u128", "[0]", "0x0100000000000000000000000000000000")]
    [InlineData(
        "u128",
        "[1,3]",
        "0x020100000000000000000000000000000003000000000000000000000000000000"
    )]
    [InlineData("u256", "[]", "0x00")]
    [InlineData(
        "u256",
        "[0]",
        "0x010000000000000000000000000000000000000000000000000000000000000000"
    )]
    [InlineData(
        "u256",
        "[1,3]",
        "0x0201000000000000000000000000000000000000000000000000000000000000000300000000000000000000000000000000000000000000000000000000000000"
    )]
    [InlineData("uleb128", "[]", "0x00")]
    [InlineData("uleb128", "[0]", "0x0100")]
    [InlineData("uleb128", "[128,127]", "0x0280017F")]
    [InlineData("address", "[]", "0x00")]
    [InlineData(
        "address",
        "[0x1]",
        "0x010000000000000000000000000000000000000000000000000000000000000001"
    )]
    [InlineData(
        "address",
        "[0x2,0x0]",
        "0x0200000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000000"
    )]
    [InlineData("string", "[]", "0x00")]
    [InlineData("string", "[\"😀🚀\"]", "0x0108F09F9880F09F9A80")]
    [InlineData("string", "[\"a\",\"b\"]", "0x0201610162")]
    public void SerializeSequence(string type, string inputValues, string expectedBytes)
    {
        var items = BaseTests.ParseArray(inputValues);
        var s = new Serializer();

        switch (type)
        {
            case "bool":
                s.Vector(items.Select(bool.Parse).ToArray(), s.Serialize);
                break;
            case "u8":
                s.Vector(items.Select(byte.Parse).ToArray(), s.Serialize);
                break;
            case "u16":
                s.Vector(items.Select(ushort.Parse).ToArray(), s.Serialize);
                break;
            case "u32":
                s.Vector(items.Select(uint.Parse).ToArray(), s.Serialize);
                break;
            case "u64":
                s.Vector(items.Select(ulong.Parse).ToArray(), s.Serialize);
                break;
            case "u128":
                s.Vector(items.Select(BigInteger.Parse).ToArray(), s.U128);
                break;
            case "u256":
                s.Vector(items.Select(BigInteger.Parse).ToArray(), s.U256);
                break;
            case "uleb128":
                s.Vector(items.Select(uint.Parse).ToArray(), s.U32AsUleb128);
                break;
            case "address":
                s.Vector(items.Select(AccountAddress.From).ToArray());
                break;
            case "string":
                s.Vector(items.Select(i => i.Replace("\"", "")).ToArray(), s.String);
                break;
        }

        Assert.Equal(ExpectedBytes(expectedBytes), s.ToBytes());
    }
}
