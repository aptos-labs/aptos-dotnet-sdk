namespace Aptos;

using Aptos.Schemes;

/// <summary>
/// Represents a MultiKey signer used to sign transactions with the MultiKey authentication scheme.
///
/// This accounts to use a M of N signing scheme. M and N are specified in the <see cref="MultiKey"/>.
/// It signs messages via the array of M number of Accounts that individually correspond to a public key in the <see cref="MultiKey"/>.
/// </summary>
public class MultiKeyAccount : Account
{
    private readonly MultiKey _publicKey;

    /// <summary>
    /// Gets the MultiKeyPublicKey for the account.
    /// </summary>
    public override AccountPublicKey PublicKey => _publicKey;

    /// <summary>
    /// Gets the address of the account.
    /// </summary>
    public override AccountAddress Address => PublicKey.AuthKey().DerivedAddress();

    /// <summary>
    /// The signers used to sign messages. These signers should correspond to public keys in the
    /// MultiKeyAccount's public key. The number of signers should be equal or greater than the
    /// number of public keys in the MultiKeyAccount's public key.
    /// </summary>
    public readonly List<Account> Signers;

    /// <summary>
    /// The corresponding indicies of the signers in the MultiKeyAccount's public key.
    /// <br/>
    /// Example: If the MultiKey has 3 public keys [0x1, 0x2, 0x3] and signers [0x1, 0x3],
    /// then the signer indices would be [0, 2].
    /// </summary>
    public readonly int[] SignerIndicies;

    /// <summary>
    /// The MultiKeyAccount uses a MultiKey signing scheme.
    /// </summary>
    public override SigningScheme SigningScheme => SigningScheme.MultiKey;

    /// <summary>
    /// Initializes a new instance of the MultiKeyAccount class with a MultiKey and a list of signers.
    ///
    /// The signers should correspond to public keys in the MultiKeyAccount's public key.
    /// </summary>
    /// <param name="multiKey">The MultiKey to use for signing.</param>
    /// <param name="signers">The signers to use for signing.</param>
    /// <exception cref="ArgumentException">If the signers do not correspond to public keys in the MultiKeyAccount's public key.</exception>
    public MultiKeyAccount(MultiKey multiKey, List<Account> signers)
    {
        _publicKey = multiKey;

        // Get the index of each respective signer in the bitmpa
        List<int> bitPositions = [];
        for (int i = 0; i < signers.Count; i++)
        {
            var signer = signers[i];
            bitPositions.Add(multiKey.GetIndex(signer.PublicKey));
        }

        if (signers.Any(s => s.PublicKey is UnifiedAccountPublicKey))
            throw new ArgumentException(
                "MultiKeyAccount cannot be used with unified account public keys (e.g. AnyPublicKey or MultiKeyPublicKey)"
            );
        if (multiKey.SignaturesRequired > signers.Count)
            throw new ArgumentException(
                $"Signatures required must be less than or equal to the number of signers"
            );

        // Zip signers and bit positions and sort signers by bit positions in order
        // to ensure the signature is signed in ascending order according to the bitmap.
        // Authentication on chain will fail otherwise.
        var signersAndBitPosition = signers
            .Select((signer, index) => new { signer, index })
            .ToList();
        signersAndBitPosition.Sort((a, b) => a.index - b.index);
        Signers = signersAndBitPosition.Select(value => value.signer).ToList();
        SignerIndicies = signersAndBitPosition.Select(value => value.index).ToArray();
    }

    /// <inheritdoc cref="VerifySignature(byte[], MultiKeySignature)"/>
    public bool VerifySignature(string message, MultiKeySignature signature) =>
        VerifySignature(SigningMessage.Convert(message), signature);

    /// <summary>
    /// Verifies a signature for a given message.
    /// </summary>
    /// <param name="message">The message that was signed.</param>
    /// <param name="signature">The signed message to verify.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    public bool VerifySignature(byte[] message, MultiKeySignature signature)
    {
        bool isSorted = SignerIndicies.All(i => i == 0 || SignerIndicies[i - 1] < i);
        if (!isSorted)
            return false;

        for (int i = 0; i < signature.Signatures.Count; i++)
        {
            Signature singleSignature = signature.Signatures[i];
            PublicKey singlePublicKey = _publicKey.PublicKeys[SignerIndicies[i]];
            if (!singlePublicKey.VerifySignature(message, singleSignature))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Signs a message into a signature using the signer.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The signed message.</returns>
    public override Signature Sign(byte[] message) =>
        new MultiKeySignature(
            Signers.Select(s => s.Sign(message)).ToList(),
            MultiKey.CreateBitmap(SignerIndicies)
        );

    /// <summary>
    /// Signs a transaction using the signer.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <returns>The transaction signature.</returns>
    public override Signature SignTransaction(AnyRawTransaction transaction) =>
        new MultiKeySignature(
            Signers.Select(s => s.SignTransaction(transaction)).ToList(),
            MultiKey.CreateBitmap(SignerIndicies)
        );

    /// <summary>
    /// Signs a message and returns an authenticator with the signature.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The authenticator containing the signature.</returns>
    public override AccountAuthenticator SignWithAuthenticator(byte[] message) =>
        new AccountAuthenticatorMultiKey(_publicKey, (MultiKeySignature)Sign(message));

    /// <summary>
    /// Signs a message and returns an authenticator with the signature.
    /// </summary>
    /// <param name="message">The message to sign as a byte array.</param>
    /// <returns>The authenticator containing the signature.</returns>
    public override AccountAuthenticator SignTransactionWithAuthenticator(
        AnyRawTransaction transaction
    ) =>
        new AccountAuthenticatorMultiKey(
            _publicKey,
            (MultiKeySignature)SignTransaction(transaction)
        );
}
