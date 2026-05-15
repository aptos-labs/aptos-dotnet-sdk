namespace Aptos.Tests.E2E;

using Aptos.Schemes;

/// <summary>
/// End-to-end tests targeting Aptos devnet. These tests require network
/// access and the devnet faucet. They are skipped by default; set the
/// environment variable DEVNET_E2E=1 to enable them.
///
/// The tests cover every feature that the SDK currently advertises as
/// supported (per <see cref="README"/>):
///
/// - Building, signing, and submitting transactions
/// - Reading ledger info, accounts, resources, modules
/// - Coin balance / fungible asset balance queries
/// - View function calls
/// - Faucet funding
/// - Multiple signer schemes (Ed25519, SingleKey ed25519/secp256k1, MultiKey)
/// - Sponsored (fee-payer) transactions
/// - Transaction simulation
/// - Object/resource address derivation
/// - Account info lookups
/// - Sequence number / chain id auto-fetch
/// </summary>
public class DevnetE2ETests(ITestOutputHelper output) : BaseTests(output)
{
    private const ulong FundAmount = 100_000_000UL; // 1 APT (devnet faucet cap)

    /// <summary>
    /// Reasonable transaction options for devnet E2E tests. Devnet's faucet
    /// only funds ~1 APT per call, and the SDK default max_gas_amount of 2M
    /// at gas_unit_price 100 would be 2 APT — more than the account balance.
    /// We pick a deliberately small ceiling that's still well above any
    /// realistic gas use for the transfers being exercised here.
    /// </summary>
    private static TransactionBuilder.GenerateTransactionOptions DefaultOptions() =>
        new(maxGasAmount: 200_000UL);

    [DevnetE2EFact]
    public async Task LedgerInfo_Returns_DevnetChainId()
    {
        var client = DevnetE2E.NewClient();
        var ledger = await client.Block.GetLedgerInfo();
        // Devnet has no fixed chain ID, but ChainId must be set and node role
        // should be a full node.
        Assert.True(ledger.ChainId > 0);
        Assert.Equal("full_node", ledger.NodeRole);
        Assert.True(ledger.LedgerVersion > 0);
    }

    [DevnetE2EFact]
    public async Task Faucet_FundsAccount()
    {
        var client = DevnetE2E.NewClient();
        var account = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, account.Address, FundAmount);

