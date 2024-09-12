using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aptos.Core
{

    public static class Utilities
    {

        /// <summary>
        /// Parses a function string into its module address, module name, and function name.
        /// </summary>
        /// <param name="function">An function split by "::". E.g. "0x1::coin::transfer"</param>
        /// <returns>A tuple containing the module address, module name, and function name.</returns>
        /// <exception cref="ArgumentException">If the function string is not in the correct format.</exception>
        public static (string moduleAddress, string moduleName, string functionName) ParseFunctionParts(string function)
        {
            string[] parts = function.Split("::");
            if (parts.Length != 3) throw new ArgumentException($"Invalid function format {function}");
            return (parts[0], parts[1], parts[2]);
        }

        public static int FindFirstNonSignerIndex(MoveFunction function)
        {
            var index = function.Parameters.FindIndex(p => p != "signer" && p != "&signer");
            return index == -1 ? function.Parameters.Count : index;
        }

        public static List<TypeTag> StandardizeTypeTags(List<object> typeTags) => typeTags.Select(t => t is string stringTag ? TypeTag.Parse(stringTag) : t is TypeTag typeTag ? typeTag : throw new ArgumentException("Invalid TypeTag, expected string or TypeTag")).ToList() ?? [];

        public static T? UnwrapOption<T>(object val) where T : class
        {
            var vec = JObject.FromObject(val)["vec"];
            if (vec is JArray arr && arr.Count > 0) return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(arr.First()));
            return null;
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            string formattedString = hexString.Replace("0x", "");
            return Enumerable.Range(0, formattedString.Length)
                      .Where(x => x % 2 == 0)
                      .Select(x => Convert.ToByte(formattedString.Substring(x, 2), 16))
                      .ToArray();
        }

        public static string HexStringToString(string hexString) => Encoding.ASCII.GetString(HexStringToBytes(hexString));

        public static long FloorToWholeHour(long timestampInSeconds)
        {
            // Convert the timestamp to DateTime (Unix timestamp is seconds since 1970-01-01T00:00:00Z)
            DateTimeOffset dateTime = DateTimeOffset.FromUnixTimeSeconds(timestampInSeconds);

            // Reset minutes, seconds, and milliseconds to zero
            DateTimeOffset flooredDateTime = new(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                0,
                0,
                dateTime.Offset
            );

            // Convert back to Unix timestamp and return
            return flooredDateTime.ToUnixTimeSeconds();
        }

    }

}