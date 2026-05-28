namespace Aptos.Tests.Core;

public sealed class AccountAddressTests
{
    [Theory]
    [InlineData("0x1", "0x1")]
    [InlineData("0x0000000000000000000000000000000000000001", "0x1")]
    [InlineData("0x2", "0x2")]
    [InlineData("0x0000000000000000000000000000000000000002", "0x2")]
    [InlineData(
        "0x1111111111111111111111111111111111111112",
        "0x1111111111111111111111111111111111111112"
    )]
    [InlineData(
        "0x0111111111111111111111111111111111111112",
        "0x0111111111111111111111111111111111111112"
    )]
    [InlineData(
        "0x111111111111111111111111111111111111112",
        "0x111111111111111111111111111111111111112"
    )]
    public void ParseAccountAddress(string str, string expectedAddress)
    {
        var result = AccountAddress.From(str, 63);
        Assert.Equal(AccountAddress.From(expectedAddress, 63), result);
    }

    [Theory]
    [InlineData("0x1", "0x1")]
    [InlineData("0x2", "0x2")]
    [InlineData("0x0000000000000000000000000000000000000002", "0x2")]
    [InlineData("0xA", "0xa")]
    [InlineData("0xB", "0xb")]
    [InlineData(
        "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
    )]
    [InlineData(
        "0x0111111111111111111111111111111111111112",
        "0x0000000000000000000000000111111111111111111111111111111111111112"
    )]
    public void AddressToString(string address, string expectedStr)
    {
        Assert.Equal(expectedStr, AccountAddress.From(address, 63).ToString());
    }

    [Theory]
    [InlineData("0x1", "0x0000000000000000000000000000000000000000000000000000000000000001")]
    [InlineData("0x2", "0x0000000000000000000000000000000000000000000000000000000000000002")]
    [InlineData("0xA", "0x000000000000000000000000000000000000000000000000000000000000000a")]
    [InlineData("0xB", "0x000000000000000000000000000000000000000000000000000000000000000b")]
    [InlineData(
        "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        "0x0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
    )]
    [InlineData(
        "0x0111111111111111111111111111111111111112",
        "0x0000000000000000000000000111111111111111111111111111111111111112"
    )]
    [InlineData(
        "0x111111111111111111111111111111111111112",
        "0x0000000000000000000000000111111111111111111111111111111111111112"
    )]
    public void AddressToStringLong(string address, string expectedStr)
    {
        Assert.Equal(expectedStr, AccountAddress.From(address, 63).ToStringLong());
    }

    [Theory]
    [InlineData("0x")]
    [InlineData("0xA0000000000000000000000000000000000000000000000000000000000000001")]
    [InlineData("0x00000000000000000000000000000000000000000000000000000000000000001")]
    [InlineData("0xG")]
    public void ParseInvalidAccountAddress(string address)
    {
        Assert.ThrowsAny<Exception>(() => AccountAddress.From(address, 63));
    }
}
