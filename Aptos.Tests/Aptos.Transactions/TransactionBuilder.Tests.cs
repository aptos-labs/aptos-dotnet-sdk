namespace Aptos.Tests.Transactions;

public class TransactionBuilderTests(ITestOutputHelper output) : BaseTests(output)
{
    private readonly AptosClient _client = new(
        new AptosConfig(
            new NetworkConfig(
                "test",
                "https://test.example.com/v1",
                "https://test.example.com/v1/graphql",
                null,
                null,
                null,
                42
            )
        )
    );

    private readonly AccountAddress _sender = AccountAddress.FromString("0x1");

    private const ulong MaxGasAmount = 100000;
    private const ulong GasUnitPrice = 100;
    private const ulong ExpireTimestamp = 1234567890;
    private const ulong SequenceNumber = 0;
    private const int ChainId = 42;

    private static TransactionEntryFunctionPayload CreateTestPayload() =>
        new(new EntryFunction(new ModuleId(AccountAddress.ZERO, "test"), "func", [], []));

    private static TransactionBuilder.GenerateTransactionOptions CreateOptions(
        TransactionExtraConfig? extraConfig = null
    ) =>
        new(
            maxGasAmount: MaxGasAmount,
            gasUnitPrice: GasUnitPrice,
            expireTimestamp: ExpireTimestamp,
            accountSequenceNumber: SequenceNumber,
            extraConfig: extraConfig
        );

    [Fact(Timeout = 10000)]
    public async Task GenerateRawTransaction_WithExtraConfig_CreatesTransactionInnerPayload()
    {
        var extraConfig = new TransactionExtraConfigV1(
            multiSigAddress: null,
            replayProtectionNonce: 12345
        );

        var rawTransaction = await TransactionBuilder.GenerateRawTransaction(
            _client,
            _sender,
            CreateTestPayload(),
            null,
            CreateOptions(extraConfig)
        );

        Assert.IsType<TransactionInnerPayload>(rawTransaction.Payload);

        var innerPayload = (TransactionInnerPayload)rawTransaction.Payload;
        Assert.IsType<InnerTransactionPayloadV1>(innerPayload.InnerPayload);

        var payloadV1 = (InnerTransactionPayloadV1)innerPayload.InnerPayload;
        Assert.IsType<TransactionExtraConfigV1>(payloadV1.ExtraConfig);

        var configV1 = (TransactionExtraConfigV1)payloadV1.ExtraConfig;
        Assert.Null(configV1.MultiSigAddress);
        Assert.Equal(12345UL, configV1.ReplayProtectionNonce);

        Assert.Equal(_sender, rawTransaction.Sender);
        Assert.Equal(SequenceNumber, rawTransaction.SequenceNumber);
        Assert.Equal(MaxGasAmount, rawTransaction.MaxGasAmount);
        Assert.Equal(GasUnitPrice, rawTransaction.GasUnitPrice);
        Assert.Equal(ExpireTimestamp, rawTransaction.ExpirationTimestampSecs);
        Assert.Equal(ChainId, rawTransaction.ChainId.Value);
    }

    [Fact(Timeout = 10000)]
    public async Task GenerateRawTransaction_WithoutExtraConfig_CreatesRegularPayload()
    {
        var rawTransaction = await TransactionBuilder.GenerateRawTransaction(
            _client,
            _sender,
            CreateTestPayload(),
            null,
            CreateOptions()
        );

        Assert.IsType<TransactionEntryFunctionPayload>(rawTransaction.Payload);

        Assert.Equal(_sender, rawTransaction.Sender);
        Assert.Equal(SequenceNumber, rawTransaction.SequenceNumber);
        Assert.Equal(MaxGasAmount, rawTransaction.MaxGasAmount);
        Assert.Equal(GasUnitPrice, rawTransaction.GasUnitPrice);
        Assert.Equal(ExpireTimestamp, rawTransaction.ExpirationTimestampSecs);
        Assert.Equal(ChainId, rawTransaction.ChainId.Value);
    }
}
