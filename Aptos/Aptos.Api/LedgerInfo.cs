namespace Aptos;

using Newtonsoft.Json;

[Serializable]
public class LedgerInfo(
    byte chainId,
    ulong epoch,
    ulong ledgerVersion,
    ulong oldestLedgerVersion,
    ulong ledgerTimestamp,
    string nodeRole,
    ulong oldestBlockHeight,
    ulong blockHeight,
    string gitHash
)
{
    [JsonProperty("chain_id")]
    public byte ChainId = chainId;

    [JsonProperty("epoch")]
    public ulong Epoch = epoch;

    [JsonProperty("ledger_version")]
    public ulong LedgerVersion = ledgerVersion;

    [JsonProperty("oldest_ledger_version")]
    public ulong OldestLedgerVersion = oldestLedgerVersion;

    [JsonProperty("ledger_timestamp")]
    public ulong LedgerTimestamp = ledgerTimestamp;

    [JsonProperty("node_role")]
    public string NodeRole = nodeRole;

    [JsonProperty("oldest_block_height")]
    public ulong OldestblockHeight = oldestBlockHeight;

    [JsonProperty("block_height")]
    public ulong BlockHeight = blockHeight;

    [JsonProperty("git_hash")]
    public string GitHash = gitHash;
}
