namespace Aptos;

using System.Text.RegularExpressions;
using Aptos.Exceptions;

public static class Ans
{
    public static string? GetRouterAddress(NetworkConfig network) => network.Name.ToLower() switch
    {
        "mainnet" => "0x867ed1f6bf916171b1de3ee92849b8978b7d1b9e0a8cc982a3d19d535dfd9c0c",
        "testnet" => "0x10x5f8fd2347449685cf41d4db97926ec3a096eaf381332be4f1318ad4d16a8497c",
        "local" => "0x585fc9f0f0c54183b039ffc770ca282ebd87307916c215a3e692f2f8e4305e82",
        _ => null,
    };

    /// <summary>
    /// Ensures that the ANS segment is valid. Throws a ANSInvalidName exception if it is not.
    /// 
    /// A valid ANS segment is a string between 3 and 63 characters long, and only contains lowercase a-z, 0-9, and hyphens. A segment may not start or end with a hyphen.
    /// </summary>
    /// <param name="segment">domain or subdomain of a name</param>
    /// <exception cref="ANSInvalidName"></exception>
    public static void EnsureValidANSSegment(string segment)
    {
        if (segment.Length < 3) throw new ANSInvalidName(segment, InvalidANSNameReason.InvalidLength);
        if (segment.Length > 63) throw new ANSInvalidName(segment, InvalidANSNameReason.InvalidLength);
        // only lowercase a-z and 0-9 are allowed, along with -. a domain may not start or end with a hyphen
        if (!Regex.IsMatch(segment, @"^[a-z\d][a-z\d-]{1,61}[a-z\d]$")) throw new ANSInvalidName(segment, InvalidANSNameReason.InvalidCharacters);
    }

    public static (string domain, string? subdomain) ParseAnsName(string name)
    {
        var parts = name.Replace(".apt", "").Split('.').ToList();

        if (parts.Count > 2) throw new ANSInvalidName(name, InvalidANSNameReason.GreaterThanTwoParts);

        string? domain = parts.ElementAtOrDefault(0);
        if (domain == null) throw new ANSInvalidName(name, InvalidANSNameReason.MissingDomain);
        EnsureValidANSSegment(domain);

        string? subdomain = parts.ElementAtOrDefault(1);
        if (subdomain != null) EnsureValidANSSegment(subdomain);

        return (domain: subdomain ?? domain, subdomain: subdomain != null ? domain : null);
    }
}