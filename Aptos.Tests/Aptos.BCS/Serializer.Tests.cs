namespace Aptos.Tests.BCS;

using System.Numerics;
using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/bcs_serialization.feature")]
public sealed class SerializerFeatureTests : Feature
{
    private string? _inputValue;
    private string[]? _inputSequence;

    private byte[]? _output;

    [Given(@"sequence of (.*) (.*)")]
    public void GivenSequenceOfTypeValue(string _, string value)
    {
        _inputSequence = BaseTests.ParseArray(value);
    }

    [Given(@"(.*) (.*)")]
    public void GivenValue(string _, string value)
    {
        _inputValue = value;
    }

    [When(@"I serialize as (.*) with length (.*)")]
    public void WhenISerializedAsTypeWithLength(string type, string _)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        Serializer serializer = new();
        switch (type)
        {
            case "fixed bytes":
                serializer.FixedBytes(Hex.FromHexString(_inputValue).ToByteArray());
                break;
            default:
                _output = serializer.ToBytes();
                break;
        }

        if (_output == null) _output = serializer.ToBytes();
    }

    [When(@"I serialize as sequence of (.*)")]
    public void WhenISerializeAsSequenceOfType(string type)
    {
        if (_inputSequence == null) throw new ArgumentException("No input sequence");

        Serializer serializer = new();
        switch (type)
        {
            case "address":
                serializer.Vector(_inputSequence.Select(AccountAddress.From).ToArray());
                break;
            case "string":
                serializer.Vector(_inputSequence.Select(s => s.Replace("\"", "")).ToArray(), serializer.String);
                break;
            case "bool":
                serializer.Vector(_inputSequence.Select(bool.Parse).ToArray(), serializer.Serialize);
                break;
            case "u8":
                serializer.Vector(_inputSequence.Select(byte.Parse).ToArray(), serializer.Serialize);
                break;
            case "u16":
                serializer.Vector(_inputSequence.Select(ushort.Parse).ToArray(), serializer.Serialize);
                break;
            case "u32":
                serializer.Vector(_inputSequence.Select(uint.Parse).ToArray(), serializer.Serialize);
                break;
            case "u64":
                serializer.Vector(_inputSequence.Select(ulong.Parse).ToArray(), serializer.Serialize);
                break;
            case "u128":
                serializer.Vector(_inputSequence.Select(BigInteger.Parse).ToArray(), serializer.U128);
                break;
            case "u256":
                serializer.Vector(_inputSequence.Select(BigInteger.Parse).ToArray(), serializer.U256);
                break;
            case "uleb128":
                serializer.Vector(_inputSequence.Select(uint.Parse).ToArray(), serializer.U32AsUleb128);
                break;
        }

        if (_output == null) _output = serializer.ToBytes();
    }

    [When(@"I serialize as (.*)")]
    public void WhenISerializeAsType(string type)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        Serializer serializer = new();
        switch (type)
        {
            case "address":
                _output = AccountAddress.From(_inputValue).BcsToBytes();
                break;
            case "string":
                serializer.String(_inputValue.Replace("\"", ""));
                break;
            case "bytes":
                serializer.Bytes(Hex.FromHexString(_inputValue).ToByteArray());
                break;
            case "bool":
                serializer.Serialize(bool.Parse(_inputValue));
                break;
            case "u8":
                serializer.Serialize(byte.Parse(_inputValue));
                break;
            case "u16":
                serializer.Serialize(ushort.Parse(_inputValue));
                break;
            case "u32":
                serializer.Serialize(uint.Parse(_inputValue));
                break;
            case "u64":
                serializer.Serialize(ulong.Parse(_inputValue));
                break;
            case "u128":
                serializer.U128(BigInteger.Parse(_inputValue));
                break;
            case "u256":
                serializer.U256(BigInteger.Parse(_inputValue));
                break;
            case "uleb128":
                serializer.U32AsUleb128(uint.Parse(_inputValue));
                break;
        }

        if (_output == null) _output = serializer.ToBytes();
    }


    [Then(@"the result should be bytes (.*)")]
    public void ThenTheResultShouldBeBytesValue(string value)
    {
        Assert.Equal(Hex.FromHexString(value).ToByteArray(), _output);
    }

}