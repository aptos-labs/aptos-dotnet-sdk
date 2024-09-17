namespace Aptos;

using Newtonsoft.Json;

public class GasEstimation(
    ulong gasEstimate,
    ulong? deprioritizedGasEstimate,
    ulong? prioritizedGasEstimate
)
{
    [JsonProperty("gas_estimate")]
    public ulong GasEstimate = gasEstimate;

    [JsonProperty("deprioritized_gas_estimate")]
    public ulong? DeprioritizedGasEstimate = deprioritizedGasEstimate;

    [JsonProperty("prioritized_gas_estimate")]
    public ulong? PrioritizedGasEstimate = prioritizedGasEstimate;
}
