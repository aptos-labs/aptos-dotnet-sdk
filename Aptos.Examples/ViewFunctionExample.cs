using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aptos.Examples;

public class ViewFunctionExample
{
    public class Option<T>(List<T> vec)
    {
        [JsonProperty("vec")]
        public List<T> Vec = vec;
    }

    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));

        // Regular view function
        var values = await aptos.View(
            new GenerateViewFunctionPayloadData(
                function: "0x1::coin::name",
                functionArguments: [],
                typeArguments: ["0x1::aptos_coin::AptosCoin"]
            )
        );
        // Returns an array of return values. In this array, we will get "["Aptos Coin]"
        Console.WriteLine($"Coin name: {JsonConvert.SerializeObject(values)}");

        // Typed view function - If you know the type of the return values, you can pass
        //                       the type arguments to the view function to get the correct
        //                       return values. You can create your own deserializable values
        //                       to pass into this function. Its important to make sure that
        //                       the return type is a List.
        var typedValues = await aptos.View<List<Option<String>>>(
            new GenerateViewFunctionPayloadData(
                function: "0x1::coin::supply",
                functionArguments: [],
                typeArguments: ["0x1::aptos_coin::AptosCoin"]
            )
        );
        // Returns an array of return values. In this array, we will get "[{"vec":["18447254572092493002"]}]"
        Console.WriteLine($"Coin supply: {JsonConvert.SerializeObject(typedValues)}");
    }
}
