namespace Aptos;

public class SimulateTransactionOptions(
    bool estimateGasUnitPrice = false,
    bool estimateMaxGasAmount = false,
    bool estimatePrioritizedGasUnitPrice = false
)
{
    public bool EstimateGasUnitPrice = estimateGasUnitPrice;
    public bool EstimateMaxGasAmount = estimateMaxGasAmount;
    public bool EstimatePrioritizedGasUnitPrice = estimatePrioritizedGasUnitPrice;
}

public class SimulateTransactionData(
    AnyRawTransaction transaction,
    PublicKey signerPublicKey,
    PublicKey[]? secondarySignersPublicKeys = null,
    PublicKey? feePayerPublicKey = null,
    SimulateTransactionOptions? options = null
)
{
    public AnyRawTransaction Transaction = transaction;
    public PublicKey SignerPublicKey = signerPublicKey;
    public PublicKey[]? SecondarySignersPublicKeys = secondarySignersPublicKeys;
    public PublicKey? FeePayerPublicKey = feePayerPublicKey;
    public SimulateTransactionOptions? Options = options;
}
