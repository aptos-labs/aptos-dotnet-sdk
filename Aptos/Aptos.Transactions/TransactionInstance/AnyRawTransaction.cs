namespace Aptos;

public abstract class AnyRawTransaction(RawTransaction rawTransaction, AccountAddress? feePayerAddress = null, List<AccountAddress>? secondarySignerAddresses = null) : Serializable
{
    public RawTransaction RawTransaction { get; } = rawTransaction;

    public AccountAddress? FeePayerAddress { get; } = feePayerAddress;

    public List<AccountAddress>? SecondarySignerAddresses { get; } = secondarySignerAddresses;
}