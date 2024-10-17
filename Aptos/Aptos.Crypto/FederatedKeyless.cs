namespace Aptos;

using System.Numerics;
using Aptos.Poseidon;
using Microsoft.IdentityModel.JsonWebTokens;


public class FederatedKeylessPublicKey : PublicKey
{
    public static readonly int ID_COMMITMENT_LENGTH = 32;

    private readonly AccountAddress JwkAddress;

    public readonly KeylessPublicKey KeylessPublicKey;

    public override Hex Value => Hex.FromHexInput(BcsToBytes());

    public FederatedKeylessPublicKey(string iss, string uidKey, string uidVal, string aud, string pepper, AccountAddress jwkAddress)
        : this(iss, Keyless.ComputeIdCommitment(uidKey, uidVal, aud, pepper), jwkAddress) { }

    public FederatedKeylessPublicKey(string iss, string uidKey, string uidVal, string aud, byte[] pepper, AccountAddress jwkAddress)
        : this(iss, Keyless.ComputeIdCommitment(uidKey, uidVal, aud, pepper), jwkAddress) { }

    public FederatedKeylessPublicKey(string iss, string idCommitment, AccountAddress jwkAddress)
        : this(iss, Hex.FromHexInput(idCommitment).ToByteArray(), jwkAddress) { }

    public FederatedKeylessPublicKey(string iss, byte[] idCommitment, AccountAddress jwkAddress)
        : this(new KeylessPublicKey(iss, idCommitment), jwkAddress) { }

    public FederatedKeylessPublicKey(KeylessPublicKey keylessPublicKey, AccountAddress jwkAddress)
        : base(PublicKeyVariant.FederatedKeyless)
    {
        JwkAddress = jwkAddress;
        KeylessPublicKey = keylessPublicKey;
    }

    public override bool VerifySignature(byte[] message, Signature signature) =>
        signature is KeylessSignature keylessSignature
        && keylessSignature.EphemeralPublicKey.VerifySignature(
            message,
            keylessSignature.EphemeralSignature.Signature
        );

    public override byte[] ToByteArray() => BcsToBytes();

    public override void Serialize(Serializer s)
    {
        JwkAddress.Serialize(s);
        KeylessPublicKey.Serialize(s);
    }

    public static new FederatedKeylessPublicKey Deserialize(Deserializer d)
    {
        AccountAddress jwkAddress = AccountAddress.Deserialize(d);
        KeylessPublicKey keylessPublicKey = KeylessPublicKey.Deserialize(d);
        return new FederatedKeylessPublicKey(keylessPublicKey, jwkAddress);
    }

    public static FederatedKeylessPublicKey FromJwt(string jwt, string pepper, AccountAddress jwkAddress, string uidKey = "sub") =>
        new(new JsonWebToken(jwt).Issuer, Keyless.ComputeIdCommitment(jwt, pepper, uidKey), jwkAddress);

    public static FederatedKeylessPublicKey FromJwt(string jwt, byte[] pepper, AccountAddress jwkAddress, string uidKey = "sub") =>
        new(new JsonWebToken(jwt).Issuer, Keyless.ComputeIdCommitment(jwt, pepper, uidKey), jwkAddress);
}
