namespace Aptos;

public class SubmitTransactionData(AnyRawTransaction transaction, AccountAuthenticator senderAuthenticator, AccountAuthenticator? feePayerAuthenticator = null, List<AccountAuthenticator>? additionalSignersAuthenticators = null)
{
    public AnyRawTransaction Transaction = transaction;
    public AccountAuthenticator SenderAuthenticator = senderAuthenticator;
    public AccountAuthenticator? FeePayerAuthenticator = feePayerAuthenticator;
    public List<AccountAuthenticator>? AdditionalSignersAuthenticators = additionalSignersAuthenticators;
}