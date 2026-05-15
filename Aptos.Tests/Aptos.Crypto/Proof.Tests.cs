namespace Aptos.Tests.Crypto;

public class ProofTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void G1Bytes_LengthValidation()
    {
        Assert.Throws<ArgumentException>(() => new G1Bytes(new byte[16]));
        var ok = new G1Bytes(new byte[32]);
        Assert.Equal(32, ok.Data.Length);
    }

    [Fact]
    public void G1Bytes_SerializeAndDeserialize()
    {
        var orig = new G1Bytes(Enumerable.Range(0, 32).Select(i => (byte)i).ToArray());
        var bytes = orig.BcsToBytes();
        Assert.Equal(32, bytes.Length);
        var d = G1Bytes.Deserialize(new Deserializer(bytes));
        Assert.Equal(orig.Data, d.Data);
    }

    [Fact]
    public void G1Bytes_FromHexString()
    {
        var hex = "0x" + new string('0', 64);
        var g = new G1Bytes(hex);
        Assert.Equal(32, g.Data.Length);
    }

    [Fact]
    public void G2Bytes_LengthValidation()
    {
        Assert.Throws<ArgumentException>(() => new G2Bytes(new byte[32]));
        var ok = new G2Bytes(new byte[64]);
        Assert.Equal(64, ok.Data.Length);
    }

    [Fact]
    public void G2Bytes_SerializeAndDeserialize()
    {
        var orig = new G2Bytes(Enumerable.Range(0, 64).Select(i => (byte)i).ToArray());
        var bytes = orig.BcsToBytes();
        Assert.Equal(64, bytes.Length);
        var d = G2Bytes.Deserialize(new Deserializer(bytes));
        Assert.Equal(orig.Data, d.Data);
    }

    [Fact]
    public void Groth16Zkp_RoundTrip()
    {
        var a = new byte[32];
        var b = new byte[64];
        var c = new byte[32];
        var zkp = new Groth16Zkp(a, b, c);
        var bytes = zkp.BcsToBytes();
        Assert.Equal(128, bytes.Length);
        var d = Groth16Zkp.Deserialize(new Deserializer(bytes));
        Assert.Equal(a, d.A.Data);
        Assert.Equal(b, d.B.Data);
        Assert.Equal(c, d.C.Data);
        Assert.NotEmpty(zkp.ToString());
    }

    [Fact]
    public void ZkProof_RoundTrip_Groth16()
    {
        var inner = new Groth16Zkp(new byte[32], new byte[64], new byte[32]);
        var zkp = new ZkProof(inner, ZkpVariant.Groth16);
        var bytes = zkp.BcsToBytes();
        Assert.Equal((byte)ZkpVariant.Groth16, bytes[0]);
        var d = ZkProof.Deserialize(new Deserializer(bytes));
        Assert.Equal(ZkpVariant.Groth16, d.Variant);
        Assert.IsType<Groth16Zkp>(d.Proof);
    }

    [Fact]
    public void ZkProof_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => ZkProof.Deserialize(new Deserializer(s.ToBytes()))
        );
    }
}

public class FederatedKeylessTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void FederatedKeylessPublicKey_SerializeAndDeserialize()
    {
        var keyless = new KeylessPublicKey("https://issuer", new byte[32]);
        var jwk = AccountAddress.FromString("0x" + new string('a', 64));
        var fed = new FederatedKeylessPublicKey(keyless, jwk);
        var bytes = fed.BcsToBytes();
        var deserialized = FederatedKeylessPublicKey.Deserialize(new Deserializer(bytes));
        Assert.Equal(fed.BcsToBytes(), deserialized.BcsToBytes());
        Assert.Equal(PublicKeyVariant.FederatedKeyless, deserialized.Type);
    }

    [Fact]
    public void FederatedKeylessPublicKey_VerifySignature_WrongTypeReturnsFalse()
    {
        var fed = new FederatedKeylessPublicKey(
            new KeylessPublicKey("https://issuer", new byte[32]),
            AccountAddress.ZERO
        );
        var ed = Ed25519PrivateKey.Generate();
        var sig = ed.Sign(new byte[] { 1 });
        Assert.False(fed.VerifySignature(new byte[] { 1 }, sig));
    }

    [Fact]
    public void FederatedKeylessPublicKey_ConstructorOverloads_AllProduceSameValue()
    {
        const string iss = "https://issuer";
        const string uidKey = "sub";
        const string uidVal = "user-1";
        const string aud = "audience";
        var pepper = new byte[31];
        var jwk = AccountAddress.ZERO;

        var byBytes = new FederatedKeylessPublicKey(iss, uidKey, uidVal, aud, pepper, jwk);
        var byHex = new FederatedKeylessPublicKey(iss, uidKey, uidVal, aud, "0x" + new string('0', 62), jwk);
        Assert.Equal(byBytes.BcsToBytes(), byHex.BcsToBytes());
    }
}
