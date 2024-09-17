namespace Aptos;

using Newtonsoft.Json;

public class PepperResponse(Hex pepper, Hex address)
{
    [JsonProperty("pepper")]
    public Hex Pepper = pepper;

    [JsonProperty("Address")]
    public Hex Address = address;
}
