namespace Aptos.Tests.Crypto;

using System.Reflection;

/// <summary>
/// Tests that PrivateKey implementations honour the IDisposable contract:
/// - the internal key bytes are zeroed after Dispose (best-effort
///   in-process scrubbing),
/// - subsequent reads throw ObjectDisposedException,
/// - Dispose is idempotent,
/// - 'using' blocks work correctly,
/// - the defensive copy returned by <see cref="PrivateKey.ToByteArray"/>
///   prior to disposal does <em>not</em> alias the internal storage
///   (so external mutation of the public copy cannot scrub or reveal
///   the live key).
/// </summary>
public class PrivateKeyDisposableTests(ITestOutputHelper output) : BaseTests(output)
{
    /// <summary>
    /// Reads the live internal bytes via reflection so the test can verify
    /// the scrub, without exposing a non-test public API.
    /// </summary>
    private static byte[] InternalBytesOf(PrivateKey pk)
    {
        var keyField =
            pk.GetType().GetField("_key", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new Xunit.Sdk.XunitException("Could not find _key field on " + pk.GetType());
        var hex = (Hex)keyField.GetValue(pk)!;
        return hex.GetUnsafeByteArrayReference();
    }

    [Fact]
    public void Ed25519PrivateKey_Dispose_ZeroesInternalBytes()
    {
        var pk = Ed25519PrivateKey.Generate();
        var internalBytes = InternalBytesOf(pk);
        Assert.Contains(internalBytes, b => b != 0);

        pk.Dispose();
        Assert.All(internalBytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Ed25519PrivateKey_ToByteArray_ReturnsDefensiveCopy()
    {
        // Mutating the array returned by ToByteArray must not zero the live
        // key. This is the immutability contract that protects callers from
        // accidentally scrubbing a key still in use.
        var pk = Ed25519PrivateKey.Generate();
        var internalBytes = InternalBytesOf(pk);
        var externalCopy = pk.ToByteArray();
        Array.Clear(externalCopy, 0, externalCopy.Length);

        // Internal bytes are unchanged.
        Assert.Contains(internalBytes, b => b != 0);
        // The key still signs successfully.
        var sig = pk.Sign(new byte[] { 1 });
        Assert.NotNull(sig);
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
        byte[] internalBytes;
        using (var pk = Ed25519PrivateKey.Generate())
        {
            internalBytes = InternalBytesOf(pk);
            Assert.Contains(internalBytes, b => b != 0);
        }
        Assert.All(internalBytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Ed25519PrivateKey_DisposeBeforeUse_SignThrows()
    {
        var pk = Ed25519PrivateKey.Generate();
        pk.Dispose();
        Assert.Throws<ObjectDisposedException>(() => ((PrivateKey)pk).Sign("hello"));
    }

    [Fact]
    public void Secp256k1PrivateKey_Dispose_ZeroesInternalBytes()
    {
        var pk = Secp256k1PrivateKey.Generate();
        var internalBytes = InternalBytesOf(pk);
        Assert.Contains(internalBytes, b => b != 0);

        pk.Dispose();
        Assert.All(internalBytes, b => Assert.Equal(0, b));
    }

    [Fact]
    public void Secp256k1PrivateKey_ToByteArray_ReturnsDefensiveCopy()
    {
        var pk = Secp256k1PrivateKey.Generate();
        var internalBytes = InternalBytesOf(pk);
        var externalCopy = pk.ToByteArray();
        Array.Clear(externalCopy, 0, externalCopy.Length);

        Assert.Contains(internalBytes, b => b != 0);
        var sig = pk.Sign(new byte[] { 1 });
        Assert.NotNull(sig);
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
        byte[] internalBytes;
        using (var pk = Secp256k1PrivateKey.Generate())
        {
            internalBytes = InternalBytesOf(pk);
            Assert.Contains(internalBytes, b => b != 0);
        }
        Assert.All(internalBytes, b => Assert.Equal(0, b));
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
