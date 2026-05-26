namespace Aptos.Tests.BCS;

using System.Numerics;

public sealed class DeserializerTests
{
    private static Deserializer MakeDeserializer(string hex) =>
        new(hex == "0x" ? [] : Hex.FromHexString(hex).ToByteArray());

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
    public void DeserializeAddress(string expectedAddress, string bytes)
    {
        var result = AccountAddress.Deserialize(MakeDeserializer(bytes));
        Assert.Equal(AccountAddress.From(expectedAddress).BcsToBytes(), result.BcsToBytes());
    }

    [Theory]
    [InlineData("false", "0x00")]
    [InlineData("true", "0x01")]
    public void DeserializeBool(string expectedValue, string bytes)
    {
        Assert.Equal(bool.Parse(expectedValue), MakeDeserializer(bytes).Bool());
    }

    [Theory]
    [InlineData("0x02")]
    [InlineData("0xFF")]
    public void DeserializeBoolFails(string bytes)
    {
        Assert.ThrowsAny<Exception>(() => MakeDeserializer(bytes).Bool());
    }

    [Theory]
    [InlineData("0", "0x00")]
    [InlineData("1", "0x01")]
    [InlineData("255", "0xFF")]
    public void DeserializeU8(string expectedValue, string bytes)
    {
        Assert.Equal(byte.Parse(expectedValue), MakeDeserializer(bytes).U8());
    }

