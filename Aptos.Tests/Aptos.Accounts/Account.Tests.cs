namespace Aptos.Tests.Accounts;

using Aptos.Schemes;

public class AccountTests(ITestOutputHelper output) : BaseTests(output)
{
    private const string MNEMONIC =
        "shoot island position soft burden budget tooth cruel issue economy destroy above";

    [Fact]
    public void Account_Generate_ReturnsEd25519()
    {
        var account = Account.Generate();
        Assert.IsType<Ed25519Account>(account);
        Assert.Equal(SigningScheme.Ed25519, account.SigningScheme);
        Assert.Equal(32, account.Address.Data.Length);
    }

    [Fact]
    public void Ed25519Account_SignAndVerify()
    {
        var account = Ed25519Account.Generate();
        var msg = new byte[] { 1, 2, 3 };
        var sig = (Ed25519Signature)account.Sign(msg);
        Assert.True(account.VerifySignature(msg, sig));
        Assert.False(account.VerifySignature(new byte[] { 9, 9, 9 }, sig));
    }

    [Fact]
    public void Ed25519Account_SignString_RoundTrip()
    {
        var account = Ed25519Account.Generate();
        var sig = (Ed25519Signature)account.Sign("hello world");
        Assert.True(
            ((Ed25519PublicKey)account.VerifyingKey).VerifySignature(
                SigningMessage.Convert("hello world"),
                sig
            )
        );
    }

    [Fact]
    public void Ed25519Account_SignWithAuthenticator()
    {
        var account = Ed25519Account.Generate();
        var msg = new byte[] { 9 };
        var auth = (AccountAuthenticatorEd25519)account.SignWithAuthenticator(msg);
        Assert.Equal(
            ((Ed25519PublicKey)account.VerifyingKey).ToByteArray(),
            auth.PublicKey.ToByteArray()
        );
        Assert.True(account.VerifySignature(msg, auth.Signature));
    }

    [Fact]
    public void Ed25519Account_FromDerivationPath_Deterministic()
    {
        var a = Ed25519Account.FromDerivationPath("m/44'/637'/0'/0'/0'", MNEMONIC);
        var b = Ed25519Account.FromDerivationPath("m/44'/637'/0'/0'/0'", MNEMONIC);
        Assert.Equal(a.Address, b.Address);
        // Different paths produce different accounts.
        var c = Ed25519Account.FromDerivationPath("m/44'/637'/1'/0'/0'", MNEMONIC);
        Assert.NotEqual(a.Address, c.Address);
    }

    [Fact]
    public void Ed25519Account_CustomAddress_Stored()
    {
        var pk = Ed25519PrivateKey.Generate();
        var longAddr = "0x" + new string('a', 64);
        var customAddress = AccountAddress.FromString(longAddr);
        var account = new Ed25519Account(pk, customAddress);
        Assert.Equal(customAddress, account.Address);

        // String overload
        var account2 = new Ed25519Account(pk, longAddr);
        Assert.Equal(customAddress, account2.Address);

        // byte[] overload
        var account3 = new Ed25519Account(pk, customAddress.Data);
        Assert.Equal(customAddress, account3.Address);
    }

    [Fact]
    public void SingleKeyAccount_GenerateEd25519_SignAndVerify()
    {
        var account = SingleKeyAccount.Generate();
        Assert.Equal(SigningScheme.SingleKey, account.SigningScheme);
        var msg = new byte[] { 1, 2, 3 };
        var sig = account.Sign(msg);
        Assert.True(account.VerifySignature(msg, sig));
    }

    [Fact]
    public void SingleKeyAccount_GenerateSecp256k1_SignAndVerify()
    {
        var account = SingleKeyAccount.Generate(PublicKeyVariant.Secp256k1Ecdsa);
        Assert.IsType<Secp256k1PrivateKey>(account.PrivateKey);
        var msg = new byte[] { 1, 2, 3 };
        var sig = account.Sign(msg);
        Assert.True(account.VerifySignature(msg, sig));
    }

