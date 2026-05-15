namespace Aptos.Tests.Transactions;

/// <summary>
/// Serialization round-trip tests for the Transaction type hierarchy. These do
/// not require a network and run quickly.
/// </summary>
public class TransactionRoundTripTests(ITestOutputHelper output) : BaseTests(output)
{
    private static EntryFunction TestEntryFunction() =>
        new(new ModuleId(AccountAddress.ZERO, "test"), "func", [], []);

    private static RawTransaction TestRawTransaction(
        TransactionPayload? payload = null,
        ulong sequenceNumber = 0
    ) =>
        new(
            sender: AccountAddress.ZERO,
            sequenceNumber: sequenceNumber,
            payload: payload ?? new TransactionEntryFunctionPayload(TestEntryFunction()),
            maxGasAmount: 1000,
            gasUnitPrice: 100,
            expirationTimestampSecs: 1234567890,
            chainId: new ChainId(4)
        );

    [Fact]
    public void RawTransaction_RoundTrip()
    {
        var raw = TestRawTransaction();
        var bytes = raw.BcsToBytes();
        var d = RawTransaction.Deserialize(new Deserializer(bytes));
        Assert.Equal(raw.Sender, d.Sender);
        Assert.Equal(raw.SequenceNumber, d.SequenceNumber);
        Assert.Equal(raw.MaxGasAmount, d.MaxGasAmount);
        Assert.Equal(raw.GasUnitPrice, d.GasUnitPrice);
        Assert.Equal(raw.ExpirationTimestampSecs, d.ExpirationTimestampSecs);
        Assert.Equal(raw.ChainId.Value, d.ChainId.Value);
        Assert.IsType<TransactionEntryFunctionPayload>(d.Payload);
    }

    [Fact]
    public void FeePayerRawTransaction_RoundTrip()
    {
        var raw = TestRawTransaction();
        var fee = AccountAddress.FromString("0x1", 63);
        var sec = AccountAddress.FromString("0x2", 63);
        var fp = new FeePayerRawTransaction(raw, [sec], fee);

        var bytes = fp.BcsToBytes();
        var d = (FeePayerRawTransaction)RawTransactionWithData.Deserialize(new Deserializer(bytes));
        Assert.Equal(fee, d.FeePayerAddress);
        Assert.Single(d.SecondarySignerAddresses);
        Assert.Equal(sec, d.SecondarySignerAddresses[0]);
    }

    [Fact]
    public void RawTransactionWithData_InvalidVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => RawTransactionWithData.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void TransactionEntryFunctionPayload_RoundTrip()
    {
        var payload = new TransactionEntryFunctionPayload(TestEntryFunction());
        var bytes = payload.BcsToBytes();
        var d = (TransactionEntryFunctionPayload)TransactionPayload.Deserialize(
            new Deserializer(bytes)
        );
        Assert.Equal("func", d.Function.FunctionName);
        Assert.Equal("test", d.Function.ModuleName.Name);
    }

    [Fact]
    public void TransactionScriptPayload_RoundTrip()
    {
        var script = new Script(new byte[] { 0xa1, 0xb2, 0xc3 }, [new TypeTagU8()], [new U8(7)]);
        var payload = new TransactionScriptPayload(script);
        var bytes = payload.BcsToBytes();
        var d = (TransactionScriptPayload)TransactionPayload.Deserialize(
            new Deserializer(bytes)
        );
        Assert.Equal(script.Bytecode, d.Script.Bytecode);
        Assert.Single(d.Script.TypeArgs);
        Assert.Single(d.Script.Args);
    }

