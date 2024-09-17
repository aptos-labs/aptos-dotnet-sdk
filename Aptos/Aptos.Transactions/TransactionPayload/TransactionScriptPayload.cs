namespace Aptos;

public class TransactionScriptPayload(Script script) : TransactionPayload
{
    public readonly Script Script = script;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionPayloadVariant.Script);
        s.Serialize(Script);
    }

    public static new TransactionScriptPayload Deserialize(Deserializer d)
    {
        Script script = Script.Deserialize(d);
        return new TransactionScriptPayload(script);
    }
}
