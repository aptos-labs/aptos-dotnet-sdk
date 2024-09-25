namespace Aptos.Examples;

public class AptosNamesExample
{
    public static async Task Run()
    {
        var aptos = new AptosClient(Networks.Mainnet);

        Console.WriteLine("=== Aptos Names Example ===\n");

        var aptosName = "aaron.apt";
        var nameAddress = "0xa746e980ae21949a4f084db7403430f00bce3c9a1da4101ffcf0bf45ebd35e7e";

        var address = await aptos.Ans.GetAnsAddress(aptosName);
        var name = await aptos.Ans.GetAnsName(nameAddress);

        Console.WriteLine($"Address for {aptosName}: {address}");
        Console.WriteLine($"Aptos name for {nameAddress}: {name}");
    }
}
