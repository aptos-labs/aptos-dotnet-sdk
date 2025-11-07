using System.Text.RegularExpressions;

namespace Aptos.Examples;

public class RunExample
{
    private static readonly Func<Task>[] exampleArray =
    {
        ViewFunctionExample.Run,
        ComplexViewFunctionExample.Run,
        SimpleTransferKeylessExample.Run,
        SimpleTransferFederatedKeylessExample.Run,
        SimpleTransferEd25519Example.Run,
        SimpleTransferSingleKeyExample.Run,
        SimpleTransferMultiKeyExample.Run,
        SimpleTransferComplexMultiKeyExample.Run,
        SimpleTransferOrderlessExample.Run,
        SponsoredTransferEd25519Example.Run,
        SimulateTransferEd25519Example.Run,
        AptosNamesExample.Run,
        PlaygroundExample.Run,
    };

    private const int ExamplesPerPage = 5;

    public static async Task Main()
    {
        DisplayAsciiArt();
        int selectedIndex = 0;
        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(exampleArray.Length / (double)ExamplesPerPage);
        string errorMessage = string.Empty;

        while (true)
        {
            // Ensure the selectedIndex stays within the current page's bounds
            selectedIndex = Math.Clamp(
                selectedIndex,
                currentPage * ExamplesPerPage,
                Math.Min((currentPage + 1) * ExamplesPerPage - 1, exampleArray.Length - 1)
            );

            DisplayMenu(selectedIndex, currentPage, totalPages, errorMessage);

            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                if (selectedIndex == currentPage * ExamplesPerPage) // First item on the current page
                {
                    // Move to the previous page and set the selected index to the last item on that page
                    currentPage = (currentPage == 0) ? totalPages - 1 : currentPage - 1;
                    selectedIndex = Math.Min(
                        (currentPage + 1) * ExamplesPerPage - 1,
                        exampleArray.Length - 1
                    ); // Wrap to last item of the previous page
                }
                else
                {
                    selectedIndex--; // Move up within the current page
                }
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                if (
                    selectedIndex
                    == Math.Min((currentPage + 1) * ExamplesPerPage - 1, exampleArray.Length - 1)
                ) // Last item on the current page
                {
                    // Move to the next page and set the selected index to the first item on that page
                    currentPage = (currentPage == totalPages - 1) ? 0 : currentPage + 1;
                    selectedIndex = currentPage * ExamplesPerPage; // Wrap to the first item of the next page
                }
                else
                {
                    selectedIndex++; // Move down within the current page
                }
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                currentPage = (currentPage == 0) ? totalPages - 1 : currentPage - 1;
                selectedIndex = currentPage * ExamplesPerPage;
                errorMessage = string.Empty; // Clear error on valid navigation
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                currentPage = (currentPage == totalPages - 1) ? 0 : currentPage + 1;
                selectedIndex = currentPage * ExamplesPerPage;
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
                int digit = int.Parse(keyInfo.KeyChar.ToString());
                int exampleIndex = currentPage * ExamplesPerPage + (digit - 1);

                if (digit > 0 && exampleIndex < exampleArray.Length)
                {
                    var selectedExample = exampleArray[exampleIndex];
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

    private static void DisplayMenu(
        int selectedIndex,
        int currentPage,
        int totalPages,
        string errorMessage
    )
    {
        Console.Clear();
        DisplayAsciiArt();

        Console.WriteLine(
            "Use arrow keys to navigate, press Enter to choose an example, and Left/Right to switch pages."
        );

        int start = currentPage * ExamplesPerPage;
        int end = Math.Min(start + ExamplesPerPage, exampleArray.Length);

        for (int i = start; i < end; i++)
        {
            var name = Regex.Replace(
                exampleArray[i].Method.DeclaringType?.Name,
                "(?<!^)([A-Z])",
                " $1"
            );
            int displayNumber = i - (currentPage * ExamplesPerPage) + 1; // Reset number per page

            if (i == selectedIndex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Highlight the selected option
                Console.WriteLine($"> {displayNumber}. {name}");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"  {displayNumber}. {name}");
            }
        }

        Console.WriteLine($"\nPage {currentPage + 1}/{totalPages}");

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
