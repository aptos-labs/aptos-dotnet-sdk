namespace Aptos.Examples;

public class RunExample
{
    private static readonly Dictionary<string, Func<Task>> exampleMap = new()
        {
            { "1", KeylessTransferExample.Run },
            { "2", SimpleTransferExample.Run },
            { "3", PlaygroundExample.Run }
        };

    public static async Task Main()
    {
        DisplayAsciiArt();
        int selectedIndex = 0;
        string[] keys = new string[exampleMap.Count];
        exampleMap.Keys.CopyTo(keys, 0);

        string errorMessage = string.Empty;

        while (true)
        {
            DisplayMenu(keys, selectedIndex, errorMessage);

            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex == 0) ? keys.Length - 1 : selectedIndex - 1;
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex == keys.Length - 1) ? 0 : selectedIndex + 1;
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                // Run the selected example using arrow keys and enter
                if (exampleMap.TryGetValue(keys[selectedIndex], out var selectedExample))
                {
                    Console.WriteLine($"\nThe {exampleMap[keys[selectedIndex]].Method.DeclaringType.Name} example was selected...\n");
                    await selectedExample();
                }
                break;
            }
            else if (char.IsDigit(keyInfo.KeyChar))
            {
                // Check if the typed number is valid
                string input = keyInfo.KeyChar.ToString();
                if (exampleMap.TryGetValue(input, out var selectedExample))
                {
                    Console.WriteLine($"\nThe {exampleMap[keys[selectedIndex]].Method.DeclaringType.Name} example was selected...\n");
                    await selectedExample();
                    break;
                }
                else
                {
                    errorMessage = $"Invalid number '{input}'. Please choose a valid example.";
                }
            }
            else
            {
                errorMessage = $"Invalid input '{keyInfo.KeyChar}'. Use arrow keys or type a valid number.";
            }
        }
    }

    private static void DisplayMenu(string[] keys, int selectedIndex, string errorMessage)
    {
        Console.Clear();
        DisplayAsciiArt();

        Console.WriteLine("Use arrow keys to navigate and press Enter or type a number to choose an example:");
        for (int i = 0; i < keys.Length; i++)
        {
            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Highlight the selected option
                Console.WriteLine($"> {keys[i]}. {exampleMap[keys[i]].Method.DeclaringType?.Name} Example");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {keys[i]}. {exampleMap[keys[i]].Method.DeclaringType?.Name} Example");
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

        Console.WriteLine(cyan + @"
    _______  _______  _______  _______  _______ 
    |   _   ||       ||       ||       ||       |
    |  |_|  ||    _  ||_     _||   _   ||  _____|
    |       ||   |_| |  |   |  |  | |  || |_____ 
    |       ||    ___|  |   |  |  |_|  ||_____  |
    |   _   ||   |      |   |  |       | _____| |
    |__| |__||___|      |___|  |_______||_______|
              " + reset + yellow + @"
              EXAMPLES RUNNER
            " + reset);
    }

}