using Newtonsoft.Json;

namespace Aptos.Examples;

public class SimpleTransferMultiKeyExample
{
    public static async Task Run()
    {
        Console.WriteLine("=== Addresses ===\n");
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));
        var aliceSigner = Account.Generate();
        var bobSigner = SingleKeyAccount.Generate();
        var account = new MultiKeyAccount(
            new([aliceSigner.PublicKey, bobSigner.PublicKey], 1),
            [aliceSigner]
        );
        Console.WriteLine($"Alice-Bob MultiKey (1/2): {account.Address}");

        Console.WriteLine("\n=== Funding accounts ===\n");
        var aliceFundTxn = await aptos.Faucet.FundAccount(account.Address, 100_000_000);
        Console.WriteLine($"Alice-Bob MultiKey (1/2)'s fund transaction: {aliceFundTxn.Hash}");

        Console.WriteLine("\n=== Building transaction ===\n");

        var txn = await aptos.Transaction.Build(
            sender: account.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::coin::transfer",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [account.Address, "100000"]
            )
        );
        Console.WriteLine($"{JsonConvert.SerializeObject(txn.RawTransaction)}");

        Console.WriteLine("\n=== Signing and submitting transaction ===\n");

        var pendingTxn = await aptos.Transaction.SignAndSubmitTransaction(account, txn);
        Console.WriteLine($"Submitted transaction with hash: {pendingTxn.Hash}");

        Console.WriteLine("Waiting for transaction...");
        var committedTxn = await aptos.Transaction.WaitForTransaction(pendingTxn.Hash.ToString());
        Console.WriteLine(
            $"Transaction {committedTxn.Hash} is {(committedTxn.Success ? "success" : "failure")}"
        );

        Console.WriteLine("\n=== Account Balance ===\n");
        var balance = await aptos.Account.GetCoinBalance(account.Address, "0xa");
        Console.WriteLine($"Account {account.Address} has {balance?.Amount ?? 0} coin balances");
    }
}
