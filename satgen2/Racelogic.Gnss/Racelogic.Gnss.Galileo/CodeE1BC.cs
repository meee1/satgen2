using System;
using System.Diagnostics;
using Racelogic.Maths;

namespace Racelogic.Gnss.Galileo;

public static class CodeE1BC
{
	private const sbyte alpha = 117;

	private const sbyte beta = 37;

	private const int subcarrierPeriodLength = 12;

	private static readonly sbyte[][] subcarrierPeriods;

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

	static CodeE1BC()
	{
		subcarrierPeriods = new sbyte[4][]
		{
			new sbyte[12]
			{
				37, -37, 37, -37, 37, -37, 37, -37, 37, -37,
				37, -37
			},
			new sbyte[12]
			{
				117, 117, 117, 117, 117, 117, -117, -117, -117, -117,
				-117, -117
			},
			new sbyte[12]
			{
				-117, -117, -117, -117, -117, -117, 117, 117, 117, 117,
				117, 117
			},
			new sbyte[12]
			{
				-37, 37, -37, 37, -37, 37, -37, 37, -37, 37,
				-37, 37
			}
		};
		outputCodes = new sbyte[CodeE1B.PrimaryCodes.Length][][];
		signalLeveldB = (117.0 / (2.0 * Math.Sqrt(0.90909090909090906)) * Math.Sqrt(2.0)).LevelToGain();
		int num = 4092;
		int num2 = num * 12;
		int num3 = CodeE1B.PrimaryCodes.Length;
		for (int i = 0; i < num3; i++)
		{
			byte[][] array = new byte[2][]
			{
				CodeE1B.PrimaryCodes[i],
				CodeE1B.NegatedPrimaryCodes[i]
			};
			byte[][] array2 = new byte[2][]
			{
				CodeE1C.PrimaryCodes[i],
				CodeE1C.NegatedPrimaryCodes[i]
			};
			sbyte[][] array3 = new sbyte[4][];
			outputCodes[i] = array3;
			for (int j = 0; j < 4; j++)
			{
				byte[] array4 = array[j >> 1];
				byte[] array5 = array2[j & 1];
				sbyte[] array6 = (array3[j] = new sbyte[num2]);
				int num4 = 0;
				for (int k = 0; k < num; k++)
				{
					int num5 = (array4[k] << 1) | array5[k];
					sbyte[] array7 = subcarrierPeriods[num5];
					for (int l = 0; l < 12; l++)
					{
						array6[num4++] = array7[l];
					}
				}
			}
		}
	}
}
