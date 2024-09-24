namespace Aptos;

using Aptos.Schemes;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public class KeylessAccount : Account
{
    public static readonly int PEPPER_LENGTH = 31;

    public static readonly string DOMAIN_SEPARATOR = "APTOS::TransactionAndProof";

    private readonly SingleKey _verifyingKey;

    /// <summary>
    /// Gets the KeylessPublicKey inside a SingleKey for the account.
    /// </summary>
    public override IVerifyingKey VerifyingKey => _verifyingKey;

    private readonly AccountAddress _address;

    /// <summary>
    /// Gets the address of the account.
    /// </summary>
    public override AccountAddress Address => _address;

    public override SigningScheme SigningScheme => SigningScheme.SingleKey;

    public readonly EphemeralKeyPair EphemeralKeyPair;

    public readonly string UidKey;

    public readonly string UidVal;

    public readonly string Aud;

    public readonly byte[] Pepper;

    public ZeroKnowledgeSignature Proof;

    public readonly string Jwt;

    public KeylessAccount(
        string jwt,
        EphemeralKeyPair ekp,
        ZeroKnowledgeSignature proof,
        byte[] pepper,
        string uidKey = "sub",
        AccountAddress? address = null
    )
    {
        if (pepper.Length != PEPPER_LENGTH)
            throw new ArgumentException($"Pepper length in bytes should be {PEPPER_LENGTH}");

        _verifyingKey = new SingleKey(KeylessPublicKey.FromJwt(jwt, pepper, uidKey));
        _address = address ?? _verifyingKey.AuthKey().DerivedAddress();
        EphemeralKeyPair = ekp;
        Proof = proof;
        Pepper = pepper;

        // Decode the JWT and extract relevant claims
        var token = new JsonWebToken(jwt);
        Jwt = jwt;
        UidKey = uidKey;
        Aud = token.GetClaim("aud").Value;
        UidVal = token.GetClaim(uidKey).Value;
    }

    public bool VerifySignature(byte[] message, KeylessSignature signature)
    {
        if (EphemeralKeyPair.IsExpired())
            return false;
        return EphemeralKeyPair.PublicKey.VerifySignature(message, signature.EphemeralSignature);
    }

    public override Signature Sign(AnyRawTransaction transaction)
    {
        RawTransaction rawTxn = transaction.RawTransaction;
        Serializer s = new();
        s.FixedBytes(rawTxn.BcsToBytes());
        s.Option(Proof.Proof);
        return Sign(SigningMessage.Generate(s.ToBytes(), DOMAIN_SEPARATOR));
    }

    public override Signature Sign(byte[] message)
    {
        if (EphemeralKeyPair.IsExpired())
            throw new Exception("Ephemeral keypair has expired");
        var token = new JsonWebToken(Jwt);
        return new KeylessSignature(
            ephemeralCertificate: new EphemeralCertificate(
                Proof,
                EphemeralSignatureVariant.ZkProof
            ),
            jwtHeader: Base64UrlEncoder.Decode(token.EncodedHeader),
            expiryDateSecs: EphemeralKeyPair.ExpiryTimestamp,
            ephemeralPublicKey: EphemeralKeyPair.PublicKey,
            ephemeralSignature: EphemeralKeyPair.Sign(message)
        );
    }

    public override AccountAuthenticator SignWithAuthenticator(byte[] message) =>
        new AccountAuthenticatorSingleKey(
            _verifyingKey.PublicKey,
            (PublicKeySignature)Sign(message)
        );

    public override AccountAuthenticator SignWithAuthenticator(AnyRawTransaction transaction) =>
        new AccountAuthenticatorSingleKey(
            _verifyingKey.PublicKey,
            // Have to declare explictly because the virtual method signs without proof
            (PublicKeySignature)Sign(transaction)
        );

    public void Serialize(Serializer s)
    {
        s.String(Jwt);
        s.String(UidKey);
        s.FixedBytes(Pepper);
        EphemeralKeyPair.Serialize(s);
        Proof.Serialize(s);
    }

    public static KeylessAccount Deserialize(Deserializer d)
    {
        string jwt = d.String();
        string uidKey = d.String();
        byte[] pepper = d.FixedBytes(PEPPER_LENGTH);
        EphemeralKeyPair ekp = EphemeralKeyPair.Deserialize(d);
        ZeroKnowledgeSignature proof = ZeroKnowledgeSignature.Deserialize(d);
        return new KeylessAccount(jwt, ekp, proof, pepper, uidKey);
    }
}
