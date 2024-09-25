namespace Aptos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(WritesSetChangeConverter))]
public abstract class WriteSetChange(string type, string stateKeyHash)
{
    [JsonProperty("type")]
    public string Type = type;

    [JsonProperty("state_key_hash")]
    public string StateKeyHash = stateKeyHash;
}

public class WritesSetChangeConverter : JsonConverter<WriteSetChange>
{
    public override bool CanWrite => false;

    static readonly JsonSerializerSettings SpecifiedSubclassConversion =
        new() { ContractResolver = new SubclassSpecifiedConcreteClassConverter<WriteSetChange>() };

    public override WriteSetChange? ReadJson(
        JsonReader reader,
        Type objectType,
        WriteSetChange? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        return type switch
        {
            "write_module" => JsonConvert.DeserializeObject<WriteSetChangeWriteModule>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "write_table_item" => JsonConvert.DeserializeObject<WriteSetChangeWriteTableItem>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "write_resource" => JsonConvert.DeserializeObject<WriteSetChangeWriteResource>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "delete_module" => JsonConvert.DeserializeObject<WriteSetChangeDeleteModule>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "delete_resource" => JsonConvert.DeserializeObject<WriteSetChangeDeleteResource>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "delete_table_item" => JsonConvert.DeserializeObject<WriteSetChangeDeleteTableItem>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            _ => throw new Exception($"Unknown transaction response type: {type}"),
        };
    }

    public override void WriteJson(
        JsonWriter writer,
        WriteSetChange? value,
        JsonSerializer serializer
    ) => throw new NotImplementedException();
}

public class WriteSetChangeDeleteModule(
    string type,
    string stateKeyHash,
    string address,
    string module
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("address")]
    public string Address = address;

    [JsonProperty("module")]
    public string Module = module;
}

public class WriteSetChangeDeleteResource(
    string type,
    string stateKeyHash,
    string address,
    string resource
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("address")]
    public string Address = address;

    [JsonProperty("resource")]
    public string Resource = resource;
}

public class WriteSetChangeDeleteTableItem(
    string type,
    string stateKeyHash,
    string handle,
    string key,
    DeletedTableData data
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("handle")]
    public string Handle = handle;

    [JsonProperty("key")]
    public string Key = key;

    [JsonProperty("data")]
    public DeletedTableData Data = data;
}

public class WriteSetChangeWriteModule(
    string type,
    string stateKeyHash,
    string address,
    MoveModuleBytecode data
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("address")]
    public string Address = address;

    [JsonProperty("data")]
    public MoveModuleBytecode Data = data;
}

public class WriteSetChangeWriteResource(
    string type,
    string stateKeyHash,
    string address,
    MoveResource data
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("address")]
    public string Address = address;

    [JsonProperty("data")]
    public MoveResource Data = data;
}

public class WriteSetChangeWriteTableItem(
    string type,
    string stateKeyHash,
    string handle,
    string key,
    string value,
    DecodedTableData? data
) : WriteSetChange(type, stateKeyHash)
{
    [JsonProperty("handle")]
    public string Handle = handle;

    [JsonProperty("key")]
    public string Key = key;

    [JsonProperty("value")]
    public string Value = value;

    [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
    public DecodedTableData? Data = data;
}

public class DeletedTableData(dynamic key, string keyType)
{
    [JsonProperty("key")]
    public dynamic Key = key;

    [JsonProperty("key_type")]
    public string KeyType = keyType;
}

public class DecodedTableData(dynamic key, string keyType, dynamic dynamic, string valueType)
{
    [JsonProperty("key")]
    public dynamic Key = key;

    [JsonProperty("key_type")]
    public string KeyType = keyType;

    [JsonProperty("value")]
    public dynamic Value = dynamic;

    [JsonProperty("value_type")]
    public string ValueType = valueType;
}
