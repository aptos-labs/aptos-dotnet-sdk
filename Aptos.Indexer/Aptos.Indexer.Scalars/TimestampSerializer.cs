namespace Aptos.Indexer.Scalars;

using StrawberryShake.Serialization;

public class TimestampSerializer : ScalarSerializer<string, DateTime>
{
    public TimestampSerializer()
        : base("timestamp") { }

    public override DateTime Parse(string runtimeValue) => DateTime.Parse(runtimeValue);

    protected override string Format(DateTime serializedValue) =>
        serializedValue.ToString("yyyy-MM-ddTHH:mm:ss");
}
