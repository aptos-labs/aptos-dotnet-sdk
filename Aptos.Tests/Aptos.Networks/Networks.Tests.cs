namespace Aptos.Tests.NetworksTests;

using Aptos.Exceptions;

public class NetworksAndConfigTests(ITestOutputHelper output) : BaseTests(output)
{
    [Fact]
    public void Networks_HavePresets()
    {
        Assert.Equal("devnet", Networks.Devnet.Name);
        Assert.Equal("testnet", Networks.Testnet.Name);
        Assert.Equal("mainnet", Networks.Mainnet.Name);
        Assert.Equal("local", Networks.Local.Name);
        Assert.Equal(1, Networks.Mainnet.ChainId);
        Assert.Equal(2, Networks.Testnet.ChainId);
        Assert.Null(Networks.Mainnet.FaucetUrl); // mainnet has no faucet
        Assert.NotNull(Networks.Devnet.FaucetUrl);
        Assert.NotNull(Networks.Testnet.FaucetUrl);
    }

    [Fact]
    public void AptosConfig_DefaultsToDevnet()
    {
        var config = new AptosConfig();
        Assert.Equal("devnet", config.NetworkConfig.Name);
        Assert.NotNull(config.RequestClient);
        Assert.NotNull(config.Headers);
    }

    [Fact]
    public void AptosConfig_AcceptsCustomNetwork()
    {
        var custom = new NetworkConfig("custom", "https://node", "https://idx", null, null, null, 99);
        var config = new AptosConfig(custom);
        Assert.Equal("custom", config.NetworkConfig.Name);
        Assert.Equal(99, config.NetworkConfig.ChainId);
    }

    [Fact]
    public void AptosConfig_GetRequestUrl_ReturnsCorrectEndpoints()
    {
        var config = new AptosConfig(Networks.Devnet);
        Assert.Equal(Networks.Devnet.NodeUrl, config.GetRequestUrl(ApiType.FullNode));
        Assert.Equal(Networks.Devnet.IndexerUrl, config.GetRequestUrl(ApiType.Indexer));
        Assert.Equal(Networks.Devnet.FaucetUrl, config.GetRequestUrl(ApiType.Faucet));
        Assert.Equal(Networks.Devnet.ProverUrl, config.GetRequestUrl(ApiType.Prover));
        Assert.Equal(Networks.Devnet.PepperUrl, config.GetRequestUrl(ApiType.Pepper));
    }

    [Fact]
    public void AptosConfig_GetRequestUrl_NoEndpointThrows()
    {
        // Mainnet has no faucet so this must throw a ConfigException.
        var config = new AptosConfig(Networks.Mainnet);
        var ex = Assert.Throws<ConfigException>(() => config.GetRequestUrl(ApiType.Faucet));
        Assert.Contains("mainnet", ex.Message);
    }

    [Fact]
    public void ChainId_Serialization_RoundTrip()
    {
        var chainId = new ChainId(42);
        var bytes = chainId.BcsToBytes();
        Assert.Single(bytes);
        Assert.Equal(42, bytes[0]);
        var deserialized = ChainId.Deserialize(new Deserializer(bytes));
        Assert.Equal(42, deserialized.Value);
    }

    [Fact]
    public void AptosClient_ConstructsFromNetworkConfig()
    {
        var client = new AptosClient(Networks.Devnet);
        Assert.Equal("devnet", client.Config.NetworkConfig.Name);
        Assert.NotNull(client.Account);
        Assert.NotNull(client.Block);
        Assert.NotNull(client.Transaction);
        Assert.NotNull(client.Faucet);
        Assert.NotNull(client.Contract);
        Assert.NotNull(client.Gas);
        Assert.NotNull(client.Indexer);
        Assert.NotNull(client.Ans);
        Assert.NotNull(client.FungibleAsset);
        Assert.NotNull(client.DigitalAsset);
        Assert.NotNull(client.Keyless);
        Assert.NotNull(client.Table);
    }
}
