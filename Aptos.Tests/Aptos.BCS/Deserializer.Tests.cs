namespace Aptos.Tests.BCS;

using System.Numerics;
using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/bcs_deserialization.feature")]
public sealed class DeserializerFeatureTests : Feature
{
    private Deserializer? _inputValue;

    private dynamic? _output;

    private Exception? _exception;

    [Given(@"(.*) (.*)")]
    public void GivenValue(string _, string bytes)
    {
        _inputValue = new Deserializer(bytes == "0x" ? [] : Hex.FromHexString(bytes).ToByteArray());
    }

    [When(@"I deserialize as (.*) with length (.*)")]
    public void WhenIDeserializedAsTypeWithLength(string type, string length)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        try
        {
            _output = type switch
            {
                "fixed bytes" => _inputValue.FixedBytes((int)uint.Parse(length)),
                _ => throw new ArgumentException("Invalid type"),
            };
        }
        catch (Exception err)
        {
            _exception = err;
        }

    }

    [When(@"I deserialize as sequence of (.*)")]
    public void WhenIDeserializeAsSequenceOfType(string type)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        try
        {
            _output = type switch
            {
                "address" => _inputValue.Vector(AccountAddress.Deserialize),
                "string" => _inputValue.Vector(d => d.String()),
                "bool" => _inputValue.Vector(d => d.Bool()),
                "u8" => _inputValue.Vector(d => d.U8()),
                "u16" => _inputValue.Vector(d => d.U16()),
                "u32" => _inputValue.Vector(d => d.U32()),
                "u64" => _inputValue.Vector(d => d.U64()),
                "u128" => _inputValue.Vector(d => d.U128()),
                "u256" => _inputValue.Vector(d => d.U256()),
                "uleb128" => _inputValue.Vector(d => d.Uleb128AsU32()),
                _ => throw new ArgumentException("Invalid type"),
            };
        }
        catch (Exception err)
        {
            _exception = err;
        }

    }

    [When(@"I deserialize as (.*)")]
    public void WhenIDeserializeAsType(string type)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        try
        {
            _output = type switch
            {
                "address" => AccountAddress.Deserialize(_inputValue),
                "string" => _inputValue.String(),
                "bytes" => _inputValue.Bytes(),
                "bool" => _inputValue.Bool(),
                "u8" => _inputValue.U8(),
                "u16" => _inputValue.U16(),
                "u32" => _inputValue.U32(),
                "u64" => _inputValue.U64(),
                "u128" => _inputValue.U128(),
                "u256" => _inputValue.U256(),
                "uleb128" => _inputValue.Uleb128AsU32(),
                _ => throw new ArgumentException("Invalid type"),
            };
        }
        catch (Exception err)
        {
            _exception = err;
        }
    }


    [Then(@"the result should be (.*) (.*)")]
    public void ThenTheResultShouldBeTypeValue(string type, string value)
    {
        if (_output == null) throw new ArgumentException("No output");

        switch (type)
        {
            case "address":
                Assert.Equal(AccountAddress.From(value).BcsToBytes(), ((AccountAddress)_output).BcsToBytes());
                break;
            case "string":
                Assert.Equal(value.Trim('\"'), _output);
                break;
            case "bytes":
                Assert.Equal(value == "0x" ? [] : Hex.FromHexString(value).ToByteArray(), (byte[])_output);
                break;
            case "bool":
                Assert.Equal(bool.Parse(value), (bool)_output);
                break;
            case "u8":
                Assert.Equal(byte.Parse(value), (byte)_output);
                break;
            case "u16":
                Assert.Equal(ushort.Parse(value), (ushort)_output);
                break;
            case "u32":
                Assert.Equal(uint.Parse(value), (uint)_output);
                break;
            case "u64":
                Assert.Equal(ulong.Parse(value), (ulong)_output);
                break;
            case "u128":
                Assert.Equal(BigInteger.Parse(value), (BigInteger)_output);
                break;
            case "u256":
                Assert.Equal(BigInteger.Parse(value), (BigInteger)_output);
                break;
            case "uleb128":
                Assert.Equal(uint.Parse(value), (uint)_output);
                break;
        }
    }

    [Then(@"the deserialization should fail")]
    public void ThenTheDeserializationShouldFail()
    {
        Assert.NotNull(_exception);
    }

}