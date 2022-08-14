using System.Diagnostics;
using Racelogic.DataTypes;

namespace Racelogic.Gnss.Gps;

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
		uint[] array = new uint[37];
		ulong octalAsLong = 624145772uL;
		Octal octal = (Octal)octalAsLong;
		array[0] = octal;
		ulong octalAsLong2 = 506610362uL;
		Octal octal2 = (Octal)octalAsLong2;
		array[1] = octal2;
		ulong octalAsLong3 = 220360016uL;
		Octal octal3 = (Octal)octalAsLong3;
		array[2] = octal3;
		ulong octalAsLong4 = 710406104uL;
		Octal octal4 = (Octal)octalAsLong4;
		array[3] = octal4;
		ulong octalAsLong5 = 1143345uL;
		Octal octal5 = (Octal)octalAsLong5;
		array[4] = octal5;
		ulong octalAsLong6 = 53023326uL;
		Octal octal6 = (Octal)octalAsLong6;
		array[5] = octal6;
		ulong octalAsLong7 = 652521276uL;
		Octal octal7 = (Octal)octalAsLong7;
		array[6] = octal7;
		ulong octalAsLong8 = 206124777uL;
		Octal octal8 = (Octal)octalAsLong8;
		array[7] = octal8;
		ulong octalAsLong9 = 15563374uL;
		Octal octal9 = (Octal)octalAsLong9;
		array[8] = octal9;
		ulong octalAsLong10 = 561522076uL;
		Octal octal10 = (Octal)octalAsLong10;
		array[9] = octal10;
		ulong octalAsLong11 = 23163525uL;
		Octal octal11 = (Octal)octalAsLong11;
		array[10] = octal11;
		ulong octalAsLong12 = 117776450uL;
		Octal octal12 = (Octal)octalAsLong12;
		array[11] = octal12;
		ulong octalAsLong13 = 606516355uL;
		Octal octal13 = (Octal)octalAsLong13;
		array[12] = octal13;
		ulong octalAsLong14 = 3037343uL;
		Octal octal14 = (Octal)octalAsLong14;
		array[13] = octal14;
		ulong octalAsLong15 = 46515565uL;
		Octal octal15 = (Octal)octalAsLong15;
		array[14] = octal15;
		ulong octalAsLong16 = 671511621uL;
		Octal octal16 = (Octal)octalAsLong16;
		array[15] = octal16;
		ulong octalAsLong17 = 605402220uL;
		Octal octal17 = (Octal)octalAsLong17;
		array[16] = octal17;
		ulong octalAsLong18 = 2576207uL;
		Octal octal18 = (Octal)octalAsLong18;
		array[17] = octal18;
		ulong octalAsLong19 = 525163451uL;
		Octal octal19 = (Octal)octalAsLong19;
		array[18] = octal19;
		ulong octalAsLong20 = 266527765uL;
		Octal octal20 = (Octal)octalAsLong20;
		array[19] = octal20;
		ulong octalAsLong21 = 6760703uL;
		Octal octal21 = (Octal)octalAsLong21;
		array[20] = octal21;
		ulong octalAsLong22 = 501474556uL;
		Octal octal22 = (Octal)octalAsLong22;
		array[21] = octal22;
		ulong octalAsLong23 = 743747443uL;
		Octal octal23 = (Octal)octalAsLong23;
		array[22] = octal23;
		ulong octalAsLong24 = 615534726uL;
		Octal octal24 = (Octal)octalAsLong24;
		array[23] = octal24;
		ulong octalAsLong25 = 763621420uL;
		Octal octal25 = (Octal)octalAsLong25;
		array[24] = octal25;
		ulong octalAsLong26 = 720727474uL;
		Octal octal26 = (Octal)octalAsLong26;
		array[25] = octal26;
		ulong octalAsLong27 = 700521043uL;
		Octal octal27 = (Octal)octalAsLong27;
		array[26] = octal27;
		ulong octalAsLong28 = 222567263uL;
		Octal octal28 = (Octal)octalAsLong28;
		array[27] = octal28;
		ulong octalAsLong29 = 132765304uL;
		Octal octal29 = (Octal)octalAsLong29;
		array[28] = octal29;
		ulong octalAsLong30 = 746332245uL;
		Octal octal30 = (Octal)octalAsLong30;
		array[29] = octal30;
		ulong octalAsLong31 = 102300466uL;
		Octal octal31 = (Octal)octalAsLong31;
		array[30] = octal31;
		ulong octalAsLong32 = 255231716uL;
		Octal octal32 = (Octal)octalAsLong32;
		array[31] = octal32;
		ulong octalAsLong33 = 437661701uL;
		Octal octal33 = (Octal)octalAsLong33;
		array[32] = octal33;
		ulong octalAsLong34 = 717047302uL;
		Octal octal34 = (Octal)octalAsLong34;
		array[33] = octal34;
		ulong octalAsLong35 = 222614207uL;
		Octal octal35 = (Octal)octalAsLong35;
		array[34] = octal35;
		ulong octalAsLong36 = 561123307uL;
		Octal octal36 = (Octal)octalAsLong36;
		array[35] = octal36;
		ulong octalAsLong37 = 240713073uL;
		Octal octal37 = (Octal)octalAsLong37;
		array[36] = octal37;
		seeds = array;
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
			foreach (byte item in GaloisShiftRegister.Generate(seeds[j], taps, 767250))
			{
				int current = item << 1;
				current--;
				array2[num++] = (sbyte)current;
			}
		}
	}
}
