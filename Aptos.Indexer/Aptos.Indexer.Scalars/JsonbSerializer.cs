namespace Aptos.Indexer.Scalars;

using System.Text.Json;
using StrawberryShake.Serialization;

public class JsonbSerializer : ScalarSerializer<JsonElement>
{
    public JsonbSerializer() : base("jsonb") { }
}