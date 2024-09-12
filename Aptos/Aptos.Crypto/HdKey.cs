
namespace Aptos;

using System.Buffers.Binary;
using System.Text;
using System.Text.RegularExpressions;
using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

public static class HdKey
{
    public class DerivedKeys(byte[] privateKey, byte[] chainCode)
    {
        public readonly byte[] PrivateKey = privateKey;
        public readonly byte[] ChainCode = chainCode;
    }

    public const string APTOS_HARDENED_REGEX = @"^m\/44'\/637'\/[0-9]+'\/[0-9]+'\/[0-9]+'?$";

    public const string APTOS_BIP44_REGEX = @"^m\/44'\/637'\/[0-9]+'\/[0-9]+\/[0-9]+$";

    public static bool IsValidHardenedPath(string path) => Regex.IsMatch(path, APTOS_HARDENED_REGEX);

    public static bool IsValidBip44Path(string path) => Regex.IsMatch(path, APTOS_BIP44_REGEX);

    public static string RemoveApostrophes(string val) => val.Replace("'", "");

    public static string[] SplitPath(string path) => path.Split("/").Skip(1).Select(s => RemoveApostrophes(s)).ToArray();

    public static DerivedKeys DeriveKey(byte[] hashSeed, byte[] data)
    {
        HMac hmac = new(new Sha512Digest());
        hmac.Init(new KeyParameter(hashSeed));

        byte[] buffer = new byte[64];
        hmac.BlockUpdate(data, 0, data.Length);
        hmac.DoFinal(buffer, 0);

        return new DerivedKeys(buffer.Take(32).ToArray(), buffer.Skip(32).ToArray());
    }

    public static DerivedKeys DeriveKey(string hashSeed, byte[] data)
    {
        byte[] hashSeedBytes = Encoding.UTF8.GetBytes(hashSeed);
        return DeriveKey(hashSeedBytes, data);
    }

    public static DerivedKeys DeriveKey(string hashSeed, string data)
    {
        byte[] hashSeedBytes = Encoding.UTF8.GetBytes(hashSeed);
        return DeriveKey(hashSeedBytes, Encoding.UTF8.GetBytes(data));
    }

    public static DerivedKeys CKDPriv(DerivedKeys parent, uint index)
    {
        MemoryStream buffer = new();

        buffer.Write([0], 0, 1);
        buffer.Write(parent.PrivateKey, 0, parent.PrivateKey.Length);
        byte[] indexBytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(indexBytes, index);
        buffer.Write(indexBytes, 0, indexBytes.Length);

        return DeriveKey(parent.ChainCode, buffer.ToArray());
    }

    public static byte[] MnemonicToSeed(string mnemonic)
    {
        // Normalize the mnemonic
        string normalizedMnemonic = string.Join(" ",
            mnemonic.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.ToLowerInvariant()));
        return new Mnemonic(normalizedMnemonic).DeriveSeed();
    }

}