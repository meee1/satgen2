using System.Diagnostics;

namespace Racelogic.Gnss.Glonass
{
	public static class CodeL1OF
	{
		private const int codeLength = 511;

		private static readonly uint[] seed;

		private static readonly int[] taps;

		private static readonly int[] outputTaps;

		private static readonly sbyte[] signedCode;

		private static readonly sbyte[] negatedSignedCode;

		public static sbyte[] SignedCode
		{
			[DebuggerStepThrough]
			get
			{
				return signedCode;
			}
		}

		public static sbyte[] NegatedSignedCode
		{
			[DebuggerStepThrough]
			get
			{
				return negatedSignedCode;
			}
		}

		static CodeL1OF()
		{
			seed = new uint[9] { 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u };
			taps = new int[2] { 5, 9 };
			outputTaps = new int[1] { 7 };
			signedCode = new sbyte[511];
			negatedSignedCode = new sbyte[511];
			int num = 0;
			foreach (byte current1 in FibonacciShiftRegister.Generate(seed, taps, outputTaps, 511))
			{
				int current = current1 << 1;
				current--;
				signedCode[num] = (sbyte)current;
				negatedSignedCode[num] = (sbyte)(-current);
				num++;
			}
		}
	}
}
