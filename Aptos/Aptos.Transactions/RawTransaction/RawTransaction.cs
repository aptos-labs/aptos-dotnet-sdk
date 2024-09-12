namespace Aptos;

public class RawTransaction(AccountAddress sender, ulong sequenceNumber, TransactionPayload payload, ulong maxGasAmount, ulong gasUnitPrice, ulong expirationTimestampSecs, ChainId chainId) : Serializable
{

    public readonly AccountAddress Sender = sender;

    public readonly ulong SequenceNumber = sequenceNumber;

    public readonly TransactionPayload Payload = payload;

    public readonly ulong MaxGasAmount = maxGasAmount;

    public readonly ulong GasUnitPrice = gasUnitPrice;

    public readonly ulong ExpirationTimestampSecs = expirationTimestampSecs;

    public readonly ChainId ChainId = chainId;

    public override void Serialize(Serializer s)
    {
        Sender.Serialize(s);
        s.U64(SequenceNumber);
        Payload.Serialize(s);
        s.U64(MaxGasAmount);
        s.U64(GasUnitPrice);
        s.U64(ExpirationTimestampSecs);
        ChainId.Serialize(s);
    }

    public static RawTransaction Deserialize(Deserializer d) => new(AccountAddress.Deserialize(d), d.U64(), TransactionPayload.Deserialize(d), d.U64(), d.U64(), d.U64(), ChainId.Deserialize(d));
}

public enum RawTransactionVariant : uint
{
    MultiAgentTransaction = 0,
    FeePayerTransaction = 1,
}

public abstract class RawTransactionWithData(RawTransaction rawTransaction, AccountAddress? feePayerAddress = null, List<AccountAddress>? secondarySignerAddresses = null) : Serializable
{
    public readonly RawTransaction RawTransaction = rawTransaction;

    public readonly AccountAddress? FeePayerAddress = feePayerAddress;

    public readonly List<AccountAddress>? SecondarySignerAddresses = secondarySignerAddresses;

    public static RawTransactionWithData Deserialize(Deserializer d)
    {
        RawTransactionVariant index = (RawTransactionVariant)d.Uleb128AsU32();
        return index switch
        {
            RawTransactionVariant.FeePayerTransaction => FeePayerRawTransaction.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}