namespace Aptos.Tests.Transactions;

using System.Numerics;

public class TransactionArgumentTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void ConvertArgument_Bool_FromString()
    {
        var result = TransactionArgument.ConvertArgument("true", new TypeTagBool(), []);
        Assert.IsType<Bool>(result);
        Assert.True(((Bool)result!).Value);

        var falseResult = TransactionArgument.ConvertArgument("false", new TypeTagBool(), []);
        Assert.False(((Bool)falseResult!).Value);
    }

    [Fact]
    public void ConvertArgument_Bool_FromBool()
    {
        var result = TransactionArgument.ConvertArgument(true, new TypeTagBool(), []);
        Assert.True(((Bool)result!).Value);
    }

    [Fact]
    public void ConvertArgument_Numerics_FromString()
    {
        Assert.Equal(7, ((U8)TransactionArgument.ConvertArgument("7", new TypeTagU8(), [])!).Value);
        Assert.Equal(
            (ushort)7,
            ((U16)TransactionArgument.ConvertArgument("7", new TypeTagU16(), [])!).Value
        );
        Assert.Equal(
            7u,
            ((U32)TransactionArgument.ConvertArgument("7", new TypeTagU32(), [])!).Value
        );
        Assert.Equal(
            7UL,
            ((U64)TransactionArgument.ConvertArgument("7", new TypeTagU64(), [])!).Value
        );
        Assert.Equal(
            new BigInteger(7),
            ((U128)TransactionArgument.ConvertArgument("7", new TypeTagU128(), [])!).Value
        );
        Assert.Equal(
            new BigInteger(7),
            ((U256)TransactionArgument.ConvertArgument("7", new TypeTagU256(), [])!).Value
        );
    }

    [Fact]
    public void ConvertArgument_Address_FromString()
    {
        var result = TransactionArgument.ConvertArgument("0x1", new TypeTagAddress(), []);
        Assert.IsType<AccountAddress>(result);
        Assert.Equal(AccountAddress.FromString("0x1", 63), result);
    }

    [Fact]
    public void ConvertArgument_VectorOfU8_FromBytes()
    {
        var result = TransactionArgument.ConvertArgument(
            new byte[] { 1, 2 },
            new TypeTagVector(new TypeTagU8()),
            []
        );
        Assert.IsType<MoveVector<U8>>(result);
        Assert.Equal(2, ((MoveVector<U8>)result!).Values.Count);
    }

    [Fact]
    public void ConvertArgument_VectorOfU8_FromEnumerable()
    {
        var result = TransactionArgument.ConvertArgument(
            new List<object> { 1.ToString(), 2.ToString() },
            new TypeTagVector(new TypeTagU8()),
            []
        );
        Assert.IsType<MoveVector<TransactionArgument?>>(result);
    }

    [Fact]
    public void ConvertArgument_OptionTypeTag_None_ReturnsTypedNone()
    {
        var typeTag = new TypeTagStruct(new StructTag(StructTag.OPTION, [new TypeTagU64()]));
        var result = TransactionArgument.ConvertArgument(null, typeTag, []);
        Assert.IsType<MoveOption<U64>>(result);
        Assert.Null(((MoveOption<U64>)result!).Value);
    }

    [Fact]
    public void ConvertArgument_OptionTypeTag_Some()
    {
        var typeTag = new TypeTagStruct(new StructTag(StructTag.OPTION, [new TypeTagU64()]));
        var result = TransactionArgument.ConvertArgument("7", typeTag, []);
        Assert.IsType<MoveOption<TransactionArgument>>(result);
    }

    [Fact]
    public void ConvertArgument_StringTypeTag()
    {
        var typeTag = new TypeTagStruct(new StructTag(StructTag.STRING));
        var result = TransactionArgument.ConvertArgument("hello", typeTag, []);
        Assert.IsType<MoveString>(result);
        Assert.Equal("hello", ((MoveString)result!).Value);
    }

    [Fact]
    public void ConvertArgument_ObjectTypeTag()
    {
        var typeTag = new TypeTagStruct(new StructTag(StructTag.OBJECT));
        var result = TransactionArgument.ConvertArgument("0x1", typeTag, []);
        Assert.IsType<AccountAddress>(result);
    }

    [Fact]
    public void ConvertArgument_GenericTypeTag_LooksUpFromTable()
    {
        var generic = new TypeTagGeneric(0);
        var result = TransactionArgument.ConvertArgument("7", generic, [new TypeTagU8()]);
        Assert.IsType<U8>(result);
    }

    [Fact]
    public void ConvertArgument_GenericTypeTag_OutOfRange_Throws()
    {
        var generic = new TypeTagGeneric(5);
        Assert.Throws<ArgumentException>(
            () => TransactionArgument.ConvertArgument("7", generic, [new TypeTagU8()])
        );
    }

    [Fact]
    public void ConvertArgument_AlreadyAnArgument_PreservedIfTypeMatches()
    {
        var arg = new U64(123);
        var result = TransactionArgument.ConvertArgument(arg, new TypeTagU64(), []);
        Assert.Same(arg, result);

        var mismatch = TransactionArgument.ConvertArgument(arg, new TypeTagU8(), []);
        Assert.Null(mismatch);
    }

    [Fact]
    public void EntryFunctionBytes_ScriptFunction_SerializesAsBytesWithLength()
    {
        var bytes = EntryFunctionBytes.Deserialize(new Deserializer(new byte[] { 1, 2, 3 }), 3);
        Assert.Equal(new byte[] { 1, 2, 3 }, bytes.Value.Value);
        var s = new Serializer();
        bytes.SerializeForEntryFunction(s);
        // Format: uleb128 length, then fixed bytes
        var output = s.ToBytes();
        Assert.Equal(3, output[0]);
        Assert.Equal(new byte[] { 1, 2, 3 }, output.Skip(1).ToArray());
    }

    [Fact]
    public void FixedBytes_FromString_AndRoundTrip()
    {
        var fixedBytes = new FixedBytes("0xabcd");
        Assert.Equal(new byte[] { 0xab, 0xcd }, fixedBytes.Value);
        var bytes = fixedBytes.BcsToBytes();
        Assert.Equal(new byte[] { 0xab, 0xcd }, bytes);

        var deserialized = FixedBytes.Deserialize(new Deserializer(bytes), 2);
        Assert.Equal(fixedBytes.Value, deserialized.Value);
    }

    [Fact]
    public void FixedBytes_AsTransactionArgument()
    {
        var fixedBytes = new FixedBytes(new byte[] { 1, 2 });
        var s = new Serializer();
        fixedBytes.SerializeForScriptFunction(s);
        Assert.Equal(new byte[] { 1, 2 }, s.ToBytes());
    }
}
