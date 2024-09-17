namespace Aptos;

using System.Collections;
using System.Numerics;

public enum ScriptTransactionArgumentVariants
{
    U8 = 0,
    U64 = 1,
    U128 = 2,
    Address = 3,
    U8Vector = 4,
    Bool = 5,
    U16 = 6,
    U32 = 7,
    U256 = 8,
}

public interface IEntryFunctionArgument
{
    public void SerializeForEntryFunction(Serializer s);
}

public interface IScriptFunctionArgument
{
    public void SerializeForScriptFunction(Serializer s);
}

public abstract class TransactionArgument
    : Serializable,
        IScriptFunctionArgument,
        IEntryFunctionArgument
{
    public abstract override void Serialize(Serializer s);

    public abstract void SerializeForScriptFunction(Serializer s);

    public void SerializeForEntryFunction(Serializer s) => s.Bytes(BcsToBytes());

    public static TransactionArgument DeserializeFromScriptArgument(Deserializer d)
    {
        ScriptTransactionArgumentVariants index = (ScriptTransactionArgumentVariants)
            d.Uleb128AsU32();
        return index switch
        {
            ScriptTransactionArgumentVariants.U8 => U8.Deserialize(d),
            ScriptTransactionArgumentVariants.U16 => U16.Deserialize(d),
            ScriptTransactionArgumentVariants.U32 => U32.Deserialize(d),
            ScriptTransactionArgumentVariants.U64 => U64.Deserialize(d),
            ScriptTransactionArgumentVariants.U128 => U128.Deserialize(d),
            ScriptTransactionArgumentVariants.U256 => U256.Deserialize(d),
            ScriptTransactionArgumentVariants.Address => AccountAddress.Deserialize(d),
            ScriptTransactionArgumentVariants.U8Vector => MoveVector<U8>.Deserialize(
                d,
                U8.Deserialize
            ),
            _ => throw new ArgumentException(
                $"Unknown variant index for ScriptTransactionArgument: {index}"
            ),
        };
    }

    public static TransactionArgument? ConvertArgument(
        object? value,
        TypeTag typeTag,
        List<TypeTag> genericTypeParams
    )
    {
        if (value is TransactionArgument argument)
        {
            return typeTag.CheckType(argument) ? argument : null;
        }
        return Parse(value, typeTag, genericTypeParams);
    }

    public static TransactionArgument? Parse(
        object? value,
        TypeTag typeTag,
        List<TypeTag> genericTypeParams
    )
    {
        if (typeTag is TypeTagBool)
        {
            if (value is bool boolValue)
                return new Bool(boolValue);
            if (value is string boolStringValue)
            {
                if (boolStringValue == "false")
                    return new Bool(false);
                if (boolStringValue == "true")
                    return new Bool(true);
            }
        }

        if (typeTag is TypeTagAddress)
        {
            if (value is string addressStringValue)
                return AccountAddress.FromString(addressStringValue);
        }

        if (typeTag is TypeTagU8)
        {
            if (value is byte u8Value)
                return new U8(u8Value);
            if (value is string u8StringValue)
                return new U8(byte.Parse(u8StringValue));
        }

        if (typeTag is TypeTagU16)
        {
            if (value is ushort u16Value)
                return new U16(u16Value);
            if (value is string u16StringValue)
                return new U16(ushort.Parse(u16StringValue));
        }

        if (typeTag is TypeTagU32)
        {
            if (value is uint u32Value)
                return new U32(u32Value);
            if (value is string u32StringValue)
                return new U32(uint.Parse(u32StringValue));
        }

        if (typeTag is TypeTagU64)
        {
            if (value is ulong u64Value)
                return new U64(u64Value);
            if (value is string u64StringValue)
                return new U64(ulong.Parse(u64StringValue));
        }

        if (typeTag is TypeTagU128)
        {
            if (value is BigInteger u128Value)
                return new U128(u128Value);
            if (value is string u128StringValue)
                return new U128(BigInteger.Parse(u128StringValue));
        }

        if (typeTag is TypeTagU256)
        {
            if (value is BigInteger u256Value)
                return new U256(u256Value);
            if (value is string u256StringValue)
                return new U256(BigInteger.Parse(u256StringValue));
        }

        if (typeTag is TypeTagGeneric typeTagGeneric)
        {
            var genericIndex = typeTagGeneric.Value;
            if (genericIndex >= genericTypeParams.Count)
                throw new ArgumentException(
                    $"Generic type parameter index {genericIndex} is out of range"
                );
            return ConvertArgument(value, genericTypeParams[(int)genericIndex], genericTypeParams);
        }

        if (typeTag is TypeTagVector typeTagVector)
        {
            if (typeTagVector.Value is TypeTagU8)
            {
                if (value is byte[] bytes)
                    return new MoveVector<U8>(bytes.Select(b => new U8(b)).ToList());
            }

            if (value is IEnumerable list)
            {
                List<TransactionArgument?> convertedList = [];
                foreach (object item in list)
                {
                    convertedList.Add(
                        ConvertArgument(item, typeTagVector.Value, genericTypeParams)
                    );
                }
                return new MoveVector<TransactionArgument?>(convertedList);
            }
        }

        if (typeTag is TypeTagStruct typeTagStruct)
        {
            if (typeTagStruct.IsStringTypeTag())
            {
                if (value is string stringValue)
                    return new MoveString(stringValue);
            }

            if (typeTagStruct.IsObjectTypeTag())
            {
                if (value is string stringValue)
                    return AccountAddress.FromString(stringValue);
            }

            if (typeTagStruct.IsOptionTypeTag())
            {
                if (value is null)
                {
                    // Here we attempt to reconstruct the underlying type
                    var innerTypeTag = typeTagStruct.Value.TypeArgs[0];
                    if (innerTypeTag is TypeTagBool)
                        return new MoveOption<Bool>(null);
                    if (innerTypeTag is TypeTagU8)
                        return new MoveOption<U8>(null);
                    if (innerTypeTag is TypeTagU16)
                        return new MoveOption<U16>(null);
                    if (innerTypeTag is TypeTagU32)
                        return new MoveOption<U32>(null);
                    if (innerTypeTag is TypeTagU64)
                        return new MoveOption<U64>(null);
                    if (innerTypeTag is TypeTagU128)
                        return new MoveOption<U128>(null);
                    if (innerTypeTag is TypeTagU256)
                        return new MoveOption<U256>(null);
                    // In all other cases, we will use a placeholder, it doesn't actually matter what the type is, but it will be obvious
                    // Note: This is a placeholder U8 type, and does not match the actual type, as that can't be dynamically grabbed
                    return new MoveOption<MoveString>(null);
                }

                return new MoveOption<TransactionArgument>(
                    ConvertArgument(value, typeTagStruct.Value.TypeArgs[0], genericTypeParams)
                );
            }
        }

        return null;
    }
}
