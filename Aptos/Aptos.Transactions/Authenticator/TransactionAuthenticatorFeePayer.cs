namespace Aptos;

public class TransactionAuthenticatorFeePayer(AccountAuthenticator sender, List<AccountAddress> secondarySignerAddresses, List<AccountAuthenticator> secondarySigners, (AccountAddress, AccountAuthenticator) feePayer) : TransactionAuthenticator
{
    public readonly AccountAuthenticator Sender = sender;
    public readonly List<AccountAddress> SecondarySignerAddresses = secondarySignerAddresses;
    public readonly List<AccountAuthenticator> SecondarySigners = secondarySigners;

    public readonly (AccountAddress AccountAddress, AccountAuthenticator Authenticator) FeePayer = feePayer;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionAuthenticatorVariant.FeePayer);
        Sender.Serialize(s);
        s.Vector(SecondarySignerAddresses);
        s.Vector(SecondarySigners);
        FeePayer.AccountAddress.Serialize(s);
        FeePayer.Authenticator.Serialize(s);
    }

    public static new TransactionAuthenticatorFeePayer Deserialize(Deserializer d)
    {
        AccountAuthenticator sender = AccountAuthenticator.Deserialize(d);
        List<AccountAddress> secondarySignerAddresses = d.Vector(AccountAddress.Deserialize);
        List<AccountAuthenticator> secondarySigners = d.Vector(AccountAuthenticator.Deserialize);
        (AccountAddress, AccountAuthenticator) feePayer = (AccountAddress.Deserialize(d), AccountAuthenticator.Deserialize(d));
        return new TransactionAuthenticatorFeePayer(sender, [.. secondarySignerAddresses], [.. secondarySigners], feePayer);
    }
}