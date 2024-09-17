namespace Aptos.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a given DateTime into a Unix timestamp
        /// </summary>
        /// <param name="value">Any DateTime</param>
        /// <returns>The given DateTime in Unix timestamp format</returns>
        public static int ToUnixTimestamp(this DateTime value)
        {
            return (int)
                Math.Truncate(
                    value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds
                );
        }
    }
}
