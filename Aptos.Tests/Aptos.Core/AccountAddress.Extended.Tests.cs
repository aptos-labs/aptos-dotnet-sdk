namespace Aptos.Tests.Core;

using Aptos.Exceptions;
using Aptos.Schemes;

/// <summary>
/// Extended tests for <see cref="AccountAddress"/> that complement the gherkin
/// feature tests. Covers equality, hashing, JSON, byte / Hex constructors, and
/// the seed-based address derivation methods.
/// </summary>
public class AccountAddressExtendedTests(ITestOutputHelper output) : BaseTests(output)
{
    private const string LONG_ADDR =
        "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void Zero_Address_RoundTripIsZero()
    {
        Assert.Equal("0x0", AccountAddress.ZERO.ToString());
        Assert.Equal(
            "0x0000000000000000000000000000000000000000000000000000000000000000",
            AccountAddress.ZERO.ToStringLong()
        );
        Assert.True(AccountAddress.ZERO.IsSpecial());
    }

    [Fact]
    public void Special_Address_Detected()
    {
        Assert.True(AccountAddress.FromString("0x1", 63).IsSpecial());
        Assert.True(AccountAddress.FromString("0xf", 63).IsSpecial());
        Assert.False(AccountAddress.FromString("0x10", 63).IsSpecial());
        Assert.False(AccountAddress.FromString(LONG_ADDR).IsSpecial());
    }

    [Fact]
    public void FromString_AcceptsBytesAndHex()
    {
        var fromLong = AccountAddress.From(LONG_ADDR);
        var fromBytes = AccountAddress.From(fromLong.Data);
        var fromHex = AccountAddress.From(Hex.FromHexString(LONG_ADDR));

        Assert.Equal(fromLong, fromBytes);
        Assert.Equal(fromLong, fromHex);
        // Pass-through overload should return the same instance reference.
        Assert.Same(fromLong, AccountAddress.From(fromLong));
    }

    [Fact]
    public void FromStringStrict_RejectsShortAddress()
    {
        Assert.Throws<AccountAddressParsingException>(
            () => AccountAddress.FromStringStrict("0x123") // not LONG form and not 0x0-0xf
        );
        Assert.Throws<AccountAddressParsingException>(
            () =>
                AccountAddress.FromStringStrict(
                    "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
                ) // missing 0x prefix
        );
    }

    [Fact]
    public void FromStringStrict_AcceptsLongAndSpecial()
    {
        // Special single-digit form is accepted
        Assert.Equal("0x0", AccountAddress.FromStringStrict("0x0").ToString());
        Assert.Equal(LONG_ADDR, AccountAddress.FromStringStrict(LONG_ADDR).ToStringLong());
    }

    [Fact]
    public void FromString_TooLong_Throws()
    {
        var ex = Assert.Throws<AccountAddressParsingException>(
            () => AccountAddress.FromString("0x" + new string('a', 65))
        );
        Assert.Equal(AccountAddressInvalidReason.TooLong, ex.Reason);
    }

    [Fact]
    public void FromString_Empty_Throws()
    {
        Assert.Throws<AccountAddressParsingException>(() => AccountAddress.FromString("0x"));
        Assert.Throws<AccountAddressParsingException>(() => AccountAddress.FromString(""));
    }

    [Fact]
    public void FromString_InvalidPaddingStrictness_Throws()
    {
        Assert.Throws<AccountAddressParsingException>(() => AccountAddress.FromString("0x1", -1));
        Assert.Throws<AccountAddressParsingException>(() => AccountAddress.FromString("0x1", 64));
    }

    [Fact]
    public void FromString_InvalidHex_Throws()
    {
        Assert.Throws<AccountAddressParsingException>(
            () => AccountAddress.FromString("0xZZ0102030405060708090a0b0c0d0e0f")
        );
    }

    [Fact]
    public void Constructor_WrongLength_Throws()
    {
        Assert.Throws<AccountAddressParsingException>(() => new AccountAddress(new byte[16]));
    }

    [Fact]
    public void IsValid_ReturnsBool()
    {
        Assert.True(AccountAddress.IsValid("0x1", false));
        Assert.True(AccountAddress.IsValid(LONG_ADDR, true));
        // Special special addresses (0x0-0xf) are valid in strict mode but
        // non-special short forms are not.
        Assert.True(AccountAddress.IsValid("0x1", true));
        Assert.False(AccountAddress.IsValid("0x10", true));
        Assert.False(AccountAddress.IsValid("not-an-address", false));
    }

    [Fact]
    public void Equals_AndHashCode_ByValue()
    {
        var a = AccountAddress.From(LONG_ADDR);
        var b = AccountAddress.From(LONG_ADDR);
        Assert.True(a.Equals(b));
        // Same value => same hash code. This is the contract that the
        // previous reference-based hash code implementation violated.
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        Assert.True(a.Equals(b.Data));
        Assert.True(a.Equals(LONG_ADDR));
        Assert.False(a.Equals(AccountAddress.ZERO));
        Assert.False(a.Equals(null));
        Assert.False(a.Equals(123));
    }

