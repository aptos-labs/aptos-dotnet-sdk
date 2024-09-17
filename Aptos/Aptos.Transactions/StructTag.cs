namespace Aptos;

public class StructTag(
    AccountAddress address,
    string moduleName,
    string name,
    List<TypeTag> typeArgs
) : Serializable
{
    public readonly AccountAddress Address = address;

    public readonly string ModuleName = moduleName;

    public readonly string Name = name;

    public readonly List<TypeTag> TypeArgs = typeArgs;

    public override void Serialize(Serializer s)
    {
        s.Serialize(Address);
        s.String(ModuleName);
        s.String(Name);
        s.Vector(TypeArgs);
    }

    public static StructTag Deserialize(Deserializer d) =>
        new(AccountAddress.Deserialize(d), d.String(), d.String(), d.Vector(TypeTag.Deserialize));
}
