namespace Aptos;

using Newtonsoft.Json;

public class MoveModuleBytecode(string bytecode, MoveModule? abi)
{
    [JsonProperty("bytecode")]
    public string Bytecode = bytecode;

    [JsonProperty("abi")]
    public MoveModule? Abi = abi;
}

public class MoveModule(
    string address,
    string name,
    List<string> friends,
    List<MoveFunction> exposedFunctions,
    List<MoveStruct> structs
)
{
    [JsonProperty("address")]
    public string Address = address;

    [JsonProperty("name")]
    public string Name = name;

    [JsonProperty("friends")]
    public List<string> Friends = friends;

    [JsonProperty("exposed_functions")]
    public List<MoveFunction> ExposedFunctions = exposedFunctions;

    [JsonProperty("structs")]
    public List<MoveStruct> Structs = structs;
}

public class MoveStruct(
    string name,
    bool isNative,
    List<MoveAbility> abilities,
    List<MoveFunctionGenericTypeParam> genericTypeParams,
    List<MoveStructField> fields
)
{
    [JsonProperty("name")]
    public string Name = name;

    [JsonProperty("is_native")]
    public bool IsNative = isNative;

    [JsonProperty("abilities")]
    public List<MoveAbility> Abilities = abilities;

    [JsonProperty("generic_type_params")]
    public List<MoveFunctionGenericTypeParam> GenericTypeParams = genericTypeParams;

    [JsonProperty("fields")]
    public List<MoveStructField> Fields = fields;
}

public class MoveStructField(string name, string type)
{
    [JsonProperty("name")]
    public string Name = name;

    [JsonProperty("type")]
    public string Type = type;
}

public class MoveScriptBytecode(string bytecode, MoveFunction abi)
{
    [JsonProperty("bytecode")]
    public string Bytecode = bytecode;

    [JsonProperty("abi")]
    public MoveFunction Abi = abi;
}

public class MoveFunction(
    string name,
    MoveFunctionVisibility visibility,
    bool isEntry,
    bool isView,
    List<MoveFunctionGenericTypeParam> genericTypeParams,
    List<string> parameters,
    List<string> returnType
)
{
    [JsonProperty("name")]
    public string Name = name;

    [JsonProperty("visibility")]
    public MoveFunctionVisibility Visibility = visibility;

    [JsonProperty("is_entry")]
    public bool IsEntry = isEntry;

    [JsonProperty("is_view")]
    public bool IsView = isView;

    [JsonProperty("generic_type_params")]
    public List<MoveFunctionGenericTypeParam> GenericTypeParams = genericTypeParams;

    [JsonProperty("params")]
    public List<string> Parameters = parameters;

    [JsonProperty("return")]
    public List<string> Return = returnType;
}

public enum MoveFunctionVisibility
{
    [JsonProperty("private")]
    Private,

    [JsonProperty("public")]
    Public,

    [JsonProperty("friend")]
    Friend,
}

public class MoveFunctionGenericTypeParam(List<MoveAbility> constraints)
{
    [JsonProperty("constraints")]
    public List<MoveAbility> Constraints = constraints;
}

public enum MoveAbility
{
    [JsonProperty("store")]
    Store,

    [JsonProperty("drop")]
    Drop,

    [JsonProperty("key")]
    Key,

    [JsonProperty("copy")]
    Copy,
}

public class MoveResource(string type, object data)
{
    [JsonProperty("type")]
    public string Type = type;

    [JsonProperty("data")]
    public object Data = data;
}
