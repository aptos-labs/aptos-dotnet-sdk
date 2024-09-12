namespace Aptos;

public class FixedBytes(byte[] value) : TransactionArgument
{
    public readonly byte[] Value = value;

    public FixedBytes(string value) : this(Hex.FromHexInput(value).ToByteArray()) { }

    public override void Serialize(Serializer s) => s.FixedBytes(Value);

    public override void SerializeForScriptFunction(Serializer s) => s.Serialize(this);

    public new void SerializeForEntryFunction(Serializer s) => s.Serialize(this);

    public static FixedBytes Deserialize(Deserializer d, int length) => new(d.FixedBytes(length));

}