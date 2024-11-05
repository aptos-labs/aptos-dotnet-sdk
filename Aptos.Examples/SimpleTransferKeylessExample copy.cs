namespace Aptos.Examples;

public class SimpleTransferKeylessExample
{
    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));
        KeylessAccount keylessAccount;
        var bob = Account.Generate();
        var ekp = EphemeralKeyPair.Generate();

        Console.WriteLine("=== Keyless Account Example ===\n");
        {
            // Begin the login flow

            var loginFlow =
                $"https://accounts.google.com/o/oauth2/v2/auth/oauthchooseaccount?redirect_uri=https%3A%2F%2Fdevelopers.google.com%2Foauthplayground&prompt=consent&response_type=code&client_id=407408718192.apps.googleusercontent.com&scope=openid&access_type=offline&service=lso&o2v=2&theme=glif&flowName=GeneralOAuthFlow&nonce={ekp.Nonce}";
            Console.WriteLine($"Login URL: {loginFlow} \n");

            Console.WriteLine("1. Open the link above in your browser");
            Console.WriteLine("2. Login with your Google account");
            Console.WriteLine("3. Click 'Exchange authorization code for tokens'");
            Console.WriteLine(
                "4. Copy the 'id_token' - (toggling 'Wrap lines' option at the bottom makes this easier)\n"
            );

            // Ask for the JWT token

            Console.WriteLine("Paste the JWT (id_token) token here and press enter: ");
            var jwt = Console.ReadLine();

            // Derive the keyless account

            Console.WriteLine("\nDeriving keyless account...");
            if (jwt == null)
                throw new ArgumentException("No JWT token provided");
            keylessAccount = await aptos.Keyless.DeriveAccount(jwt, ekp);

            Console.WriteLine("=== Addresses ===\n");

            Console.WriteLine($"Keyless account address is: {keylessAccount.Address}");
            Console.WriteLine($"Bob account address is: {bob.Address}");
        }

        Console.WriteLine("\n=== Funding Accounts ===\n");
        {
            await aptos.Faucet.FundAccount(keylessAccount.Address, 100_000_000);
            await aptos.Faucet.FundAccount(bob.Address, 100_000_000);

            Console.WriteLine("Successfully funded keyless account!");
        }

        Console.WriteLine("\n=== Sending APT from Keyless Account to Bob ===\n");
        {
            Console.WriteLine("Building transaction...");
            var txn = await aptos.Transaction.Build(
                sender: keylessAccount,
                data: new GenerateEntryFunctionPayloadData(
                    function: "0x1::aptos_account::transfer_coins",
                    typeArguments: ["0x1::aptos_coin::AptosCoin"],
                    functionArguments: [bob.Address, "100000"]
                )
            );

            Console.WriteLine("Signing and submitting transaction...");
            var pendingTxn = await aptos.Transaction.SignAndSubmitTransaction(keylessAccount, txn);
            Console.WriteLine($"Submitted transaction with hash: {pendingTxn.Hash}");
            var committedTxn = await aptos.Transaction.WaitForTransaction(pendingTxn.Hash);
            if (committedTxn.Success)
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction failed!");
            }
        }

        Console.WriteLine("\n=== Balances ===\n");
        {
            var keylessAccountBalance = await aptos.Account.GetCoinBalance(keylessAccount.Address);
            var bobAccountBalance = await aptos.Account.GetCoinBalance(bob.Address);
            Console.WriteLine($"Keyless account balance: {keylessAccountBalance?.Amount ?? 0} APT");
            Console.WriteLine($"Bob account balance: {bobAccountBalance?.Amount ?? 0} APT");
        }
    }
}
