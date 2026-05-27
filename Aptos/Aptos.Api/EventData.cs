namespace Aptos;

using System.Text.Json;
using Aptos.Indexer.GraphQL;

public class EventData(
    string accountAddress,
    long creationNumber,
    JsonElement? data,
    long eventIndex,
    decimal sequenceNumber,
    long transactionBlockHeight,
    decimal transactionVersion,
    string type,
    string indexedType
)
{
    public EventData(IEventData eventData)
        : this(
            eventData.Account_address,
            eventData.Creation_number,
            eventData.Data,
            eventData.Event_index,
            eventData.Sequence_number,
            eventData.Transaction_block_height,
            eventData.Transaction_version,
            eventData.Type,
            eventData.Indexed_type
        ) { }

    public string AccountAddress = accountAddress;
    public long CreationNumber = creationNumber;
    public JsonElement? Data = data;
    public long EventIndex = eventIndex;
    public decimal SequenceNumber = sequenceNumber;
    public long TransactionBlockHeight = transactionBlockHeight;
    public decimal TransactionVersion = transactionVersion;
    public string Type = type;
    public string IndexedType = indexedType;
}
