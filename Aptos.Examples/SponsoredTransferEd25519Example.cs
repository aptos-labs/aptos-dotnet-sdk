using Newtonsoft.Json;

namespace Aptos.Examples;

public class SponsoredTransferEd25519Example
{
    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));

        Console.WriteLine("=== Addresses ===\n");

        var alice = Ed25519Account.Generate();
        var receiver = Ed25519Account.Generate();
        var feePayer = Ed25519Account.Generate();
        Console.WriteLine($"Alice: {alice.Address}");
        Console.WriteLine($"Receiver: {receiver.Address}");
        Console.WriteLine($"FeePayer: {feePayer.Address}");

        Console.WriteLine("\n=== Funding accounts ===\n");

        var aliceFundTxn = await aptos.Faucet.FundAccount(alice.Address, 100_000_000);
        var feePayerFundTxn = await aptos.Faucet.FundAccount(feePayer.Address, 100_000_000);
        Console.WriteLine($"Alice's fund transaction: {aliceFundTxn.Hash}");
        Console.WriteLine($"FeePayer's fund transaction: {feePayerFundTxn.Hash}");

        Console.WriteLine("\n=== Building transaction ===\n");

        var txn = await aptos.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [receiver.Address, "100000"]
            ),
            // It's important to set this flag to true to enable sponsored transactions
            withFeePayer: true
        );
        Console.WriteLine($"{JsonConvert.SerializeObject(txn)}");

        Console.WriteLine("\n=== Signing and submitting transaction ===\n");

        var aliceSignature = alice.SignWithAuthenticator(txn);
        Console.WriteLine($"Alice has signed the transaction: {aliceSignature.BcsToHex()}");

        var feePayerSignature = aptos.Transaction.SignAsFeePayer(feePayer, txn);
        Console.WriteLine($"FeePayer has signed the transaction: {feePayerSignature.BcsToHex()}");

        var pendingTxn = await aptos.Transaction.SubmitTransaction(
            new(txn, aliceSignature, feePayerSignature)
        );
        Console.WriteLine($"Submitted transaction with hash: {pendingTxn.Hash}");

        Console.WriteLine("Waiting for transaction...");
        var committedTxn = await aptos.Transaction.WaitForTransaction(pendingTxn.Hash.ToString());
        Console.WriteLine(
            $"Transaction {committedTxn.Hash} is {(committedTxn.Success ? "success" : "failure")}"
        );

        Console.WriteLine("\n=== Account Balance ===\n");

        var aliceBalance = await aptos.Account.GetCoinBalance(alice.Address);
        var feePayerBalance = await aptos.Account.GetCoinBalance(feePayer.Address);
        var receiverBalance = await aptos.Account.GetCoinBalance(receiver.Address);
        Console.WriteLine($"Alice {alice.Address} has {aliceBalance?.Amount ?? 0} APT");
        Console.WriteLine($"Receiver {receiver.Address} has {receiverBalance?.Amount ?? 0} APT");
        Console.WriteLine($"FeePayer {feePayer.Address} has {feePayerBalance?.Amount ?? 0} APT");
    }
}
