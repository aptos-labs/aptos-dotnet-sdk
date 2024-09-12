namespace Aptos;

using Aptos.Schemes;

public partial class MultiKey
{

    public static byte[] CreateBitmap(int[] bits)
    {
        const byte firstBitInByte = 128;
        byte[] bitmap = new byte[4];

        var dupCheckSet = new HashSet<int>();

        foreach (var bit in bits)
        {
            if (bit >= MultiKeySignature.MAX_SIGNATURES_SUPPORTED) throw new ArgumentException($"Cannot have a signature larger than {MultiKeySignature.MAX_SIGNATURES_SUPPORTED - 1}.");
            if (!dupCheckSet.Add(bit)) throw new ArgumentException("Duplicate bits detected.");

            int byteOffset = bit / 8;

            byte currentByte = bitmap[byteOffset];

            currentByte |= (byte)(firstBitInByte >> (bit % 8));

            bitmap[byteOffset] = currentByte;
        }

        return bitmap;
    }

    public static int BitCount(byte b)
    {
        uint n = b; // Use uint for bit manipulation
        n -= (n >> 1) & 0x55555555u;
        n = (n & 0x33333333u) + ((n >> 2) & 0x33333333u);
        n = (n + (n >> 4)) & 0x0F0F0F0Fu;
        return (int)((n * 0x01010101u) >> 24);
    }

}

public partial class MultiKey : UnifiedAccountPublicKey
{

    public readonly List<AnyPublicKey> PublicKeys;

    public readonly byte SignaturesRequired;

    public MultiKey(List<PublicKey> publicKeys, byte signaturesRequired)
    {
        if (signaturesRequired < 1) throw new ArgumentException("Signatures required must be greater than 0");
        if (signaturesRequired > publicKeys.Count) throw new ArgumentException("Signatures required must be less than or equal to the number of public keys");

        // Make sure that all public keys are normalized to the SingleKey authentication scheme
        PublicKeys = publicKeys.Select(p => p is AnyPublicKey anyPublicKey ? anyPublicKey : new AnyPublicKey(p)).ToList();
        SignaturesRequired = signaturesRequired;
    }

    public int GetIndex(PublicKey publicKey)
    {
        var pk = publicKey is AnyPublicKey anyPublicKey ? anyPublicKey : new AnyPublicKey(publicKey);
        var index = PublicKeys.FindIndex((pk) => pk.ToString().Equals(pk.ToString()));
        if (index == -1) throw new ArgumentException("Public key not found");
        return index;
    }

    public override byte[] ToByteArray() => BcsToBytes();

    public override AuthenticationKey AuthKey() => AuthenticationKey.FromSchemeAndBytes(AuthenticationKeyScheme.MultiKey, BcsToBytes());

    public override bool VerifySignature(byte[] message, Signature signature) => throw new NotImplementedException();

    public override void Serialize(Serializer s)
    {
        s.Vector(PublicKeys);
        s.U8(SignaturesRequired);
    }

    public static MultiKey Deserialize(Deserializer d)
    {
        List<PublicKey> publicKeys = d.Vector(AnyPublicKey.Deserialize).Cast<PublicKey>().ToList();
        byte signaturesRequired = d.U8();
        return new MultiKey(publicKeys, signaturesRequired);
    }

}

public class MultiKeySignature : UnifiedSignature
{

    public static readonly int BITMAP_LEN = 4;

    public static readonly int MAX_SIGNATURES_SUPPORTED = BITMAP_LEN * 8;

    public readonly List<AnySignature> Signatures;

    public readonly byte[] Bitmap;

    public MultiKeySignature(List<Signature> signatures, int[] bitmap) : this(signatures, MultiKey.CreateBitmap(bitmap)) { }
    public MultiKeySignature(List<Signature> signatures, byte[] bitmap)
    {
        // Make sure that all signatures are normalized to the SingleKey authentication scheme
        if (signatures.Count > MAX_SIGNATURES_SUPPORTED) throw new ArgumentException($"Signatures count should be less than or equal to {MAX_SIGNATURES_SUPPORTED}");
        Signatures = signatures.Select(s => s is AnySignature anySignature ? anySignature : new AnySignature(s)).ToList();

        // Make sure that the bitmap is the correct length
        if (bitmap.Length != BITMAP_LEN) throw new ArgumentException($"Bitmap length should be {BITMAP_LEN}");
        Bitmap = bitmap;

        int nSignatures = Bitmap.Aggregate(0, (acc, e) => acc + MultiKey.BitCount(e));
        if (nSignatures != Signatures.Count) throw new ArgumentException($"Expecting {nSignatures} signatures from the bitmap, but got {Signatures.Count}");
    }

    public override byte[] ToByteArray() => BcsToBytes();

    public override void Serialize(Serializer s)
    {
        s.Vector(Signatures);
        s.Bytes(Bitmap);
    }

    public static MultiKeySignature Deserialize(Deserializer d)
    {
        List<Signature> signatures = d.Vector(AnySignature.Deserialize).Cast<Signature>().ToList();
        byte[] bitmap = d.Bytes();
        return new MultiKeySignature(signatures, bitmap);
    }
}