namespace Aptos;

using Newtonsoft.Json;

public class OriginatingAddress(ResourceHandle addressMap)
{
    [JsonProperty("address_map")]
    public ResourceHandle AddressMap = addressMap;
}
