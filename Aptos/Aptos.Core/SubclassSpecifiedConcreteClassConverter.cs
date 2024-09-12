namespace Aptos;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

class SubclassSpecifiedConcreteClassConverter<T> : DefaultContractResolver
{
    protected override JsonConverter? ResolveContractConverter(Type objectType)
    {
        if (typeof(T).IsAssignableFrom(objectType) && !objectType.IsAbstract)
            return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
        return base.ResolveContractConverter(objectType);
    }
}
