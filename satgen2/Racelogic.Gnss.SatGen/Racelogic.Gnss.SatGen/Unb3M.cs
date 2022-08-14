using System;

namespace Racelogic.Gnss.SatGen;

internal static class Unb3M
{
	private const double elevationCutoff = 0.0017453292519943296;

	private const double EXCEN2 = 0.0066943799901413;

	private const double MD = 28.9644;

	private const double MW = 18.0152;

	private const double K1 = 77.604;

	private const double K2 = 64.79;

	private const double K3 = 377600.0;

	private const double R = 8314.34;

	private const double C1 = 0.0022768;

	private const double K2PRIM = 16.522071757053496;

	private const double RD = 287.0537625498888;

	private const double DOY2RAD = 0.017202423838958487;

	private const double A_HT = 2.53E-05;

	private const double B_HT = 0.00549;

	private const double C_HT = 0.00114;

	private const double HT_TOPCON = 1.0000251620178218;

	private static readonly double[,] AVG = new double[5, 6]
	{
		{ 15.0, 1013.25, 299.65, 75.0, 6.3, 2.77 },
		{ 30.0, 1017.25, 294.15, 80.0, 6.05, 3.15 },
		{ 45.0, 1015.75, 283.15, 76.0, 5.58, 2.57 },
		{ 60.0, 1011.75, 272.15, 77.5, 5.39, 1.81 },
		{ 75.0, 1013.0, 263.65, 82.5, 4.53, 1.55 }
	};

