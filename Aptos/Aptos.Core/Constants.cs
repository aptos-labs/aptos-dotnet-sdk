namespace Aptos.Core
{
    public static class Constants
    {

        public const ulong DEFAULT_TIMEOUT_SECS = 20;

        public const ulong DEFAULT_MAX_GAS_AMOUNT = 200000;

        public const ulong DEFAULT_TXN_EXP_SEC = 20;

        public static AccountAddress ZERO_ADDRESS = AccountAddress.FromString("0x0");

        public static string APTOS_FA = "0x000000000000000000000000000000000000000000000000000000000000000a";

        public static string APTOS_COIN_FA = APTOS_FA;

        public static string APTOS_COIN_TYPE = "0x1::aptos_coin::AptosCoin";

        public static AccountAddress APTOS_FA_ADDRESS = AccountAddress.FromString(APTOS_FA);

    }
}