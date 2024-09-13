namespace Aptos;

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransactionResponseType
{
    [EnumMember(Value = "pending_transaction")]
    Pending,

    [EnumMember(Value = "user_transaction")]
    User,

    [EnumMember(Value = "genesis_transaction")]
    Genesis,

    [EnumMember(Value = "block_metadata_transaction")]
    BlockMetadata,

    [EnumMember(Value = "state_checkpoint_transaction")]
    StateCheckpoint,

    [EnumMember(Value = "validator_transaction")]
    Validator,

    [EnumMember(Value = "block_epilogue_transaction")]
    BlockEpilogue
}

[JsonConverter(typeof(TransactionResponseConverter))]
public abstract class TransactionResponse(TransactionResponseType type, Hex hash)
{
    [JsonProperty("type")]
    public TransactionResponseType Type = type;

    [JsonProperty("hash")]
    public Hex Hash = hash;
}

public class TransactionResponseConverter : JsonConverter<TransactionResponse>
{

    static readonly JsonSerializerSettings SpecifiedSubclassConversion = new() { ContractResolver = new SubclassSpecifiedConcreteClassConverter<TransactionResponse>() };

    public override TransactionResponse? ReadJson(JsonReader reader, Type objectType, TransactionResponse? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.ToString();

        return type switch
        {
            "user_transaction" => JsonConvert.DeserializeObject<UserTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "genesis_transaction" => JsonConvert.DeserializeObject<GenesisTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "block_metadata_transaction" => JsonConvert.DeserializeObject<BlockMetadataTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "state_checkpoint_transaction" => JsonConvert.DeserializeObject<StateCheckpointTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "validator_transaction" => JsonConvert.DeserializeObject<ValidatorTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            "block_epilogue_transaction" => JsonConvert.DeserializeObject<BlockEpilogueTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
            // Covers "pending_transaction" and submitted transactions
            _ => JsonConvert.DeserializeObject<PendingTransactionResponse>(jsonObject.ToString(), SpecifiedSubclassConversion),
        };
    }

    public override void WriteJson(JsonWriter writer, TransactionResponse? value, JsonSerializer serializer) => serializer.Serialize(writer, value);
}

[Serializable]
public class PendingTransactionResponse(Hex hash, AccountAddress sender, ulong sequenceNumber, ulong maxGasAmount, ulong gasUnitPrice, ulong expirationTimestampSecs, TransactionPayloadResponse payload, TransactionSignature? signature = null) : TransactionResponse(TransactionResponseType.Pending, hash)
{

    [JsonProperty("sender")]
    public AccountAddress Sender = sender;

    [JsonProperty("sequence_number")]
    public ulong SequenceNumber = sequenceNumber;

    [JsonProperty("max_gas_amount")]
    public ulong MaxGasAmount = maxGasAmount;

    [JsonProperty("gas_unit_price")]
    public ulong GasUnitPrice = gasUnitPrice;

    [JsonProperty("expiration_timestamp_secs")]
    public ulong ExpirationTimestampSecs = expirationTimestampSecs;

    [JsonProperty("payload")]
    public TransactionPayloadResponse Payload = payload;

    [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
    public TransactionSignature? Signature = signature;
}

[Serializable]
public abstract class CommittedTransactionResponse(TransactionResponseType type, Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes) : TransactionResponse(type, hash)
{
    [JsonProperty("version")]
    public ulong Version = version;

    [JsonProperty("state_change_hash")]
    public Hex StateChangeHash = stateChangeHash;

    [JsonProperty("event_root_hash")]
    public Hex EventRootHash = eventRootHash;

    [JsonProperty("gas_used")]
    public ulong GasUsed = gasUsed;

    [JsonProperty("success")]
    public bool Success = success;

    [JsonProperty("vm_status")]
    public string VmStatus = vmStatus;

    [JsonProperty("accumulator_root_hash")]
    public Hex accumulatorRootHash = accumulatorRootHash;

