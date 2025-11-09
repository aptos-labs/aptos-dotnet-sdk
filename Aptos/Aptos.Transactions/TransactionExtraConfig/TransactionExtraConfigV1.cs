namespace Aptos;

public class TransactionExtraConfigV1(
    AccountAddress? multiSigAddress = null,
    ulong? replayProtectionNonce = null
) : TransactionExtraConfig
{
    public readonly AccountAddress? MultiSigAddress = multiSigAddress;
    public readonly ulong? ReplayProtectionNonce = replayProtectionNonce;

    public override void Serialize(Serializer s)
    {
        s.U32AsUleb128((uint)TransactionExtraConfigVariant.V1);
        new MoveOption<AccountAddress>(MultiSigAddress).Serialize(s);
        new MoveOption<U64>(
            ReplayProtectionNonce.HasValue ? new U64(ReplayProtectionNonce.Value) : null
        ).Serialize(s);
    }

    public static new TransactionExtraConfigV1 Deserialize(Deserializer d)
    {
        var multiSigAddress = MoveOption<AccountAddress>
            .Deserialize(d, AccountAddress.Deserialize)
            .Value;
        var replayProtectionNonce = MoveOption<U64>.Deserialize(d, U64.Deserialize).Value?.Value;
        return new(multiSigAddress, replayProtectionNonce);
    }

    public override bool HasReplayProtectionNonce()
    {
        return ReplayProtectionNonce != null;
    }
}
