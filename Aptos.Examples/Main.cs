using System.Text.RegularExpressions;

namespace Aptos.Examples;

public class RunExample
{
    private static readonly Func<Task>[] exampleArray =
    {
        SimpleTransferKeylessExample.Run,
        SimpleTransferEd25519Example.Run,
        SimpleTransferSingleKeyExample.Run,
        SimpleTransferMultiKeyExample.Run,
        SponsoredTransferEd25519Example.Run,
        SimulateTransferEd25519Example.Run,
        AptosNamesExample.Run,
        PlaygroundExample.Run,
    };

    public static async Task Main()
    {
        DisplayAsciiArt();
        int selectedIndex = 0;

        string errorMessage = string.Empty;

        while (true)
        {
            DisplayMenu(selectedIndex, errorMessage);

            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex == 0) ? exampleArray.Length - 1 : selectedIndex - 1;
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex == exampleArray.Length - 1) ? 0 : selectedIndex + 1;
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                // Run the selected example using arrow keys and enter
                var selectedExample = exampleArray[selectedIndex];
                Console.WriteLine(
                    $"\nThe {selectedExample.Method.DeclaringType.Name} example was selected...\n"
                );
                await selectedExample();
                break;
            }
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                // Check if the typed number is valid and within the array range
                if (
                    int.TryParse(keyInfo.KeyChar.ToString(), out int inputIndex)
                    && inputIndex > 0
                    && inputIndex <= exampleArray.Length
                )
                {
                    var selectedExample = exampleArray[inputIndex - 1];
                    Console.WriteLine(
                        $"\nThe {selectedExample.Method.DeclaringType.Name} example was selected...\n"
                    );
                    await selectedExample();
                    break;
                }
                else
                {
                    errorMessage =
                        $"Invalid number '{keyInfo.KeyChar}'. Please choose a valid example.";
                }
            }
            else
            {
                errorMessage =
                    $"Invalid input '{keyInfo.KeyChar}'. Use arrow keys or type a valid number.";
            }
        }
    }

    private static void DisplayMenu(int selectedIndex, string errorMessage)
    {
        Console.Clear();
        DisplayAsciiArt();

        Console.WriteLine(
            "Use arrow keys to navigate and press Enter or type a number to choose an example:"
        );
        for (int i = 0; i < exampleArray.Length; i++)
        {
            var name = Regex.Replace(
                exampleArray[i].Method.DeclaringType?.Name,
                "(?<!^)([A-Z])",
                " $1"
            );
            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Highlight the selected option
                Console.WriteLine($"> {i + 1}. {name}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {i + 1}. {name}");
            }
        }

        // Show error message if any
        if (!string.IsNullOrEmpty(errorMessage))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{errorMessage}");
            Console.ResetColor();
        }
    }

    private static void DisplayAsciiArt()
    {
        // Color codes for fun output
        string cyan = "\u001b[36m";
        string yellow = "\u001b[33m";
        string reset = "\u001b[0m";

        Console.WriteLine(
            cyan
                + @"
    _______  _______  _______  _______  _______ 
    |   _   ||       ||       ||       ||       |
    |  |_|  ||    _  ||_     _||   _   ||  _____|
    |       ||   |_| |  |   |  |  | |  || |_____ 
    |       ||    ___|  |   |  |  |_|  ||_____  |
    |   _   ||   |      |   |  |       | _____| |
    |__| |__||___|      |___|  |_______||_______|
              "
                + reset
                + yellow
                + @"
              EXAMPLES RUNNER
            "
                + reset
        );
    }
}
