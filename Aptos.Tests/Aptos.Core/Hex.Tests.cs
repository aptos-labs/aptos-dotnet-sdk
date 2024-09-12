namespace Aptos.Tests.Core;

using Aptos.Exceptions;

public class AptosHexTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact(Timeout = 10000)]
    public void FromHexString()
    {
        Assert.Throws<HexException>(() => Hex.FromHexString("0x"));
        Assert.Throws<HexException>(() => Hex.FromHexString("0x0"));
        Assert.Equal(Hex.FromHexString("0x00"), new Hex([0]));
        Assert.Equal(Hex.FromHexString("0x01"), new Hex([1]));
        Assert.Equal(Hex.FromHexString("0x0001"), new Hex([0, 1]));
    }
}