	private static readonly double[,] AMP = new double[5, 6]
	{
		{ 15.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
		{ 30.0, -3.75, 7.0, 0.0, 0.25, 0.33 },
		{ 45.0, -2.25, 11.0, -1.0, 0.32, 0.46 },
		{ 60.0, -1.75, 15.0, -2.5, 0.81, 0.74 },
		{ 75.0, -0.5, 14.5, 2.5, 0.62, 0.3 }
	};

	private static readonly double[,] ABC_AVG = new double[5, 4]
	{
		{ 15.0, 0.0012769934, 0.0029153695, 0.062610505 },
		{ 30.0, 0.001268323, 0.0029152299, 0.062837393 },
		{ 45.0, 0.0012465397, 0.0029288445, 0.063721774 },
		{ 60.0, 0.0012196049, 0.0029022565, 0.063824265 },
		{ 75.0, 0.0012045996, 0.0029024912, 0.064258455 }
	};

	private static readonly double[,] ABC_AMP = new double[5, 4]
	{
		{ 15.0, 0.0, 0.0, 0.0 },
		{ 30.0, 1.2709626E-05, 2.1414979E-05, 9.01284E-05 },
		{ 45.0, 2.6523662E-05, 3.0160779E-05, 4.3497037E-05 },
		{ 60.0, 3.4000452E-05, 7.2562722E-05, 0.00084795348 },
		{ 75.0, 4.1202191E-05, 0.00011723375, 0.0017037206 }
	};

	private static readonly double[,] ABC_W2P0 = new double[5, 4]
	{
		{ 15.0, 0.00058021897, 0.0014275268, 0.043472961 },
		{ 30.0, 0.00056794847, 0.0015138625, 0.04672951 },
		{ 45.0, 0.00058118019, 0.0014572752, 0.043908931 },
		{ 60.0, 0.00059727542, 0.0015007428, 0.044626982 },
		{ 75.0, 0.00061641693, 0.0017599082, 0.054736038 }
	};

	public static double GetTroposphericDelay(in double latitudeRadians, in double height, double dayOfYear, double elevation)
	{
		if (latitudeRadians < 0.0)
		{
			dayOfYear += 182.625;
		}
		if (elevation < 0.0017453292519943296)
		{
			elevation = 0.0017453292519943296;
		}
		double num = Math.Abs(latitudeRadians * (180.0 / Math.PI));
		double num2 = Math.Cos((dayOfYear - 28.0) * 0.017202423838958487);
		int num3;
		int num4;
		double num5;
		if (num >= 75.0)
		{
			num3 = 4;
			num4 = 4;
			num5 = 0.0;
		}
		else if (num <= 15.0)
		{
			num3 = 0;
			num4 = 0;
			num5 = 0.0;
		}
		else
		{
			num3 = (int)Math.Truncate((num - 15.0) / 15.0);
			num4 = num3 + 1;
			num5 = (num - AVG[num3, 0]) / (AVG[num4, 0] - AVG[num3, 0]);
		}
		double num6 = num5 * (AVG[num4, 1] - AVG[num3, 1]) + AVG[num3, 1];
		double num7 = num5 * (AVG[num4, 2] - AVG[num3, 2]) + AVG[num3, 2];
		double num8 = num5 * (AVG[num4, 3] - AVG[num3, 3]) + AVG[num3, 3];
		double num9 = num5 * (AVG[num4, 4] - AVG[num3, 4]) + AVG[num3, 4];
		double num10 = num5 * (AVG[num4, 5] - AVG[num3, 5]) + AVG[num3, 5];
		double num11 = num5 * (AMP[num4, 1] - AMP[num3, 1]) + AMP[num3, 1];
		double num12 = num5 * (AMP[num4, 2] - AMP[num3, 2]) + AMP[num3, 2];
		double num13 = num5 * (AMP[num4, 3] - AMP[num3, 3]) + AMP[num3, 3];
		double num14 = num5 * (AMP[num4, 4] - AMP[num3, 4]) + AMP[num3, 4];
		double num15 = num5 * (AMP[num4, 5] - AMP[num3, 5]) + AMP[num3, 5];
		double num16 = num6 - num11 * num2;
		double num17 = num7 - num12 * num2;
		double num18 = num8 - num13 * num2;
		double num19 = (num9 - num14 * num2) * 0.001;
		double num20 = num10 - num15 * num2 + 1.0;
		double num21 = 0.01 * Math.Exp(1.2378847E-05 * (num17 * num17) - 0.019121316 * num17 + 33.93711047 - 6343.1645 / num17);
		double num22 = num17 - 273.15;
		double num23 = 1.00062 + 3.14E-06 * num16 + 5.6E-07 * num22 * num22;
		num18 *= 0.01 * num21 * num23;
		double num24 = 0.034163084297727957 / num19;
		double num25 = num17 - num19 * height;
		double num26 = num25 / num17;
		if (num26 < 0.0)
		{
			return 0.0;
		}
		double num27 = num16 * Math.Pow(num26, num24);
		double num28 = num18 * Math.Pow(num26, num24 * num20);
		double num29 = Math.Atan(0.99330562000985867 * Math.Tan(latitudeRadians));
		double num30 = 1.0 - 0.00266 * Math.Cos(2.0 * num29) - 2.8E-07 * height;
		double num31 = 9.784 * num30;
		double num32 = num20 * num31;
		double num33 = 287.0537625498888 / num32;
		double num34 = num25 * (1.0 - num19 * num33);
		double num35 = 0.0022768 / num30 * num27;
		double num36 = 1E-06 * (16.522071757053496 + 377600.0 / num34) * num28 * num33;
		double num37 = num5 * (ABC_AVG[num4, 1] - ABC_AVG[num3, 1]) + ABC_AVG[num3, 1];
		double num38 = num5 * (ABC_AVG[num4, 2] - ABC_AVG[num3, 2]) + ABC_AVG[num3, 2];
		double num39 = num5 * (ABC_AVG[num4, 3] - ABC_AVG[num3, 3]) + ABC_AVG[num3, 3];
		double num40 = num5 * (ABC_AMP[num4, 1] - ABC_AMP[num3, 1]) + ABC_AMP[num3, 1];
		double num41 = num5 * (ABC_AMP[num4, 2] - ABC_AMP[num3, 2]) + ABC_AMP[num3, 2];
		double num42 = num5 * (ABC_AMP[num4, 3] - ABC_AMP[num3, 3]) + ABC_AMP[num3, 3];
		double num43 = num37 - num40 * num2;
		double num44 = num38 - num41 * num2;
		double num45 = num39 - num42 * num2;
		double num46 = Math.Sin(elevation);
		double num47 = num44 / (num46 + num45);
		double num48 = num43 / (num46 + num47);
		double num49 = (1.0 + num43 / (1.0 + num44 / (1.0 + num45))) / (num46 + num48);
		num47 = 0.00549 / (num46 + 0.00114);
		num48 = 2.53E-05 / (num46 + num47);
		double num50 = (1.0 / num46 - 1.0000251620178218 / (num46 + num48)) * height * 0.001;
		num49 += num50;
		num43 = num5 * (ABC_W2P0[num4, 1] - ABC_W2P0[num3, 1]) + ABC_W2P0[num3, 1];
		num44 = num5 * (ABC_W2P0[num4, 2] - ABC_W2P0[num3, 2]) + ABC_W2P0[num3, 2];
		num45 = num5 * (ABC_W2P0[num4, 3] - ABC_W2P0[num3, 3]) + ABC_W2P0[num3, 3];
		num47 = num44 / (num46 + num45);
		num48 = num43 / (num46 + num47);
		double num51 = (1.0 + num43 / (1.0 + num44 / (1.0 + num45))) / (num46 + num48);
		return num35 * num49 + num36 * num51;
	}
}
