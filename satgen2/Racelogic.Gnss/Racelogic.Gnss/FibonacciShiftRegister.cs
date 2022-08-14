using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;

namespace Racelogic.Gnss;

internal static class FibonacciShiftRegister
{
	public static IEnumerable<byte> Generate(IEnumerable<uint> seed, IEnumerable<int> feedbackTaps, IEnumerable<int> outputTaps, int period)
	{
		FixedSizeStack<uint> register = new FixedSizeStack<uint>(seed);
		int[] registerTaps = (from b in feedbackTaps
			where b > 0
			select b - 1).ToArray();
		int[] registerOutputs = (from b in outputTaps
			where b > 0
			select b - 1).ToArray();
		for (int outputIndex = 0; outputIndex < period; outputIndex++)
		{
			uint num = 0u;
			int[] array = registerOutputs;
			foreach (int i2 in array)
			{
				num ^= register[i2];
			}
			yield return (byte)num;
			uint num2 = 0u;
			array = registerTaps;
			foreach (int i3 in array)
			{
				num2 ^= register[i3];
			}
			register.Push(num2);
		}
	}

	public static IEnumerable<byte> Generate(IEnumerable<uint> seed, IEnumerable<int> feedbackTaps, int period)
	{
		FixedSizeStack<uint> register = new FixedSizeStack<uint>(seed);
		int[] registerTaps = (from b in feedbackTaps
			where b > 0
			select b - 1).ToArray();
		int registerOutput = register.Capacity - 1;
		for (uint outputIndex = 0u; outputIndex < period; outputIndex++)
		{
			yield return (byte)register[registerOutput];
			uint num = 0u;
			int[] array = registerTaps;
			foreach (int i2 in array)
			{
				num ^= register[i2];
			}
			register.Push(num);
		}
	}
}
