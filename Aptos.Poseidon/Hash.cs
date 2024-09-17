namespace Aptos.Poseidon;

using System.Numerics;

public static class Hash
{
    private static readonly BigInteger F = BigInteger.Parse(
        "21888242871839275222246405745257275088548364400416034343698204186575808495617"
    );

    private const int N_ROUNDS_F = 8;

    private static readonly List<int> N_ROUNDS_P =
    [
        56,
        57,
        56,
        60,
        60,
        63,
        64,
        63,
        60,
        66,
        60,
        65,
        70,
        60,
        64,
        68,
    ];

    private static BigInteger Pow5(BigInteger v)
    {
        var o = v * v;
        return v * o * o % F;
    }

    private static List<BigInteger> Mix(List<BigInteger> state, List<List<BigInteger>> M)
    {
        List<BigInteger> output = [];

        for (int x = 0; x < state.Count; x++)
        {
            var o = BigInteger.Zero;
            for (int y = 0; y < state.Count; y++)
            {
                o = o + M[x][y] * state[y];
            }
            output.Add(o % F);
        }

        return output;
    }

    public static BigInteger Poseidon(
        List<BigInteger> inputs,
        (List<BigInteger> C, List<List<BigInteger>> M) opt
    )
    {
        if (inputs.Count <= 0)
            throw new ArgumentException("Not enough inputs");
        if (inputs.Count > N_ROUNDS_P.Count)
            throw new ArgumentException("Too many inputs");

        int t = inputs.Count + 1;
        int nRoundsF = N_ROUNDS_F;
        int nRoundsP = N_ROUNDS_P[t - 2];

        var C = opt.C;
        var M = opt.M;

        if (M.Count != t)
            throw new ArgumentException($"Incorrect M length, expected {t} got {M.Count}");

        List<BigInteger> state = [0];
        state.AddRange(inputs);

        for (int x = 0; x < nRoundsF + nRoundsP; x++)
        {
            for (int y = 0; y < state.Count; y++)
            {
                state[y] = state[y] + C[x * t + y];

                if (x < nRoundsF / 2 || x >= nRoundsF / 2 + nRoundsP)
                {
                    state[y] = Pow5(state[y]);
                }
                else if (y == 0)
                {
                    state[y] = Pow5(state[y]);
                }
            }

            state = Mix(state, M);
        }

        return state[0];
    }

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC1 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C1);

    public static BigInteger Poseidon1(List<BigInteger> inputs) => Poseidon(inputs, BC1);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC2 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C2);

    public static BigInteger Poseidon2(List<BigInteger> inputs) => Poseidon(inputs, BC2);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC3 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C3);

    public static BigInteger Poseidon3(List<BigInteger> inputs) => Poseidon(inputs, BC3);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC4 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C4);

    public static BigInteger Poseidon4(List<BigInteger> inputs) => Poseidon(inputs, BC4);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC5 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C5);

    public static BigInteger Poseidon5(List<BigInteger> inputs) => Poseidon(inputs, BC5);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC6 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C6);

    public static BigInteger Poseidon6(List<BigInteger> inputs) => Poseidon(inputs, BC6);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC7 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C7);

    public static BigInteger Poseidon7(List<BigInteger> inputs) => Poseidon(inputs, BC7);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC8 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C8);

    public static BigInteger Poseidon8(List<BigInteger> inputs) => Poseidon(inputs, BC8);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC9 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C9);

    public static BigInteger Poseidon9(List<BigInteger> inputs) => Poseidon(inputs, BC9);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC10 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C10);

    public static BigInteger Poseidon10(List<BigInteger> inputs) => Poseidon(inputs, BC10);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC11 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C11);

    public static BigInteger Poseidon11(List<BigInteger> inputs) => Poseidon(inputs, BC11);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC12 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C12);

    public static BigInteger Poseidon12(List<BigInteger> inputs) => Poseidon(inputs, BC12);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC13 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C13);

    public static BigInteger Poseidon13(List<BigInteger> inputs) => Poseidon(inputs, BC13);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC14 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C14);

    public static BigInteger Poseidon14(List<BigInteger> inputs) => Poseidon(inputs, BC14);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC15 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C15);

    public static BigInteger Poseidon15(List<BigInteger> inputs) => Poseidon(inputs, BC15);

    private static (List<BigInteger> C, List<List<BigInteger>> M) BC16 = ((
        List<BigInteger> C,
        List<List<BigInteger>> M
    ))
        Utilities.UnstringifyBigInts(Constants.C16);

    public static BigInteger Poseidon16(List<BigInteger> inputs) => Poseidon(inputs, BC16);

    public static BigInteger PoseidonHash(List<BigInteger> inputs) =>
        (inputs.Count - 1) switch
        {
            0 => Poseidon1(inputs),
            1 => Poseidon2(inputs),
            2 => Poseidon3(inputs),
            3 => Poseidon4(inputs),
            4 => Poseidon5(inputs),
            5 => Poseidon6(inputs),
            6 => Poseidon7(inputs),
            7 => Poseidon8(inputs),
            8 => Poseidon9(inputs),
            9 => Poseidon10(inputs),
            10 => Poseidon11(inputs),
            11 => Poseidon12(inputs),
            12 => Poseidon13(inputs),
            13 => Poseidon14(inputs),
            14 => Poseidon15(inputs),
            15 => Poseidon16(inputs),
            _ => throw new ArgumentException(
                $"Unable to hash input of length {inputs.Count}. Max input length is {16}"
            ),
        };
}
