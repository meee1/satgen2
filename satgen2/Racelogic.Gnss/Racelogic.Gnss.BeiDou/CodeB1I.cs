using System.Diagnostics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.BeiDou;

public static class CodeB1I
{
	private static readonly int[][] g2OutputTaps;

	private const int codeLength = 2046;

	private static readonly uint[] seed;

	private static readonly int[] g1Taps;

	private static readonly int[] g2Taps;

	private static readonly int satCount;

	private static readonly sbyte[][] signedCodes;

	private static readonly sbyte[][] negatedSignedCodes;

	public static sbyte[][] SignedCodes
	{
		[DebuggerStepThrough]
		get
		{
			return signedCodes;
		}
	}

	public static sbyte[][] NegatedSignedCodes
	{
		[DebuggerStepThrough]
		get
		{
			return negatedSignedCodes;
		}
	}

	static CodeB1I()
	{
		g2OutputTaps = new int[37][]
		{
			new int[2] { 1, 3 },
			new int[2] { 1, 4 },
			new int[2] { 1, 5 },
			new int[2] { 1, 6 },
			new int[2] { 1, 8 },
			new int[2] { 1, 9 },
			new int[2] { 1, 10 },
			new int[2] { 1, 11 },
			new int[2] { 2, 7 },
			new int[2] { 3, 4 },
			new int[2] { 3, 5 },
			new int[2] { 3, 6 },
			new int[2] { 3, 8 },
			new int[2] { 3, 9 },
			new int[2] { 3, 10 },
			new int[2] { 3, 11 },
			new int[2] { 4, 5 },
			new int[2] { 4, 6 },
			new int[2] { 4, 8 },
			new int[2] { 4, 9 },
			new int[2] { 4, 10 },
			new int[2] { 4, 11 },
			new int[2] { 5, 6 },
			new int[2] { 5, 8 },
			new int[2] { 5, 9 },
			new int[2] { 5, 10 },
			new int[2] { 5, 11 },
			new int[2] { 6, 8 },
			new int[2] { 6, 9 },
			new int[2] { 6, 10 },
			new int[2] { 6, 11 },
			new int[2] { 8, 9 },
			new int[2] { 8, 10 },
			new int[2] { 8, 11 },
			new int[2] { 9, 10 },
			new int[2] { 9, 11 },
			new int[2] { 10, 11 }
		};
		seed = new uint[11]
		{
			0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u,
			0u
		};
		g1Taps = new int[6] { 1, 7, 8, 9, 10, 11 };
		g2Taps = new int[8] { 1, 2, 3, 4, 5, 8, 9, 11 };
		satCount = g2OutputTaps.Length;
		signedCodes = new sbyte[satCount][];
		negatedSignedCodes = new sbyte[satCount][];
		byte[] array = FibonacciShiftRegister.Generate(seed, g1Taps, 2046).ToArray(2046);
		for (int i = 0; i < satCount; i++)
		{
			signedCodes[i] = new sbyte[2046];
			negatedSignedCodes[i] = new sbyte[2046];
		}
		for (int j = 0; j < satCount; j++)
		{
			sbyte[] array2 = signedCodes[j];
			sbyte[] array3 = negatedSignedCodes[j];
			int num = 0;
			foreach (byte item in FibonacciShiftRegister.Generate(seed, g2Taps, g2OutputTaps[j], 2046))
			{
				int num2 = array[num] ^ item;
				num2 <<= 1;
				num2--;
				array2[num] = (sbyte)num2;
				array3[num] = (sbyte)(-num2);
				num++;
			}
		}
	}
}
