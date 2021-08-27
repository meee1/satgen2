using System.Diagnostics;
using Racelogic.DataTypes;

namespace Racelogic.Gnss.Gps
{
	public static class CodeL2CM
	{
		private const int codeLength = 10230;

		private static readonly int[] taps;

		private static readonly uint[] seeds;

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

		static CodeL2CM()
		{
			taps = new int[12]
			{
				3, 4, 5, 6, 9, 11, 13, 16, 19, 21,
				24, 27
			};
			uint[] array4 = new uint[37];
			ulong octalAsLong = 742417664uL;
			Octal octal = (Octal)octalAsLong;
			array4[0] = octal;
			ulong octalAsLong12 = 756014035uL;
			Octal octal12 = (Octal)octalAsLong12;
			array4[1] = octal12;
			ulong octalAsLong23 = 2747144uL;
			Octal octal23 = (Octal)octalAsLong23;
			array4[2] = octal23;
			ulong octalAsLong32 = 66265724uL;
			Octal octal32 = (Octal)octalAsLong32;
			array4[3] = octal32;
			ulong octalAsLong33 = 601403471uL;
			Octal octal33 = (Octal)octalAsLong33;
			array4[4] = octal33;
			ulong octalAsLong34 = 703232733uL;
			Octal octal34 = (Octal)octalAsLong34;
			array4[5] = octal34;
			ulong octalAsLong35 = 124510070uL;
			Octal octal35 = (Octal)octalAsLong35;
			array4[6] = octal35;
			ulong octalAsLong36 = 617316361uL;
			Octal octal36 = (Octal)octalAsLong36;
			array4[7] = octal36;
			ulong octalAsLong37 = 47541621uL;
			Octal octal37 = (Octal)octalAsLong37;
			array4[8] = octal37;
			ulong octalAsLong2 = 733031046uL;
			Octal octal2 = (Octal)octalAsLong2;
			array4[9] = octal2;
			ulong octalAsLong3 = 713512145uL;
			Octal octal3 = (Octal)octalAsLong3;
			array4[10] = octal3;
			ulong octalAsLong4 = 24437606uL;
			Octal octal4 = (Octal)octalAsLong4;
			array4[11] = octal4;
			ulong octalAsLong5 = 21264003uL;
			Octal octal5 = (Octal)octalAsLong5;
			array4[12] = octal5;
			ulong octalAsLong6 = 230655351uL;
			Octal octal6 = (Octal)octalAsLong6;
			array4[13] = octal6;
			ulong octalAsLong7 = 1314400uL;
			Octal octal7 = (Octal)octalAsLong7;
			array4[14] = octal7;
			ulong octalAsLong8 = 222021506uL;
			Octal octal8 = (Octal)octalAsLong8;
			array4[15] = octal8;
			ulong octalAsLong9 = 540264026uL;
			Octal octal9 = (Octal)octalAsLong9;
			array4[16] = octal9;
			ulong octalAsLong10 = 205521705uL;
			Octal octal10 = (Octal)octalAsLong10;
			array4[17] = octal10;
			ulong octalAsLong11 = 64022144uL;
			Octal octal11 = (Octal)octalAsLong11;
			array4[18] = octal11;
			ulong octalAsLong13 = 120161274uL;
			Octal octal13 = (Octal)octalAsLong13;
			array4[19] = octal13;
			ulong octalAsLong14 = 44023533uL;
			Octal octal14 = (Octal)octalAsLong14;
			array4[20] = octal14;
			ulong octalAsLong15 = 724744327uL;
			Octal octal15 = (Octal)octalAsLong15;
			array4[21] = octal15;
			ulong octalAsLong16 = 45743577uL;
			Octal octal16 = (Octal)octalAsLong16;
			array4[22] = octal16;
			ulong octalAsLong17 = 741201660uL;
			Octal octal17 = (Octal)octalAsLong17;
			array4[23] = octal17;
			ulong octalAsLong18 = 700274134uL;
			Octal octal18 = (Octal)octalAsLong18;
			array4[24] = octal18;
			ulong octalAsLong19 = 10247261uL;
			Octal octal19 = (Octal)octalAsLong19;
			array4[25] = octal19;
			ulong octalAsLong20 = 713433445uL;
			Octal octal20 = (Octal)octalAsLong20;
			array4[26] = octal20;
			ulong octalAsLong21 = 737324162uL;
			Octal octal21 = (Octal)octalAsLong21;
			array4[27] = octal21;
			ulong octalAsLong22 = 311627434uL;
			Octal octal22 = (Octal)octalAsLong22;
			array4[28] = octal22;
			ulong octalAsLong24 = 710452007uL;
			Octal octal24 = (Octal)octalAsLong24;
			array4[29] = octal24;
			ulong octalAsLong25 = 722462133uL;
			Octal octal25 = (Octal)octalAsLong25;
			array4[30] = octal25;
			ulong octalAsLong26 = 50172213uL;
			Octal octal26 = (Octal)octalAsLong26;
			array4[31] = octal26;
			ulong octalAsLong27 = 500653703uL;
			Octal octal27 = (Octal)octalAsLong27;
			array4[32] = octal27;
			ulong octalAsLong28 = 755077436uL;
			Octal octal28 = (Octal)octalAsLong28;
			array4[33] = octal28;
			ulong octalAsLong29 = 136717361uL;
			Octal octal29 = (Octal)octalAsLong29;
			array4[34] = octal29;
			ulong octalAsLong30 = 756675453uL;
			Octal octal30 = (Octal)octalAsLong30;
			array4[35] = octal30;
			ulong octalAsLong31 = 435506112uL;
			Octal octal31 = (Octal)octalAsLong31;
			array4[36] = octal31;
			seeds = array4;
			satCount = seeds.Length;
			signedCodes = new sbyte[satCount][];
			negatedSignedCodes = new sbyte[satCount][];
			for (int i = 0; i < satCount; i++)
			{
				signedCodes[i] = new sbyte[10230];
				negatedSignedCodes[i] = new sbyte[10230];
			}
			for (int j = 0; j < satCount; j++)
			{
				sbyte[] array2 = signedCodes[j];
				sbyte[] array3 = negatedSignedCodes[j];
				int num = 0;
				foreach (byte current1 in GaloisShiftRegister.Generate(seeds[j], taps, 10230))
				{
					int current = current1 << 1;
					current--;
					array2[num] = (sbyte)current;
					array3[num] = (sbyte)(-current);
					num++;
				}
			}
		}
	}
}
