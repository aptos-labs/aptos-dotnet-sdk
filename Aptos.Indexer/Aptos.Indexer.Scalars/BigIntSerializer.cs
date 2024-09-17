namespace Aptos.Indexer.Scalars;

using StrawberryShake.Serialization;

public class BigIntSerializer : ScalarSerializer<long>
{
    public BigIntSerializer()
        : base("bigint") { }
}