    [Fact]
    public void TransactionPayload_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => TransactionPayload.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void SignedTransaction_RoundTrip()
    {
        var account = Ed25519Account.Generate();
        var raw = TestRawTransaction();
        var sig = (Ed25519Signature)account.Sign(SigningMessage.GenerateForTransaction(new SimpleTransaction(raw)));
        var signed = new SignedTransaction(
            raw,
            new TransactionAuthenticatorEd25519((Ed25519PublicKey)account.VerifyingKey, sig)
        );

        var bytes = signed.BcsToBytes();
        var d = SignedTransaction.Deserialize(new Deserializer(bytes));
        Assert.Equal(raw.MaxGasAmount, d.RawTransaction.MaxGasAmount);
        Assert.IsType<TransactionAuthenticatorEd25519>(d.Authenticator);
    }

    [Fact]
    public void SimpleTransaction_RoundTrip_NoFeePayer()
    {
        var raw = TestRawTransaction();
        var simple = new SimpleTransaction(raw);
        var bytes = simple.BcsToBytes();
        var d = SimpleTransaction.Deserialize(new Deserializer(bytes));
        Assert.Null(d.FeePayerAddress);
        Assert.Equal(raw.Sender, d.RawTransaction.Sender);
    }

    [Fact]
    public void SimpleTransaction_RoundTrip_WithFeePayer()
    {
        var raw = TestRawTransaction();
        var fp = AccountAddress.FromString("0x1", 63);
        var simple = new SimpleTransaction(raw, fp);
        var bytes = simple.BcsToBytes();
        var d = SimpleTransaction.Deserialize(new Deserializer(bytes));
        Assert.Equal(fp, d.FeePayerAddress);
    }

    [Fact]
    public void TransactionExecutable_EntryFunction_RoundTrip()
    {
        var ex = new TransactionEntryFunctionExecutable(TestEntryFunction());
        var bytes = ex.BcsToBytes();
        var d = (TransactionEntryFunctionExecutable)TransactionExecutable.Deserialize(
            new Deserializer(bytes)
        );
        Assert.Equal("func", d.Function.FunctionName);
    }

    [Fact]
    public void TransactionExecutable_Script_RoundTrip()
    {
        var ex = new TransactionScriptExecutable(
            new Script(new byte[] { 1 }, [], [])
        );
        var bytes = ex.BcsToBytes();
        var d = (TransactionScriptExecutable)TransactionExecutable.Deserialize(
            new Deserializer(bytes)
        );
        Assert.Equal(new byte[] { 1 }, d.Script.Bytecode);
    }

    [Fact]
    public void TransactionExecutable_Empty_RoundTrip()
    {
        var ex = new TransactionEmptyExecutable();
        var bytes = ex.BcsToBytes();
        var d = TransactionExecutable.Deserialize(new Deserializer(bytes));
        Assert.IsType<TransactionEmptyExecutable>(d);
    }

    [Fact]
    public void TransactionExecutable_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => TransactionExecutable.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void TransactionExtraConfig_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => TransactionExtraConfig.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void TransactionExtraConfigV1_HasReplayProtectionNonce()
    {
        var withNonce = new TransactionExtraConfigV1(replayProtectionNonce: 1);
        var withoutNonce = new TransactionExtraConfigV1();
        Assert.True(withNonce.HasReplayProtectionNonce());
        Assert.False(withoutNonce.HasReplayProtectionNonce());
    }

    [Fact]
    public void InnerTransactionPayload_UnknownVariant_Throws()
    {
        var s = new Serializer();
        s.U32AsUleb128(99);
        Assert.Throws<ArgumentException>(
            () => InnerTransactionPayload.Deserialize(new Deserializer(s.ToBytes()))
        );
    }

    [Fact]
    public void InnerTransactionPayload_FromLegacy_HandlesAllVariants()
    {
        var entryPayload = new TransactionEntryFunctionPayload(TestEntryFunction());
        var scriptPayload = new TransactionScriptPayload(
            new Script(new byte[] { 1 }, [], [])
        );
        var extra = new TransactionExtraConfigV1(replayProtectionNonce: 99);

        var fromEntry = (InnerTransactionPayloadV1)InnerTransactionPayload.FromLegacy(
            entryPayload,
            extra
        );
        var fromScript = (InnerTransactionPayloadV1)InnerTransactionPayload.FromLegacy(
            scriptPayload,
            extra
        );

        Assert.IsType<TransactionEntryFunctionExecutable>(fromEntry.Executable);
        Assert.IsType<TransactionScriptExecutable>(fromScript.Executable);

        // Wrapping an already-inner payload preserves the inner payload.
        var wrapped = new TransactionInnerPayload(fromEntry);
        var rewrapped = InnerTransactionPayload.FromLegacy(wrapped, extra);
        Assert.Same(fromEntry, rewrapped);
    }