    [Fact]
    public void SingleKeyAccount_Generate_UnsupportedScheme_Throws()
    {
        Assert.Throws<ArgumentException>(() => SingleKeyAccount.Generate((PublicKeyVariant)999));
    }

    [Fact]
    public void SingleKeyAccount_SignWithAuthenticator()
    {
        var account = SingleKeyAccount.Generate();
        var auth = (AccountAuthenticatorSingleKey)account.SignWithAuthenticator(new byte[] { 1 });
        Assert.NotNull(auth.Signature);
    }

    [Fact]
    public void SingleKeyAccount_FromDerivationPath_DifferentSchemes()
    {
        var ed = SingleKeyAccount.FromDerivationPath(
            PublicKeyVariant.Ed25519,
            "m/44'/637'/0'/0'/0'",
            MNEMONIC
        );
        Assert.IsType<Ed25519PrivateKey>(ed.PrivateKey);

        var secp = SingleKeyAccount.FromDerivationPath(
            PublicKeyVariant.Secp256k1Ecdsa,
            "m/44'/637'/0'/0/0",
            MNEMONIC
        );
        Assert.IsType<Secp256k1PrivateKey>(secp.PrivateKey);

        Assert.Throws<ArgumentException>(
            () => SingleKeyAccount.FromDerivationPath((PublicKeyVariant)99, "m/0'", MNEMONIC)
        );
    }

    [Fact]
    public void MultiKeyAccount_SignAndVerify()
    {
        var a = Ed25519Account.Generate();
        var b = Ed25519Account.Generate();
        var multiKey = new MultiKey(
            [(Ed25519PublicKey)a.VerifyingKey, (Ed25519PublicKey)b.VerifyingKey],
            1
        );
        var account = new MultiKeyAccount(multiKey, [a]);
        Assert.Equal(SigningScheme.MultiKey, account.SigningScheme);
        var msg = new byte[] { 1, 2, 3 };
        var sig = (MultiKeySignature)account.Sign(msg);
        Assert.True(account.VerifySignature(msg, sig));
    }

    [Fact]
    public void MultiKeyAccount_TooFewSigners_Throws()
    {
        var a = Ed25519Account.Generate();
        var b = Ed25519Account.Generate();
        var multiKey = new MultiKey(
            [(Ed25519PublicKey)a.VerifyingKey, (Ed25519PublicKey)b.VerifyingKey],
            2
        );
        Assert.Throws<ArgumentException>(() => new MultiKeyAccount(multiKey, [a]));
    }

    [Fact]
    public void MultiKeyAccount_SignWithAuthenticator()
    {
        var a = Ed25519Account.Generate();
        var multiKey = new MultiKey([(Ed25519PublicKey)a.VerifyingKey], 1);
        var account = new MultiKeyAccount(multiKey, [a]);
        var auth = (AccountAuthenticatorMultiKey)account.SignWithAuthenticator(new byte[] { 1 });
        Assert.NotNull(auth.Signature);
    }

    [Fact]
    public void EphemeralKeyPair_GenerateAndSerialize()
    {
        var kp = EphemeralKeyPair.Generate();
        Assert.False(kp.IsExpired());
        Assert.Equal(31, kp.Blinder.Length);

        var bytes = kp.BcsToBytes();
        var roundTripped = EphemeralKeyPair.Deserialize(new Deserializer(bytes));
        Assert.Equal(kp.ExpiryTimestamp, roundTripped.ExpiryTimestamp);
        Assert.Equal(kp.Blinder, roundTripped.Blinder);
        Assert.Equal(kp.Nonce, roundTripped.Nonce);
    }

    [Fact]
    public void EphemeralKeyPair_ExpiredCannotSign()
    {
        // Construct an already-expired key pair.
        var kp = new EphemeralKeyPair(Ed25519PrivateKey.Generate(), 1UL, new byte[31]);
        Assert.True(kp.IsExpired());
        Assert.Throws<Exception>(() => kp.Sign(new byte[] { 1 }));
    }
}
