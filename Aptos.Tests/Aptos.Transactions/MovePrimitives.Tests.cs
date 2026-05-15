namespace Aptos.Tests.Transactions;

using System.Numerics;

public class MovePrimitivesAndStructsTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void Bool_RoundTrip()
    {
        var b = new Bool(true);
        var bytes = b.BcsToBytes();
        Assert.Equal(new byte[] { 1 }, bytes);
        var d = Bool.Deserialize(new Deserializer(bytes));
        Assert.True(d.Value);
        Assert.False(Bool.Deserialize(new Deserializer(new byte[] { 0 })).Value);
    }

    [Fact]
    public void U8_U16_U32_U64_RoundTrip()
    {
        Assert.Equal(7, U8.Deserialize(new Deserializer(new U8(7).BcsToBytes())).Value);
        Assert.Equal(
            (ushort)1234,
            U16.Deserialize(new Deserializer(new U16(1234).BcsToBytes())).Value
        );
        Assert.Equal(
            123_456u,
            U32.Deserialize(new Deserializer(new U32(123_456).BcsToBytes())).Value
        );
        Assert.Equal(
            123_456_789UL,
            U64.Deserialize(new Deserializer(new U64(123_456_789).BcsToBytes())).Value
        );
    }

    [Fact]
    public void U128_U256_RoundTrip()
    {
        var max128 = BigInteger.Pow(2, 128) - 1;
        var u128 = new U128(max128);
        var d128 = U128.Deserialize(new Deserializer(u128.BcsToBytes()));
        Assert.Equal(max128, d128.Value);

        var max256 = BigInteger.Pow(2, 256) - 1;
        var u256 = new U256(max256);
        var d256 = U256.Deserialize(new Deserializer(u256.BcsToBytes()));
        Assert.Equal(max256, d256.Value);
    }

    [Fact]
    public void ScriptArgument_AllVariants_RoundTrip()
    {
        TransactionArgument[] args =
        [
            new Bool(true),
            new U8(1),
            new U16(2),
            new U32(3),
            new U64(4),
            new U128(new BigInteger(5)),
            new U256(new BigInteger(6)),
            AccountAddress.ZERO,
            new MoveVector<U8>([new U8(1), new U8(2)]),
        ];
        foreach (var arg in args)
        {
            var s = new Serializer();
            arg.SerializeForScriptFunction(s);
            var d = TransactionArgument.DeserializeFromScriptArgument(
                new Deserializer(s.ToBytes())
            );
            Assert.Equal(arg.GetType(), d.GetType());
        }
    }

    [Fact]
    public void ScriptArgument_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => TransactionArgument.DeserializeFromScriptArgument(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void MoveVector_U8_RoundTrip()
    {
        var v = new MoveVector<U8>([new U8(1), new U8(2), new U8(3)]);
        var d = MoveVector<U8>.Deserialize(new Deserializer(v.BcsToBytes()), U8.Deserialize);
        Assert.Equal(3, d.Values.Count);
        Assert.Equal(1, d.Values[0].Value);
        Assert.Equal(3, d.Values[2].Value);
    }

    [Fact]
    public void MoveVector_NonU8_AsScript_Throws()
    {
        var v = new MoveVector<U16>([new U16(1)]);
        Assert.Throws<ArgumentException>(() => v.SerializeForScriptFunction(new Serializer()));
    }

    [Fact]
    public void MoveString_RoundTrip()
    {
        var s = new MoveString("hello");
        var d = MoveString.Deserialize(new Deserializer(s.BcsToBytes()));
        Assert.Equal("hello", d.Value);
    }

    [Fact]
    public void MoveString_ScriptFunction_EncodingHasVariantByte()
    {
        var s = new MoveString("hi");
        var serializer = new Serializer();
        s.SerializeForScriptFunction(serializer);
        var bytes = serializer.ToBytes();
        // The MoveString script-function encoding is a u8 vector wrapping the
        // raw UTF-8 bytes (no length prefix), so:
        //   variant = ScriptTransactionArgumentVariants.U8Vector
        //   followed by uleb128 length and the byte content.
        Assert.Equal((byte)ScriptTransactionArgumentVariants.U8Vector, bytes[0]);
    }

    [Fact]
    public void MoveOption_Some_RoundTrip()
    {
        var opt = new MoveOption<U64>(new U64(123));
        var d = MoveOption<U64>.Deserialize(new Deserializer(opt.BcsToBytes()), U64.Deserialize);
        Assert.NotNull(d.Value);
        Assert.Equal(123UL, d.Value!.Value);
    }

    [Fact]
    public void MoveOption_None_RoundTrip()
    {
        var opt = new MoveOption<U64>(null);
        var d = MoveOption<U64>.Deserialize(new Deserializer(opt.BcsToBytes()), U64.Deserialize);
        Assert.Null(d.Value);
    }

    [Fact]
    public void MoveOption_DoesNotSupportScriptArgument()
    {
        Assert.Throws<NotImplementedException>(
            () => new MoveOption<U64>(null).SerializeForScriptFunction(new Serializer())
        );
    }
}
