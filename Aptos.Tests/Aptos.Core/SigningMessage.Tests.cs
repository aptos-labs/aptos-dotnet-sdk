namespace Aptos.Tests.Core;

public class SigningMessageTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void Convert_HexString_DecodesBytes()
    {
        var bytes = SigningMessage.Convert("0x010203");
        Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
    }

    [Fact]
    public void Convert_PlainString_AsciiEncoded()
    {
        var bytes = SigningMessage.Convert("hello");
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("hello"), bytes);
    }

    [Fact]
    public void Convert_Bytes_PassesThrough()
    {
        var input = new byte[] { 9, 8, 7 };
        Assert.Equal(input, SigningMessage.Convert(input));
    }

    [Fact]
    public void Generate_InvalidDomainSeparator_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => SigningMessage.Generate(new byte[] { 1 }, "NOT_APTOS::foo")
        );
    }

    [Fact]
    public void Generate_ProducesPrefixedMessage()
    {
        var msg = new byte[] { 1, 2, 3 };
        var result = SigningMessage.Generate(msg, SigningMessage.RAW_TRANSACTION_SALT);
        // Result must end with the original message and start with the 32-byte
        // SHA3-256 prefix derived from the salt.
        Assert.Equal(32 + msg.Length, result.Length);
        Assert.Equal(msg, result.Skip(32).ToArray());
    }

    [Fact]
    public void Generate_DifferentSaltsProduceDifferentPrefixes()
    {
        var msg = new byte[] { 1 };
        var a = SigningMessage.Generate(msg, SigningMessage.RAW_TRANSACTION_SALT);
        var b = SigningMessage.Generate(msg, SigningMessage.RAW_TRANSACTION_WITH_DATA_SALT);
        Assert.NotEqual(a.Take(32).ToArray(), b.Take(32).ToArray());
    }

    [Fact]
    public void GenerateForTransaction_RawTransaction_UsesRawSalt()
    {
        var raw = new RawTransaction(
            AccountAddress.ZERO,
            0,
            new TransactionEntryFunctionPayload(
                new EntryFunction(new ModuleId(AccountAddress.ZERO, "m"), "f", [], [])
            ),
            100,
            100,
            100,
            new ChainId(4)
        );
        var simple = new SimpleTransaction(raw);
        var msg = SigningMessage.GenerateForTransaction(simple);
        var expected = SigningMessage.Generate(
            raw.BcsToBytes(),
            SigningMessage.RAW_TRANSACTION_SALT
        );
        Assert.Equal(expected, msg);
    }

    [Fact]
    public void GenerateForTransaction_FeePayer_UsesWithDataSalt()
    {
        var raw = new RawTransaction(
            AccountAddress.ZERO,
            0,
            new TransactionEntryFunctionPayload(
                new EntryFunction(new ModuleId(AccountAddress.ZERO, "m"), "f", [], [])
            ),
            100,
            100,
            100,
            new ChainId(4)
        );
        var fp = AccountAddress.FromString("0x1", 63);
        var simple = new SimpleTransaction(raw, fp);
        var msg = SigningMessage.GenerateForTransaction(simple);

        var fpRaw = new FeePayerRawTransaction(raw, [], fp);
        var expected = SigningMessage.Generate(
            fpRaw.BcsToBytes(),
            SigningMessage.RAW_TRANSACTION_WITH_DATA_SALT
        );
        Assert.Equal(expected, msg);
    }
}
