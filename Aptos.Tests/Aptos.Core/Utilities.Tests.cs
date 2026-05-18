namespace Aptos.Tests.Core;

using Aptos.Core;
using Aptos.Exceptions;

public class UtilitiesTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void ParseFunctionParts_ValidString()
    {
        var (addr, mod, fn) = Utilities.ParseFunctionParts("0x1::aptos_account::transfer");
        Assert.Equal("0x1", addr);
        Assert.Equal("aptos_account", mod);
        Assert.Equal("transfer", fn);
    }

    [Fact]
    public void ParseFunctionParts_InvalidFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() => Utilities.ParseFunctionParts("not_a_function"));
        Assert.Throws<ArgumentException>(() => Utilities.ParseFunctionParts("0x1::module"));
        Assert.Throws<ArgumentException>(() => Utilities.ParseFunctionParts("0x1::m::f::extra"));
    }

    [Fact]
    public void HexStringToBytes_AcceptsBothPrefixed_AndPlain()
    {
        Assert.Equal(new byte[] { 0xab, 0xcd }, Utilities.HexStringToBytes("0xABCD"));
        Assert.Equal(new byte[] { 0xab, 0xcd }, Utilities.HexStringToBytes("abcd"));
        Assert.Empty(Utilities.HexStringToBytes(""));
    }

    [Fact]
    public void HexStringToString_DecodesAscii()
    {
        // "Hi" is 0x4869
        Assert.Equal("Hi", Utilities.HexStringToString("0x4869"));
    }

    [Fact]
    public void FloorToWholeHour_RoundsDown()
    {
        // 2024-01-01 12:34:56 UTC = 1704112496
        // Floored to 12:00:00 UTC = 1704110400
        Assert.Equal(1704110400L, Utilities.FloorToWholeHour(1704112496L));
        Assert.Equal(0L, Utilities.FloorToWholeHour(0L));
    }

    [Fact]
    public void DeserializeJObjectOrString_ValidJson_ReturnsJObject()
    {
        var result = Utilities.DeserializeJObjectOrString("{\"a\":1}");
        Assert.NotNull(result);
        Assert.Equal(
            Newtonsoft.Json.Linq.JObject.Parse("{\"a\":1}").ToString(),
            result!.ToString()
        );
    }

    [Fact]
    public void DeserializeJObjectOrString_NotJson_ReturnsString()
    {
        var result = Utilities.DeserializeJObjectOrString("just a string");
        Assert.Equal("just a string", result);
    }

    [Fact]
    public void StandardizeTypeTags_ConvertsAllVariants()
    {
        var tags = Utilities.StandardizeTypeTags(["u8", new TypeTagU64()]);
        Assert.Equal(2, tags.Count);
        Assert.IsType<TypeTagU8>(tags[0]);
        Assert.IsType<TypeTagU64>(tags[1]);

        Assert.Throws<ArgumentException>(() => Utilities.StandardizeTypeTags([123]));
    }

    [Fact]
    public void ToUnixTimestamp_RoundsCorrectly()
    {
        var ts = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToUnixTimestamp();
        Assert.Equal(1577836800, ts);
    }

    [Fact]
    public void UnwrapOption_SomeReturnsValue_NoneReturnsNull()
    {
        var some = new { vec = new[] { "hello" } };
        Assert.Equal("hello", Utilities.UnwrapOption<string>(some));

        var none = new { vec = Array.Empty<string>() };
        Assert.Null(Utilities.UnwrapOption<string>(none));
    }

    [Fact]
    public void Hex_FromHexInput_RoundTrip()
    {
        var bytes = new byte[] { 1, 2, 3 };
        Assert.Equal(bytes, Hex.FromHexInput(bytes).ToByteArray());
    }

    [Fact]
    public void Extensions_ToUnixTimestamp_HandlesLocalAndUtc()
    {
        var utcTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(1735689600, utcTime.ToUnixTimestamp());
    }
}
