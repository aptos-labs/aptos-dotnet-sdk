namespace Aptos.Tests.Transactions;

using System.Numerics;

public class TypeTagRoundTripTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void Primitive_TypeTags_SerializeAndDeserialize()
    {
        TypeTag[] primitives =
        [
            new TypeTagBool(),
            new TypeTagU8(),
            new TypeTagU16(),
            new TypeTagU32(),
            new TypeTagU64(),
            new TypeTagU128(),
            new TypeTagU256(),
            new TypeTagAddress(),
            new TypeTagSigner(),
        ];
        foreach (var t in primitives)
        {
            var bytes = t.BcsToBytes();
            var d = TypeTag.Deserialize(new Deserializer(bytes));
            Assert.Equal(t.Variant, d.Variant);
            Assert.Equal(t.ToString(), d.ToString());
        }
    }

    [Fact]
    public void TypeTagVector_RoundTrip()
    {
        var t = new TypeTagVector(new TypeTagU8());
        var bytes = t.BcsToBytes();
        var d = (TypeTagVector)TypeTag.Deserialize(new Deserializer(bytes));
        Assert.Equal("vector<u8>", d.ToString());
        Assert.IsType<TypeTagU8>(d.Value);
    }

    [Fact]
    public void TypeTagStruct_RoundTrip_WithGenerics()
    {
        var t = new TypeTagStruct(
            new StructTag(
                AccountAddress.FromString("0x1", 63),
                "option",
                "Option",
                [new TypeTagU64()]
            )
        );
        var bytes = t.BcsToBytes();
        var d = (TypeTagStruct)TypeTag.Deserialize(new Deserializer(bytes));
        Assert.True(d.IsOptionTypeTag());
        Assert.Single(d.Value.TypeArgs);
        Assert.Contains("Option<u64>", d.ToString());
    }

    [Fact]
    public void TypeTagStruct_IsStringObjectOption()
    {
        Assert.True(new TypeTagStruct(new StructTag(StructTag.STRING)).IsStringTypeTag());
        Assert.True(new TypeTagStruct(new StructTag(StructTag.OBJECT)).IsObjectTypeTag());
        Assert.True(new TypeTagStruct(new StructTag(StructTag.OPTION)).IsOptionTypeTag());
        Assert.False(new TypeTagStruct(new StructTag(StructTag.TAG)).IsStringTypeTag());
    }

    [Fact]
    public void TypeTagReference_RoundTrip()
    {
        var t = new TypeTagReference(new TypeTagAddress());
        var bytes = t.BcsToBytes();
        var d = (TypeTagReference)TypeTag.Deserialize(new Deserializer(bytes));
        Assert.Equal("&address", d.ToString());
    }

    [Fact]
    public void TypeTagGeneric_RoundTrip()
    {
        var t = new TypeTagGeneric(7);
        var bytes = t.BcsToBytes();
        var d = (TypeTagGeneric)TypeTag.Deserialize(new Deserializer(bytes));
        Assert.Equal(7u, d.Value);
        Assert.Equal("T7", d.ToString());
    }

    [Fact]
    public void TypeTag_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(100); // not a valid variant
        Assert.Throws<ArgumentException>(() => TypeTag.Deserialize(new Deserializer(s.ToBytes())));
    }

    [Fact]
    public void TypeTag_CheckType_MatchesValues()
    {
        Assert.True(new TypeTagBool().CheckType(new Bool(true)));
        Assert.True(new TypeTagU8().CheckType(new U8(1)));
        Assert.True(new TypeTagU16().CheckType(new U16(1)));
        Assert.True(new TypeTagU32().CheckType(new U32(1)));
        Assert.True(new TypeTagU64().CheckType(new U64(1)));
        Assert.True(new TypeTagU128().CheckType(new U128(new BigInteger(1))));
        Assert.True(new TypeTagU256().CheckType(new U256(new BigInteger(1))));
        Assert.True(new TypeTagAddress().CheckType(AccountAddress.ZERO));

        // Wrong types return false
        Assert.False(new TypeTagBool().CheckType(new U8(1)));
        Assert.False(new TypeTagU8().CheckType(new Bool(true)));

        // String / object struct
        Assert.True(
            new TypeTagStruct(new StructTag(StructTag.STRING)).CheckType(new MoveString("a"))
        );
        Assert.True(
            new TypeTagStruct(new StructTag(StructTag.OBJECT)).CheckType(AccountAddress.ZERO)
        );

        // Vector type tag is harder to satisfy due to generic constraints but
        // a non-MoveVector value must return false.
        Assert.False(new TypeTagVector(new TypeTagU8()).CheckType(new U8(1)));
    }
}
