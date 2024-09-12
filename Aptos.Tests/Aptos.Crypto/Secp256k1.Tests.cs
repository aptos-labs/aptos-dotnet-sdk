namespace Aptos.Tests.Core;

using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/secp256k1.feature")]
public sealed class Secp256k1FeatureTests : Feature
{
    private dynamic? _inputValue;

    private dynamic? _output;

    [Given(@"(private key|mnemonic) (.*)")]
    public void GivenValue(string type, string value)
    {
        switch (type)
        {
            case "private key":
                _inputValue = Secp256k1PrivateKey.Deserialize(new Deserializer(value));
                break;
            case "mnemonic":
                _inputValue = value;
                break;
        }
    }

    [When(@"I derive public key")]
    public void WhenIDerivePublicKey()
    {
        if (_inputValue == null) throw new ArgumentException("No input value");
        if (_inputValue is Secp256k1PrivateKey privateKey) _output = privateKey.PublicKey().BcsToHex();
    }

    [When(@"I sign message (.*)")]
    public void WhenISignMessage(string message)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");
        if (_inputValue is Secp256k1PrivateKey privateKey) _output = privateKey.Sign(message).BcsToHex();
    }


    [When(@"I verify signature (.*) with message (.*)")]
    public void WhenIVerifySignatureWithMessage(string signature, string message)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");

        if (_inputValue is Secp256k1PrivateKey privateKey)
        {
            _output = privateKey.PublicKey().VerifySignature(message, Secp256k1Signature.Deserialize(new Deserializer(signature)));
        }
    }

    [When(@"I derive from derivation path (.*)")]
    public void WhenIDeriveFromDerivationPath(string path)
    {
        if (_inputValue == null) throw new ArgumentException("No input value");
        if (_inputValue is string mnemonic) _output = Secp256k1PrivateKey.FromDerivationPath(path, mnemonic).BcsToHex();
    }

    [Then(@"the result should be (.*) (.*)")]
    public void ThenTheResultShouldBeTypeValue(string type, string value)
    {
        if (_output == null) throw new ArgumentException("No output value");

        switch (type)
        {
            case "bool":
                Assert.Equal(bool.Parse(value), _output);
                break;
            case "hex":
                Assert.Equal(Hex.FromHexString(value), _output);
                break;
        }
    }

}