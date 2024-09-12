namespace Aptos;

public class MultiAgentRawTransaction(RawTransaction rawTransaction, List<AccountAddress> secondarySignerAddresses) : RawTransactionWithData(rawTransaction, null, secondarySignerAddresses)
{

    public new readonly List<AccountAddress> SecondarySignerAddresses = secondarySignerAddresses;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)RawTransactionVariant.MultiAgentTransaction);
        RawTransaction.Serialize(s);
        s.Vector(SecondarySignerAddresses);
    }

    public static new MultiAgentRawTransaction Deserialize(Deserializer d) => new(RawTransaction.Deserialize(d), d.Vector(AccountAddress.Deserialize));

}