using System.Diagnostics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.Gps;

public static class CodeL5
{
	private const int codeLength = 10230;

	private const int periodA = 8190;

	private const int periodB = 8191;

	private static readonly int[] tapsA;

	private static readonly int[] tapsB;

	private static readonly uint[] seed;

	private static readonly int[] codeAdvancesI5B;

	private static readonly int[] codeAdvancesQ5B;

	private static readonly int satCount;

	private static readonly sbyte[][] signedCodesI5;

	private static readonly sbyte[][] negatedSignedCodesI5;

	private static readonly sbyte[][] signedCodesQ5;

	private static readonly sbyte[][] negatedSignedCodesQ5;

	public static sbyte[][] SignedCodesI5
	{
		[DebuggerStepThrough]
		get
		{
			return signedCodesI5;
		}
	}

	public static sbyte[][] NegatedSignedCodesI5
	{
		[DebuggerStepThrough]
		get
		{
			return negatedSignedCodesI5;
		}
	}

	public static sbyte[][] SignedCodesQ5
	{
		[DebuggerStepThrough]
		get
		{
			return signedCodesQ5;
		}
	}

	public static sbyte[][] NegatedSignedCodesQ5
	{
		[DebuggerStepThrough]
		get
		{
			return negatedSignedCodesQ5;
		}
	}

	static CodeL5()
	{
		tapsA = new int[4] { 9, 10, 12, 13 };
		tapsB = new int[8] { 1, 3, 4, 6, 7, 8, 12, 13 };
		seed = new uint[13]
		{
			1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u,
			1u, 1u, 1u
		};
		codeAdvancesI5B = new int[37]
		{
			266, 365, 804, 1138, 1509, 1559, 1756, 2084, 2170, 2303,
			2527, 2687, 2930, 3471, 3940, 4132, 4332, 4924, 5343, 5443,
			5641, 5816, 5898, 5918, 5955, 6243, 6345, 6477, 6518, 6875,
			7168, 7187, 7329, 7577, 7720, 7777, 8057
		};
		codeAdvancesQ5B = new int[37]
		{
			1701, 323, 5292, 2020, 5429, 7136, 1041, 5947, 4315, 148,
			535, 1939, 5206, 5910, 3595, 5135, 6082, 6990, 3546, 1523,
			4548, 4484, 1893, 3961, 7106, 5299, 4660, 276, 4389, 3783,
			1591, 1601, 749, 1387, 1661, 3210, 708
		};
		satCount = codeAdvancesI5B.Length;
		signedCodesI5 = new sbyte[satCount][];
		negatedSignedCodesI5 = new sbyte[satCount][];
		signedCodesQ5 = new sbyte[satCount][];
		negatedSignedCodesQ5 = new sbyte[satCount][];
		byte[] array = FibonacciShiftRegister.Generate(seed, tapsA, 8190).ToArray(8190);
		byte[] array2 = FibonacciShiftRegister.Generate(seed, tapsB, 8191).ToArray(8191);
		for (int i = 0; i < satCount; i++)
		{
			signedCodesI5[i] = new sbyte[10230];
			negatedSignedCodesI5[i] = new sbyte[10230];
			signedCodesQ5[i] = new sbyte[10230];
			negatedSignedCodesQ5[i] = new sbyte[10230];
		}
		for (int j = 0; j < satCount; j++)
		{
			sbyte[] array3 = signedCodesI5[j];
			sbyte[] array4 = negatedSignedCodesI5[j];
			sbyte[] array5 = signedCodesQ5[j];
			sbyte[] array6 = negatedSignedCodesQ5[j];
			int num = codeAdvancesI5B[j];
			int num2 = codeAdvancesQ5B[j];
			for (int k = 0; k < 10230; k++)
			{
				byte num3 = array[k % 8190];
				int num4 = array2[(k + num) % 8191];
				int num5 = array2[(k + num2) % 8191];
				int num6 = num3 ^ num4;
				num6 <<= 1;
				num6--;
				array3[k] = (sbyte)num6;
				array4[k] = (sbyte)(-num6);
				int num7 = num3 ^ num5;
				num7 <<= 1;
				num7--;
				array5[k] = (sbyte)num7;
				array6[k] = (sbyte)(-num7);
			}
		}
	}
}
