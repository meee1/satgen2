using System.Diagnostics;
using Racelogic.Maths;

namespace Racelogic.Gnss.Galileo
{
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
			sbyte[][] array12 = new sbyte[16][];
			for (int i = 0; i < 16; i++)
			{
				int[] array21 = carrierPhases[i];
				sbyte[] array22 = new sbyte[8];
				sbyte[] array23 = new sbyte[8];
				for (int j = 0; j < 8; j++)
				{
					int num = array21[j];
					SByteIQ sByteIQ = modulationStates[num];
					array22[j] = sByteIQ.I;
					array23[j] = sByteIQ.Q;
				}
				array[i] = array22;
				array12[i] = array23;
			}
			sbyte[][] array24 = new sbyte[256][];
			sbyte[][] array25 = new sbyte[256][];
			for (int k = 0; k < 16; k++)
			{
				sbyte[] array26 = array[k];
				sbyte[] array27 = array12[k];
				for (int l = 0; l < 16; l++)
				{
					int num10 = (k << 4) | l;
					sbyte[] array2 = array[l];
					sbyte[] array3 = array12[l];
					sbyte[] array4 = new sbyte[24];
					sbyte[] array5 = new sbyte[24];
					int num11 = 0;
					int num12 = 0;
					while (num12 < 8)
					{
						array4[num11] = array26[num12];
						array5[num11] = array27[num12];
						num12++;
						num11++;
					}
					int num13 = 0;
					while (num13 < 4)
					{
						array4[num11] = array26[num13];
						array5[num11] = array27[num13];
						num13++;
						num11++;
					}
					int num14 = 4;
					while (num14 < 8)
					{
						array4[num11] = array2[num14];
						array5[num11] = array3[num14];
						num14++;
						num11++;
					}
					int num15 = 0;
					while (num15 < 8)
					{
						array4[num11] = array2[num15];
						array5[num11] = array3[num15];
						num15++;
						num11++;
					}
					array24[num10] = array4;
					array25[num10] = array5;
				}
			}
			int num16 = CodeE5aI.PrimaryCodes.Length;
			int num17 = CodeE5aI.PrimaryCodes[0].Length;
			for (int m = 0; m < num16; m++)
			{
				byte[][] array6 = new byte[2][]
				{
					CodeE5aI.PrimaryCodes[m],
					CodeE5aI.NegatedPrimaryCodes[m]
				};
				byte[][] array7 = new byte[2][]
				{
					CodeE5bI.PrimaryCodes[m],
					CodeE5bI.NegatedPrimaryCodes[m]
				};
				byte[][] array8 = new byte[2][]
				{
					CodeE5aQ.PrimaryCodes[m],
					CodeE5aQ.NegatedPrimaryCodes[m]
				};
				byte[][] array9 = new byte[2][]
				{
					CodeE5bQ.PrimaryCodes[m],
					CodeE5bQ.NegatedPrimaryCodes[m]
				};
				sbyte[][] array10 = new sbyte[16][];
				sbyte[][] array11 = new sbyte[16][];
				for (int n = 0; n < 16; n++)
				{
					int num2 = n >> 3;
					int num3 = (n >> 2) & 1;
					int num4 = (n >> 1) & 1;
					int num5 = n & 1;
					byte[] array13 = array6[num2];
					byte[] array14 = array7[num3];
					byte[] array15 = array8[num4];
					byte[] array16 = array9[num5];
					sbyte[] array17 = new sbyte[num17 * 12];
					sbyte[] array18 = new sbyte[num17 * 12];
					int num6 = 0;
					int num7;
					for (num7 = 0; num7 < num17; num7++)
					{
						int num8 = array13[num7];
						num8 <<= 1;
						num8 |= array14[num7];
						num8 <<= 1;
						num8 |= array15[num7];
						num8 <<= 1;
						num8 |= array16[num7];
						num8 <<= 1;
						num7++;
						num8 |= array13[num7];
						num8 <<= 1;
						num8 |= array14[num7];
						num8 <<= 1;
						num8 |= array15[num7];
						num8 <<= 1;
						num8 |= array16[num7];
						sbyte[] array19 = array24[num8];
						sbyte[] array20 = array25[num8];
						for (int num9 = 0; num9 < 24; num9++)
						{
							array17[num6] = array19[num9];
							array18[num6] = array20[num9];
							num6++;
						}
					}
					array10[n] = array17;
					array11[n] = array18;
				}
				outputCodesI[m] = array10;
				outputCodesQ[m] = array11;
			}
		}

		public static uint GetModulationIndex(uint bitE5aI, uint bitE5bI, uint bitE5aQ, uint bitE5bQ)
		{
			return (((((bitE5aI << 1) | bitE5bI) << 1) | bitE5aQ) << 1) | bitE5bQ;
		}
	}
}
