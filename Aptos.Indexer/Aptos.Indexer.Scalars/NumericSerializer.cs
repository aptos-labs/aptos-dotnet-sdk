namespace Aptos.Indexer.Scalars;

using StrawberryShake.Serialization;

public class NumericSerializer : ScalarSerializer<decimal>
{
    public NumericSerializer()
        : base("numeric") { }
}
