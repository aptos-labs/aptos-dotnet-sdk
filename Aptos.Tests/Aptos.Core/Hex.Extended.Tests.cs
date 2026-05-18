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
        var set = new HashSet<Hex> { new(new byte[] { 1, 2, 3 }) };
        Assert.Contains(new Hex(new byte[] { 1, 2, 3 }), set);
        Assert.DoesNotContain(new Hex(new byte[] { 1, 2, 4 }), set);
    }

    [Fact]
    public void Constructor_DefensiveCopiesInput()
    {
        // Mutating the source array after construction must not affect the
        // Hex instance — otherwise the hash code / equality drifts and
        // callers that have already inserted the Hex into a Dictionary or
        // HashSet would silently get wrong lookups.
        var source = new byte[] { 1, 2, 3 };
        var hex = new Hex(source);
        var originalHash = hex.GetHashCode();

        source[0] = 0xff;

        Assert.Equal(originalHash, hex.GetHashCode());
        Assert.Equal(new byte[] { 1, 2, 3 }, hex.ToByteArray());
    }

    [Fact]
    public void ToByteArray_ReturnsDefensiveCopy()
    {
        var hex = new Hex(new byte[] { 1, 2, 3 });
        var originalHash = hex.GetHashCode();

        var got = hex.ToByteArray();
        got[0] = 0xff;

        Assert.Equal(originalHash, hex.GetHashCode());
        Assert.Equal(new byte[] { 1, 2, 3 }, hex.ToByteArray());
        // Two consecutive calls must return distinct array instances.
        Assert.NotSame(hex.ToByteArray(), hex.ToByteArray());
    }

    [Fact]
    public void HashSet_RemainsUsableAfterSourceMutation()
    {
        var source = new byte[] { 1, 2, 3 };
        var key = new Hex(source);
        var set = new HashSet<Hex> { key };

        // Mutate the source array and confirm lookups still find the entry.
        source[0] = 0;

        Assert.Contains(new Hex(new byte[] { 1, 2, 3 }), set);
        Assert.Single(set);
    }
}
