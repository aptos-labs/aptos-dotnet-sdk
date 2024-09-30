using Aptos.Exceptions;
using Newtonsoft.Json;

namespace Aptos.Tests.Transactions;

public class TypeTagParserTests(ITestOutputHelper output) : BaseTests(output)
{
    const string TAG_STRUCT_NAME = "0x1::tag::Tag";

    readonly Dictionary<string, TypeTag> primitiveTypesDict =
        new()
        {
            { "bool", new TypeTagBool() },
            { "address", new TypeTagAddress() },
            { "signer", new TypeTagSigner() },
            { "u8", new TypeTagU8() },
            { "u16", new TypeTagU16() },
            { "u32", new TypeTagU32() },
            { "u64", new TypeTagU64() },
            { "u128", new TypeTagU128() },
            { "u256", new TypeTagU256() },
        };

    readonly Dictionary<string, TypeTag> structTypesDict =
        new()
        {
            { "0x1::string::String", new TypeTagStruct(new StructTag(StructTag.STRING)) },
            {
                "0x1::aptos_coin::AptosCoin",
                new TypeTagStruct(
                    new StructTag(AccountAddress.FromString("0x1"), "aptos_coin", "AptosCoin")
                )
            },
            {
                "0x1::option::Option<u8>",
                new TypeTagStruct(new StructTag(StructTag.OPTION, [new TypeTagU8()]))
            },
            {
                "0x1::object::Object<u8>",
                new TypeTagStruct(new StructTag(StructTag.OBJECT, [new TypeTagU8()]))
            },
            { $"{TAG_STRUCT_NAME}", new TypeTagStruct(new StructTag(StructTag.TAG)) },
            {
                $"{TAG_STRUCT_NAME}<u8>",
                new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagU8()]))
            },
            {
                $"{TAG_STRUCT_NAME}<u8, u8>",
                new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagU8(), new TypeTagU8()]))
            },
            {
                $"{TAG_STRUCT_NAME}<u64, u8>",
                new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagU64(), new TypeTagU8()]))
            },
            {
                $"{TAG_STRUCT_NAME}<{TAG_STRUCT_NAME}<u8>, u8>",
                new TypeTagStruct(
                    new StructTag(
                        StructTag.TAG,
                        [
                            new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagU8()])),
                            new TypeTagU8(),
                        ]
                    )
                )
            },
            {
                $"{TAG_STRUCT_NAME}<u8, {TAG_STRUCT_NAME}<u8>>",
                new TypeTagStruct(
                    new StructTag(
                        StructTag.TAG,
                        [
                            new TypeTagU8(),
                            new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagU8()])),
                        ]
                    )
                )
            },
        };

    readonly Dictionary<string, TypeTag> genericTypesDict =
        new()
        {
            { "T0", new TypeTagGeneric(0) },
            { "T1", new TypeTagGeneric(1) },
            { "T1337", new TypeTagGeneric(1337) },
            {
                $"{TAG_STRUCT_NAME}<T0>",
                new TypeTagStruct(new StructTag(StructTag.TAG, [new TypeTagGeneric(0)]))
            },
            {
                $"{TAG_STRUCT_NAME}<T0, T1>",
                new TypeTagStruct(
                    new StructTag(StructTag.TAG, [new TypeTagGeneric(0), new TypeTagGeneric(1)])
                )
            },
        };

    private Dictionary<string, TypeTag> mergedTypes =>
        primitiveTypesDict
            .Concat(structTypesDict)
            .ToDictionary(pair => pair.Key, pair => pair.Value);

    private Dictionary<string, TypeTag> mergedTypesWithGeneric =>
        mergedTypes.Concat(genericTypesDict).ToDictionary(pair => pair.Key, pair => pair.Value);

    [Fact(Timeout = 10000)]
    public void InvalidStructType()
    {
        Assert.Throws<TypeTagParserException>(() => TypeTag.Parse("notAnAddress::tag::Tag<u8>"));
        Assert.Throws<TypeTagParserException>(() => TypeTag.Parse("0x1::not-a-module::Tag<u8>"));
        Assert.Throws<TypeTagParserException>(() => TypeTag.Parse("0x1::tag::Not-A-Name<u8>"));
    }

    [Fact(Timeout = 10000)]
    public void ValidStandardType()
    {
        foreach (var entry in mergedTypes)
        {
            Assert.Equal(TypeTag.Parse($"{entry.Key}").ToString(), entry.Value.ToString());
            Assert.Equal(TypeTag.Parse($" {entry.Key}").ToString(), entry.Value.ToString());
            Assert.Equal(TypeTag.Parse($"{entry.Key} ").ToString(), entry.Value.ToString());
            Assert.Equal(TypeTag.Parse($" {entry.Key} ").ToString(), entry.Value.ToString());
        }
    }

    [Fact(Timeout = 10000)]
    public void ValidStructType()
    {
        foreach (var entry in mergedTypes)
        {
            Assert.Equal(
                TypeTag.Parse($"vector<{entry.Key}>").ToString(),
                new TypeTagVector(entry.Value).ToString()
            );
            Assert.Equal(
                TypeTag.Parse($"vector< {entry.Key}>").ToString(),
                new TypeTagVector(entry.Value).ToString()
            );
            Assert.Equal(
                TypeTag.Parse($"vector<{entry.Key} >").ToString(),
                new TypeTagVector(entry.Value).ToString()
            );
            Assert.Equal(
                TypeTag.Parse($"vector< {entry.Key} >").ToString(),
                new TypeTagVector(entry.Value).ToString()
            );
        }
    }

    [Fact(Timeout = 10000)]
    public void ValidNestedVector()
    {
        foreach (var entry in mergedTypes)
        {
            Assert.Equal(
                TypeTag.Parse($"vector<vector<{entry.Key}>>").ToString(),
                new TypeTagVector(new TypeTagVector(entry.Value)).ToString()
            );
        }
    }

    [Fact(Timeout = 10000)]
    public void ValidCapitalizedPrimitiveType()
    {
        foreach (var entry in primitiveTypesDict)
        {
            Assert.Equal(TypeTag.Parse(entry.Key.ToUpper()).ToString(), entry.Value.ToString());
        }
    }

    [Fact(Timeout = 10000)]
    public void ValidReferenceType()
    {
        foreach (var entry in mergedTypes)
        {
            Assert.Equal(
                TypeTag.Parse($"&{entry.Key}").ToString(),
                new TypeTagReference(entry.Value).ToString()
            );
        }
    }

    [Fact(Timeout = 10000)]
    public void ValidGenericType()
    {
        foreach (var entry in mergedTypesWithGeneric)
        {
            Assert.Equal(TypeTag.Parse(entry.Key, true).ToString(), entry.Value.ToString());
            Assert.Equal(
                TypeTag.Parse($"&{entry.Key}", true).ToString(),
                new TypeTagReference(entry.Value).ToString()
            );
            Assert.Equal(
                TypeTag.Parse($"vector<{entry.Key}>", true).ToString(),
                new TypeTagVector(entry.Value).ToString()
            );
            Assert.Equal(
                TypeTag.Parse($"0x1::tag::Tag<{entry.Key}, {entry.Key}>", true).ToString(),
                new TypeTagStruct(
                    new StructTag(
                        AccountAddress.FromString("0x1"),
                        "tag",
                        "Tag",
                        [entry.Value, entry.Value]
                    )
                ).ToString()
            );
        }
    }
}
