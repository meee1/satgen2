using System.Diagnostics;
using Racelogic.DataTypes;

namespace Racelogic.Gnss.Gps
{
	public static class CodeL2CL
	{
		private const int sequencePeriod = 767250;

		private static readonly int[] taps;

		private static readonly uint[] seeds;

		private static readonly int satCount;

		private static readonly sbyte[][] signedCodes;

		public static sbyte[][] SignedCodes
		{
			[DebuggerStepThrough]
			get
			{
				return signedCodes;
			}
		}

		static CodeL2CL()
		{
			taps = new int[12]
			{
				3, 4, 5, 6, 9, 11, 13, 16, 19, 21,
				24, 27
			};
			uint[] array3 = new uint[37];
			ulong octalAsLong = 624145772uL;
			Octal octal = (Octal)octalAsLong;
			array3[0] = octal;
			ulong octalAsLong12 = 506610362uL;
			Octal octal12 = (Octal)octalAsLong12;
			array3[1] = octal12;
			ulong octalAsLong23 = 220360016uL;
			Octal octal23 = (Octal)octalAsLong23;
			array3[2] = octal23;
			ulong octalAsLong32 = 710406104uL;
			Octal octal32 = (Octal)octalAsLong32;
			array3[3] = octal32;
			ulong octalAsLong33 = 1143345uL;
			Octal octal33 = (Octal)octalAsLong33;
			array3[4] = octal33;
			ulong octalAsLong34 = 53023326uL;
			Octal octal34 = (Octal)octalAsLong34;
			array3[5] = octal34;
			ulong octalAsLong35 = 652521276uL;
			Octal octal35 = (Octal)octalAsLong35;
			array3[6] = octal35;
			ulong octalAsLong36 = 206124777uL;
			Octal octal36 = (Octal)octalAsLong36;
			array3[7] = octal36;
			ulong octalAsLong37 = 15563374uL;
			Octal octal37 = (Octal)octalAsLong37;
			array3[8] = octal37;
			ulong octalAsLong2 = 561522076uL;
			Octal octal2 = (Octal)octalAsLong2;
			array3[9] = octal2;
			ulong octalAsLong3 = 23163525uL;
			Octal octal3 = (Octal)octalAsLong3;
			array3[10] = octal3;
			ulong octalAsLong4 = 117776450uL;
			Octal octal4 = (Octal)octalAsLong4;
			array3[11] = octal4;
			ulong octalAsLong5 = 606516355uL;
			Octal octal5 = (Octal)octalAsLong5;
			array3[12] = octal5;
			ulong octalAsLong6 = 3037343uL;
			Octal octal6 = (Octal)octalAsLong6;
			array3[13] = octal6;
			ulong octalAsLong7 = 46515565uL;
			Octal octal7 = (Octal)octalAsLong7;
			array3[14] = octal7;
			ulong octalAsLong8 = 671511621uL;
			Octal octal8 = (Octal)octalAsLong8;
			array3[15] = octal8;
			ulong octalAsLong9 = 605402220uL;
			Octal octal9 = (Octal)octalAsLong9;
			array3[16] = octal9;
			ulong octalAsLong10 = 2576207uL;
			Octal octal10 = (Octal)octalAsLong10;
			array3[17] = octal10;
			ulong octalAsLong11 = 525163451uL;
			Octal octal11 = (Octal)octalAsLong11;
			array3[18] = octal11;
			ulong octalAsLong13 = 266527765uL;
			Octal octal13 = (Octal)octalAsLong13;
			array3[19] = octal13;
			ulong octalAsLong14 = 6760703uL;
			Octal octal14 = (Octal)octalAsLong14;
			array3[20] = octal14;
			ulong octalAsLong15 = 501474556uL;
			Octal octal15 = (Octal)octalAsLong15;
			array3[21] = octal15;
			ulong octalAsLong16 = 743747443uL;
			Octal octal16 = (Octal)octalAsLong16;
			array3[22] = octal16;
			ulong octalAsLong17 = 615534726uL;
			Octal octal17 = (Octal)octalAsLong17;
			array3[23] = octal17;
			ulong octalAsLong18 = 763621420uL;
			Octal octal18 = (Octal)octalAsLong18;
			array3[24] = octal18;
			ulong octalAsLong19 = 720727474uL;
			Octal octal19 = (Octal)octalAsLong19;
			array3[25] = octal19;
			ulong octalAsLong20 = 700521043uL;
			Octal octal20 = (Octal)octalAsLong20;
			array3[26] = octal20;
			ulong octalAsLong21 = 222567263uL;
			Octal octal21 = (Octal)octalAsLong21;
			array3[27] = octal21;
			ulong octalAsLong22 = 132765304uL;
			Octal octal22 = (Octal)octalAsLong22;
			array3[28] = octal22;
			ulong octalAsLong24 = 746332245uL;
			Octal octal24 = (Octal)octalAsLong24;
			array3[29] = octal24;
			ulong octalAsLong25 = 102300466uL;
			Octal octal25 = (Octal)octalAsLong25;
			array3[30] = octal25;
			ulong octalAsLong26 = 255231716uL;
			Octal octal26 = (Octal)octalAsLong26;
			array3[31] = octal26;
			ulong octalAsLong27 = 437661701uL;
			Octal octal27 = (Octal)octalAsLong27;
			array3[32] = octal27;
			ulong octalAsLong28 = 717047302uL;
			Octal octal28 = (Octal)octalAsLong28;
			array3[33] = octal28;
			ulong octalAsLong29 = 222614207uL;
			Octal octal29 = (Octal)octalAsLong29;
			array3[34] = octal29;
			ulong octalAsLong30 = 561123307uL;
			Octal octal30 = (Octal)octalAsLong30;
			array3[35] = octal30;
			ulong octalAsLong31 = 240713073uL;
			Octal octal31 = (Octal)octalAsLong31;
			array3[36] = octal31;
			seeds = array3;
			satCount = seeds.Length;
			signedCodes = new sbyte[satCount][];
			for (int i = 0; i < satCount; i++)
			{
				signedCodes[i] = new sbyte[767250];
			}
			for (int j = 0; j < satCount; j++)
			{
				sbyte[] array2 = signedCodes[j];
				int num = 0;
				foreach (byte current1 in GaloisShiftRegister.Generate(seeds[j], taps, 767250))
				{
					int current = current1 << 1;
					current--;
					array2[num++] = (sbyte)current;
				}
			}
		}
	}
}
