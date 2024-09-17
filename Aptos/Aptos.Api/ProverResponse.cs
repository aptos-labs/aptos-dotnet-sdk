namespace Aptos;

using Newtonsoft.Json;

public class ProverResponse(
    Groth16Zkp proof,
    string publicInputsHash,
    string trainingWheelsSignature
)
{
    [JsonProperty("proof")]
    public Groth16Zkp Proof = proof;

    [JsonProperty("public_inputs_hash")]
    public string PublicInputsHash = publicInputsHash;

    [JsonProperty("training_wheels_signature")]
    public string TrainingWheelsSignature = trainingWheelsSignature;
}
