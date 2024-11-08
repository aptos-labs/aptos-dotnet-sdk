namespace Aptos.Examples;

public class SimpleTransferFederatedKeylessExample
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
                $"https://dev-qtdgjv22jh0v1k7g.us.auth0.com/authorize?client_id=dzqI77x0M5YwdOSUx6j25xkdOt8SIxeE&redirect_uri=http%3A%2F%2Flocalhost%3A5173%2Fcallback&response_type=id_token&scope=openid&nonce={ekp.Nonce}";
            Console.WriteLine($"Login URL: {loginFlow} \n");

            Console.WriteLine("1. Open the link above in your browser");
            Console.WriteLine("2. Login with your Auth0 account");
            Console.WriteLine("3. Copy the 'id_token' from the url bar\n");

            // Ask for the JWT token

            Console.WriteLine("Paste the JWT (id_token) token here and press enter: ");
            var jwt = Console.ReadLine();

            Console.WriteLine("\nPaste the address where the JWKs are installed: ");
            var address = Console.ReadLine();

            // Derive the keyless account

            Console.WriteLine("\nDeriving federated keyless account...");
            if (jwt == null)
                throw new ArgumentException("No JWT token provided");
            keylessAccount = await aptos.Keyless.DeriveAccount(
                jwt,
                ekp,
                jwkAddress: AccountAddress.FromString(address)
            );

            Console.WriteLine("=== Addresses ===\n");

            Console.WriteLine($"Federated keyless account address is: {keylessAccount.Address}");
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
