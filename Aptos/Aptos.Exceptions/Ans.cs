namespace Aptos.Exceptions
{
    public enum InvalidANSNameReason
    {
        GreaterThanTwoParts,
        MissingDomain,
        InvalidLength,
        InvalidCharacters,
    }


    public class ANSInvalidName(string name, InvalidANSNameReason reason) : BaseException($"\"{name}\" is an invalid ANS name. {GetInvalidReason(reason)}")
    {
        private static string GetInvalidReason(InvalidANSNameReason? typeTag) => typeTag switch
        {
            InvalidANSNameReason.MissingDomain => "A name must have a domain.",
            InvalidANSNameReason.GreaterThanTwoParts => "A name can only have two parts, a domain and a subdomain separated by a \".\"",
            InvalidANSNameReason.InvalidLength => "A name must be between 2 and 63 characters long",
            InvalidANSNameReason.InvalidCharacters => "A name can only contain lowercase a-z, 0-9, and hyphens. A name may not start or end with a hyphen.",
            _ => "unknown error"
        };
    }

    public class ANSUnsupportedNetwork() : BaseException("No ANS resolver address is available for this network.") { }

}
