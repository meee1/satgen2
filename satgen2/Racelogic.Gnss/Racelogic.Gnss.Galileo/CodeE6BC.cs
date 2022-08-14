using System;
using System.Diagnostics;
using Racelogic.Maths;

namespace Racelogic.Gnss.Galileo;

public static class CodeE6BC
{
	private static readonly sbyte[][][] outputCodes;

	private static readonly double signalLeveldB;

	public static sbyte[][][] ModulationCodes
	{
		[DebuggerStepThrough]
		get
		{
			return outputCodes;
		}
	}

	public static double SignalLeveldB
	{
		[DebuggerStepThrough]
		get
		{
			return signalLeveldB;
		}
	}

	static CodeE6BC()
	{
		outputCodes = new sbyte[CodeE6B.PrimarySignedCodes.Length][][];
		signalLeveldB = Math.Sqrt(2.0).LevelToGain();
		int num = CodeE6B.PrimarySignedCodes.Length;
		int num2 = 5115;
		for (int i = 0; i < num; i++)
		{
			sbyte[] array = CodeE6B.PrimarySignedCodes[i];
			sbyte[] array2 = CodeE6C.PrimarySignedCodes[i];
			sbyte[] array3 = new sbyte[num2];
			sbyte[] array4 = new sbyte[num2];
			sbyte[] array5 = new sbyte[num2];
			sbyte[] array6 = new sbyte[num2];
			for (int j = 0; j < num2; j++)
			{
				int num3 = array[j];
				int num4 = array2[j];
				array3[j] = (sbyte)(num3 - num4);
				array4[j] = (sbyte)(num3 + num4);
				array5[j] = (sbyte)(-num3 - num4);
				array6[j] = (sbyte)(-num3 + num4);
			}
			outputCodes[i] = new sbyte[4][] { array3, array4, array5, array6 };
		}
	}
}
