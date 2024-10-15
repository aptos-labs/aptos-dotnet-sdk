using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aptos.Examples;

public class ComplexViewFunctionExample
{
    [JsonConverter(typeof(CurrentEpochProposalCountsConverter))]
    public class CurrentEpochProposalCounts(ulong successfulProposals, ulong failedProposals)
    {
        public ulong? SuccessfulProposals = successfulProposals;

        public ulong? FailedProposals = failedProposals;
    }

    public class CurrentEpochProposalCountsConverter : JsonConverter<CurrentEpochProposalCounts>
    {
        public override bool CanWrite => false;

        public override CurrentEpochProposalCounts? ReadJson(
            JsonReader reader,
            Type objectType,
            CurrentEpochProposalCounts? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            // Load the result of the View function which would be ["100", "100"] since the return type is (u64,u64).
            // We then deserialize the return value into a CurrentEpochProposalCounts.
            var jArrayObject = JArray.Load(reader);
            return new CurrentEpochProposalCounts(
                ulong.Parse(jArrayObject[0].ToString()),
                ulong.Parse(jArrayObject[0].ToString())
            );
        }

        public override void WriteJson(
            JsonWriter writer,
            CurrentEpochProposalCounts? value,
            JsonSerializer serializer
        ) => throw new NotImplementedException();
    }

    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));

        // Complex typed view function - We will deserialize the return value into a custom class by using a JsonConverter.
        //                               This is only recommended if the return type is complex and cannot be easily deserialized
        //                               using the default deserializer (multiple return types).
        var proposals = await aptos.View<CurrentEpochProposalCounts>(
            new GenerateViewFunctionPayloadData(
                function: "0x1::stake::get_current_epoch_proposal_counts",
                functionArguments: [(ulong)0]
            )
        );

        Console.WriteLine(
            $"Successful Proposals: {proposals.SuccessfulProposals} and Failed Proposals: {proposals.FailedProposals}"
        );
    }
}