    [Fact]
    public void ModuleId_FromString_AndRoundTrip()
    {
        var moduleId = ModuleId.FromString("0x1::aptos_account");
        Assert.Equal("aptos_account", moduleId.Name);

        var bytes = moduleId.BcsToBytes();
        var d = ModuleId.Deserialize(new Deserializer(bytes));
        Assert.Equal(moduleId.Name, d.Name);
        Assert.Equal(moduleId.Address, d.Address);

        Assert.Throws<ArgumentException>(() => ModuleId.FromString("0x1::a::b"));
        Assert.Throws<ArgumentException>(() => ModuleId.FromString("invalid"));
    }

    [Fact]
    public void StructTag_RoundTrip()
    {
        var tag = new StructTag(
            AccountAddress.FromString("0x1", 63),
            "aptos_coin",
            "AptosCoin",
            []
        );
        var bytes = tag.BcsToBytes();
        var d = StructTag.Deserialize(new Deserializer(bytes));
        Assert.Equal("aptos_coin", d.ModuleName);
        Assert.Equal("AptosCoin", d.Name);
    }

    [Fact]
    public void StructTag_Constants_AreWellKnown()
    {
        Assert.Equal("string", StructTag.STRING.ModuleName);
        Assert.Equal("String", StructTag.STRING.Name);
        Assert.Equal("option", StructTag.OPTION.ModuleName);
        Assert.Equal("object", StructTag.OBJECT.ModuleName);
        Assert.Equal("tag", StructTag.TAG.ModuleName);
    }

    [Fact]
    public void StructTag_CopyConstructor_WithTypeArgs()
    {
        var newArgs = new List<TypeTag> { new TypeTagU8() };
        var withArgs = new StructTag(StructTag.STRING, newArgs);
        Assert.Single(withArgs.TypeArgs);
    }

    [Fact]
    public void EntryFunction_RoundTrip()
    {
        var fn = new EntryFunction(
            new ModuleId(AccountAddress.FromString("0x1", 63), "coin"),
            "transfer",
            [new TypeTagU64()],
            [new U64(123)]
        );
        var bytes = fn.BcsToBytes();
        var d = EntryFunction.Deserialize(new Deserializer(bytes));
        Assert.Equal("transfer", d.FunctionName);
        Assert.Single(d.TypeArgs);
        Assert.Single(d.Args);
        // Args are deserialised as bytes-with-length wrappers; the original
        // bytes content should match U64(123).BcsToBytes().
        Assert.IsType<EntryFunctionBytes>(d.Args[0]);
    }

    [Fact]
    public void EntryFunction_Build_FromString()
    {
        var fn = EntryFunction.Build("0x1::coin", "name", [new TypeTagU8()], []);
        Assert.Equal("name", fn.FunctionName);
        Assert.Equal("coin", fn.ModuleName.Name);
    }

    [Fact]
    public void TransactionInnerPayload_RoundTrip()
    {
        var executable = new TransactionEntryFunctionExecutable(TestEntryFunction());
        var extra = new TransactionExtraConfigV1(replayProtectionNonce: 7);
        var inner = new InnerTransactionPayloadV1(executable, extra);
        var wrapped = new TransactionInnerPayload(inner);

        var bytes = wrapped.BcsToBytes();
        var d = (TransactionInnerPayload)TransactionPayload.Deserialize(new Deserializer(bytes));
        Assert.IsType<InnerTransactionPayloadV1>(d.InnerPayload);
    }
}
