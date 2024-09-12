namespace Aptos;

public class SignedTransaction(RawTransaction rawTransaction, TransactionAuthenticator authenticator) : Serializable
{

    public readonly RawTransaction RawTransaction = rawTransaction;

    public readonly TransactionAuthenticator Authenticator = authenticator;

    public override void Serialize(Serializer s)
    {
        RawTransaction.Serialize(s);
        Authenticator.Serialize(s);
    }

    public static SignedTransaction Deserialize(Deserializer d) => new(RawTransaction.Deserialize(d), TransactionAuthenticator.Deserialize(d));
}