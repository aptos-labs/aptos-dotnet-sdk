namespace Aptos;

using Newtonsoft.Json;

public class Event(EventGuid guid, ulong sequenceNumber, string type, dynamic data)
{
    [JsonProperty("guid")]
    public EventGuid Guid = guid;

    [JsonProperty("sequence_number")]
    public ulong SequenceNumber = sequenceNumber;

    [JsonProperty("type")]
    public string Type = type;

    [JsonProperty("data")]
    public dynamic Data = data;
}

public class EventGuid(string creationNumber, string accountAddress)
{
    [JsonProperty("creation_number")]
    public string CreationNumber = creationNumber;

    [JsonProperty("account_address")]
    public string AccountAddress = accountAddress;
}
