namespace Aptos.Exceptions
{

    public class InvalidPublicKey(PublicKey publicKey, string message) : BaseException($"{publicKey.GetType().Name} is not supported: ${message}") { }

    public class WaitForTransactionException(string message, TransactionResponse? lastResponse = null) : BaseException(message)
    {
        public TransactionResponse? LastResponse { get; } = lastResponse;
    }

    public class FailedTransactionException(string message, CommittedTransactionResponse? transaction = null) : BaseException(message)
    {
        public CommittedTransactionResponse? Transaction { get; } = transaction;
    }

}
