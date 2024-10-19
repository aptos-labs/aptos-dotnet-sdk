using OneOf;

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
    OneOf<PublicKey, IVerifyingKey> signerPublicKey,
    OneOf<PublicKey, IVerifyingKey>[]? secondarySignersPublicKeys = null,
    OneOf<PublicKey, IVerifyingKey>? feePayerPublicKey = null,
    SimulateTransactionOptions? options = null
)
{
    public AnyRawTransaction Transaction = transaction;
    public OneOf<PublicKey, IVerifyingKey> SignerPublicKey = signerPublicKey;
    public OneOf<PublicKey, IVerifyingKey>[]? SecondarySignersPublicKeys =
        secondarySignersPublicKeys;
    public OneOf<PublicKey, IVerifyingKey>? FeePayerPublicKey = feePayerPublicKey;
    public SimulateTransactionOptions? Options = options;
}
