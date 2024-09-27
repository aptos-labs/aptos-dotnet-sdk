using Newtonsoft.Json;

namespace Aptos.Examples;

public class SimpleTransferComplexMultiKeyExample
{
    public static async Task Run()
    {
        var aptos = new AptosClient(new AptosConfig(Networks.Devnet));

        Console.WriteLine("=== Set up Accounts ===\n");

        KeylessAccount keylessAccount;
        Ed25519Account ed25519Account;
        SingleKeyAccount secp256k1Account;

        Console.WriteLine("Setting up KeylessAccount...");
        {
            var ekp = EphemeralKeyPair.Generate();
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
        }
        Console.WriteLine("Setting up Ed25519Account...");
        {
            ed25519Account = Ed25519Account.Generate();
        }
        Console.WriteLine("Setting up Secp2561k1Account...");
        {
            secp256k1Account = SingleKeyAccount.Generate(PublicKeyVariant.Secp256k1Ecdsa);
        }

        Console.WriteLine("\n=== Addresses ===\n");

        var account = new MultiKeyAccount(
            new(
                [
                    ed25519Account.PublicKey,
                    secp256k1Account.PublicKey,
                    (keylessAccount.VerifyingKey as SingleKey).PublicKey,
                ],
                3
            ),
            [ed25519Account, secp256k1Account, keylessAccount]
        );
        Console.WriteLine($"Ed25519-Secp256k1-Keyless MultiKey (3/3): {account.Address}");

        Console.WriteLine("\n=== Funding accounts ===\n");
        var fundAccountTxn = await aptos.Faucet.FundAccount(account.Address, 100_000_000);
        Console.WriteLine(
            $"Ed25519-Secp256k1-Keyless MultiKey (3/3)'s fund transaction: {fundAccountTxn.Hash}"
        );

        Console.WriteLine("\n=== Building transaction ===\n");

        var txn = await aptos.Transaction.Build(
            sender: account.Address,
            data: new GenerateEntryFunctionPayloadData(
                function: "0x1::coin::transfer",
                typeArguments: ["0x1::aptos_coin::AptosCoin"],
                functionArguments: [account.Address, "100000"]
            )
        );
        Console.WriteLine($"{JsonConvert.SerializeObject(txn.RawTransaction)}");

        Console.WriteLine("\n=== Signing and submitting transaction ===\n");

        var pendingTxn = await aptos.Transaction.SignAndSubmitTransaction(account, txn);
        Console.WriteLine($"Submitted transaction with hash: {pendingTxn.Hash}");

        Console.WriteLine("Waiting for transaction...");
        var committedTxn = await aptos.Transaction.WaitForTransaction(pendingTxn.Hash.ToString());
        Console.WriteLine(
            $"Transaction {committedTxn.Hash} is {(committedTxn.Success ? "success" : "failure")}"
        );

        Console.WriteLine("\n=== Account Balance ===\n");
        var balance = await aptos.Account.GetCoinBalance(account.Address, "0xa");
        Console.WriteLine($"Account {account.Address} has {balance?.Amount ?? 0} coin balances");
    }
}
