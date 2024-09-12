namespace Aptos;

public class MultiAgentTransaction(RawTransaction rawTransaction, List<AccountAddress> secondarySignerAddresses, AccountAddress? feePayerAddress = null) : AnyRawTransaction(rawTransaction, feePayerAddress, secondarySignerAddresses)
{
    public new List<AccountAddress> SecondarySignerAddresses { get; } = secondarySignerAddresses;

    public override void Serialize(Serializer s)
    {
        RawTransaction.Serialize(s);

        s.Vector(SecondarySignerAddresses ?? []);

        if (FeePayerAddress == null)
        {
            s.Serialize(false);
        }
        else
        {
            s.Serialize(true);
            FeePayerAddress.Serialize(s);
        }
    }

    public static MultiAgentTransaction Deserialize(Deserializer d)
    {
        RawTransaction rawTransaction = RawTransaction.Deserialize(d);
        List<AccountAddress> secondarySignerAddresses = d.Vector(AccountAddress.Deserialize);
        bool hasFeePayer = d.Bool();
        return new MultiAgentTransaction(rawTransaction, secondarySignerAddresses, hasFeePayer ? AccountAddress.Deserialize(d) : null);
    }
}