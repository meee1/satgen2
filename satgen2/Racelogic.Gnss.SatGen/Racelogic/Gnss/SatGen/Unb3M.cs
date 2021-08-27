using System;

namespace Racelogic.Gnss.SatGen
{
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
			double num11 = Math.Cos((dayOfYear - 28.0) * 0.017202423838958487);
			int num22;
			int num31;
			double num42;
			if (num >= 75.0)
			{
				num22 = 4;
				num31 = 4;
				num42 = 0.0;
			}
			else if (num <= 15.0)
			{
				num22 = 0;
				num31 = 0;
				num42 = 0.0;
			}
			else
			{
				num22 = (int)Math.Truncate((num - 15.0) / 15.0);
				num31 = num22 + 1;
				num42 = (num - AVG[num22, 0]) / (AVG[num31, 0] - AVG[num22, 0]);
			}
			double num45 = num42 * (AVG[num31, 1] - AVG[num22, 1]) + AVG[num22, 1];
			double num46 = num42 * (AVG[num31, 2] - AVG[num22, 2]) + AVG[num22, 2];
			double num47 = num42 * (AVG[num31, 3] - AVG[num22, 3]) + AVG[num22, 3];
			double num48 = num42 * (AVG[num31, 4] - AVG[num22, 4]) + AVG[num22, 4];
			double num49 = num42 * (AVG[num31, 5] - AVG[num22, 5]) + AVG[num22, 5];
			double num2 = num42 * (AMP[num31, 1] - AMP[num22, 1]) + AMP[num22, 1];
			double num3 = num42 * (AMP[num31, 2] - AMP[num22, 2]) + AMP[num22, 2];
			double num4 = num42 * (AMP[num31, 3] - AMP[num22, 3]) + AMP[num22, 3];
			double num5 = num42 * (AMP[num31, 4] - AMP[num22, 4]) + AMP[num22, 4];
			double num6 = num42 * (AMP[num31, 5] - AMP[num22, 5]) + AMP[num22, 5];
			double num7 = num45 - num2 * num11;
			double num8 = num46 - num3 * num11;
			double num9 = num47 - num4 * num11;
			double num10 = (num48 - num5 * num11) * 0.001;
			double num12 = num49 - num6 * num11 + 1.0;
			double num13 = 0.01 * Math.Exp(1.2378847E-05 * (num8 * num8) - 0.019121316 * num8 + 33.93711047 - 6343.1645 / num8);
			double num14 = num8 - 273.15;
			double num15 = 1.00062 + 3.14E-06 * num7 + 5.6E-07 * num14 * num14;
			num9 *= 0.01 * num13 * num15;
			double num16 = 0.034163084297727957 / num10;
			double num17 = num8 - num10 * height;
			double num18 = num17 / num8;
			if (num18 < 0.0)
			{
				return 0.0;
			}
			double num19 = num7 * Math.Pow(num18, num16);
			double num20 = num9 * Math.Pow(num18, num16 * num12);
			double num21 = Math.Atan(0.99330562000985867 * Math.Tan(latitudeRadians));
			double num23 = 1.0 - 0.00266 * Math.Cos(2.0 * num21) - 2.8E-07 * height;
			double num24 = 9.784 * num23;
			double num25 = num12 * num24;
			double num26 = 287.0537625498888 / num25;
			double num27 = num17 * (1.0 - num10 * num26);
			double num50 = 0.0022768 / num23 * num19;
			double num28 = 1E-06 * (16.522071757053496 + 377600.0 / num27) * num20 * num26;
			double num29 = num42 * (ABC_AVG[num31, 1] - ABC_AVG[num22, 1]) + ABC_AVG[num22, 1];
			double num30 = num42 * (ABC_AVG[num31, 2] - ABC_AVG[num22, 2]) + ABC_AVG[num22, 2];
			double num51 = num42 * (ABC_AVG[num31, 3] - ABC_AVG[num22, 3]) + ABC_AVG[num22, 3];
			double num32 = num42 * (ABC_AMP[num31, 1] - ABC_AMP[num22, 1]) + ABC_AMP[num22, 1];
			double num33 = num42 * (ABC_AMP[num31, 2] - ABC_AMP[num22, 2]) + ABC_AMP[num22, 2];
			double num34 = num42 * (ABC_AMP[num31, 3] - ABC_AMP[num22, 3]) + ABC_AMP[num22, 3];
			double num35 = num29 - num32 * num11;
			double num36 = num30 - num33 * num11;
			double num37 = num51 - num34 * num11;
			double num38 = Math.Sin(elevation);
			double num39 = num36 / (num38 + num37);
			double num40 = num35 / (num38 + num39);
			double num41 = (1.0 + num35 / (1.0 + num36 / (1.0 + num37))) / (num38 + num40);
			num39 = 0.00549 / (num38 + 0.00114);
			num40 = 2.53E-05 / (num38 + num39);
			double num43 = (1.0 / num38 - 1.0000251620178218 / (num38 + num40)) * height * 0.001;
			num41 += num43;
			num35 = num42 * (ABC_W2P0[num31, 1] - ABC_W2P0[num22, 1]) + ABC_W2P0[num22, 1];
			num36 = num42 * (ABC_W2P0[num31, 2] - ABC_W2P0[num22, 2]) + ABC_W2P0[num22, 2];
			num37 = num42 * (ABC_W2P0[num31, 3] - ABC_W2P0[num22, 3]) + ABC_W2P0[num22, 3];
			num39 = num36 / (num38 + num37);
			num40 = num35 / (num38 + num39);
			double num44 = (1.0 + num35 / (1.0 + num36 / (1.0 + num37))) / (num38 + num40);
			return num50 * num41 + num28 * num44;
		}
	}
}
