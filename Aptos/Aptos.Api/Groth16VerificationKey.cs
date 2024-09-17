namespace Aptos;

using Newtonsoft.Json;

public class Groth16VerificationKey(
    G1Bytes alphaG1,
    G2Bytes betaG2,
    G2Bytes deltaG2,
    G1Bytes[] gammaAbcG1,
    G2Bytes gammaG2
)
{
    [JsonProperty("alpha_g1")]
    public readonly G1Bytes AlphaG1 = alphaG1;

    [JsonProperty("beta_g2")]
    public readonly G2Bytes BetaG2 = betaG2;

    [JsonProperty("delta_g2")]
    public readonly G2Bytes DeltaG2 = deltaG2;

    [JsonProperty("gamma_abc_g1")]
    public readonly G1Bytes[] GammaAbcG1 = gammaAbcG1;

    [JsonProperty("gamma_g2")]
    public readonly G2Bytes GammeG2 = gammaG2;
}
