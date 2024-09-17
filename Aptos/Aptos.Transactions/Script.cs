namespace Aptos;

public class Script(byte[] bytecode, List<TypeTag> typeArgs, List<IScriptFunctionArgument> args)
    : Serializable
{
    public readonly byte[] Bytecode = bytecode;

    public readonly List<TypeTag> TypeArgs = typeArgs;

    public readonly List<IScriptFunctionArgument> Args = args;

    public override void Serialize(Serializer s)
    {
        s.Bytes(Bytecode);
        s.Vector(TypeArgs);
        s.U32AsUleb128((uint)Args.Count);
        Args.ForEach(a => a.SerializeForScriptFunction(s));
    }

    public static Script Deserialize(Deserializer d)
    {
        byte[] bytecode = d.Bytes();
        List<TypeTag> typeArgs = d.Vector(TypeTag.Deserialize);

        uint length = d.Uleb128AsU32();
        List<IScriptFunctionArgument> args = [];
        for (uint i = 0; i < length; i++)
        {
            // Note that we deserialize directly to the Move value, not its Script argument representation.
            // We are abstracting away the Script argument representation because knowing about it is
            // functionally useless.
            IScriptFunctionArgument scriptArgument =
                TransactionArgument.DeserializeFromScriptArgument(d);
            args.Add(scriptArgument);
        }

        return new Script(bytecode, typeArgs, args);
    }
}
