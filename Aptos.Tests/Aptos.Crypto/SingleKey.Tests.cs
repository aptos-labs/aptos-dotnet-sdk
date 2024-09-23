namespace Aptos.Tests.Core;

using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/single_key.feature")]
public sealed class SingleKeyFeatureTests : Feature
{
    private string? _publicKeyType;

    private dynamic? _inputValue;

    private dynamic? _output;

    [Given(@"(ed25519|secp256k1|keyless) (.*)")]
    public void GivenValue(string type, string value)
    {
        PublicKey publicKey = type switch
        {
            "ed25519" => Ed25519PublicKey.Deserialize(new(value)),
            "secp256k1" => Secp256k1PublicKey.Deserialize(new(value)),
            "keyless" => KeylessPublicKey.Deserialize(new(value)),
            _ => throw new ArgumentException("Invalid public key type"),
        };
        _publicKeyType = type;
        _inputValue = new SingleKey(publicKey);
    }

    [When(@"I serialize")]
    public void WhenISerialize()
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");
        if (_inputValue is SingleKey singleKey)
            _output = singleKey.BcsToHex();
    }

    [When(@"I derive authentication key")]
    public void WhenIDeriveAuthenticationKey()
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");
        if (_inputValue is SingleKey singleKey)
            _output = singleKey.AuthKey().BcsToHex();
    }

    [When(@"I verify signature (.*) with message (.*)")]
    public void WhenIVerifySignatureWithMessage(string signature, string message)
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");

        if (_inputValue is SingleKey singleKey)
        {
            PublicKeySignature publicKeySignature = _publicKeyType switch
            {
                "ed25519" => Ed25519Signature.Deserialize(new(signature)),
                "secp256k1" => Secp256k1Signature.Deserialize(new(signature)),
                "keyless" => KeylessSignature.Deserialize(new(signature)),
                _ => throw new ArgumentException("Invalid signature public key type"),
            };
            _output = singleKey.VerifySignature(message, publicKeySignature);
        }
    }

    [Then(@"the result should be (.*) (.*)")]
    public void ThenTheResultShouldBeTypeValue(string type, string value)
    {
        if (_output == null)
            throw new ArgumentException("No output value");

        switch (type)
        {
            case "bool":
                Assert.Equal(bool.Parse(value), _output);
                break;
            case "bcs":
                Assert.Equal(Hex.FromHexString(value), _output);
                break;
        }
    }
}
