using dotenv.net;

namespace Aptos.Tests;

public class BaseTests
{
    protected readonly ITestOutputHelper _output;

    public BaseTests(ITestOutputHelper output)
    {
        DotEnv.Load();
        _output = output;
        _output.WriteLine($"Started {GetType().FullName}");
    }

    /// <summary>
    /// Parses a string representation of an array and returns the array as a string array.
    /// </summary>
    /// <param name="input">The string representation of the array.</param>
    /// <returns>The array as a string array.</returns>
    public static string[] ParseArray(string input)
    {
        // Trim the square brackets
        input = input.Trim('[', ']');

        // If the input is empty (i.e., "[]"), return an empty array
        if (string.IsNullOrWhiteSpace(input))
            return [];

        // Split the input string by commas and convert to integers
        return [.. input.Split(',')];
    }
}
