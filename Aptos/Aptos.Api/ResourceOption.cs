namespace Aptos;

using Newtonsoft.Json;

public class ResourceOption(List<string> vec)
{
    [JsonProperty("vec")]
    public List<string> Vec = vec;
}