namespace Aptos;

public class MoveVector<T>(List<T> values) : TransactionArgument
{
    public readonly List<T> Values = values;

    public override void SerializeForScriptFunction(Serializer s)
    {
        if (typeof(T) != typeof(U8))
            throw new ArgumentException("ScriptFunctionArgument only supports U8 vectors");
        // A script-function argument is encoded as the variant tag followed by
        // the regular BCS payload for that variant. Previously the variant
        // byte was missing which made the bytes unparseable as a script
        // argument (the deserializer would dispatch to the wrong type).
        s.U32AsUleb128((uint)ScriptTransactionArgumentVariants.U8Vector);
        Serialize(s);
    }

    public override void Serialize(Serializer s) =>
        s.Vector(Values.Cast<TransactionArgument>().ToList());

    public static MoveVector<T> Deserialize(Deserializer d, Func<Deserializer, T> deserializeFunc)
    {
        uint length = d.Uleb128AsU32();
        List<T> values = [];
        for (uint i = 0; i < length; i++)
            values.Add(deserializeFunc(d));
        return new(values);
    }
}

public class MoveString(string value) : TransactionArgument
{
    public readonly string Value = value;

    public override void Serialize(Serializer s) => s.String(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        // A Move String is represented at the script level as vector<u8>. The
        // script-function argument encoding therefore needs:
        //   variant tag (U8Vector) || uleb128 length || bytes
        // The raw UTF-8 bytes (BcsToBytes drops the length prefix below).
        var rawBytes = System.Text.Encoding.UTF8.GetBytes(Value);
        var vectorU8 = new MoveVector<U8>([.. rawBytes.Select(b => new U8(b))]);
        vectorU8.SerializeForScriptFunction(s);
    }

    public static MoveString Deserialize(Deserializer d) => new(d.String());
}

public class MoveOption<T> : TransactionArgument
    where T : class
{
    public readonly T? Value;

    public MoveOption(T? value)
    {
        Value = value;
    }

    public override void Serialize(Serializer s)
    {
        MoveVector<T> vec;
        if (Value != null)
            vec = new([Value]);
        else
            vec = new([]);
        vec.Serialize(s);
    }

    public static MoveOption<T> Deserialize(Deserializer d, Func<Deserializer, T> deserializeFunc)
    {
        var vector = MoveVector<T>.Deserialize(d, deserializeFunc);
        T? value = vector.Values.Count > 0 ? vector.Values[0] : null;
        return new(value);
    }

    /// <summary>
    /// Not provided for MoveOption<T> because it's not a script function argument.
    /// </summary>
    /// <param name="s"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void SerializeForScriptFunction(Serializer s) =>
        throw new NotImplementedException();
}
