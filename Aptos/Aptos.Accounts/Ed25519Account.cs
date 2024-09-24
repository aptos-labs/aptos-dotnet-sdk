namespace Aptos;

using Aptos.Schemes;

/// <summary>
/// Represents an Ed25519 signer used to sign transaction with a Ed25519 private key.
/// </summary>
public class Ed25519Account : Account
{
    /// <summary>
    /// Gets the Ed25519PrivateKey for the account.
    /// </summary>
    public readonly Ed25519PrivateKey PrivateKey;

    private readonly Ed25519PublicKey _verifyingKey;

    /// <summary>
    /// Gets the Ed25519PublicKey for the account.
    /// </summary>
    public override IVerifyingKey VerifyingKey => _verifyingKey;

    /// <summary>
    /// Gets the Ed25519PublicKey for the account.
    /// </summary>
    public PublicKey PublicKey => _verifyingKey;

    private readonly AccountAddress _address;

    /// <summary>
    /// Gets the address of the account.
    /// </summary>
    public override AccountAddress Address => _address;

    /// <summary>
    /// The Ed25519 account uses a Ed25519 signing scheme.
    /// </summary>
    public override SigningScheme SigningScheme => SigningScheme.Ed25519;

    /// <inheritdoc cref="Ed25519Account(Ed25519PrivateKey, AccountAddress?)"/>
    public Ed25519Account(Ed25519PrivateKey privateKey)
        : this(privateKey, (AccountAddress?)null) { }

    /// <inheritdoc cref="Ed25519Account(Ed25519PrivateKey, AccountAddress?)"/>
    public Ed25519Account(Ed25519PrivateKey privateKey, string? address = null)
        : this(privateKey, address != null ? AccountAddress.From(address) : null) { }

    /// <inheritdoc cref="Ed25519Account(Ed25519PrivateKey, AccountAddress?)"/>
    public Ed25519Account(Ed25519PrivateKey privateKey, byte[]? address = null)
        : this(privateKey, address != null ? AccountAddress.From(address) : null) { }

    /// <summary>
    /// Initializes a new instance of the Ed25519Account class with a private key and an optional account address.
    /// </summary>
    /// <param name="privateKey">The private key for the account.</param>
    /// <param name="address">The account address.</param>
    public Ed25519Account(Ed25519PrivateKey privateKey, AccountAddress? address = null)
    {
        _verifyingKey = (Ed25519PublicKey)privateKey.PublicKey();
        _address = address ?? _verifyingKey.AuthKey().DerivedAddress();
        PrivateKey = privateKey;
    }

    /// <summary>
    /// Verifies a signature for a given message.
    /// </summary>
    /// <param name="message">The message that was signed.</param>
    /// <param name="signature">The signed message to verify.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    public bool VerifySignature(byte[] message, Ed25519Signature signature) =>
        _verifyingKey.VerifySignature(message, signature);

    /// <summary>
    /// Signs a message with the using the account's private key.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The signed message.</returns>
    public override Signature Sign(byte[] message) => PrivateKey.Sign(message);

    /// <summary>
    /// Signs a message and returns an authenticator with the signature.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The authenticator containing the signature.</returns>
    public override AccountAuthenticator SignWithAuthenticator(byte[] message) =>
        new AccountAuthenticatorEd25519(_verifyingKey, (Ed25519Signature)Sign(message));

    /// <summary>
    /// Generates a new Ed25519 account.
    /// </summary>
    /// <returns>A new instance of <see cref="Ed25519Account"/>.</returns>
    public static new Ed25519Account Generate() => new(Ed25519PrivateKey.Generate());

    /// <summary>
    /// Generates a new Ed25519 account from a derivation path and mnemonic.
    ///
    /// The derivation path is a string that follows the BIP-44 standard.
    /// </summary>
    /// <param name="path">The derivation path (e.g. "m/44'/637'/0'/0'/0'").</param>
    /// <param name="mnemonic">The mnemonic phrase (e.g. "abandon ... flyer about").</param>
    /// <returns>A new instance of <see cref="Ed25519Account"/>.</returns>
    public static Ed25519Account FromDerivationPath(string path, string mnemonic) =>
        new(Ed25519PrivateKey.FromDerivationPath(path, mnemonic), (AccountAddress?)null);
}
