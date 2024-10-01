namespace Aptos.Tests.Crypto;

using Aptos.Indexer.GraphQL;
using Newtonsoft.Json;
using Xunit.Gherkin.Quick;

[FeatureFile("../../../../features/multi_key.feature")]
public sealed class MultiKeyFeatureTests : Feature
{
    private dynamic? _inputValue;

    private dynamic? _output;

    [Given(@"(public_keys|multikey|multikey_account) (.*)")]
    public void GivenValue(string type, string values)
    {
        if (type == "public_keys")
        {
            var splitValues = values.Split("|");
            var types = splitValues[0].Split(",");
            var keys = splitValues[1].Split(",");
            var signaturesRequired = byte.Parse(splitValues[2]);
            List<PublicKey> deserializedKeys = types
                .Select<string, PublicKey>(
                    (type, i) =>
                        type switch
                        {
                            "ed25519" => Ed25519PublicKey.Deserialize(new(keys[i])),
                            "secp256k1" => Secp256k1PublicKey.Deserialize(new(keys[i])),
                            "keyless" => KeylessPublicKey.Deserialize(new(keys[i])),
                            _ => throw new ArgumentException("Invalid public key type"),
                        }
                )
                .ToList();
            _inputValue = new MultiKey(deserializedKeys, signaturesRequired);
        }
        else if (type == "multikey")
        {
            _inputValue = MultiKey.Deserialize(new(values));
        }
        else if (type == "multikey_account")
        {
            var splitValues = values.Split("|");
            var key = splitValues[0];
            var signerTypes = splitValues[1].Split(",");
            var signers = splitValues[2].Split(",");
            List<Account> deserializedSigners = signerTypes
                .Select<string, Account>(
                    (type, i) =>
                        type switch
                        {
                            "ed25519_ed25519_pk" => new Ed25519Account(
                                Ed25519PrivateKey.Deserialize(new(signers[i]))
                            ),
                            "single_secp256k1_pk" => new SingleKeyAccount(
                                Secp256k1PrivateKey.Deserialize(new(signers[i]))
                            ),
                            "account_keyless" => KeylessAccount.Deserialize(new(signers[i])),
                            _ => throw new ArgumentException("Invalid signer type"),
                        }
                )
                .ToList();
            _inputValue = new MultiKeyAccount(MultiKey.Deserialize(new(key)), deserializedSigners);
        }
    }

    [When(@"I serialize")]
    public void WhenISerialize()
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");
        if (_inputValue is MultiKey multiKey)
            _output = multiKey.BcsToHex();
    }

    [When(@"I derive authentication key")]
    public void WhenIDeriveAuthenticationKey()
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");
        if (_inputValue is MultiKey multiKey)
            _output = multiKey.AuthKey().BcsToHex();
    }

    [When(@"I verify signature (.*) with message (.*)")]
    public void WhenIVerifySignatureWithMessage(string signature, string message)
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");

        if (_inputValue is MultiKey singleKey)
        {
            _output = singleKey.VerifySignature(
                message,
                MultiKeySignature.Deserialize(new(signature))
            );
        }
    }

    [When(@"I sign message (.*)")]
    public void WhenISignMessage(string message)
    {
        if (_inputValue == null)
            throw new ArgumentException("No input value");
        if (_inputValue is MultiKeyAccount multiKeyAccount)
            _output = multiKeyAccount.Sign(message).BcsToHex();
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
