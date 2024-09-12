namespace Aptos;

using Newtonsoft.Json;

public class ResourceHandle(string address)
{
    [JsonProperty("handle")]
    public string handle = address;
}