    [Fact]
    public void Equals_StringInvalidReturnsFalse_NotThrow()
    {
        var a = AccountAddress.From(LONG_ADDR);
        // Invalid input should result in false, not a thrown exception.
        Assert.False(a.Equals("not-an-address"));
    }

    [Fact]
    public void GetHashCode_WorksInDictionary()
    {
        var dict = new Dictionary<AccountAddress, int>();
        var addr1 = AccountAddress.From(LONG_ADDR);
        dict[addr1] = 1;

        // A new AccountAddress with the same bytes must be a valid key.
        var addr2 = AccountAddress.From(LONG_ADDR);
        Assert.True(dict.ContainsKey(addr2));
        Assert.Equal(1, dict[addr2]);
    }

    [Fact]
    public void CreateObjectAddress_Deterministic()
    {
        var creator = AccountAddress.From(LONG_ADDR);
        var seed1 = AccountAddress.CreateObjectAddress(creator, "seed");
        var seed2 = AccountAddress.CreateObjectAddress(creator, "seed");
        var seed3 = AccountAddress.CreateObjectAddress(creator, "different");

        Assert.Equal(seed1, seed2);
        Assert.NotEqual(seed1, seed3);
        Assert.Equal(32, seed1.Data.Length);
    }

    [Fact]
    public void CreateResourceAddress_Deterministic()
    {
        var creator = AccountAddress.From(LONG_ADDR);
        var seed1 = AccountAddress.CreateResourceAddress(creator, "x");
        var seed2 = AccountAddress.CreateResourceAddress(creator, "x");
        var seed3 = AccountAddress.CreateResourceAddress(creator, "y");
        Assert.Equal(seed1, seed2);
        Assert.NotEqual(seed1, seed3);
    }

    [Fact]
    public void CreateSeedAddress_DifferentSchemes_DifferentResults()
    {
        var creator = AccountAddress.From(LONG_ADDR);
        var obj = AccountAddress.CreateSeedAddress(
            creator,
            new byte[] { 1, 2, 3 },
            DeriveScheme.DeriveObjectAddressFromObject
        );
        var res = AccountAddress.CreateSeedAddress(
            creator,
            new byte[] { 1, 2, 3 },
            DeriveScheme.DeriveResourceAccountAddress
        );
        Assert.NotEqual(obj, res);
    }

    [Fact]
    public void Serialize_Deserialize_RoundTrip()
    {
        var original = AccountAddress.From(LONG_ADDR);
        var bytes = original.BcsToBytes();
        Assert.Equal(32, bytes.Length);
        var deserialized = AccountAddress.Deserialize(new Deserializer(bytes));
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void SerializeForScriptFunction_PrependsVariant()
    {
        var addr = AccountAddress.ZERO;
        var s = new Serializer();
        addr.SerializeForScriptFunction(s);
        var bytes = s.ToBytes();
        Assert.Equal((byte)ScriptTransactionArgumentVariants.Address, bytes[0]);
        Assert.Equal(33, bytes.Length);
    }

    [Fact]
    public void Constructor_DefensiveCopiesInput()
    {
        // Mutating the source array after construction must not affect the
        // AccountAddress's hash code or equality.
        var source = new byte[32];
        source[0] = 1;
        var addr = new AccountAddress(source);
        var originalHash = addr.GetHashCode();

        source[0] = 0xff;

        Assert.Equal(originalHash, addr.GetHashCode());
        Assert.Equal(1, addr.Data[0]);
    }

    [Fact]
    public void Data_ReturnsDefensiveCopy()
    {
        var addr = AccountAddress.From(LONG_ADDR);
        var originalHash = addr.GetHashCode();

        var got = addr.Data;
        got[0] = 0xff;

        Assert.Equal(originalHash, addr.GetHashCode());
        Assert.NotEqual(0xff, addr.Data[0]);
        // Two consecutive Data reads must return distinct array instances.
        Assert.NotSame(addr.Data, addr.Data);
    }

    [Fact]
    public void ToByteArray_ReturnsDefensiveCopy()
    {
        var addr = AccountAddress.From(LONG_ADDR);
        var originalHash = addr.GetHashCode();

        var got = addr.ToByteArray();
        got[0] = 0xff;

        Assert.Equal(originalHash, addr.GetHashCode());
        Assert.NotEqual(0xff, addr.ToByteArray()[0]);
    }

    [Fact]
    public void Dictionary_RemainsUsableAfterSourceMutation()
    {
        var source = new byte[32];
        source[31] = 1;
        var key = new AccountAddress(source);
        var dict = new Dictionary<AccountAddress, string> { [key] = "hello" };

        source[31] = 0;

        // The dictionary entry is still keyed by the original 0x...01.
        Assert.True(
            dict.ContainsKey(
                new AccountAddress(
                    new byte[31]
                        .Concat(new byte[] { 1 })
                        .ToArray()
                )
            )
        );
    }
}
