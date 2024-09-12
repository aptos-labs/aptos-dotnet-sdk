namespace Aptos;

using Newtonsoft.Json;

public class KeylessConfiguration(ushort maxCommitedEpkBytes, ulong maxExpHorizonSecs, ushort maxExtraFieldBytes, ushort maxIssValBytes, uint maxJwtHeaderB64Bytes, ushort maxSignaturesPerTxn, List<string> overrideAudVals, ResourceOption trainingWheelsPubkey)
{

    [JsonProperty("max_committed_epk_bytes")]
    public readonly ushort MaxCommitedEpkBytes = maxCommitedEpkBytes;

    [JsonProperty("max_exp_horizon_secs")]
    public readonly ulong MaxExpHorizonSecs = maxExpHorizonSecs;

    [JsonProperty("max_extra_field_bytes")]
    public readonly ushort MaxExtraFieldBytes = maxExtraFieldBytes;

    [JsonProperty("max_iss_val_bytes")]
    public readonly ushort MaxIssValBytes = maxIssValBytes;

    [JsonProperty("max_jwt_header_b64_bytes")]
    public readonly uint MaxJwtHeaderB64Bytes = maxJwtHeaderB64Bytes;

    [JsonProperty("max_signatures_per_txn")]
    public readonly ushort MaxSignaturesPerTxn = maxSignaturesPerTxn;

    [JsonProperty("override_aud_vals")]
    public readonly List<string> OverrideAudVals = overrideAudVals;

    [JsonProperty("training_wheels_pubkey")]
    public readonly ResourceOption TrainingWheelsPubkey = trainingWheelsPubkey;

}