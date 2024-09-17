namespace Aptos;

using Newtonsoft.Json;

[Serializable]
public class BlockEndInfo(
    bool blockGasLimitReached,
    bool blockOutputLimitReached,
    ulong blockEffectiveBlockGasUnits,
    ulong blockApproxOutputSize
)
{
    [JsonProperty("block_gas_limit_reached")]
    public bool BlockGasLimitReached = blockGasLimitReached;

    [JsonProperty("block_output_limit_reached")]
    public bool BlockOutputLimitReached = blockOutputLimitReached;

    [JsonProperty("block_effective_block_gas_units")]
    public ulong BlockEffectiveBlockGasUnits = blockEffectiveBlockGasUnits;

    [JsonProperty("block_approx_output_size")]
    public ulong BlockApproxOutputSize = blockApproxOutputSize;
}
