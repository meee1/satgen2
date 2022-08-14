using System.Collections.Generic;

namespace Racelogic.Gnss;

internal static class GaloisShiftRegister
{
	public static IEnumerable<byte> Generate(uint seed, IEnumerable<int> taps, int period)
	{
		uint register2 = seed;
		uint shiftedTapMask = 0u;
		foreach (int tap in taps)
		{
			shiftedTapMask |= (uint)(1 << tap - 1);
		}
		for (int i = 0; i < period; i++)
		{
			uint output = register2 & 1u;
			yield return (byte)output;
			register2 >>= 1;
			register2 ^= shiftedTapMask * output;
		}
	}
}