    [Theory]
    [InlineData("0", "0x0000")]
    [InlineData("1", "0x0100")]
    [InlineData("255", "0xFF00")]
    [InlineData("256", "0x0001")]
    [InlineData("65535", "0xFFFF")]
    public void DeserializeU16(string expectedValue, string bytes)
    {
        Assert.Equal(ushort.Parse(expectedValue), MakeDeserializer(bytes).U16());
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
    public void DeserializeU32(string expectedValue, string bytes)
    {
        Assert.Equal(uint.Parse(expectedValue), MakeDeserializer(bytes).U32());
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
    public void DeserializeU64(string expectedValue, string bytes)
    {
        Assert.Equal(ulong.Parse(expectedValue), MakeDeserializer(bytes).U64());
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
    public void DeserializeU128(string expectedValue, string bytes)
    {
        Assert.Equal(BigInteger.Parse(expectedValue), MakeDeserializer(bytes).U128());
    }

    public static TheoryData<string, string> DeserializeU256Data =>
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
    [MemberData(nameof(DeserializeU256Data))]
    public void DeserializeU256(string expectedValue, string bytes)
    {
        Assert.Equal(BigInteger.Parse(expectedValue), MakeDeserializer(bytes).U256());
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
    public void DeserializeUleb128(string expectedValue, string bytes)
    {
        Assert.Equal(uint.Parse(expectedValue), MakeDeserializer(bytes).Uleb128AsU32());
    }

    [Theory]
    [InlineData("0x80")]
    [InlineData("0xFFFFFFFF10")]
    public void DeserializeUleb128Fails(string bytes)
    {
        Assert.ThrowsAny<Exception>(() => MakeDeserializer(bytes).Uleb128AsU32());
    }

    [Theory]
    [InlineData("0x", "0x", 0)]
    [InlineData("0x00", "0x00", 1)]
    [InlineData("0x7F", "0x7F", 1)]
    [InlineData("0x0102", "0x0102", 2)]
    public void DeserializeFixedBytes(string expectedHex, string bytes, int length)
    {
        var result = MakeDeserializer(bytes).FixedBytes(length);
        Assert.Equal(
            expectedHex == "0x" ? [] : Hex.FromHexString(expectedHex).ToByteArray(),
            result
        );
    }

    [Fact]
    public void DeserializeFixedBytesTooShortFails()
    {
        Assert.ThrowsAny<Exception>(() => new Deserializer([0x00]).FixedBytes(2));
    }

    public static TheoryData<string, string> DeserializeBytesData =>
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
    [MemberData(nameof(DeserializeBytesData))]
    public void DeserializeBytes(string expectedHex, string bytes)
    {
        var result = MakeDeserializer(bytes).Bytes();
        Assert.Equal(Hex.FromHexString(expectedHex).ToByteArray(), result);
    }

    [Theory]
    [InlineData("", "0x00")]
    [InlineData("A", "0x0141")]
    [InlineData("a", "0x0161")]
    [InlineData("abc", "0x03616263")]
    [InlineData("abcd", "0x0461626364")]
    [InlineData("1234abcd", "0x083132333461626364")]
    [InlineData("😀🚀", "0x08F09F9880F09F9A80")]
    public void DeserializeString(string expectedValue, string bytes)
    {
        Assert.Equal(expectedValue, MakeDeserializer(bytes).String());
    }

    [Theory]
    [InlineData("bool", "0x00", "[]")]
    [InlineData("bool", "0x0101", "[true]")]
    [InlineData("bool", "0x020001", "[false,true]")]
    [InlineData("u8", "0x00", "[]")]
    [InlineData("u8", "0x0100", "[0]")]
    [InlineData("u8", "0x020103", "[1,3]")]
    [InlineData("u16", "0x00", "[]")]
    [InlineData("u16", "0x010000", "[0]")]
    [InlineData("u16", "0x0201000300", "[1,3]")]
    [InlineData("u32", "0x00", "[]")]
    [InlineData("u32", "0x0100000000", "[0]")]
    [InlineData("u32", "0x020100000003000000", "[1,3]")]
    [InlineData("u64", "0x00", "[]")]
    [InlineData("u64", "0x010000000000000000", "[0]")]
    [InlineData("u64", "0x0201000000000000000300000000000000", "[1,3]")]
    [InlineData("u128", "0x00", "[]")]
    [InlineData("u128", "0x0100000000000000000000000000000000", "[0]")]
    [InlineData("u128", "0x020100000000000000000000000000000003000000000000000000000000000000", "[1,3]")]
    [InlineData("u256", "0x00", "[]")]
    [InlineData("u256", "0x010000000000000000000000000000000000000000000000000000000000000000", "[0]")]
    [InlineData(
        "u256",
        "0x0201000000000000000000000000000000000000000000000000000000000000000300000000000000000000000000000000000000000000000000000000000000",
        "[1,3]"
    )]
    [InlineData("uleb128", "0x00", "[]")]
    [InlineData("uleb128", "0x0100", "[0]")]
    [InlineData("uleb128", "0x0280017F", "[128,127]")]
    [InlineData("address", "0x00", "[]")]
    [InlineData("address", "0x010000000000000000000000000000000000000000000000000000000000000001", "[0x1]")]
    [InlineData(
        "address",
        "0x0200000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000000",
        "[0x2,0x0]"
    )]
    [InlineData("string", "0x00", "[]")]
    [InlineData("string", "0x0108F09F9880F09F9A80", "[\"😀🚀\"]")]
    [InlineData("string", "0x0201610162", "[\"a\",\"b\"]")]
    public void DeserializeSequence(string type, string bytes, string expectedValues)
    {
        var d = MakeDeserializer(bytes);
        var items = BaseTests.ParseArray(expectedValues);

        switch (type)
        {
            case "bool":
                Assert.Equal(
                    items.Select(bool.Parse).ToList(),
                    d.Vector(x => x.Bool())
                );
                break;
            case "u8":
                Assert.Equal(items.Select(byte.Parse).ToList(), d.Vector(x => x.U8()));
                break;
            case "u16":
                Assert.Equal(items.Select(ushort.Parse).ToList(), d.Vector(x => x.U16()));
                break;
            case "u32":
                Assert.Equal(items.Select(uint.Parse).ToList(), d.Vector(x => x.U32()));
                break;
            case "u64":
                Assert.Equal(items.Select(ulong.Parse).ToList(), d.Vector(x => x.U64()));
                break;
            case "u128":
                Assert.Equal(
                    items.Select(BigInteger.Parse).ToList(),
                    d.Vector(x => x.U128())
                );
                break;
            case "u256":
                Assert.Equal(
                    items.Select(BigInteger.Parse).ToList(),
                    d.Vector(x => x.U256())
                );
                break;
            case "uleb128":
                Assert.Equal(
                    items.Select(uint.Parse).ToList(),
                    d.Vector(x => x.Uleb128AsU32())
                );
                break;
            case "address":
                Assert.Equal(
                    items.Select(a => AccountAddress.From(a).ToStringLong()),
                    d.Vector(AccountAddress.Deserialize).Select(a => a.ToStringLong())
                );
                break;
            case "string":
                Assert.Equal(
                    items.Select(s => s.Trim('"')).ToList(),
                    d.Vector(x => x.String())
                );
                break;
        }
    }

    [Theory]
    [InlineData("bool", "0x")]
    [InlineData("u8", "0x")]
    [InlineData("u16", "0x00")]
    [InlineData("u32", "0x000000")]
    [InlineData("u64", "0x00000000000000")]
    [InlineData("u128", "0x000000000000000000000000000000")]
    [InlineData("u256", "0x00000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("address", "0x00000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("address", "0x01")]
    [InlineData("bytes", "0x")]
    [InlineData("bytes", "0x01")]
    [InlineData("string", "0x")]
    [InlineData("string", "0x01")]
    public void DeserializeTooFewBytesFails(string type, string bytes)
    {
        var d = MakeDeserializer(bytes);
        Assert.ThrowsAny<Exception>(() =>
        {
            _ = type switch
            {
                "bool" => (object)d.Bool(),
                "u8" => d.U8(),
                "u16" => d.U16(),
                "u32" => d.U32(),
                "u64" => d.U64(),
                "u128" => d.U128(),
                "u256" => d.U256(),
                "address" => AccountAddress.Deserialize(d),
                "bytes" => d.Bytes(),
                "string" => d.String(),
                _ => throw new ArgumentException("Invalid type"),
            };
        });
    }
}
