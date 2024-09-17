namespace Aptos;

using Aptos.Schemes;

/// <summary>
/// Abstract class representing a signer account.
/// </summary>
public abstract class Account
{
    /// <summary>
    /// Gets the public key of the account.
    /// </summary> 
    public abstract PublicKey PublicKey { get; }

    /// <summary>
    /// Gets the address of the account.
    /// </summary>
    public abstract AccountAddress Address { get; }

    /// <summary>
    /// Gets the signing scheme used by the account.
    /// </summary>
    public abstract SigningScheme SigningScheme { get; }

    /// <summary>
    /// Gets the authentication key for the account.
    /// </summary>
    /// <returns>The authentication key for the account.</returns>
    public abstract AuthenticationKey AuthKey();

    /// <inheritdoc cref="Sign(byte[])"/>
    public Signature Sign(string message) => Sign(SigningMessage.Convert(message));

    /// <summary>
    /// Signs a message with the using the signer.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The signed message.</returns>
    public abstract Signature Sign(byte[] message);

    /// <summary>
    /// Signs a transaction using the account's private key.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <returns>The signature of the transaction.</returns>
    public abstract Signature SignTransaction(AnyRawTransaction transaction);

    /// <inheritdoc cref="SignWithAuthenticator(byte[])"/>
    public AccountAuthenticator SignWithAuthenticator(string message) => SignWithAuthenticator(SigningMessage.Convert(message));

    /// <summary>
    /// Signs a message and returns an authenticator for the account.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The authenticator containing the signature.</returns>
    public abstract AccountAuthenticator SignWithAuthenticator(byte[] message);

    /// <summary>
    /// Signs a transaction and returns an authenticator for the account.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <returns>The authenticator containing the signature.</returns>
    public abstract AccountAuthenticator SignTransactionWithAuthenticator(AnyRawTransaction transaction);

    /// <summary>
    /// Generates a new Ed25519 account.
    /// </summary>
    /// <returns>A new instance of <see cref="Ed25519Account"/>.</returns>
    public static Ed25519Account Generate() => Ed25519Account.Generate();
}