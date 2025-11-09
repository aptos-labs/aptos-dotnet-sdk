namespace Aptos;

public class InnerTransactionPayloadV1(
    TransactionExecutable executable,
    TransactionExtraConfig extraConfig
) : InnerTransactionPayload
{
    public readonly TransactionExecutable Executable = executable;
    public readonly TransactionExtraConfig ExtraConfig = extraConfig;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)InnerTransactionPayloadVariant.V1);
        Executable.Serialize(s);
        ExtraConfig.Serialize(s);
    }

    public static new InnerTransactionPayloadV1 Deserialize(Deserializer d) =>
        new(TransactionExecutable.Deserialize(d), TransactionExtraConfig.Deserialize(d));
}
