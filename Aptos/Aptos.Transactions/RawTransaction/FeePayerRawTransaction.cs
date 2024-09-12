namespace Aptos;

public class FeePayerRawTransaction(RawTransaction rawTransaction, List<AccountAddress> secondarySignerAddresses, AccountAddress feePayerAddress) : RawTransactionWithData(rawTransaction, feePayerAddress, secondarySignerAddresses)
{
    public new readonly AccountAddress FeePayerAddress = feePayerAddress;

    public new readonly List<AccountAddress> SecondarySignerAddresses = secondarySignerAddresses;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)RawTransactionVariant.FeePayerTransaction);
        RawTransaction.Serialize(s);
        s.Vector(SecondarySignerAddresses);
        FeePayerAddress.Serialize(s);
    }

    public static new FeePayerRawTransaction Deserialize(Deserializer d) => new(RawTransaction.Deserialize(d), [.. d.Vector(AccountAddress.Deserialize)], AccountAddress.Deserialize(d));

}