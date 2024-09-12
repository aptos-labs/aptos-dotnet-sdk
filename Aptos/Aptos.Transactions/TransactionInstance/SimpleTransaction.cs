namespace Aptos;

public class SimpleTransaction(RawTransaction rawTransaction, AccountAddress? feePayerAddress = null) : AnyRawTransaction(rawTransaction, feePayerAddress)
{
    public override void Serialize(Serializer s)
    {
        RawTransaction.Serialize(s);

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

    public static SimpleTransaction Deserialize(Deserializer d)
    {
        RawTransaction rawTransaction = RawTransaction.Deserialize(d);
        bool hasFeePayer = d.Bool();
        return new SimpleTransaction(rawTransaction, hasFeePayer ? AccountAddress.Deserialize(d) : null);
    }
}