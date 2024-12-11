namespace Aptos;

public enum TypeTagVariant : uint
{
    Bool = 0,
    U8 = 1,
    U64 = 2,
    U128 = 3,
    Address = 4,
    Signer = 5,
    Vector = 6,
    Struct = 7,
    U16 = 8,
    U32 = 9,
    U256 = 10,
    Reference = 254,
    Generic = 255,
}

public abstract partial class TypeTag(TypeTagVariant variant) : Serializable
{
    public readonly TypeTagVariant Variant = variant;

    public override void Serialize(Serializer s) => s.U32AsUleb128((uint)Variant);

    public override string ToString() => $"{Variant.ToString().ToLower()}";

    public bool CheckType(object value)
    {
        switch (Variant)
        {
            case TypeTagVariant.Bool:
                return value is Bool;
            case TypeTagVariant.U8:
                return value is U8;
            case TypeTagVariant.U16:
                return value is U16;
            case TypeTagVariant.U32:
                return value is U32;
            case TypeTagVariant.U64:
                return value is U64;
            case TypeTagVariant.U128:
                return value is U128;
            case TypeTagVariant.U256:
                return value is U256;
            case TypeTagVariant.Address:
                return value is AccountAddress;
            case TypeTagVariant.Vector:
                {
                    if (this is TypeTagVector typeTagVector)
                        if (value.GetType().Name.Contains("MoveVector"))
                        {
                            // Cannot pattern match on MoveVector<T> because it's a generic type
                            MoveVector<object>? vector = value as MoveVector<object>;
                            if (vector == null)
                                return false;
                            return typeTagVector.Value.CheckType(vector.Values[0]);
                        }
                }
                return false;
            case TypeTagVariant.Struct:
                // Cannot pattern match on <T> because it's a generic type
                if (this is TypeTagStruct typeTagStruct)
                {
                    if (typeTagStruct.IsStringTypeTag())
                        return value is MoveString;
                    if (typeTagStruct.IsObjectTypeTag())
                        return value is AccountAddress;
                    if (typeTagStruct.IsOptionTypeTag())
                    {
                        if (value.GetType().Name.Contains("MoveOption"))
                        {
                            // Cannot pattern match on MoveVector<T> because it's a generic type
                            MoveOption<object>? option = value as MoveOption<object>;
                            if (option?.Value != null && typeTagStruct.Value.TypeArgs.Count > 0)
                            {
                                return typeTagStruct.Value.TypeArgs[0].CheckType(option?.Value!);
                            }
                        }
                    }
                }
                return false;
        }
        return false;
    }

    public static TypeTag Deserialize(Deserializer d)
    {
        TypeTagVariant variant = (TypeTagVariant)d.Uleb128AsU32();
        return variant switch
        {
            TypeTagVariant.Bool => TypeTagBool.Deserialize(d),
            TypeTagVariant.U8 => TypeTagU8.Deserialize(d),
            TypeTagVariant.U16 => TypeTagU16.Deserialize(d),
            TypeTagVariant.U32 => TypeTagU32.Deserialize(d),
            TypeTagVariant.U64 => TypeTagU64.Deserialize(d),
            TypeTagVariant.U128 => TypeTagU128.Deserialize(d),
            TypeTagVariant.U256 => TypeTagU256.Deserialize(d),
            TypeTagVariant.Address => TypeTagAddress.Deserialize(d),
            TypeTagVariant.Signer => TypeTagSigner.Deserialize(d),
            TypeTagVariant.Vector => TypeTagVector.Deserialize(d),
            TypeTagVariant.Struct => TypeTagStruct.Deserialize(d),
            TypeTagVariant.Reference => TypeTagReference.Deserialize(d),
            TypeTagVariant.Generic => TypeTagGeneric.Deserialize(d),
            _ => throw new ArgumentException("Invalid variant"),
        };
    }
}

public class TypeTagBool() : TypeTag(TypeTagVariant.Bool)
{
    public static new TypeTagBool Deserialize(Deserializer d) => new();
}

public class TypeTagU8() : TypeTag(TypeTagVariant.U8)
{
    public static new TypeTagU8 Deserialize(Deserializer d) => new();
}

public class TypeTagU16() : TypeTag(TypeTagVariant.U16)
{
    public static new TypeTagU16 Deserialize(Deserializer d) => new();
}

public class TypeTagU32() : TypeTag(TypeTagVariant.U32)
{
    public static new TypeTagU32 Deserialize(Deserializer d) => new();
}

public class TypeTagU64() : TypeTag(TypeTagVariant.U64)
{
    public static new TypeTagU64 Deserialize(Deserializer d) => new();
}

public class TypeTagU128() : TypeTag(TypeTagVariant.U128)
{
    public static new TypeTagU128 Deserialize(Deserializer d) => new();
}

public class TypeTagU256() : TypeTag(TypeTagVariant.U256)
{
    public static new TypeTagU256 Deserialize(Deserializer d) => new();
}

public class TypeTagAddress() : TypeTag(TypeTagVariant.Address)
{
    public static new TypeTagAddress Deserialize(Deserializer d) => new();
}

public class TypeTagSigner() : TypeTag(TypeTagVariant.Signer)
{
    public static new TypeTagSigner Deserialize(Deserializer d) => new();
}

public class TypeTagReference(TypeTag value) : TypeTag(TypeTagVariant.Reference)
{
    public readonly TypeTag Value = value;

    public override string ToString() => $"&{Value}";

    public static new TypeTagReference Deserialize(Deserializer d) => new(TypeTag.Deserialize(d));
}

public class TypeTagGeneric : TypeTag
{
    public readonly uint Value;

    public TypeTagGeneric(uint value)
        : base(TypeTagVariant.Generic)
    {
        Value = value;
    }

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TypeTagVariant.Generic);
        s.U32(Value);
    }

    public override string ToString() => $"T{Value}";

    public static new TypeTagGeneric Deserialize(Deserializer d) => new(d.U32());
}

public class TypeTagVector(TypeTag value) : TypeTag(TypeTagVariant.Vector)
{
    public readonly TypeTag Value = value;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TypeTagVariant.Vector);
        Value.Serialize(s);
    }

    public override string ToString() => $"vector<{Value}>";

    public static new TypeTagVector Deserialize(Deserializer d) => new(TypeTag.Deserialize(d));
}

public class TypeTagStruct(StructTag value) : TypeTag(TypeTagVariant.Struct)
{
    public readonly StructTag Value = value;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TypeTagVariant.Struct);
        Value.Serialize(s);
    }

    public override string ToString()
    {
        string typePredicate = "";
        if (Value.TypeArgs.Count > 0)
            typePredicate = $"<{string.Join(", ", Value.TypeArgs)}>";
        return $"{Value.Address}::{Value.ModuleName}::{Value.Name}{typePredicate}";
    }

    public bool IsTypeTag(AccountAddress address, string moduleName, string name) =>
        Value.Address.Equals(address) && Value.ModuleName == moduleName && Value.Name == name;

    public bool IsStringTypeTag() =>
        IsTypeTag(AccountAddress.FromString("0x1"), "string", "String");

    public bool IsObjectTypeTag() =>
        IsTypeTag(AccountAddress.FromString("0x1"), "object", "Object");

    public bool IsOptionTypeTag() =>
        IsTypeTag(AccountAddress.FromString("0x1"), "option", "Option");

    public static new TypeTagStruct Deserialize(Deserializer d) => new(StructTag.Deserialize(d));
}
