namespace Aptos.Tests.Transactions;

public class MultiAgentTransactionTests(ITestOutputHelper output) : BaseTests(output)
{
    private static RawTransaction MakeRaw() =>
        new(
            sender: AccountAddress.ZERO,
            sequenceNumber: 0,
            payload: new TransactionEntryFunctionPayload(
                new EntryFunction(new ModuleId(AccountAddress.ZERO, "m"), "f", [], [])
            ),
            maxGasAmount: 1000,
            gasUnitPrice: 100,
            expirationTimestampSecs: 1000,
            chainId: new ChainId(4)
        );

    [Fact]
    public void MultiAgentTransaction_RoundTrip_NoFeePayer()
    {
        var sec = AccountAddress.FromString("0x1", 63);
        var txn = new MultiAgentTransaction(MakeRaw(), [sec]);
        var bytes = txn.BcsToBytes();
        var d = MultiAgentTransaction.Deserialize(new Deserializer(bytes));
        Assert.Null(d.FeePayerAddress);
        Assert.Single(d.SecondarySignerAddresses);
        Assert.Equal(sec, d.SecondarySignerAddresses[0]);
    }

    [Fact]
    public void MultiAgentTransaction_RoundTrip_WithFeePayer()
    {
        var sec = AccountAddress.FromString("0x1", 63);
        var fp = AccountAddress.FromString("0x2", 63);
        var txn = new MultiAgentTransaction(MakeRaw(), [sec], fp);
        var bytes = txn.BcsToBytes();
        var d = MultiAgentTransaction.Deserialize(new Deserializer(bytes));
        Assert.Equal(fp, d.FeePayerAddress);
        Assert.Single(d.SecondarySignerAddresses);
    }

    [Fact]
    public void MultiAgentRawTransaction_RoundTripViaBase()
    {
        var sec = AccountAddress.FromString("0x1", 63);
        var txn = new MultiAgentRawTransaction(MakeRaw(), [sec]);
        var bytes = txn.BcsToBytes();
        // Round-trip via the base dispatcher so the variant byte is consumed.
        var d = (MultiAgentRawTransaction)
            RawTransactionWithData.Deserialize(new Deserializer(bytes));
        Assert.Single(d.SecondarySignerAddresses);
        Assert.Equal(sec, d.SecondarySignerAddresses[0]);
    }
}

public class GeneratePayloadDataTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void GenerateEntryFunctionPayloadData_StoresFields()
    {
        var payload = new GenerateEntryFunctionPayloadData(
            function: "0x1::aptos_account::transfer_coins",
            functionArguments: [AccountAddress.ZERO, "100"],
            typeArguments: ["0x1::aptos_coin::AptosCoin"]
        );
        Assert.Equal("0x1::aptos_account::transfer_coins", payload.Function);
        Assert.Equal(2, payload.FunctionArguments!.Count);
        Assert.Single(payload.TypeArguments!);
        Assert.Null(payload.Abi);
    }

    [Fact]
    public void GenerateEntryFunctionPayloadData_CopyConstructor()
    {
        var payload = new GenerateEntryFunctionPayloadData(
            function: "0x1::a::b",
            functionArguments: [1],
            typeArguments: []
        );
        var copy = new GenerateEntryFunctionPayloadData(payload);
        Assert.Equal(payload.Function, copy.Function);
        Assert.Same(payload.FunctionArguments, copy.FunctionArguments);
    }

    [Fact]
    public void GenerateViewFunctionPayloadData_StoresFields()
    {
        var data = new GenerateViewFunctionPayloadData(
            function: "0x1::coin::name",
            functionArguments: [],
            typeArguments: ["0x1::aptos_coin::AptosCoin"]
        );
        Assert.Equal("0x1::coin::name", data.Function);
        Assert.Empty(data.FunctionArguments!);
    }

    [Fact]
    public void GenerateMultisigPayloadData_StoresMultisigAddress()
    {
        var data = new GenerateMultisigPayloadData(
            function: "0x1::a::b",
            multisigAddress: AccountAddress.FromString("0x1", 63),
            functionArguments: []
        );
        Assert.Equal(AccountAddress.FromString("0x1", 63), data.MultisigAddress);
    }

    [Fact]
    public void GenerateMultisigPayloadData_StringAddressOverload()
    {
        var data = new GenerateMultisigPayloadData(
            function: "0x1::a::b",
            multisigAddress: "0x" + new string('1', 64)
        );
        Assert.Equal(32, data.MultisigAddress.Data.Length);
    }

    [Fact]
    public void GenerateScriptPayloadData_StoresBytecode()
    {
        var data = new GenerateScriptPayloadData(
            bytecode: new byte[] { 0xa1, 0xb2 },
            functionArguments: [new U8(7)]
        );
        Assert.Equal(2, data.Bytecode.Length);
        Assert.Single(data.FunctionArguments!);
    }

    [Fact]
    public void SimulateTransactionOptions_HasDefaults()
    {
        var opts = new SimulateTransactionOptions();
        Assert.False(opts.EstimateGasUnitPrice);
        Assert.False(opts.EstimateMaxGasAmount);
        Assert.False(opts.EstimatePrioritizedGasUnitPrice);

        var opts2 = new SimulateTransactionOptions(true, true, true);
        Assert.True(opts2.EstimateGasUnitPrice);
    }

    [Fact]
    public void SubmitTransactionData_StoresFields()
    {
        var raw = new SimpleTransaction(
            new RawTransaction(
                AccountAddress.ZERO,
                0,
                new TransactionEntryFunctionPayload(
                    new EntryFunction(new ModuleId(AccountAddress.ZERO, "m"), "f", [], [])
                ),
                100,
                100,
                100,
                new ChainId(4)
            )
        );
        var account = Ed25519Account.Generate();
        var auth = account.SignWithAuthenticator(new byte[] { 1 });
        var data = new SubmitTransactionData(raw, auth);
        Assert.Same(raw, data.Transaction);
        Assert.Same(auth, data.SenderAuthenticator);
        Assert.Null(data.FeePayerAuthenticator);
        Assert.Null(data.AdditionalSignersAuthenticators);
    }
}

public class ExceptionsTests
{
    [Fact]
    public void WaitForTransactionException_StoresLastResponse()
    {
        var ex = new Aptos.Exceptions.WaitForTransactionException("msg");
        Assert.Equal("msg", ex.Message);
        Assert.Null(ex.LastResponse);
    }

    [Fact]
    public void FailedTransactionException_StoresTransaction()
    {
        var ex = new Aptos.Exceptions.FailedTransactionException("oops");
        Assert.Equal("oops", ex.Message);
        Assert.Null(ex.Transaction);
    }

    [Fact]
    public void ConfigException_StoresMessage()
    {
        var ex = new Aptos.Exceptions.ConfigException("config bad");
        Assert.Equal("config bad", ex.Message);
    }

    [Fact]
    public void FaucetException_StoresMessage()
    {
        var ex = new Aptos.Exceptions.FaucetException("faucet broken");
        Assert.Equal("faucet broken", ex.Message);
    }
}
