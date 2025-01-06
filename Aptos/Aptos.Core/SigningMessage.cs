namespace Aptos;

using System.Text;
using Org.BouncyCastle.Crypto.Digests;

public static class SigningMessage
{
    public const string RAW_TRANSACTION_SALT = "APTOS::RawTransaction";

    public const string RAW_TRANSACTION_WITH_DATA_SALT = "APTOS::RawTransactionWithData";

    public static byte[] Convert(string message) =>
        Hex.IsValid(message)
            ? Hex.FromHexInput(message).ToByteArray()
            : Encoding.ASCII.GetBytes(message);

    public static byte[] Convert(byte[] message) => message;

    public static byte[] Generate(byte[] message, string domainSeparator)
    {
        Sha3Digest digest = new();

        if (!domainSeparator.StartsWith("APTOS::"))
        {
            throw new ArgumentException(
                $"Domain separator needs to start with 'APTOS::'.  Provided - {domainSeparator}"
            );
        }

        byte[] domainSeparatorBytes = Encoding.UTF8.GetBytes(domainSeparator);
        digest.BlockUpdate(domainSeparatorBytes, 0, domainSeparatorBytes.Length);

        byte[] prefix = new byte[digest.GetDigestSize()];
        digest.DoFinal(prefix, 0);

        byte[] body = message;

        byte[] mergedArray = new byte[prefix.Length + body.Length];
        Array.Copy(prefix, mergedArray, prefix.Length);
        Array.Copy(body, 0, mergedArray, prefix.Length, body.Length);

        return mergedArray;
    }

    public static byte[] GenerateForTransaction(AnyRawTransaction transaction)
    {
        if (transaction.FeePayerAddress != null)
        {
            FeePayerRawTransaction rawTxnWithData = new(
                transaction.RawTransaction,
                transaction.SecondarySignerAddresses ?? [],
                transaction.FeePayerAddress
            );
            return Generate(rawTxnWithData.BcsToBytes(), RAW_TRANSACTION_WITH_DATA_SALT);
        }

        if (transaction.SecondarySignerAddresses != null)
        {
            MultiAgentRawTransaction rawTxnWithData = new(
                transaction.RawTransaction,
                transaction.SecondarySignerAddresses!
            );
            return Generate(rawTxnWithData.BcsToBytes(), RAW_TRANSACTION_WITH_DATA_SALT);
        }

        return Generate(transaction.RawTransaction.BcsToBytes(), RAW_TRANSACTION_SALT);
    }
}
