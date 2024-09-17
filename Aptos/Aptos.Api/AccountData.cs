namespace Aptos;

using Newtonsoft.Json;

public class AccountData(ulong sequenceNumber, Hex authenticationKey)
{
    [JsonProperty("sequence_number")]
    public ulong SequenceNumber = sequenceNumber;

    [JsonProperty("authentication_key")]
    public Hex AuthenticationKey = authenticationKey;
}
