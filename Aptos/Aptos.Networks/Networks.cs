namespace Aptos;

public static class Networks
{

    public static readonly NetworkConfig Devnet = new(
        "devnet",
        "https://api.devnet.aptoslabs.com/v1",
        "https://api.devnet.aptoslabs.com/v1/graphql",
        "https://faucet.devnet.aptoslabs.com",
        "https://api.devnet.aptoslabs.com/keyless/prover/v0",
        "https://api.devnet.aptoslabs.com/keyless/pepper/v0",
        -1
    );

    public static readonly NetworkConfig Testnet = new(
        "testnet",
        "https://api.testnet.aptoslabs.com/v1",
        "https://api.testnet.aptoslabs.com/v1/graphql",
        "https://faucet.testnet.aptoslabs.com",
        "https://api.testnet.aptoslabs.com/keyless/prover/v0",
        "https://api.testnet.aptoslabs.com/keyless/pepper/v0",
        2
    );

    public static readonly NetworkConfig Mainnet = new(
        "mainnet",
        "https://api.mainnet.aptoslabs.com/v1",
        "https://api.mainnet.aptoslabs.com/v1/graphql",
        null,
        "https://api.mainnet.aptoslabs.com/keyless/prover/v0",
        "https://api.mainnet.aptoslabs.com/keyless/pepper/v0",
        1
    );

    public static readonly NetworkConfig Local = new(
        "local",
        "http://127.0.0.1:8080/v1",
        "http://127.0.0.1:8090/v1/graphql",
        "http://127.0.0.1:8081",
        // Use the Devnet service for local environments
        "https://api.devnet.aptoslabs.com/keyless/prover/v0",
        "https://api.devnet.aptoslabs.com/keyless/pepper/v0",
        4
    );

}