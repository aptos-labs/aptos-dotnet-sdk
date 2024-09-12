namespace Aptos;

public class TransactionAuthenticatorSingleSender(AccountAuthenticator sender) : TransactionAuthenticator
{
    public readonly AccountAuthenticator Sender = sender;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionAuthenticatorVariant.SingleSender);
        Sender.Serialize(s);
    }

    public static new TransactionAuthenticatorSingleSender Deserialize(Deserializer d)
    {
        AccountAuthenticator sender = AccountAuthenticator.Deserialize(d);
        return new TransactionAuthenticatorSingleSender(sender);
    }
}