namespace Aptos;

public class MoveVector<T>(List<T> values) : TransactionArgument
{

    public readonly List<T> Values = values;

    public override void SerializeForScriptFunction(Serializer s)
    {
        if (typeof(T) != typeof(U8)) throw new ArgumentException("ScriptFunctionArgument only supports U8 vectors");
        s.U32AsUleb128((uint)Values.Count);
        s.Serialize(this);
    }

    public override void Serialize(Serializer s) => s.Vector(Values.Cast<TransactionArgument>().ToList());

    public static MoveVector<T> Deserialize(Deserializer d, Func<Deserializer, T> deserializeFunc)
    {
        uint length = d.Uleb128AsU32();
        List<T> values = [];
        for (uint i = 0; i < length; i++) values.Add(deserializeFunc(d));
        return new(values);
    }
}

public class MoveString(string value) : TransactionArgument
{
    public readonly string Value = value;

    public override void Serialize(Serializer s) => s.String(Value);

    public override void SerializeForScriptFunction(Serializer s)
    {
        // Serialize the string as a fixed byte string, i.e., without the length prefix
        var fixedStringBytes = BcsToBytes().Skip(1).ToList();
        // Put those bytes into a vector<u8> and serialize it as a script function argument
        var vectorU8 = new MoveVector<U8>(fixedStringBytes.Select(b => new U8(b)).ToList());
        s.Serialize(vectorU8);
    }

    public static MoveString Deserialize(Deserializer d) => new(d.String());

}

public class MoveOption<T> : TransactionArgument where T : class
{
    private readonly MoveVector<T> _vec;

    public readonly T? Value;

    public MoveOption(T? value)
    {
        if (value != null) _vec = new([value]);
        else _vec = new([]);
    }

    public override void Serialize(Serializer s) => _vec.Serialize(s);

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
    public override void SerializeForScriptFunction(Serializer s) => throw new NotImplementedException();
}