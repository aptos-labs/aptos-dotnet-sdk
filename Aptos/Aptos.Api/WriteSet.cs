namespace Aptos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(WriteSetConverter))]
public abstract class WriteSet(string type)
{
    [JsonProperty("type")]
    public string Type = type;
}

public class WriteSetConverter : JsonConverter<WriteSet>
{
    static readonly JsonSerializerSettings SpecifiedSubclassConversion =
        new() { ContractResolver = new SubclassSpecifiedConcreteClassConverter<WriteSet>() };

    public override WriteSet? ReadJson(
        JsonReader reader,
        Type objectType,
        WriteSet? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        return type switch
        {
            "script_write_set" => JsonConvert.DeserializeObject<ScriptWriteSet>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            "direct_write_set" => JsonConvert.DeserializeObject<DirectWriteSet>(
                jsonObject.ToString(),
                SpecifiedSubclassConversion
            ),
            _ => throw new Exception($"Unknown write set type: {type}"),
        };
    }

    public override void WriteJson(JsonWriter writer, WriteSet? value, JsonSerializer serializer) =>
        serializer.Serialize(writer, value);
}

public class ScriptWriteSet(string type, string executeAs, ScriptPayloadResponse script)
    : WriteSet(type)
{
    [JsonProperty("execute_as")]
    public string ExecuteAs = executeAs;

    [JsonProperty("script")]
    public ScriptPayloadResponse Script = script;
}

public class DirectWriteSet(string type, List<WriteSetChange> changes, List<Event> events)
    : WriteSet(type)
{
    [JsonProperty("changes")]
    public List<WriteSetChange> Changes = changes;

    [JsonProperty("events")]
    public List<Event> Events = events;
}
