namespace Aptos;

public class ModuleId(AccountAddress address, string name) : Serializable
{

    public readonly AccountAddress Address = address;

    public readonly string Name = name;

    public override void Serialize(Serializer s)
    {
        Address.Serialize(s);
        s.String(Name);
    }

    public static ModuleId Deserialize(Deserializer d) => new(AccountAddress.Deserialize(d), d.String());

    public static ModuleId FromString(string value)
    {
        string[] parts = value.Split("::");
        if (parts.Length != 2) throw new ArgumentException("Invalid module id");
        return new ModuleId(AccountAddress.FromString(parts[0]), parts[1]);
    }
}