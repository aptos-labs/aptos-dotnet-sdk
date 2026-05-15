namespace Aptos.Tests.Crypto;

/// <summary>
/// Tests that PrivateKey implementations honour IDisposable contract:
/// - bytes are zeroed after Dispose
/// - subsequent reads throw ObjectDisposedException
/// - Dispose is idempotent
/// - using-blocks work correctly
/// </summary>
public class PrivateKeyDisposableTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void Ed25519PrivateKey_Dispose_ZeroesBytes()
    {
        var pk = Ed25519PrivateKey.Generate();
        // Capture a reference to the internal byte array so we can observe
        // it being zeroed by Dispose.
        var internalBytes = pk.ToByteArray();
        Assert.Contains(internalBytes, b => b != 0);

        pk.Dispose();
        Assert.All(internalBytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Ed25519PrivateKey_Dispose_IsIdempotent()
    {
        var pk = Ed25519PrivateKey.Generate();
        pk.Dispose();
        pk.Dispose();
        pk.Dispose();
    }

    [Fact]
    public void Ed25519PrivateKey_AfterDispose_ThrowsOnUse()
    {
        var pk = Ed25519PrivateKey.Generate();
        pk.Dispose();
        Assert.Throws<ObjectDisposedException>(() => pk.Sign(new byte[] { 1 }));
        Assert.Throws<ObjectDisposedException>(() => pk.PublicKey());
        Assert.Throws<ObjectDisposedException>(() => pk.ToByteArray());
        Assert.Throws<ObjectDisposedException>(() => pk.Serialize(new Serializer()));
    }

    [Fact]
    public void Ed25519PrivateKey_UsingBlock_DisposesAutomatically()
    {
        byte[]? captured;
        using (var pk = Ed25519PrivateKey.Generate())
        {
            captured = pk.ToByteArray();
            Assert.Contains(captured, b => b != 0);
        }
        Assert.All(captured!, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Ed25519PrivateKey_DisposeBeforeUse_SignThrows()
    {
        var pk = Ed25519PrivateKey.Generate();
        pk.Dispose();
        Assert.Throws<ObjectDisposedException>(() => ((PrivateKey)pk).Sign("hello"));
    }

    [Fact]
    public void Secp256k1PrivateKey_Dispose_ZeroesBytes()
    {
        var pk = Secp256k1PrivateKey.Generate();
        var internalBytes = pk.ToByteArray();
        Assert.Contains(internalBytes, b => b != 0);

        pk.Dispose();
        Assert.All(internalBytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Secp256k1PrivateKey_AfterDispose_ThrowsOnUse()
    {
        var pk = Secp256k1PrivateKey.Generate();
        pk.Dispose();
        Assert.Throws<ObjectDisposedException>(() => pk.Sign(new byte[] { 1 }));
        Assert.Throws<ObjectDisposedException>(() => pk.PublicKey());
        Assert.Throws<ObjectDisposedException>(() => pk.ToByteArray());
    }

    [Fact]
    public void Secp256k1PrivateKey_UsingBlock_DisposesAutomatically()
    {
        byte[]? captured;
        using (var pk = Secp256k1PrivateKey.Generate())
        {
            captured = pk.ToByteArray();
            Assert.Contains(captured, b => b != 0);
        }
        Assert.All(captured!, b => Assert.Equal(0, b));
    }

    [Fact]
    public void PrivateKey_AsIDisposable_WorksThroughBaseInterface()
    {
        // Demonstrates the contract works against IDisposable directly so
        // callers can store keys in fields of type IDisposable.
        IDisposable pk = Ed25519PrivateKey.Generate();
        pk.Dispose();
        // No exception.
    }
}
