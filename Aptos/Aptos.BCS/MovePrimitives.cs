using System.Numerics;

namespace Aptos;

public class Bool(bool value) : TransactionArgument
{
    public readonly bool Value = value;

    public override void Serialize(Serializer s) => s.Bool(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.Bool);
        s.Bool(Value);
    }

    public static Bool Deserialize(Deserializer d) => new(d.Bool());
}

public class U8(byte value) : TransactionArgument
{
    public readonly byte Value = value;

    public override void Serialize(Serializer s) => s.U8(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U8);
        s.U8(Value);
    }

    public static U8 Deserialize(Deserializer d) => new(d.U8());
}

public class U16(ushort value) : TransactionArgument
{
    public readonly ushort Value = value;

    public override void Serialize(Serializer s) => s.U16(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U16);
        s.U16(Value);
    }

    public static U16 Deserialize(Deserializer d) => new(d.U16());
}

public class U32(uint value) : TransactionArgument
{
    public readonly uint Value = value;

    public override void Serialize(Serializer s) => s.U32(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U32);
        s.U32(Value);
    }

    public static U32 Deserialize(Deserializer d) => new(d.U32());
}

public class U64(ulong value) : TransactionArgument
{
    public readonly ulong Value = value;

    public override void Serialize(Serializer s) => s.U64(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U64);
        s.U64(Value);
    }

    public static U64 Deserialize(Deserializer d) => new(d.U64());
}

public class U128(BigInteger value) : TransactionArgument
{
    public readonly BigInteger Value = value;

    public override void Serialize(Serializer s) => s.U128(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U128);
        s.U128(Value);
    }

    public static U128 Deserialize(Deserializer d) => new(d.U128());
}

public class U256(BigInteger value) : TransactionArgument
{
    public readonly BigInteger Value = value;

    public override void Serialize(Serializer s) => s.U256(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U256);
        s.U256(Value);
    }

    public static U256 Deserialize(Deserializer d) => new(d.U256());
}