    [JsonProperty("changes")]
    public List<WriteSetChange> Changes = changes;

}

[Serializable]
public class UserTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, string? stateCheckpointHash, AccountAddress sender, ulong sequenceNumber, ulong maxGasAmount, ulong gasUnitPrice, ulong expirationTimestampSecs, List<WriteSetChange> changes, TransactionPayloadResponse payload, TransactionSignature? signature, List<Event> events, ulong timestamp) : CommittedTransactionResponse(TransactionResponseType.User, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash", NullValueHandling = NullValueHandling.Ignore)]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("sender")]
    public AccountAddress Sender = sender;

    [JsonProperty("sequence_number")]
    public ulong SequenceNumber = sequenceNumber;

    [JsonProperty("max_gas_amount")]
    public ulong MaxGasAmount = maxGasAmount;

    [JsonProperty("gas_unit_price")]
    public ulong GasUnitPrice = gasUnitPrice;

    [JsonProperty("expiration_timestamp_secs")]
    public ulong ExpirationTimestampSecs = expirationTimestampSecs;

    [JsonProperty("payload")]
    public TransactionPayloadResponse Payload = payload;

    [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
    public TransactionSignature? Signature = signature;

    [JsonProperty("events")]
    public List<Event> Events = events;

    [JsonProperty("timestamp")]
    public ulong Timestamp = timestamp;

}

[Serializable]
public class GenesisTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes, string stateCheckpointHash, GenesisPayloadResponse payload, List<Event> events) : CommittedTransactionResponse(TransactionResponseType.Genesis, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash")]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("payload")]
    public GenesisPayloadResponse Payload = payload;

    [JsonProperty("events")]
    public List<Event> Events = events;
}

[Serializable]
public class BlockMetadataTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes, string stateCheckpointHash, string id, ulong epoch, ulong round, List<byte> previousBlockVotesBitvec, string proposer, List<uint> failedProposerIndices, List<Event> events, ulong timestamp) : CommittedTransactionResponse(TransactionResponseType.BlockMetadata, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash")]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("id")]
    public string Id = id;

    [JsonProperty("epoch")]
    public ulong Epoch = epoch;

    [JsonProperty("round")]
    public ulong Round = round;

    [JsonProperty("events")]
    public List<Event> Events = events;

    [JsonProperty("previous_block_votes_bitvec")]
    public List<byte> PreviousBlockVotesBitvec = previousBlockVotesBitvec;

    [JsonProperty("proposer")]
    public string Proposer = proposer;

    [JsonProperty("failed_proposer_indices")]
    public List<uint> FailedProposerIndices = failedProposerIndices;

    [JsonProperty("timestamp")]
    public ulong Timestamp = timestamp;
}

public class ValidatorTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes, string stateCheckpointHash, List<Event> events, ulong timestamp) : CommittedTransactionResponse(TransactionResponseType.Validator, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash")]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("events")]
    public List<Event> Events = events;

    [JsonProperty("timestamp")]
    public ulong Timestamp = timestamp;
}

[Serializable]
public class StateCheckpointTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes, string? stateCheckpointHash, ulong timestamp) : CommittedTransactionResponse(TransactionResponseType.StateCheckpoint, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash")]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("timestamp")]
    public ulong Timestamp = timestamp;
}

[Serializable]
public class BlockEpilogueTransactionResponse(Hex hash, ulong version, Hex stateChangeHash, Hex eventRootHash, ulong gasUsed, bool success, string vmStatus, Hex accumulatorRootHash, List<WriteSetChange> changes, string stateCheckpointHash, ulong timestamp, BlockEndInfo? blockEndInfo) : CommittedTransactionResponse(TransactionResponseType.BlockEpilogue, hash, version, stateChangeHash, eventRootHash, gasUsed, success, vmStatus, accumulatorRootHash, changes)
{
    [JsonProperty("state_checkpoint_hash")]
    public string? StateCheckpointHash = stateCheckpointHash;

    [JsonProperty("timestamp")]
    public ulong Timestamp = timestamp;

    [JsonProperty("block_end_info")]
    public BlockEndInfo? BlockEndInfo = blockEndInfo;
}