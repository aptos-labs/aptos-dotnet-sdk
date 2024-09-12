namespace Aptos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(TransactionPayloadResponseConverter))]
abstract public class TransactionPayloadResponse(string type)
{

    [JsonProperty("type")]
    public string Type = type;
}

public class TransactionPayloadResponseConverter : JsonConverter<TransactionPayloadResponse>
{
    static readonly JsonSerializerSettings SpecifiedSubclassConversion = new() { ContractResolver = new SubclassSpecifiedConcreteClassConverter<TransactionPayloadResponse>() };

    public override TransactionPayloadResponse? ReadJson(JsonReader reader, Type objectType, TransactionPayloadResponse? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        return type switch
        {
            "entry_function_payload" => JsonConvert.DeserializeObject<EntryFunctionPayloadResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "script_payload" => JsonConvert.DeserializeObject<ScriptPayloadResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "multisig_payload" => JsonConvert.DeserializeObject<MultisigPayloadResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "write_set_payload" => JsonConvert.DeserializeObject<GenesisPayloadResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            _ => throw new Exception($"Unknown transaction payload response type: {type}"),
        };
    }

    public override void WriteJson(JsonWriter writer, TransactionPayloadResponse? value, JsonSerializer serializer) => serializer.Serialize(writer, value);
}

public class EntryFunctionPayloadResponse(string type, string function, List<object> typeArguments, List<object> arguments) : TransactionPayloadResponse(type)
{
    [JsonProperty("function")]
    public string Function = function;

    [JsonProperty("type_arguments")]
    public List<object> TypeArguments = typeArguments;

    [JsonProperty("arguments")]
    public List<object> Arguments = arguments;
}

public class ScriptPayloadResponse(string type, MoveScriptBytecode code, List<object> typeArguments, List<object> arguments) : TransactionPayloadResponse(type)
{

    [JsonProperty("code")]
    public MoveScriptBytecode Code = code;

    [JsonProperty("type_arguments")]
    public List<object> TypeArguments = typeArguments;

    [JsonProperty("arguments")]
    public List<object> Arguments = arguments;
}

public class MultisigPayloadResponse(string type, string multisigAddress, EntryFunctionPayloadResponse transactionPayload) : TransactionPayloadResponse(type)
{

    [JsonProperty("multisig_address")]
    public string MultisigAddress = multisigAddress;

    [JsonProperty("transaction_payload")]
    public EntryFunctionPayloadResponse TransactionPayload = transactionPayload;
}

public class GenesisPayloadResponse(string type, WriteSet writeSet) : TransactionPayloadResponse(type)
{
    [JsonProperty("write_set")]
    public WriteSet WriteSet = writeSet;
}