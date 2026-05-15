namespace Aptos.Tests.Api;

using Newtonsoft.Json;

/// <summary>
/// Tests that JSON-serialised API response payloads deserialize correctly
/// into the SDK's response model classes. These use representative samples
/// of real Aptos fullnode responses but do not require network access.
/// </summary>
public class ApiModelTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void AccountData_DeserializesFromJson()
    {
        const string json = """
        {
            "sequence_number": "42",
            "authentication_key": "0xabcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789"
        }
        """;
        var data = JsonConvert.DeserializeObject<AccountData>(json);
        Assert.NotNull(data);
        Assert.Equal(42UL, data!.SequenceNumber);
        Assert.Equal(
            "0xabcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
            data.AuthenticationKey.ToString()
        );
    }

    [Fact]
    public void GasEstimation_DeserializesFromJson()
    {
        const string json = """
        {
            "gas_estimate": 100,
            "deprioritized_gas_estimate": 50,
            "prioritized_gas_estimate": 200
        }
        """;
        var data = JsonConvert.DeserializeObject<GasEstimation>(json);
        Assert.NotNull(data);
        Assert.Equal(100UL, data!.GasEstimate);
        Assert.Equal(50UL, data.DeprioritizedGasEstimate);
        Assert.Equal(200UL, data.PrioritizedGasEstimate);
    }

    [Fact]
    public void LedgerInfo_DeserializesFromJson()
    {
        const string json = """
        {
            "chain_id": 1,
            "epoch": "100",
            "ledger_version": "12345",
            "oldest_ledger_version": "0",
            "ledger_timestamp": "1234567890",
            "node_role": "full_node",
            "oldest_block_height": "0",
            "block_height": "100",
            "git_hash": "abc"
        }
        """;
        var data = JsonConvert.DeserializeObject<LedgerInfo>(json);
        Assert.NotNull(data);
        Assert.Equal(1, data!.ChainId);
        Assert.Equal("full_node", data.NodeRole);
        Assert.Equal(100UL, data.Epoch);
        Assert.Equal(0UL, data.OldestblockHeight);
    }

    [Fact]
    public void PendingTransactionResponse_DeserializesFromJson()
    {
        const string json = """
        {
            "type": "pending_transaction",
            "hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "sender": "0x0000000000000000000000000000000000000000000000000000000000000001",
            "sequence_number": "0",
            "max_gas_amount": "1000",
            "gas_unit_price": "100",
            "expiration_timestamp_secs": "1234567890",
            "payload": {
                "type": "entry_function_payload",
                "function": "0x1::aptos_account::transfer",
                "type_arguments": [],
                "arguments": []
            }
        }
        """;
        var data = JsonConvert.DeserializeObject<TransactionResponse>(json);
        Assert.NotNull(data);
        Assert.Equal(TransactionResponseType.Pending, data!.Type);
        Assert.IsType<PendingTransactionResponse>(data);
        var pending = (PendingTransactionResponse)data;
        Assert.Equal(AccountAddress.FromStringStrict(
            "0x0000000000000000000000000000000000000000000000000000000000000001"
        ), pending.Sender);
    }

    [Fact]
    public void UserTransactionResponse_DeserializesFromJson()
    {
        const string json = """
        {
            "type": "user_transaction",
            "hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "version": "5000",
            "state_change_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "event_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "gas_used": "100",
            "success": true,
            "vm_status": "Executed successfully",
            "accumulator_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "state_checkpoint_hash": null,
            "sender": "0x0000000000000000000000000000000000000000000000000000000000000001",
            "sequence_number": "0",
            "max_gas_amount": "1000",
            "gas_unit_price": "100",
            "expiration_timestamp_secs": "1234567890",
            "changes": [],
            "payload": {
                "type": "entry_function_payload",
                "function": "0x1::aptos_account::transfer",
                "type_arguments": [],
                "arguments": []
            },
            "events": [],
            "timestamp": "1234567890"
        }
        """;
        var data = JsonConvert.DeserializeObject<TransactionResponse>(json);
        Assert.IsType<UserTransactionResponse>(data);
        var user = (UserTransactionResponse)data!;
        Assert.True(user.Success);
        Assert.Equal(100UL, user.GasUsed);
        Assert.Equal(5000UL, user.Version);
    }

    [Fact]
    public void TransactionResponse_StateCheckpointAndBlockEpilogue_DeserializeFromJson()
    {
        const string stateCheckpoint = """
        {
            "type": "state_checkpoint_transaction",
            "hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "version": "1",
            "state_change_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "event_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "gas_used": "0",
            "success": true,
            "vm_status": "ok",
            "accumulator_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "changes": [],
            "state_checkpoint_hash": "0x00",
            "timestamp": "1"
        }
        """;
        var sc = (StateCheckpointTransactionResponse)
            JsonConvert.DeserializeObject<TransactionResponse>(stateCheckpoint)!;
        Assert.Equal(TransactionResponseType.StateCheckpoint, sc.Type);
        Assert.Equal(1UL, sc.Timestamp);

        const string blockEpilogue = """
        {
            "type": "block_epilogue_transaction",
            "hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "version": "1",
            "state_change_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "event_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "gas_used": "0",
            "success": true,
            "vm_status": "ok",
            "accumulator_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "changes": [],
            "state_checkpoint_hash": "0x00",
            "timestamp": "1",
            "block_end_info": null
        }
        """;
        var be = (BlockEpilogueTransactionResponse)
            JsonConvert.DeserializeObject<TransactionResponse>(blockEpilogue)!;
        Assert.Equal(TransactionResponseType.BlockEpilogue, be.Type);
    }

    [Fact]
    public void TransactionResponse_GenesisTransaction_DeserializeFromJson()
    {
        const string json = """
        {
            "type": "genesis_transaction",
            "hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "version": "0",
            "state_change_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "event_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "gas_used": "0",
            "success": true,
            "vm_status": "ok",
            "accumulator_root_hash": "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            "changes": [],
            "state_checkpoint_hash": "0x00",
            "payload": {
                "type": "write_set_payload",
                "write_set": {
                    "type": "direct_write_set",
                    "changes": [],
                    "events": []
                }
            },
            "events": []
        }
        """;
        var data = JsonConvert.DeserializeObject<TransactionResponse>(json);
        Assert.IsType<GenesisTransactionResponse>(data);
    }
}
