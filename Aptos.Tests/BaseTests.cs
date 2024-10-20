using System.Reflection;
using dotenv.net;
using Moq;

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

    /// <summary>
    /// Mock a KeylessAccount to enable signing even if the EphemeralKeyPair is expired.
    /// </summary>
    /// <param name="keylessAccount">The keyless account with an expired values.</param>
    public static void MockKeylessAccount(KeylessAccount keylessAccount)
    {
        var field = typeof(KeylessAccount).GetField(
            "EphemeralKeyPair",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
        var mock = new Mock<EphemeralKeyPair>(keylessAccount.EphemeralKeyPair);
        mock.Setup(m => m.IsExpired()).Returns(false);
        field?.SetValue(keylessAccount, mock.Object);
    }
}
