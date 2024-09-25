using Newtonsoft.Json;

namespace Aptos;

public class SimulateTransferEd25519Example
{
    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));

        Console.WriteLine("=== Addresses ===\n");

        var alice = Ed25519Account.Generate();
        Console.WriteLine($"Alice: {alice.Address}");

        Console.WriteLine("\n=== Funding accounts ===\n");

        var aliceFundTxn = await aptos.Faucet.FundAccount(alice.Address, 100_000_000);
        Console.WriteLine($"Alice's fund transaction: {aliceFundTxn.Hash}");

        Console.WriteLine("\n=== Building transaction ===\n");

        var txn = await aptos.Transaction.Build(
            sender: alice.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::aptos_account::transfer_coins",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [alice.Address, "100000"]
            )
        );
        Console.WriteLine($"{JsonConvert.SerializeObject(txn)}");

        Console.WriteLine("\n=== Simulating transaction ===\n");

        var simulatedTxn = await aptos.Transaction.Simulate(new(txn, alice.PublicKey));
        Console.WriteLine(
            $"Simulated Transaction: {JsonConvert.SerializeObject(simulatedTxn, Formatting.Indented)}"
        );
    }
}
