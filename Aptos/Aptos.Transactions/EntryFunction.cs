using Aptos.Core;

namespace Aptos;

[Serializable]
public class EntryFunction(ModuleId moduleName, string functionName, List<TypeTag> typeArgs, List<IEntryFunctionArgument> args) : Serializable
{
    public readonly ModuleId ModuleName = moduleName;

    public readonly string FunctionName = functionName;

    public readonly List<TypeTag> TypeArgs = typeArgs;

    public readonly List<IEntryFunctionArgument> Args = args;

    public override void Serialize(Serializer s)
    {
        ModuleName.Serialize(s);
        s.String(FunctionName);
        s.Vector(TypeArgs);
        s.U32AsUleb128((uint)Args.Count);
        Args.ForEach(a => a.SerializeForEntryFunction(s));
    }

    public static EntryFunction Deserialize(Deserializer d)
    {
        ModuleId moduleName = ModuleId.Deserialize(d);
        string functionName = d.String();
        List<TypeTag> typeArgs = d.Vector(TypeTag.Deserialize);

        uint length = d.Uleb128AsU32();
        List<IEntryFunctionArgument> args = [];
        for (uint i = 0; i < length; i++)
        {
            EntryFunctionBytes fixedBytes = EntryFunctionBytes.Deserialize(d, (int)d.Uleb128AsU32());
            args.Add(fixedBytes);
        }

        return new EntryFunction(moduleName, functionName, typeArgs, args);
    }

    public static EntryFunction Build(string moduleId, string functionName, List<TypeTag> typeArgs, List<IEntryFunctionArgument> args) => new(ModuleId.FromString(moduleId), functionName, typeArgs, args);

    public static EntryFunction Generate(string function, List<object> functionArguments, List<object> typeArguments, FunctionAbi functionAbi)
    {
        (string moduleAddress, string moduleName, string functionName) = Utilities.ParseFunctionParts(function);

        // Ensure that all type arguments are typed properly
        List<TypeTag> parsedTypeArguments = Utilities.StandardizeTypeTags(typeArguments);

        // Check the type arguments against the Abi
        if (parsedTypeArguments.Count != functionAbi.TypeParameters.Count) throw new ArgumentException("Type arguments count does not match Abi type parameters");

        TransactionArgument CheckAndConvertArgument(object value, int position)
        {
            if (position >= functionAbi.Parameters.Count) throw new ArgumentException($"Parsing argument out of range '{functionName}, expected ${functionAbi.Parameters.Count} but got {position}");
            return TransactionArgument.ConvertArgument(value, functionAbi.Parameters[position], parsedTypeArguments) ?? throw new ArgumentException($"Function argument type mismatch for '{functionName}' expected {functionAbi.Parameters[position]} but got {value.GetType()} at position {position}");
        }

        List<IEntryFunctionArgument> convertedFunctionArguments = functionArguments.Select(CheckAndConvertArgument).Cast<IEntryFunctionArgument>().ToList();

        if (convertedFunctionArguments.Count != functionAbi.Parameters.Count) throw new ArgumentException($"Function arguments count does not match Abi parameters. Expected {functionAbi.Parameters.Count} but got {convertedFunctionArguments.Count}");

        return Build($"{moduleAddress}::{moduleName}", functionName, parsedTypeArguments, convertedFunctionArguments);
    }
}