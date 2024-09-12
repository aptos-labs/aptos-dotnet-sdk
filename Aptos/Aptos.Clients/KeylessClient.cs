namespace Aptos;

using Aptos.Core;
using Newtonsoft.Json;

public class KeylessClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    public async Task<KeylessAccount> DeriveAccount(string jwt, EphemeralKeyPair ekp, string uidKey = "sub", byte[]? pepper = null)
    {
        if (pepper == null) pepper = await GetPepper(jwt, ekp, uidKey);
        var proof = await GetProof(jwt, ekp, pepper, uidKey);

        // Derive the keyless account from the JWT and EphemeralKeyPair
        var publicKey = KeylessPublicKey.FromJwt(jwt, pepper, uidKey);
        var address = await _client.Account.LookupOriginalAccountAddress(publicKey.AuthKey().DerivedAddress().ToString());

        // Create and return the keyless account
        return new KeylessAccount(jwt, ekp, proof, pepper, uidKey, address);
    }

    public async Task<byte[]> GetPepper(string jwt, EphemeralKeyPair ekp, string? uidKey = "sub", string? derivationPath = null)
    {
        var response = await _client.PostPepper<PepperResponse>(new(
            body: new
            {
                jwt_b64 = jwt,
                epk = ekp.PublicKey.BcsToHex().ToStringWithoutPrefix(),
                exp_date_secs = ekp.ExpiryTimestamp,
                epk_blinder = Hex.FromHexInput(ekp.Blinder).ToStringWithoutPrefix(),
                uid_key = uidKey,
                derivation_path = derivationPath
            },
            path: "fetch",
            originMethod: "getPepper"
        ));
        return response.Data.Pepper.ToByteArray();
    }


    public async Task<ZeroKnowledgeSignature> GetProof(string jwt, EphemeralKeyPair ekp, byte[]? pepper = null, string? uidKey = "sub")
    {
        if (pepper == null) pepper = await GetPepper(jwt, ekp, uidKey);
        if (pepper.Length != KeylessAccount.PEPPER_LENGTH) throw new ArgumentException($"Pepper length in bytes should be {KeylessAccount.PEPPER_LENGTH}");

        var (_, maxExpHorizonSecs) = await GetKeylessConfig();
        if (maxExpHorizonSecs < (ekp.ExpiryTimestamp - (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds())) throw new ArgumentException($"The EphemeralKeyPair is too long lived.  It's lifespan must be less than {maxExpHorizonSecs}");

        var response = await _client.PostProver<ProverResponse>(new(
            body: new
            {
                jwt_b64 = jwt,
                epk = ekp.PublicKey.BcsToHex().ToStringWithoutPrefix(),
                epk_blinder = Hex.FromHexInput(ekp.Blinder).ToStringWithoutPrefix(),
                exp_date_secs = ekp.ExpiryTimestamp,
                exp_horizon_secs = maxExpHorizonSecs,
                pepper = Hex.FromHexInput(pepper).ToStringWithoutPrefix(),
                uid_key = uidKey,
            },
            path: "prove",
            originMethod: "getProof"
        ));

        return new ZeroKnowledgeSignature(
            proof: new ZkProof(response.Data.Proof, ZkpVariant.Groth16),
            expHorizonSecs: maxExpHorizonSecs,
            null,
            null,
            trainingWheelSignature: EphemeralSignature.Deserialize(new Deserializer(response.Data.TrainingWheelsSignature))
        );
    }

    public async Task<(Groth16VerificationKey VerificationKey, ulong MaxExpHorizonSecs)> GetKeylessConfig(ulong? ledgerVersion = null)
    {
        return await Memoize.MemoAsync(
            async () => (await GetGroth16VerificationKey(ledgerVersion), (await GetConfigurationResource(ledgerVersion)).MaxExpHorizonSecs),
            $"keyless-config-{_client.Config.NetworkConfig.Name}",
            1000 * 60 * 5 // 5 minutes
        )();
    }

    public async Task<KeylessConfiguration> GetConfigurationResource(ulong? ledgerVersion = null)
    {
        Dictionary<string, string> queryParams = [];
        if (ledgerVersion != null) { queryParams.Add("ledger_version", ledgerVersion!.ToString()!); }

        var response = await _client.GetFullNode<MoveResource>(new(
            path: $"accounts/{AccountAddress.From("0x1")}/resource/0x1::keyless_account::Configuration",
            originMethod: "getConfigurationResource",
            queryParams: queryParams
        ));

        return JsonConvert.DeserializeObject<KeylessConfiguration>(JsonConvert.SerializeObject(response.Data.Data))!;
    }

    public async Task<Groth16VerificationKey> GetGroth16VerificationKey(ulong? ledgerVersion = null)
    {
        Dictionary<string, string> queryParams = [];
        if (ledgerVersion != null) { queryParams.Add("ledger_version", ledgerVersion!.ToString()!); }

        var response = await _client.GetFullNode<MoveResource>(new(
            path: $"accounts/{AccountAddress.From("0x1")}/resource/0x1::keyless_account::Groth16VerificationKey",
            originMethod: "getVerificationKey",
            queryParams: queryParams
        ));

        return JsonConvert.DeserializeObject<Groth16VerificationKey>(JsonConvert.SerializeObject(response.Data.Data))!;
    }

}