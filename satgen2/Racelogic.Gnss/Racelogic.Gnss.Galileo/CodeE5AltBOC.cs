using System.Diagnostics;
using Racelogic.Maths;

namespace Racelogic.Gnss.Galileo;

public static class CodeE5AltBOC
{
	private readonly struct SByteIQ
	{
		public readonly sbyte I;

		public readonly sbyte Q;

		public SByteIQ(sbyte i, sbyte q)
		{
			I = i;
			Q = q;
		}
	}

	private const sbyte alpha = 99;

	private const sbyte beta = 70;

	private const int subcarrierPeriodLength = 8;

	private const int halfSubcarrierPeriodLength = 4;

	private const int chipLength = 12;

	private const int twoChipsLength = 24;

	private const int inputCount = 4;

	private const int inputCombinationCount = 16;

	private const int twoChipsInputCombinationCount = 256;

	private static readonly sbyte[][][] outputCodesI;

	private static readonly sbyte[][][] outputCodesQ;

	private static readonly double signalLeveldB;

	private static readonly SByteIQ[] modulationStates;

	private static readonly int[][] carrierPhases;

	public static double SignalLeveldB
	{
		[DebuggerStepThrough]
		get
		{
			return signalLeveldB;
		}
	}

	public static sbyte[][][] ModulationCodesI
	{
		[DebuggerStepThrough]
		get
		{
			return outputCodesI;
		}
	}

	public static sbyte[][][] ModulationCodesQ
	{
		[DebuggerStepThrough]
		get
		{
			return outputCodesQ;
		}
	}

