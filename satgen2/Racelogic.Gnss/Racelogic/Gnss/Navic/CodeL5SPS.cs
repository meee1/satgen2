using System.Diagnostics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.Navic
{
	public static class CodeL5SPS
	{
		private const int codeLength = 1023;

		private const int registerSize = 10;

		private static readonly int[] g1Taps;

		private static readonly int[] g2Taps;

		private static readonly uint[] g1Seed;

		private static readonly uint[][] g2Seeds;

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

		static CodeL5SPS()
		{
			g1Taps = new int[2] { 3, 10 };
			g2Taps = new int[6] { 2, 3, 6, 8, 9, 10 };
			g1Seed = new uint[10] { 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u };
			g2Seeds = new uint[14][]
			{
				new uint[10] { 1u, 1u, 1u, 0u, 1u, 0u, 0u, 1u, 1u, 1u },
				new uint[10] { 0u, 0u, 0u, 0u, 1u, 0u, 0u, 1u, 1u, 0u },
				new uint[10] { 1u, 0u, 0u, 0u, 1u, 1u, 0u, 1u, 0u, 0u },
				new uint[10] { 0u, 1u, 0u, 1u, 1u, 1u, 0u, 0u, 1u, 0u },
				new uint[10] { 1u, 1u, 1u, 0u, 1u, 1u, 0u, 0u, 0u, 0u },
				new uint[10] { 0u, 0u, 0u, 1u, 1u, 0u, 1u, 0u, 1u, 1u },
				new uint[10] { 0u, 0u, 0u, 0u, 0u, 1u, 0u, 1u, 0u, 0u },
				new uint[10] { 0u, 1u, 0u, 0u, 1u, 1u, 0u, 0u, 0u, 0u },
				new uint[10] { 0u, 0u, 1u, 0u, 0u, 1u, 1u, 0u, 0u, 0u },
				new uint[10] { 1u, 1u, 0u, 1u, 1u, 0u, 0u, 1u, 0u, 0u },
				new uint[10] { 0u, 0u, 0u, 1u, 0u, 0u, 1u, 1u, 0u, 0u },
				new uint[10] { 1u, 1u, 0u, 1u, 1u, 1u, 1u, 1u, 0u, 0u },
				new uint[10] { 1u, 0u, 1u, 1u, 0u, 1u, 0u, 0u, 1u, 0u },
				new uint[10] { 0u, 1u, 1u, 1u, 1u, 0u, 1u, 0u, 1u, 0u }
			};
			satCount = g2Seeds.Length;
			signedCodes = new sbyte[satCount][];
			negatedSignedCodes = new sbyte[satCount][];
			byte[] array = FibonacciShiftRegister.Generate(g1Seed, g1Taps, 1023).ToArray(1023);
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
				foreach (byte item in FibonacciShiftRegister.Generate(g2Seeds[j], g2Taps, 1023))
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
