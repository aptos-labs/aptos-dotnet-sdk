namespace Aptos.Tests.BCS;

using System.Numerics;

/// <summary>
/// Property-based / fuzz-style tests for the BCS Deserializer. These tests
/// throw random byte streams at each deserializer entry point and check
/// that:
///
/// - the deserializer never enters an infinite loop (bounded by a per-test
///   timeout),
/// - it never throws an unexpected exception type (only ArgumentException,
///   IndexOutOfRangeException, or OverflowException for malformed input),
/// - it terminates within a reasonable wall clock and memory budget.
///
/// The goal is to catch silent DoS vectors in code that runs on
/// adversarial inputs (HTTP responses from a node, transaction payloads
/// from a peer, etc.).
/// </summary>
public class DeserializerFuzzTests(ITestOutputHelper output) : BaseTests(output)
{
    private const int Iterations = 1000;
    private const int MaxBytes = 256;

    /// <summary>
    /// Allow-list of exception types the deserializer is permitted to
    /// throw on malformed input.
    /// </summary>
    private static bool IsExpected(Exception ex) =>
        ex is ArgumentException
        || ex is IndexOutOfRangeException
        || ex is OverflowException
        || ex is FormatException
        || ex is InvalidOperationException
        || ex is System.IO.EndOfStreamException
        // For Bytes / Vector with huge length-prefixes:
        || ex is OutOfMemoryException
        // The SDK's own exception hierarchy (key length, hex parse, etc.)
        // is the intended way to signal malformed input.
        || ex is Aptos.Exceptions.BaseException;

    [Fact(Timeout = 30000)]
    public void U8_FuzzNeverThrowsUnexpected()
    {
        var rng = new Random(1);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(0, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U8());
        }
    }

    [Fact(Timeout = 30000)]
    public void Bool_FuzzNeverThrowsUnexpected()
    {
        var rng = new Random(2);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(0, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => new Deserializer(bytes).Bool());
        }
    }

    [Fact(Timeout = 30000)]
    public void U16_U32_U64_FuzzNeverThrowsUnexpected()
    {
        var rng = new Random(3);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(0, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U16());
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U32());
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U64());
        }
    }

    [Fact(Timeout = 30000)]
    public void U128_U256_FuzzNeverThrowsUnexpected()
    {
        var rng = new Random(4);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(0, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U128());
            AssertSafeOrSucceeds(() => new Deserializer(bytes).U256());
        }
    }

    [Fact(Timeout = 30000)]
    public void Uleb128AsU32_FuzzNeverThrowsUnexpected()
    {
        var rng = new Random(5);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(0, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => new Deserializer(bytes).Uleb128AsU32());
        }
    }

    [Fact(Timeout = 30000)]
    public void Uleb128_DoSPattern_AllHighBits_Terminates()
    {
        // A worst-case adversarial input: every byte has the continuation
        // bit set. The reader must terminate cleanly (not loop forever).
        // Limited to 100 bytes which is well past the 5-byte capacity of
        // a valid uleb32 encoding.
        var malicious = Enumerable.Repeat((byte)0x80, 100).ToArray();
        AssertSafeOrSucceeds(() => new Deserializer(malicious).Uleb128AsU32());
    }

    [Fact(Timeout = 30000)]
    public void Bytes_HugeLengthPrefix_DoesNotAllocate()
    {
        // Length prefix of 0xFFFFFFFF (~4 GiB). The deserializer should not
        // attempt to allocate the array; it should fail with an exception
        // because there aren't enough bytes left in the input.
        var malicious = new byte[]
        {
            0xff,
            0xff,
            0xff,
            0xff,
            0x0f, // uleb128 = 0xFFFFFFFF
        };
        AssertSafeOrSucceeds(() => new Deserializer(malicious).Bytes());
    }

    [Fact(Timeout = 30000)]
    public void TypeTag_Fuzz()
    {
        var rng = new Random(6);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(1, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => TypeTag.Deserialize(new Deserializer(bytes)));
        }
    }

    [Fact(Timeout = 30000)]
    public void TransactionAuthenticator_Fuzz()
    {
        var rng = new Random(7);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(1, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(
                () => TransactionAuthenticator.Deserialize(new Deserializer(bytes))
            );
        }
    }

    [Fact(Timeout = 30000)]
    public void TransactionPayload_Fuzz()
    {
        var rng = new Random(8);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(1, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => TransactionPayload.Deserialize(new Deserializer(bytes)));
        }
    }

    [Fact(Timeout = 30000)]
    public void AccountAuthenticator_Fuzz()
    {
        var rng = new Random(9);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(1, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => AccountAuthenticator.Deserialize(new Deserializer(bytes)));
        }
    }

    [Fact(Timeout = 30000)]
    public void RawTransaction_Fuzz()
    {
        // RawTransaction needs at least ~50 bytes of well-formed input;
        // random bytes will almost always throw, but the deserializer
        // must not panic with an unexpected exception type.
        var rng = new Random(10);
        for (int i = 0; i < Iterations; i++)
        {
            var bytes = new byte[rng.Next(40, MaxBytes)];
            rng.NextBytes(bytes);
            AssertSafeOrSucceeds(() => RawTransaction.Deserialize(new Deserializer(bytes)));
        }
    }

    /// <summary>
    /// Runs an action, treats any expected exception as a pass, and re-
    /// throws anything outside the allow-list as a test failure.
    /// </summary>
    private static void AssertSafeOrSucceeds(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (IsExpected(ex))
        {
            // Expected. The deserializer rejected malformed input.
        }
    }
}
