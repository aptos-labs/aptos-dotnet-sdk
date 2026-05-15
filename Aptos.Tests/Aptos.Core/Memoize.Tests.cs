namespace Aptos.Tests.Core;

using Aptos.Core;
using Aptos.Exceptions;
using Newtonsoft.Json;

public class MemoizeTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public async Task MemoAsync_CachesResult()
    {
        int callCount = 0;
        var cached = Memoize_CallableMemoAsync(
            async () =>
            {
                await Task.Yield();
                callCount++;
                return 42;
            },
            "test-key-async-1",
            ttlMs: 10000
        );

        Assert.Equal(42, await cached());
        Assert.Equal(42, await cached());
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Memo_CachesResult()
    {
        int callCount = 0;
        var cached = Memoize_CallableMemo(
            () =>
            {
                callCount++;
                return 7;
            },
            "test-key-sync-1",
            ttlMs: 10000
        );

        Assert.Equal(7, cached());
        Assert.Equal(7, cached());
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task MemoAsync_TtlExpires()
    {
        int callCount = 0;
        var cached = Memoize_CallableMemoAsync(
            async () =>
            {
                await Task.Yield();
                callCount++;
                return callCount;
            },
            "test-key-async-ttl",
            ttlMs: 1
        );
        Assert.Equal(1, await cached());
        await Task.Delay(20);
        Assert.Equal(2, await cached());
    }

    /// <summary>
    /// Reflection wrappers since Memoize is internal.
    /// </summary>
    private static Func<Task<T>> Memoize_CallableMemoAsync<T>(
        Func<Task<T>> func,
        string key,
        long? ttlMs = null
    )
    {
        var type = typeof(AptosClient).Assembly.GetType("Aptos.Core.Memoize")!;
        var method = type.GetMethod(
            "MemoAsync",
            System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
        )!;
        var generic = method.MakeGenericMethod(typeof(T));
        return (Func<Task<T>>)generic.Invoke(null, [func, key, ttlMs])!;
    }

    private static Func<T> Memoize_CallableMemo<T>(Func<T> func, string key, long? ttlMs = null)
    {
        var type = typeof(AptosClient).Assembly.GetType("Aptos.Core.Memoize")!;
        var method = type.GetMethod(
            "Memo",
            System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
        )!;
        var generic = method.MakeGenericMethod(typeof(T));
        return (Func<T>)generic.Invoke(null, [func, key, ttlMs])!;
    }
}

public class AccountSignatureJsonTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void AccountEd25519Signature_RoundTripsViaJson()
    {
        const string json = """
        {
            "type": "ed25519_signature",
            "public_key": "0xaabbccddeeff00112233445566778899aabbccddeeff00112233445566778899",
            "signature": "0xaabbccddeeff00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff00112233445566778899"
        }
        """;
        var deserialized = (AccountEd25519Signature)
            JsonConvert.DeserializeObject<AccountSignature>(json)!;
        Assert.Equal(32, deserialized.PublicKey.ToByteArray().Length);
        Assert.Equal(64, deserialized.Signature.ToByteArray().Length);
    }

    [Fact]
    public void AccountSignature_UnknownType_Throws()
    {
        Assert.Throws<Exception>(
            () =>
                JsonConvert.DeserializeObject<AccountSignature>("{\"type\":\"not_a_type\"}")
        );
    }
}

public class ExceptionsCryptoTests
{
    [Fact]
    public void KeyLengthMismatch_HasDescriptiveMessage()
    {
        var ex = new KeyLengthMismatch("Ed25519PublicKey", 32);
        Assert.Contains("Ed25519PublicKey", ex.Message);
        Assert.Contains("32", ex.Message);
    }

    [Fact]
    public void InvalidDerivationPath_HasDescriptiveMessage()
    {
        var ex = new InvalidDerivationPath("not-a-path");
        Assert.Contains("not-a-path", ex.Message);
    }

    [Fact]
    public void EphemeralSignatureVariantUnsupported_HasDescriptiveMessage()
    {
        var ex = new EphemeralSignatureVariantUnsupported(
            (PublicKeySignatureVariant)99
        );
        Assert.Contains("Ephemeral", ex.Message);
    }

    [Fact]
    public void EphemeralKeyVariantUnsupported_HasDescriptiveMessage()
    {
        var ex = new EphemeralKeyVariantUnsupported((PublicKeyVariant)99);
        Assert.Contains("Ephemeral", ex.Message);
    }

    [Fact]
    public void HexException_StoresReason()
    {
        var ex = new HexException("invalid", HexInvalidReason.InvalidLength);
        Assert.Equal(HexInvalidReason.InvalidLength, ex.Reason);
        Assert.Equal("invalid", ex.Message);
    }

    [Fact]
    public void AccountAddressParsingException_StoresReason()
    {
        var ex = new AccountAddressParsingException(
            "bad",
            AccountAddressInvalidReason.TooLong
        );
        Assert.Equal(AccountAddressInvalidReason.TooLong, ex.Reason);
    }

    [Fact]
    public void TypeTagParserException_HasDescriptiveMessage()
    {
        var ex = new TypeTagParserException(
            "invalid tag",
            TypeTagParserExceptionReason.InvalidTypeTag
        );
        Assert.Contains("invalid tag", ex.Message);
        Assert.Contains("unknown type", ex.Message);
    }

    [Fact]
    public void TypeTagParserException_AllReasonsMessagesNonEmpty()
    {
        foreach (TypeTagParserExceptionReason reason in Enum.GetValues<TypeTagParserExceptionReason>())
        {
            var ex = new TypeTagParserException("tag", reason);
            Assert.NotEqual(string.Empty, ex.Message);
        }
    }

    [Fact]
    public void UnexpectedResponseException_HasMessage()
    {
        var ex = new UnexpectedResponseException("oops");
        Assert.Equal("oops", ex.Message);
    }
}
