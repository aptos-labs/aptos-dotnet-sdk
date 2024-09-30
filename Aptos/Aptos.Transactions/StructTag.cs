namespace Aptos;

public class StructTag(
    AccountAddress address,
    string moduleName,
    string name,
    List<TypeTag>? typeArgs = null
) : Serializable
{
    public static StructTag OBJECT => new(AccountAddress.FromString("0x1"), "object", "Object");

    public static StructTag OPTION => new(AccountAddress.FromString("0x1"), "option", "Option");

    public static StructTag STRING => new(AccountAddress.FromString("0x1"), "string", "String");

    public static StructTag TAG => new(AccountAddress.FromString("0x1"), "tag", "Tag");

    public readonly AccountAddress Address = address;

    public readonly string ModuleName = moduleName;

    public readonly string Name = name;

    public readonly List<TypeTag> TypeArgs = typeArgs ?? [];

    public StructTag(StructTag other, List<TypeTag>? typeArgs = null)
        : this(other.Address, other.ModuleName, other.Name, typeArgs ?? other.TypeArgs) { }

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
