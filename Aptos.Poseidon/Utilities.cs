namespace Aptos.Poseidon;

using System.Collections;
using System.Numerics;

internal static class Utilities
{

    public static object UnstringifyBigInts(object o)
    {
        if (o is IEnumerable enumerable && (o is not string))
        {
            List<object> res = [];
            foreach (object item in enumerable)
            {
                res.Add(UnstringifyBigInts(item));
            }
            return res;
        }
        else if (o is ValueTuple<List<string>, List<List<string>>> tuple)
        {
            (List<BigInteger> C, List<List<BigInteger>> M) res = (
                tuple.Item1.Select(UnstringifyBigInts).Cast<BigInteger>().ToList(),
                tuple.Item2.Select((e) => e.Select(UnstringifyBigInts).Cast<BigInteger>().ToList()).Cast<List<BigInteger>>().ToList()
            );
            return res;
        }
        else if (o is string str)
        {
            byte[] byteArray = Convert.FromBase64String(str);

            string hex = BitConverter.ToString(byteArray).Replace("-", string.Empty).ToLower();

            return BigInteger.Parse($"0{hex}", System.Globalization.NumberStyles.HexNumber);
        }
        return o;
    }

}