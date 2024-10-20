namespace Aptos.Exceptions
{
    public enum HexInvalidReason
    {
        TooShort,
        InvalidLength,
        InvalidCharacters,
    }

    public class HexException(string message, HexInvalidReason reason) : BaseException(message)
    {
        public HexInvalidReason Reason { get; } = reason;
    }

    public enum AccountAddressInvalidReason
    {
        IncorrectNumberOfBytes,
        InvalidHexChars,
        TooShort,
        TooLong,
        LeadingZeroXRequired,
        LongFormRequiredUnlessSpecial,
        InvalidPaddingZeroes,
        InvalidPaddingStrictness,
    }

    public class AccountAddressParsingException(string message, AccountAddressInvalidReason reason)
        : BaseException(message)
    {
        public AccountAddressInvalidReason Reason { get; } = reason;
    }

    public class UnexpectedResponseException(string message) : BaseException(message) { }
}