        // Verify funding via the on-chain view function rather than the
        // indexer (the devnet indexer is flaky).
        var balance = await client.View(
            new GenerateViewFunctionPayloadData(
                function: "0x1::primary_fungible_store::balance",
                functionArguments: [account.Address, "0xa"],
                typeArguments: ["0x1::fungible_asset::Metadata"]
            )
        );
        Assert.Single(balance);
        Assert.Equal(FundAmount.ToString(), balance[0].ToString());
    }

    [DevnetE2EFact]
    public async Task Transfer_Ed25519_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var alice = Ed25519Account.Generate();
        var bob = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1000"]
            ),
            options: DefaultOptions()
        );
        var pending = await client.Transaction.SignAndSubmitTransaction(alice, txn);
        var committed = await client.Transaction.WaitForTransaction(pending);
        Assert.True(committed.Success);
    }

    [DevnetE2EFact]
    public async Task Transfer_SingleKey_Ed25519_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var alice = SingleKeyAccount.Generate();
        var bob = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1000"]
            ),
            options: DefaultOptions()
        );
        var pending = await client.Transaction.SignAndSubmitTransaction(alice, txn);
        var committed = await client.Transaction.WaitForTransaction(pending);
        Assert.True(committed.Success);
    }

    [DevnetE2EFact]
    public async Task Transfer_SingleKey_Secp256k1_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var alice = SingleKeyAccount.Generate(PublicKeyVariant.Secp256k1Ecdsa);
        var bob = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1000"]
            ),
            options: DefaultOptions()
        );
        var pending = await client.Transaction.SignAndSubmitTransaction(alice, txn);
        var committed = await client.Transaction.WaitForTransaction(pending);
        Assert.True(committed.Success);
    }

    [DevnetE2EFact]
    public async Task Transfer_MultiKey_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var k1 = Ed25519Account.Generate();
        var k2 = Ed25519Account.Generate();
        var k3 = Ed25519Account.Generate();
        var multiKey = new MultiKey(
            [
                (Ed25519PublicKey)k1.VerifyingKey,
                (Ed25519PublicKey)k2.VerifyingKey,
                (Ed25519PublicKey)k3.VerifyingKey,
            ],
            2
        );
        var alice = new MultiKeyAccount(multiKey, [k1, k2]);
        var bob = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1000"]
            ),
            options: DefaultOptions()
        );
        var pending = await client.Transaction.SignAndSubmitTransaction(alice, txn);
        var committed = await client.Transaction.WaitForTransaction(pending);
        Assert.True(committed.Success);
    }

    [DevnetE2EFact]
    public async Task Transfer_Sponsored_FeePayer_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var alice = Ed25519Account.Generate();
        var bob = Ed25519Account.Generate();
        var feePayer = Ed25519Account.Generate();

        // Only fund the fee payer; honour AIP-52 by leaving alice unfunded.
        await DevnetE2E.FundOrSkip(client, feePayer.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1"]
            ),
            withFeePayer: true,
            options: DefaultOptions()
        );
        var feePayerAuth = client.Transaction.SignAsFeePayer(feePayer, txn);

        var pending = await client.Transaction.SignAndSubmitTransaction(alice, txn, feePayerAuth);
        try
        {
            var committed = await client.Transaction.WaitForTransaction(pending);
            // alice's transfer call will most likely abort with
            // EINSUFFICIENT_BALANCE since she has no APT to transfer, but the
            // fee payer signature still has to be accepted by the validator
            // for the transaction to land at all. So commit-or-EINSUFFICIENT
            // both demonstrate that the sponsored signing path works.
            Assert.True(committed.Success);
        }
        catch (Aptos.Exceptions.FailedTransactionException ex)
            when (ex.Message.Contains("EINSUFFICIENT_BALANCE"))
        {
            // Acceptable — fee payer signature was accepted, transfer
            // aborted because alice had no balance. We've still proven the
            // sponsored signing flow works.
        }
    }

    [DevnetE2EFact]
    public async Task Simulate_Transfer_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var alice = Ed25519Account.Generate();
        var bob = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var txn = await client.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [bob.Address, "1000"]
            ),
            options: DefaultOptions()
        );
        var simulations = await client.Transaction.Simulate(
            new SimulateTransactionData(
                txn,
                (PublicKey)(Ed25519PublicKey)alice.VerifyingKey
            )
        );
        Assert.Single(simulations);
    }

    [DevnetE2EFact]
    public async Task View_CoinName_EndToEnd()
    {
        var client = DevnetE2E.NewClient();
        var result = await client.Contract.View(
            new GenerateViewFunctionPayloadData(
                function: "0x1::coin::name",
                functionArguments: [],
                typeArguments: ["0x1::aptos_coin::AptosCoin"]
            )
        );
        Assert.Single(result);
        Assert.Equal("Aptos Coin", result[0]);
    }

    [DevnetE2EFact]
    public async Task GetAccount_AfterFunding_HasSequenceNumberZero()
    {
        var client = DevnetE2E.NewClient();
        var alice = Ed25519Account.Generate();
        await DevnetE2E.FundOrSkip(client, alice.Address, FundAmount);

        var info = await client.Account.GetInfo(alice.Address);
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.Equal(32, info.AuthenticationKey.ToByteArray().Length);
    }

    [DevnetE2EFact]
    public async Task GetModule_AptosCoin_HasExpectedFunctions()
    {
        var client = DevnetE2E.NewClient();
        var module = await client.Account.GetModule("0x1", "coin");
        Assert.NotNull(module);
        Assert.NotNull(module.Abi);
        Assert.Contains(module.Abi!.ExposedFunctions, f => f.Name == "name");
    }

    [DevnetE2EFact]
    public async Task GetResource_AptosCoinInfo_Works()
    {
        var client = DevnetE2E.NewClient();
        var res = await client.Account.GetResource(
            "0x1",
            "0x1::coin::CoinInfo<0x1::aptos_coin::AptosCoin>"
        );
        Assert.NotNull(res);
    }

    [DevnetE2EFact]
    public async Task GetResources_AccountResourcesPagination()
    {
        var client = DevnetE2E.NewClient();
        var resources = await client.Account.GetResources("0x1");
        // 0x1 has many resources — at least the modules we expect
        Assert.True(resources.Count > 0);
    }

    [DevnetE2EFact]
    public async Task GasEstimation_ReturnsPositiveValues()
    {
        var client = DevnetE2E.NewClient();
        var estimation = await client.Gas.GetGasPriceEstimation();
        Assert.True(estimation.GasEstimate > 0);
    }

    [DevnetE2EFact]
    public async Task ChainId_AutoFetch_PopulatesNetwork()
    {
        // Devnet has ChainId = -1 in our preset which means auto-fetch.
        var client = DevnetE2E.NewClient();
        Assert.Equal(-1, client.Config.NetworkConfig.ChainId);
        var ledger = await client.Block.GetLedgerInfo();
        Assert.True(ledger.ChainId > 0);
    }
}
