namespace Aptos.Examples;

using Aptos.Core;

public class PlaygroundExample
{

    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));
        Console.WriteLine(Utilities.HexStringToString("6170746f735f636f696e"));
    }

}
