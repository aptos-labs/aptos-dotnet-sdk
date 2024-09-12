using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aptos.Schemes
{

    public enum AuthenticationKeyScheme
    {
        Ed25519 = 0,
        MultiEd25519 = 1,
        SingleKey = 2,
        MultiKey = 3,
        DeriveAuid = 251,
        DeriveObjectAddressFromObject = 252,
        DeriveObjectAddressFromGuid = 253,
        DeriveObjectAddressFromSeed = 254,
        DeriveResourceAccountAddress = 255
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SigningScheme
    {
        [EnumMember(Value = "ed25519_signature")]
        Ed25519 = AuthenticationKeyScheme.Ed25519,
        [EnumMember(Value = "multi_ed25519_signature")]
        MultiEd25519 = AuthenticationKeyScheme.MultiEd25519,
        [EnumMember(Value = "single_key_signature")]
        SingleKey = AuthenticationKeyScheme.SingleKey,
        [EnumMember(Value = "multi_key_signature")]
        MultiKey = AuthenticationKeyScheme.MultiKey,
    }

    public enum DeriveScheme
    {
        DeriveAuid = AuthenticationKeyScheme.DeriveAuid,
        DeriveObjectAddressFromObject = AuthenticationKeyScheme.DeriveObjectAddressFromObject,
        DeriveObjectAddressFromGuid = AuthenticationKeyScheme.DeriveObjectAddressFromGuid,
        DeriveObjectAddressFromSeed = AuthenticationKeyScheme.DeriveObjectAddressFromSeed,
        DeriveResourceAccountAddress = AuthenticationKeyScheme.DeriveResourceAccountAddress
    }

}