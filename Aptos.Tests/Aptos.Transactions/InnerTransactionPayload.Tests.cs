namespace Aptos.Tests.Transactions;

public class InnerTransactionPayloadTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact(Timeout = 10000)]
    public void SerializeInnerTransactionPayloadV1()
    {
        var testEntryFunction = new EntryFunction(
            new ModuleId(AccountAddress.ZERO, "test"),
            "func",
            [],
            []
        );

        var executable = new TransactionEntryFunctionExecutable(testEntryFunction);
        var extraConfig = new TransactionExtraConfigV1(
            multiSigAddress: null,
            replayProtectionNonce: 12345
        );

        var payload = new InnerTransactionPayloadV1(executable, extraConfig);

        var expectedBytes = Hex.FromHexString(
                "0x0001000000000000000000000000000000000000000000000000000000000000000004746573740466756e6300000000013930000000000000"
            )
            .ToByteArray();

        var serializedBytes = payload.BcsToBytes();

        Assert.Equal(expectedBytes, serializedBytes);
    }

    [Fact(Timeout = 10000)]
    public void DeserializeInnerTransactionPayloadV1()
    {
        var bytes = Hex.FromHexString(
                "0x0001000000000000000000000000000000000000000000000000000000000000000004746573740466756e6300000000013930000000000000"
            )
            .ToByteArray();

        var deserializer = new Deserializer(bytes);
        var payload = InnerTransactionPayload.Deserialize(deserializer);

        Assert.IsType<InnerTransactionPayloadV1>(payload);

        var payloadV1 = (InnerTransactionPayloadV1)payload;

        Assert.IsType<TransactionEntryFunctionExecutable>(payloadV1.Executable);
        var executable = (TransactionEntryFunctionExecutable)payloadV1.Executable;

        Assert.Equal(AccountAddress.ZERO, executable.Function.ModuleName.Address);
        Assert.Equal("test", executable.Function.ModuleName.Name);
        Assert.Equal("func", executable.Function.FunctionName);
        Assert.Empty(executable.Function.TypeArgs);
        Assert.Empty(executable.Function.Args);

        Assert.IsType<TransactionExtraConfigV1>(payloadV1.ExtraConfig);
        var extraConfig = (TransactionExtraConfigV1)payloadV1.ExtraConfig;

        Assert.Null(extraConfig.MultiSigAddress);
        Assert.Equal(12345UL, extraConfig.ReplayProtectionNonce);
    }

    [Fact(Timeout = 10000)]
    public void SerializeAndDeserializeRoundTrip()
    {
        var testEntryFunction = new EntryFunction(
            new ModuleId(AccountAddress.ZERO, "test"),
            "func",
            [],
            []
        );

        var executable = new TransactionEntryFunctionExecutable(testEntryFunction);
        var extraConfig = new TransactionExtraConfigV1(
            multiSigAddress: null,
            replayProtectionNonce: 12345
        );

        var originalPayload = new InnerTransactionPayloadV1(executable, extraConfig);

        var serializedBytes = originalPayload.BcsToBytes();
        var deserializer = new Deserializer(serializedBytes);
        var deserializedPayload = InnerTransactionPayload.Deserialize(deserializer);

        Assert.IsType<InnerTransactionPayloadV1>(deserializedPayload);

        var deserializedPayloadV1 = (InnerTransactionPayloadV1)deserializedPayload;

        Assert.IsType<TransactionEntryFunctionExecutable>(deserializedPayloadV1.Executable);
        var deserializedExecutable = (TransactionEntryFunctionExecutable)
            deserializedPayloadV1.Executable;

        Assert.Equal(AccountAddress.ZERO, deserializedExecutable.Function.ModuleName.Address);
        Assert.Equal("test", deserializedExecutable.Function.ModuleName.Name);
        Assert.Equal("func", deserializedExecutable.Function.FunctionName);
        Assert.Empty(deserializedExecutable.Function.TypeArgs);
        Assert.Empty(deserializedExecutable.Function.Args);

        Assert.IsType<TransactionExtraConfigV1>(deserializedPayloadV1.ExtraConfig);
        var deserializedExtraConfig = (TransactionExtraConfigV1)deserializedPayloadV1.ExtraConfig;

        Assert.Null(deserializedExtraConfig.MultiSigAddress);
        Assert.Equal(12345UL, deserializedExtraConfig.ReplayProtectionNonce);
    }

    [Fact(Timeout = 10000)]
    public void SerializeInnerTransactionPayloadV1WithMultiSigAddress()
    {
        var testEntryFunction = new EntryFunction(
            new ModuleId(AccountAddress.FromString("0x1"), "module"),
            "transfer",
            [],
            []
        );

        var executable = new TransactionEntryFunctionExecutable(testEntryFunction);
        var multiSigAddress = AccountAddress.FromString(
            "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
        );
        var extraConfig = new TransactionExtraConfigV1(
            multiSigAddress: multiSigAddress,
            replayProtectionNonce: 999
        );

        var payload = new InnerTransactionPayloadV1(executable, extraConfig);

        var serializedBytes = payload.BcsToBytes();

        var deserializer = new Deserializer(serializedBytes);
        var deserializedPayload = InnerTransactionPayload.Deserialize(deserializer);

        Assert.IsType<InnerTransactionPayloadV1>(deserializedPayload);
        var payloadV1 = (InnerTransactionPayloadV1)deserializedPayload;

        Assert.IsType<TransactionExtraConfigV1>(payloadV1.ExtraConfig);
        var extraConfigV1 = (TransactionExtraConfigV1)payloadV1.ExtraConfig;

        Assert.NotNull(extraConfigV1.MultiSigAddress);
        Assert.Equal(multiSigAddress, extraConfigV1.MultiSigAddress);
        Assert.Equal(999UL, extraConfigV1.ReplayProtectionNonce);
    }

    [Fact(Timeout = 10000)]
    public void SerializeTransactionInnerPayloadWrapper()
    {
        var testEntryFunction = new EntryFunction(
            new ModuleId(AccountAddress.ZERO, "test"),
            "func",
            [],
            []
        );

        var executable = new TransactionEntryFunctionExecutable(testEntryFunction);
        var extraConfig = new TransactionExtraConfigV1(
            multiSigAddress: null,
            replayProtectionNonce: 12345
        );

        var innerPayload = new InnerTransactionPayloadV1(executable, extraConfig);
        var wrappedPayload = new TransactionInnerPayload(innerPayload);

        var serializedBytes = wrappedPayload.BcsToBytes();

        var expectedBytes = Hex.FromHexString(
                "0x040001000000000000000000000000000000000000000000000000000000000000000004746573740466756e6300000000013930000000000000"
            )
            .ToByteArray();

        Assert.Equal(expectedBytes, serializedBytes);
    }
}
