using System.Diagnostics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.Gps
{
	public static class CodeL1CA
	{
		private const int codeLength = 1023;

		private static readonly uint[] seed;

		private static readonly int[] g1Taps;

		private static readonly int[] g2Taps;

		private static readonly int[][] g2OutputTaps;

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

		static CodeL1CA()
		{
			seed = new uint[10] { 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u };
			g1Taps = new int[2] { 3, 10 };
			g2Taps = new int[6] { 2, 3, 6, 8, 9, 10 };
			g2OutputTaps = new int[37][]
			{
				new int[2] { 2, 6 },
				new int[2] { 3, 7 },
				new int[2] { 4, 8 },
				new int[2] { 5, 9 },
				new int[2] { 1, 9 },
				new int[2] { 2, 10 },
				new int[2] { 1, 8 },
				new int[2] { 2, 9 },
				new int[2] { 3, 10 },
				new int[2] { 2, 3 },
				new int[2] { 3, 4 },
				new int[2] { 5, 6 },
				new int[2] { 6, 7 },
				new int[2] { 7, 8 },
				new int[2] { 8, 9 },
				new int[2] { 9, 10 },
				new int[2] { 1, 4 },
				new int[2] { 2, 5 },
				new int[2] { 3, 6 },
				new int[2] { 4, 7 },
				new int[2] { 5, 8 },
				new int[2] { 6, 9 },
				new int[2] { 1, 3 },
				new int[2] { 4, 6 },
				new int[2] { 5, 7 },
				new int[2] { 6, 8 },
				new int[2] { 7, 9 },
				new int[2] { 8, 10 },
				new int[2] { 1, 6 },
				new int[2] { 2, 7 },
				new int[2] { 3, 8 },
				new int[2] { 4, 9 },
				new int[2] { 5, 10 },
				new int[2] { 4, 10 },
				new int[2] { 1, 7 },
				new int[2] { 2, 8 },
				new int[2] { 4, 10 }
			};
			satCount = g2OutputTaps.Length;
			signedCodes = new sbyte[satCount][];
			negatedSignedCodes = new sbyte[satCount][];
			byte[] array = FibonacciShiftRegister.Generate(seed, g1Taps, 1023).ToArray(1023);
			for (int i = 0; i < satCount; i++)
			{
				signedCodes[i] = new sbyte[1023];
				negatedSignedCodes[i] = new sbyte[1023];
			}
			for (int j = 0; j < satCount; j++)
			{
				sbyte[] array2 = signedCodes[j];
				sbyte[] array3 = negatedSignedCodes[j];
				int num = 0;
				foreach (byte item in FibonacciShiftRegister.Generate(seed, g2Taps, g2OutputTaps[j], 1023))
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
}
