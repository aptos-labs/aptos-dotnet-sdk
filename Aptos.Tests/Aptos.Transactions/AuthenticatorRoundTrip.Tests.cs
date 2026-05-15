namespace Aptos.Tests.Transactions;

/// <summary>
/// Serialization / deserialization round trip tests for the entire Transaction /
/// Authenticator type hierarchy. These tests do not require a network and run
/// quickly.
/// </summary>
public class AuthenticatorRoundTripTests(ITestOutputHelper output) : BaseTests(output)
{
    private static Ed25519Account NewEd25519() => Ed25519Account.Generate();

    private static SingleKeyAccount NewSingleKey() => SingleKeyAccount.Generate();

    private static (Ed25519PublicKey pk, Ed25519Signature sig) PkAndSig()
    {
        var account = NewEd25519();
        var sig = (Ed25519Signature)account.Sign("hello");
        return ((Ed25519PublicKey)account.VerifyingKey, sig);
    }

    [Fact]
    public void AccountAuthenticatorEd25519_RoundTrip()
    {
        var (pk, sig) = PkAndSig();
        var auth = new AccountAuthenticatorEd25519(pk, sig);

        var bytes = auth.BcsToBytes();
        var roundTripped = (AccountAuthenticatorEd25519)
            AccountAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(pk.ToByteArray(), roundTripped.PublicKey.ToByteArray());
        Assert.Equal(sig.ToByteArray(), roundTripped.Signature.ToByteArray());
        Assert.Equal((byte)AccountAuthenticatorVariant.Ed25519, bytes[0]);
    }

    [Fact]
    public void AccountAuthenticatorSingleKey_RoundTrip()
    {
        var account = NewSingleKey();
        var sig = (PublicKeySignature)account.Sign("hello");
        var auth = new AccountAuthenticatorSingleKey((PublicKey)account.PublicKey, sig);

        var bytes = auth.BcsToBytes();
        var roundTripped = (AccountAuthenticatorSingleKey)
            AccountAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(
            ((PublicKey)account.PublicKey).BcsToBytes(),
            roundTripped.PublicKey.BcsToBytes()
        );
        Assert.Equal(sig.BcsToBytes(), roundTripped.Signature.BcsToBytes());
        Assert.Equal((byte)AccountAuthenticatorVariant.SingleKey, bytes[0]);
    }

    [Fact]
    public void AccountAuthenticatorMultiKey_RoundTrip()
    {
        // Build a 2-of-3 multikey with three ed25519 accounts.
        var a = NewEd25519();
        var b = NewEd25519();
        var c = NewEd25519();
        var multiKey = new MultiKey(
            [
                (Ed25519PublicKey)a.VerifyingKey,
                (Ed25519PublicKey)b.VerifyingKey,
                (Ed25519PublicKey)c.VerifyingKey,
            ],
            2
        );

        var multiKeyAccount = new MultiKeyAccount(multiKey, [a, b]);
        var sig = (MultiKeySignature)multiKeyAccount.Sign("hello");
        var auth = new AccountAuthenticatorMultiKey(multiKey, sig);

        var bytes = auth.BcsToBytes();
        var roundTripped = (AccountAuthenticatorMultiKey)
            AccountAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(multiKey.BcsToBytes(), roundTripped.PublicKey.BcsToBytes());
        Assert.Equal(sig.BcsToBytes(), roundTripped.Signature.BcsToBytes());
        Assert.Equal((byte)AccountAuthenticatorVariant.MultiKey, bytes[0]);
    }

    [Fact]
    public void AccountAuthenticator_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        var ex = Assert.Throws<ArgumentException>(
            () => AccountAuthenticator.Deserialize(new Deserializer(s.ToBytes()))
        );
        Assert.Contains("Invalid", ex.Message);
    }

    [Fact]
    public void TransactionAuthenticatorEd25519_RoundTrip()
    {
        var (pk, sig) = PkAndSig();
        var auth = new TransactionAuthenticatorEd25519(pk, sig);

        var bytes = auth.BcsToBytes();
        var roundTripped = (TransactionAuthenticatorEd25519)
            TransactionAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(pk.ToByteArray(), roundTripped.PublicKey.ToByteArray());
        Assert.Equal(sig.ToByteArray(), roundTripped.Signature.ToByteArray());
        Assert.Equal((byte)TransactionAuthenticatorVariant.Ed25519, bytes[0]);
    }

    [Fact]
    public void TransactionAuthenticatorSingleSender_RoundTrip()
    {
        var (pk, sig) = PkAndSig();
        var inner = new AccountAuthenticatorEd25519(pk, sig);
        var auth = new TransactionAuthenticatorSingleSender(inner);

        var bytes = auth.BcsToBytes();
        var roundTripped = (TransactionAuthenticatorSingleSender)
            TransactionAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.IsType<AccountAuthenticatorEd25519>(roundTripped.Sender);
        Assert.Equal((byte)TransactionAuthenticatorVariant.SingleSender, bytes[0]);
    }

    [Fact]
    public void TransactionAuthenticatorMultiAgent_RoundTrip()
    {
        var (pk, sig) = PkAndSig();
        var (pk2, sig2) = PkAndSig();
        var auth = new TransactionAuthenticatorMultiAgent(
            new AccountAuthenticatorEd25519(pk, sig),
            [AccountAddress.FromString("0x42", 63)],
            [new AccountAuthenticatorEd25519(pk2, sig2)]
        );

        var bytes = auth.BcsToBytes();
        var roundTripped = (TransactionAuthenticatorMultiAgent)
            TransactionAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(auth.SecondarySignerAddresses.Count, roundTripped.SecondarySignerAddresses.Count);
        Assert.Equal(auth.SecondarySigners.Count, roundTripped.SecondarySigners.Count);
        Assert.Equal(
            auth.SecondarySignerAddresses[0],
            roundTripped.SecondarySignerAddresses[0]
        );
        Assert.Equal((byte)TransactionAuthenticatorVariant.MultiAgent, bytes[0]);
    }

    [Fact]
    public void TransactionAuthenticatorFeePayer_RoundTrip()
    {
        var (pk, sig) = PkAndSig();
        var (pk2, sig2) = PkAndSig();
        var (pkFp, sigFp) = PkAndSig();
        var feePayerAddress = AccountAddress.FromString("0xfee", 63);
        var auth = new TransactionAuthenticatorFeePayer(
            new AccountAuthenticatorEd25519(pk, sig),
            [AccountAddress.FromString("0x1", 63)],
            [new AccountAuthenticatorEd25519(pk2, sig2)],
            (feePayerAddress, new AccountAuthenticatorEd25519(pkFp, sigFp))
        );

        var bytes = auth.BcsToBytes();
        var roundTripped = (TransactionAuthenticatorFeePayer)
            TransactionAuthenticator.Deserialize(new Deserializer(bytes));

        Assert.Equal(feePayerAddress, roundTripped.FeePayer.AccountAddress);
        Assert.IsType<AccountAuthenticatorEd25519>(roundTripped.FeePayer.Authenticator);
        Assert.Single(roundTripped.SecondarySignerAddresses);
        Assert.Equal((byte)TransactionAuthenticatorVariant.FeePayer, bytes[0]);
    }

    [Fact]
    public void TransactionAuthenticator_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => TransactionAuthenticator.Deserialize(new Deserializer(s.ToBytes()))
        );
    }
}
