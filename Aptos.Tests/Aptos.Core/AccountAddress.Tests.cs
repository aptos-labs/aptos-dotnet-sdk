namespace Aptos.Tests.Core;

using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/account_address.feature")]
public sealed class AccountAddressFeatureTests : Feature
{
    private string? _inputValue;

    private dynamic? _output;

    private Exception? _exception;

    [Given(@"(.*) (.*)")]
    public void GivenValue(string _, string value)
    {
        _inputValue = value.Trim('\"');
    }

    [When(@"I parse the account address")]
    public void WhenIParseTheAccountAddress() => WhenIConvertTheTypeToAType("", "string");

    [When(@"I convert the (.*) to a (.*)")]
    public void WhenIConvertTheTypeToAType(string _, string outputType)
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");

        try
        {
            _output = outputType switch
            {
                "string" => AccountAddress.From(_inputValue, 63),
                "string long" => AccountAddress.From(_inputValue, 63).ToStringLong(),
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
        if (_output == null)
            throw new ArgumentException("No output value");

        switch (type)
        {
            case "address":
                Assert.Equal(AccountAddress.From(value, 63), _output);
                break;
            case "string":
                Assert.Equal(value.Trim('\"'), _output.ToString());
                break;
        }
    }

    [Then(@"I should fail to parse the account address")]
    public void ThenIShouldFailToParseTheAccountAddress()
    {
        Assert.NotNull(_exception);
    }
}
