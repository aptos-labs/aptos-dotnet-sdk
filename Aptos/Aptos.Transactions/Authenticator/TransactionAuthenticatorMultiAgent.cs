namespace Aptos;

public class TransactionAuthenticatorMultiAgent(AccountAuthenticator sender, List<AccountAddress> secondarySignerAddresses, List<AccountAuthenticator> secondarySigners) : TransactionAuthenticator
{
    public readonly AccountAuthenticator Sender = sender;

    public readonly List<AccountAddress> SecondarySignerAddresses = secondarySignerAddresses;

    public readonly List<AccountAuthenticator> SecondarySigners = secondarySigners;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionAuthenticatorVariant.MultiAgent);
        Sender.Serialize(s);
        s.Vector(SecondarySignerAddresses);
        s.Vector(SecondarySigners);
    }

    public static new TransactionAuthenticatorMultiAgent Deserialize(Deserializer d)
    {
        AccountAuthenticator sender = AccountAuthenticator.Deserialize(d);
        List<AccountAddress> secondarySignerAddresses = d.Vector(AccountAddress.Deserialize);
        List<AccountAuthenticator> secondarySigners = d.Vector(AccountAuthenticator.Deserialize);
        return new TransactionAuthenticatorMultiAgent(sender, secondarySignerAddresses, secondarySigners);
    }
}