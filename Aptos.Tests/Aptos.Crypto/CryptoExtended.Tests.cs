namespace Aptos.Tests.Crypto;

using Aptos.Exceptions;
using Aptos.Schemes;

public class CryptoExtendedTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void AuthenticationKey_FromSchemeAndBytes_Deterministic()
    {
        var bytes = new byte[32];
        var key1 = AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.Ed25519, bytes);
        var key2 = AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.Ed25519, bytes);
        Assert.Equal(key1.ToString(), key2.ToString());
        Assert.Equal(32, key1.ToByteArray().Length);
    }

    [Fact]
    public void AuthenticationKey_DifferentSchemesProduceDifferentKeys()
    {
        var bytes = new byte[32];
        var ed = AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.Ed25519, bytes);
        var sk = AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.SingleKey, bytes);
        Assert.NotEqual(ed.ToString(), sk.ToString());
    }

    [Fact]
    public void AuthenticationKey_FromSchemeAndBytes_StringOverload()
    {
        var key = AuthenticationKey.FromSchemeAndBytes(
            AuthenticationKeyScheme.Ed25519,
            "0x" + new string('0', 64)
        );
        Assert.Equal(32, key.ToByteArray().Length);
    }

    [Fact]
    public void AuthenticationKey_WrongLength_Throws()
    {
        Assert.Throws<KeyLengthMismatch>(() => new AuthenticationKey(new byte[16]));
    }

    [Fact]
    public void AuthenticationKey_RoundTrip()
    {
        var addr = AccountAddress.FromString("0x1", 63);
        var key = AuthenticationKey.FromSchemeAndBytes(
            AuthenticationKeyScheme.Ed25519,
            new byte[32]
        );
        var bytes = key.BcsToBytes();
        var deserialized = AuthenticationKey.Deserialize(new Deserializer(bytes));
        Assert.Equal(key.Data, deserialized.Data);
        Assert.NotEqual(addr, key.DerivedAddress()); // derived address from random bytes
    }

    [Fact]
    public void Ed25519PrivateKey_GenerateAndSign()
    {
        var pk = Ed25519PrivateKey.Generate();
        var sig = pk.Sign("hello");
        Assert.True(pk.PublicKey().VerifySignature("hello", sig));
    }

    [Fact]
    public void Ed25519PrivateKey_WrongLength_Throws()
    {
        Assert.Throws<KeyLengthMismatch>(() => new Ed25519PrivateKey(new byte[16]));
    }

    [Fact]
    public void Ed25519PublicKey_WrongLength_Throws()
    {
        Assert.Throws<KeyLengthMismatch>(() => new Ed25519PublicKey(new byte[16]));
    }

    [Fact]
    public void Ed25519Signature_WrongLength_Throws()
    {
        Assert.Throws<KeyLengthMismatch>(() => new Ed25519Signature(new byte[16]));
    }

    [Fact]
    public void Ed25519PrivateKey_FromDerivationPath_InvalidPath_Throws()
    {
        Assert.Throws<InvalidDerivationPath>(
            () =>
                Ed25519PrivateKey.FromDerivationPath(
                    "not-a-path",
                    "shoot island position soft burden budget tooth cruel issue economy destroy above"
                )
        );
    }

    [Fact]
    public void Secp256k1PrivateKey_GenerateAndSign()
    {
        var pk = Secp256k1PrivateKey.Generate();
        var sig = pk.Sign("hello");
        Assert.True(pk.PublicKey().VerifySignature("hello", sig));
    }

    [Fact]
    public void Secp256k1Signature_RoundTrip()
    {
        var pk = Secp256k1PrivateKey.Generate();
        var sig = (Secp256k1Signature)pk.Sign("hello");
        var bytes = sig.BcsToBytes();
        var deserialized = Secp256k1Signature.Deserialize(new Deserializer(bytes));
        Assert.Equal(sig.ToByteArray(), deserialized.ToByteArray());
    }

    [Fact]
    public void Secp256k1PrivateKey_Generate_AlwaysReturns32Bytes()
    {
        // Regression test for BigInteger.ToByteArrayUnsigned dropping leading
        // zero bytes. Without zero-padding, ~1 in 256 generated keys had
        // fewer than 32 bytes and the constructor threw KeyLengthMismatch.
        // Run many iterations so we'll catch a regression on at least one
        // private key with a leading-zero byte.
        for (int i = 0; i < 1000; i++)
        {
            var pk = Secp256k1PrivateKey.Generate();
            Assert.Equal(32, pk.ToByteArray().Length);
        }
    }

    [Fact]
    public void Secp256k1PrivateKey_Sign_AlwaysReturns64ByteSignature()
    {
        // Companion regression test for the same leading-zero stripping bug
        // in the signature path. Both r and s must be exactly 32 bytes; a
        // leading-zero r or s would otherwise produce a sub-64-byte
        // signature ~1 in 128 calls.
        var pk = Secp256k1PrivateKey.Generate();
        for (int i = 0; i < 1000; i++)
        {
            // Vary the message so each call picks a different deterministic k
            // and exercises the r / s output space.
            var msg = System.Text.Encoding.UTF8.GetBytes($"msg-{i}");
            var sig = (Secp256k1Signature)pk.Sign(msg);
            Assert.Equal(64, sig.ToByteArray().Length);
        }
    }

    [Fact]
    public void PublicKey_Deserialize_DispatchesOnVariant()
    {
        var ed = Ed25519PrivateKey.Generate().PublicKey();
        var s = new Serializer();
        s.U32AsUleb128((uint)PublicKeyVariant.Ed25519);
        ed.Serialize(s);
        var deserialized = PublicKey.Deserialize(new Deserializer(s.ToBytes()));
        Assert.IsType<Ed25519PublicKey>(deserialized);
    }

    [Fact]
    public void PublicKey_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => PublicKey.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void PublicKeySignature_Deserialize_DispatchesOnVariant()
    {
        var pk = Ed25519PrivateKey.Generate();
        var sig = pk.Sign("hi");
        var s = new Serializer();
        s.U32AsUleb128((uint)PublicKeySignatureVariant.Ed25519);
        sig.Serialize(s);
        var deserialized = PublicKeySignature.Deserialize(new Deserializer(s.ToBytes()));
        Assert.IsType<Ed25519Signature>(deserialized);
    }

    [Fact]
    public void PublicKeySignature_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => PublicKeySignature.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void PrivateKey_FormatPrivateKey_AppliesAIP80Prefix()
    {
        var hex = "0x" + new string('1', 64);
        var formatted = PrivateKey.FormatPrivateKey(hex, PrivateKeyVariant.Ed25519);
        Assert.StartsWith("ed25519-priv-0x", formatted);
        // Idempotent
        Assert.Equal(formatted, PrivateKey.FormatPrivateKey(formatted, PrivateKeyVariant.Ed25519));
    }

    [Fact]
    public void PrivateKey_ParseHexInput_HandlesBothFormats()
    {
        var hex = "0x" + new string('1', 64);
        var formatted = PrivateKey.FormatPrivateKey(hex, PrivateKeyVariant.Ed25519);

        var parsedFromAip80 = PrivateKey.ParseHexInput(formatted, PrivateKeyVariant.Ed25519, true);
        Assert.Equal(32, parsedFromAip80.ToByteArray().Length);

        // Non-strict input is parsed but may emit a warning to stdout.
        var parsedFromPlain = PrivateKey.ParseHexInput(hex, PrivateKeyVariant.Ed25519, false);
        Assert.Equal(parsedFromAip80.ToByteArray(), parsedFromPlain.ToByteArray());
    }

    [Fact]
    public void Ed25519PrivateKey_ToAIP80_ReturnsPrefixedString()
    {
        var pk = Ed25519PrivateKey.Generate();
        Assert.StartsWith("ed25519-priv-", pk.ToAIP80String());
        Assert.StartsWith("0x", pk.ToHexString());
        Assert.Equal(pk.ToAIP80String(), pk.ToString());
    }

    [Fact]
    public void Ed25519_IsCanonicalSignature_HandlesEdgeCases()
    {
        var pk = Ed25519PrivateKey.Generate();
        var sig = pk.Sign("hello");
        Assert.True(Ed25519.IsCanonicalEd25519Signature(sig));
    }

    [Fact]
    public void Ed25519PublicKey_VerifyingNonCanonicalSignatureReturnsFalse()
    {
        var pk = Ed25519PrivateKey.Generate();
        // Build a signature where S > L (definitely non-canonical).
        var badSig = new Ed25519Signature(Enumerable.Repeat((byte)0xff, 64).ToArray());
        Assert.False(((Ed25519PublicKey)pk.PublicKey()).VerifySignature(new byte[] { 1 }, badSig));
    }
}
