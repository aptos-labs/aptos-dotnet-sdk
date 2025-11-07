namespace Aptos;

public class TransactionScriptExecutable(Script script) : TransactionExecutable
{
    public readonly Script Script = script;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionExecutableVariant.Script);
        s.Serialize(Script);
    }

    public static new TransactionScriptExecutable Deserialize(Deserializer d)
    {
        Script script = Script.Deserialize(d);
        return new TransactionScriptExecutable(script);
    }
}
