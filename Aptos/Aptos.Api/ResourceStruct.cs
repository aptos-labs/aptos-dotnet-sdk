namespace Aptos;

using Aptos.Core;
using Newtonsoft.Json;

public class ResourceStruct(List<ResourceStruct.InnerStruct> vec)
{
    /// <summary>
    /// Resource structs will have an inner struct inside a vec. The inner struct will contain the account address, module name, and struct name. Typically
    /// the module name and struct name will be in hex format.
    /// <br/>
    /// Example of 0x1::aptos_coin::AptosCoin struct:
    /// {
    ///     account_address: "0x1",
    ///     module_name: "0x6170746f735f636f696e",
    ///     struct_name: "0x4170746f73436f696e",
    /// }
    /// </summary>
    public class InnerStruct(string accountAddress, string moduleName, string structName)
    {
        [JsonProperty("account_address")]
        public string AccountAddress = accountAddress;

        [JsonProperty("module_name")]
        public string ModuleName = moduleName;

        [JsonProperty("struct_name")]
        public string StructName = structName;
    }

    [JsonProperty("vec")]
    public List<InnerStruct> Vec = vec;

    public override string ToString()
    {
        if (Vec.Count == 0)
            throw new ArgumentException("Invalid resource struct provided. No inner struct found.");
        var innerStruct = Vec[0];
        return $"{innerStruct.AccountAddress}::{Utilities.HexStringToString(innerStruct.ModuleName)}::{Utilities.HexStringToString(innerStruct.StructName)}";
    }
}
