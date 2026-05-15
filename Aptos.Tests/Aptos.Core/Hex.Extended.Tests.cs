namespace Aptos.Tests.Core;

using Aptos.Exceptions;

public class HexExtendedTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void FromHexInput_Bytes_AndString()
    {
        var fromBytes = Hex.FromHexInput(new byte[] { 0x01, 0x02 });
        var fromString = Hex.FromHexInput("0x0102");
        Assert.Equal(fromBytes, fromString);
        Assert.Equal("0x0102", fromBytes.ToString());
    }

    [Fact]
    public void FromHexString_NoPrefix()
    {
        var hex = Hex.FromHexString("ff");
        Assert.Equal(new byte[] { 0xff }, hex.ToByteArray());
        Assert.Equal("0xff", hex.ToString());
    }

    [Fact]
    public void FromHexString_OddLength_Throws()
    {
        var ex = Assert.Throws<HexException>(() => Hex.FromHexString("0xabc"));
        Assert.Equal(HexInvalidReason.InvalidLength, ex.Reason);
    }

    [Fact]
    public void FromHexString_InvalidChars_Throws()
    {
        var ex = Assert.Throws<HexException>(() => Hex.FromHexString("0xZZ"));
        Assert.Equal(HexInvalidReason.InvalidCharacters, ex.Reason);
    }

    [Fact]
    public void FromHexString_Empty_Throws()
    {
        var ex = Assert.Throws<HexException>(() => Hex.FromHexString("0x"));
        Assert.Equal(HexInvalidReason.TooShort, ex.Reason);
    }

    [Fact]
    public void IsValid_ReturnsBool()
    {
        Assert.True(Hex.IsValid("0xff"));
        Assert.True(Hex.IsValid("ff"));
        Assert.False(Hex.IsValid("0xZZ"));
        Assert.False(Hex.IsValid("0x"));
    }

    [Fact]
    public void Equals_AndHashCode_ByValue()
    {
        var a = new Hex(new byte[] { 1, 2, 3 });
        var b = new Hex(new byte[] { 1, 2, 3 });
        Assert.True(a.Equals(b));
        Assert.True(a.Equals(b.ToByteArray()));
        Assert.True(a.Equals("0x010203"));
        Assert.False(a.Equals("0x010204"));
        // Equal instances must have equal hash codes.
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.False(a.Equals(null));
        Assert.False(a.Equals(123));
    }

    [Fact]
    public void Equals_OnMalformedString_ReturnsFalse()
    {
        var a = new Hex(new byte[] { 1 });
        // Equality must not throw on malformed input.
        Assert.False(a.Equals("not-hex"));
    }

    [Fact]
    public void GetHashCode_WorksInHashSet()
    {
        var set = new HashSet<Hex>
        {
            new(new byte[] { 1, 2, 3 }),
        };
        Assert.Contains(new Hex(new byte[] { 1, 2, 3 }), set);
        Assert.DoesNotContain(new Hex(new byte[] { 1, 2, 4 }), set);
    }
}
