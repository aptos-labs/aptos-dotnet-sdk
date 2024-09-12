namespace Aptos;

using Newtonsoft.Json;

public class AnyValue(string type, string value)
{
    [JsonProperty("type")]
    public string Type = type;

    [JsonProperty("value")]
    public string Value = value;
}
