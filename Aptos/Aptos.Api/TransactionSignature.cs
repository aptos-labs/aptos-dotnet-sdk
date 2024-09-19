namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransactionSignatureVariant
{
    [EnumMember(Value = "ed25519_signature")]
    Ed25519 = 0,

    [EnumMember(Value = "multi_ed25519_signature")]
    MultiEd25519 = 1,

    [EnumMember(Value = "multi_agent_signature")]
    MultiAgent = 2,

    [EnumMember(Value = "fee_payer_signature")]
    FeePayer = 3,

    [EnumMember(Value = "single_sender")]
    SingleSender = 4,
}

[JsonConverter(typeof(TransactionSignatureConverter))]
public abstract class TransactionSignature(TransactionSignatureVariant type)
{
    [JsonProperty("type")]
    public TransactionSignatureVariant Type = type;
}

public class TransactionSignatureConverter : JsonConverter<TransactionSignature>
{
    static readonly JsonSerializerSettings SpecifiedSubclassConversion =
        new()
        {
            ContractResolver = new SubclassSpecifiedConcreteClassConverter<TransactionSignature>(),
        };

    public override TransactionSignature? ReadJson(
        JsonReader reader,
        Type objectType,
        TransactionSignature? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        switch (type)
        {
            case "ed25519_signature":
                return JsonConvert.DeserializeObject<TransactionEd25519Signature>(
                    jsonObject.ToString(),
                    SpecifiedSubclassConversion
                );
            case "fee_payer_signature":
                return JsonConvert.DeserializeObject<TransactionFeePayerSignature>(
                    jsonObject.ToString(),
                    SpecifiedSubclassConversion
                );
            case "multi_agent_signature":
                return JsonConvert.DeserializeObject<TransactionMultiAgentSignature>(
                    jsonObject.ToString(),
                    SpecifiedSubclassConversion
                );
            case "single_sender":
                AccountSignature? signature = null;

                // AccountSignature_Ed25519Signature || AccountSignature_SingleKeySignature
                if (jsonObject.ContainsKey("public_key"))
                {
                    var publicKey = jsonObject["public_key"]?.ToString();

                    // AccountSignature_Ed25519Signature
                    if (publicKey?.StartsWith("0x") ?? false)
                    {
                        jsonObject["type"] = "ed25519_signature";
                        signature = JsonConvert.DeserializeObject<AccountEd25519Signature>(
                            jsonObject.ToString()
                        );
                    }
                    // AccountSignature_SingleKeySignature
                    else if (publicKey?.Contains('{') ?? false)
                    {
                        jsonObject["type"] = "single_key_signature";
                        signature = JsonConvert.DeserializeObject<AccountSingleKeySignature>(
                            jsonObject.ToString()
                        );
                    }
                }
                else if (jsonObject.ContainsKey("public_keys"))
                {
                    var publicKeys = jsonObject["public_keys"]?.ToString();
                    var signatures = jsonObject["signatures"]?.ToString();
                    var signaturesRequired = jsonObject["signatures_required"]?.ToString();

                    // AccountSignature_MultiKeySignature
                    if (publicKeys != null && signatures != null && signaturesRequired != null)
                    {
                        jsonObject["type"] = "multi_key_signature";
                        signature = JsonConvert.DeserializeObject<AccountMultiKeySignature>(
                            jsonObject.ToString()
                        );
                    }
                }

                if (signature == null)
                    throw new Exception("Invalid account signature");

                return new TransactionSingleSenderSignature(signature);
            default:
                throw new Exception($"Unknown transaction signature type: {type}");
        }
    }

    public override void WriteJson(
        JsonWriter writer,
        TransactionSignature? value,
        JsonSerializer serializer
    ) => serializer.Serialize(writer, value);
}

[Serializable]
public class TransactionEd25519Signature(Hex publicKey, Hex signature)
    : TransactionSignature(TransactionSignatureVariant.Ed25519)
{
    [JsonProperty("signature")]
    public Hex Signature = signature;

    [JsonProperty("public_key")]
    public Hex PublicKey = publicKey;
}

[Serializable]
public class TransactionFeePayerSignature(
    AccountSignature sender,
    List<string> secondarySignerAddresses,
    List<AccountSignature> secondarySigners,
    string feePayerAddress,
    AccountSignature feePayerSigner
) : TransactionSignature(TransactionSignatureVariant.FeePayer)
{
    [JsonProperty("sender")]
    public AccountSignature Sender = sender;

    [JsonProperty("secondary_signer_addresses")]
    public List<string> SecondarySignerAddresses = secondarySignerAddresses;

    [JsonProperty("secondary_signers")]
    public List<AccountSignature> SecondarySigners = secondarySigners;

    [JsonProperty("fee_payer_address")]
    public string FeePayerAddress = feePayerAddress;

    [JsonProperty("fee_payer_signer")]
    public AccountSignature FeePayerSigner = feePayerSigner;
}

public class TransactionMultiAgentSignature(
    AccountSignature sender,
    List<string> secondarySignerAddresses,
    List<AccountSignature> secondarySigners
) : TransactionSignature(TransactionSignatureVariant.MultiAgent)
{
    [JsonProperty("sender")]
    public AccountSignature Sender = sender;

    [JsonProperty("secondary_signer_addresses")]
    public List<string> SecondarySignerAddresses = secondarySignerAddresses;

    [JsonProperty("secondary_signers")]
    public List<AccountSignature> SecondarySigners = secondarySigners;
}

public class TransactionSingleSenderSignature(AccountSignature accountSignature)
    : TransactionSignature(TransactionSignatureVariant.SingleSender)
{
    public AccountSignature AccountSignature = accountSignature;
}
