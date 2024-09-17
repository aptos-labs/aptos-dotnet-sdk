namespace Aptos;

public class EntryFunctionBytes : Serializable, IEntryFunctionArgument
{
    public readonly FixedBytes Value;

    private EntryFunctionBytes(byte[] value) => Value = new FixedBytes(value);

    public override void Serialize(Serializer s) => s.Serialize(Value);

    public void SerializeForEntryFunction(Serializer s)
    {
        s.U32AsUleb128((uint)Value.Value.Length);
        s.Serialize(this);
    }

    public static EntryFunctionBytes Deserialize(Deserializer d, int length) =>
        new(FixedBytes.Deserialize(d, length).Value);
}