	static CodeE5AltBOC()
	{
		outputCodesI = new sbyte[CodeE5aI.PrimaryCodes.Length][][];
		outputCodesQ = new sbyte[CodeE5aI.PrimaryCodes.Length][][];
		signalLeveldB = 12.375.LevelToGain();
		modulationStates = new SByteIQ[9]
		{
			new SByteIQ(0, 0),
			new SByteIQ(70, 70),
			new SByteIQ(0, 99),
			new SByteIQ(-70, 70),
			new SByteIQ(-99, 0),
			new SByteIQ(-70, -70),
			new SByteIQ(0, -99),
			new SByteIQ(70, -70),
			new SByteIQ(99, 0)
		};
		carrierPhases = new int[16][]
		{
			new int[8] { 5, 5, 1, 1, 1, 1, 5, 5 },
			new int[8] { 4, 4, 4, 8, 8, 8, 8, 4 },
			new int[8] { 4, 8, 8, 8, 8, 4, 4, 4 },
			new int[8] { 3, 3, 7, 7, 7, 7, 3, 3 },
			new int[8] { 6, 2, 2, 2, 2, 6, 6, 6 },
			new int[8] { 3, 3, 3, 3, 7, 7, 7, 7 },
			new int[8] { 1, 1, 1, 1, 5, 5, 5, 5 },
			new int[8] { 2, 2, 2, 6, 6, 6, 6, 2 },
			new int[8] { 6, 6, 6, 2, 2, 2, 2, 6 },
			new int[8] { 5, 5, 5, 5, 1, 1, 1, 1 },
			new int[8] { 7, 7, 7, 7, 3, 3, 3, 3 },
			new int[8] { 2, 6, 6, 6, 6, 2, 2, 2 },
			new int[8] { 7, 7, 3, 3, 3, 3, 7, 7 },
			new int[8] { 8, 4, 4, 4, 4, 8, 8, 8 },
			new int[8] { 8, 8, 8, 4, 4, 4, 4, 8 },
			new int[8] { 1, 1, 5, 5, 5, 5, 1, 1 }
		};
		sbyte[][] array = new sbyte[16][];
		sbyte[][] array2 = new sbyte[16][];
		for (int i = 0; i < 16; i++)
		{
			int[] array3 = carrierPhases[i];
			sbyte[] array4 = new sbyte[8];
			sbyte[] array5 = new sbyte[8];
			for (int j = 0; j < 8; j++)
			{
				int num = array3[j];
				SByteIQ sByteIQ = modulationStates[num];
				array4[j] = sByteIQ.I;
				array5[j] = sByteIQ.Q;
			}
			array[i] = array4;
			array2[i] = array5;
		}
		sbyte[][] array6 = new sbyte[256][];
		sbyte[][] array7 = new sbyte[256][];
		for (int k = 0; k < 16; k++)
		{
			sbyte[] array8 = array[k];
			sbyte[] array9 = array2[k];
			for (int l = 0; l < 16; l++)
			{
				int num2 = (k << 4) | l;
				sbyte[] array10 = array[l];
				sbyte[] array11 = array2[l];
				sbyte[] array12 = new sbyte[24];
				sbyte[] array13 = new sbyte[24];
				int num3 = 0;
				int num4 = 0;
				while (num4 < 8)
				{
					array12[num3] = array8[num4];
					array13[num3] = array9[num4];
					num4++;
					num3++;
				}
				int num5 = 0;
				while (num5 < 4)
				{
					array12[num3] = array8[num5];
					array13[num3] = array9[num5];
					num5++;
					num3++;
				}
				int num6 = 4;
				while (num6 < 8)
				{
					array12[num3] = array10[num6];
					array13[num3] = array11[num6];
					num6++;
					num3++;
				}
				int num7 = 0;
				while (num7 < 8)
				{
					array12[num3] = array10[num7];
					array13[num3] = array11[num7];
					num7++;
					num3++;
				}
				array6[num2] = array12;
				array7[num2] = array13;
			}
		}
		int num8 = CodeE5aI.PrimaryCodes.Length;
		int num9 = CodeE5aI.PrimaryCodes[0].Length;
		for (int m = 0; m < num8; m++)
		{
			byte[][] array14 = new byte[2][]
			{
				CodeE5aI.PrimaryCodes[m],
				CodeE5aI.NegatedPrimaryCodes[m]
			};
			byte[][] array15 = new byte[2][]
			{
				CodeE5bI.PrimaryCodes[m],
				CodeE5bI.NegatedPrimaryCodes[m]
			};
			byte[][] array16 = new byte[2][]
			{
				CodeE5aQ.PrimaryCodes[m],
				CodeE5aQ.NegatedPrimaryCodes[m]
			};
			byte[][] array17 = new byte[2][]
			{
				CodeE5bQ.PrimaryCodes[m],
				CodeE5bQ.NegatedPrimaryCodes[m]
			};
			sbyte[][] array18 = new sbyte[16][];
			sbyte[][] array19 = new sbyte[16][];
			for (int n = 0; n < 16; n++)
			{
				int num10 = n >> 3;
				int num11 = (n >> 2) & 1;
				int num12 = (n >> 1) & 1;
				int num13 = n & 1;
				byte[] array20 = array14[num10];
				byte[] array21 = array15[num11];
				byte[] array22 = array16[num12];
				byte[] array23 = array17[num13];
				sbyte[] array24 = new sbyte[num9 * 12];
				sbyte[] array25 = new sbyte[num9 * 12];
				int num14 = 0;
				int num15;
				for (num15 = 0; num15 < num9; num15++)
				{
					int num16 = array20[num15];
					num16 <<= 1;
					num16 |= array21[num15];
					num16 <<= 1;
					num16 |= array22[num15];
					num16 <<= 1;
					num16 |= array23[num15];
					num16 <<= 1;
					num15++;
					num16 |= array20[num15];
					num16 <<= 1;
					num16 |= array21[num15];
					num16 <<= 1;
					num16 |= array22[num15];
					num16 <<= 1;
					num16 |= array23[num15];
					sbyte[] array26 = array6[num16];
					sbyte[] array27 = array7[num16];
					for (int num17 = 0; num17 < 24; num17++)
					{
						array24[num14] = array26[num17];
						array25[num14] = array27[num17];
						num14++;
					}
				}
				array18[n] = array24;
				array19[n] = array25;
			}
			outputCodesI[m] = array18;
			outputCodesQ[m] = array19;
		}
	}

	public static uint GetModulationIndex(uint bitE5aI, uint bitE5bI, uint bitE5aQ, uint bitE5bQ)
	{
		return (((((bitE5aI << 1) | bitE5bI) << 1) | bitE5aQ) << 1) | bitE5bQ;
	}
}
