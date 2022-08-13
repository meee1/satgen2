using System;

namespace Racelogic.Maths;

public static class CartesianTransforms
{
	public static double[] Degrees2Meters(double lat1, double lon1, double lat2, double lon2)
	{
		double bearing;
		double num = DistanceVincenty(lat1, lon1, lat2, lon2, out bearing);
		double num2 = num * Math.Sin(bearing);
		double num3 = num * Math.Cos(bearing);
		return new double[2] { num2, num3 };
	}

	public static double DistanceVincenty(double lat1, double lon1, double lat2, double lon2)
	{
		double bearing;
		return DistanceVincenty(lat1, lon1, lat2, lon2, out bearing);
	}

	public static double DistanceVincenty(double lat1, double lon1, double lat2, double lon2, out double bearing)
	{
		double num = ToRad(lon2 - lon1);
		double num2 = Math.Atan(0.99664718933525254 * Math.Tan(ToRad(lat1)));
		double num3 = Math.Atan(0.99664718933525254 * Math.Tan(ToRad(lat2)));
		double num4 = Math.Sin(num2);
		double num5 = Math.Cos(num2);
		double num6 = Math.Sin(num3);
		double num7 = Math.Cos(num3);
		double num8 = num;
		int num9 = 100;
		double num10;
		double num11;
		double num12;
		double num13;
		double num14;
		double num16;
		double num17;
		double num19;
		do
		{
			num10 = Math.Sin(num8);
			num11 = Math.Cos(num8);
			num12 = Math.Sqrt(num7 * num10 * (num7 * num10) + (num5 * num6 - num4 * num7 * num11) * (num5 * num6 - num4 * num7 * num11));
			if (num12 == 0.0)
			{
				bearing = -1.0;
				return 0.0;
			}
			num13 = num4 * num6 + num5 * num7 * num11;
			num14 = Math.Atan2(num12, num13);
			double num15 = num5 * num7 * num10 / num12;
			num16 = 1.0 - num15 * num15;
			num17 = num13 - 2.0 * num4 * num6 / num16;
			if (double.IsNaN(num17))
			{
				num17 = 0.0;
			}
			double num18 = 0.00020955066654671753 * num16 * (4.0 + 0.0033528106647474805 * (4.0 - 3.0 * num16));
			num19 = num8;
			num8 = num + (1.0 - num18) * 0.0033528106647474805 * num15 * (num14 + num18 * num12 * (num17 + num18 * num13 * (-1.0 + 2.0 * num17 * num17)));
		}
		while (Math.Abs(num8 - num19) > 1E-12 && --num9 > 0);
		if (num9 == 0)
		{
			bearing = -1.0;
			return -1.0;
		}
		double num20 = num16 * 272331606109.84375 / 40408299984659.156;
		double num21 = 1.0 + num20 / 16384.0 * (4096.0 + num20 * (-768.0 + num20 * (320.0 - 175.0 * num20)));
		double num22 = num20 / 1024.0 * (256.0 + num20 * (-128.0 + num20 * (74.0 - 47.0 * num20)));
		double num23 = num22 * num12 * (num17 + num22 / 4.0 * (num13 * (-1.0 + 2.0 * num17 * num17) - num22 / 6.0 * num17 * (-3.0 + 4.0 * num12 * num12) * (-3.0 + 4.0 * num17 * num17)));
		double result = 6356752.314245 * num21 * (num14 - num23);
		bearing = Math.Atan2(num7 * num10, num5 * num6 - num4 * num7 * num11);
		return result;
	}

	public static double[] Meters2Degrees(double baseLat, double baseLong, double x, double y)
	{
		double num = Math.Sqrt(x * x + y * y);
		double bearing;
		if (y > 0.0)
		{
			bearing = Todeg(Math.Asin(x / num));
		}
		else
		{
			bearing = Todeg(Math.Acos(x / num));
			bearing += 90.0;
		}
		return Vincenty(baseLat / 60.0, baseLong / 60.0, num, bearing);
	}

	public static double ToRad(double deg)
	{
		return deg * Math.PI / 180.0;
	}

	public static double Todeg(double rad)
	{
		return rad * 180.0 / Math.PI;
	}

	public static double[] Vincenty(double baseLat, double baseLong, double distance, double bearing)
	{
		double num = ToRad(bearing);
		double num2 = Math.Sin(num);
		double num3 = Math.Cos(num);
		double num4 = 0.99664718933525254 * Math.Tan(ToRad(baseLat));
		double num5 = 1.0 / Math.Sqrt(1.0 + num4 * num4);
		double num6 = num4 * num5;
		double num7 = Math.Atan2(num4, num3);
		double num8 = num5 * num2;
		double num9 = 1.0 - num8 * num8;
		double num10 = num9 * 272331606681.94531 / 40408299984087.055;
		double num11 = 1.0 + num10 / 16384.0 * (4096.0 + num10 * (-768.0 + num10 * (320.0 - 175.0 * num10)));
		double num12 = num10 / 1024.0 * (256.0 + num10 * (-128.0 + num10 * (74.0 - 47.0 * num10)));
		double num13 = distance / (6356752.3142 * num11);
		double num14 = Math.PI * 2.0;
		double num15 = 0.0;
		double num16 = 0.0;
		double num17 = 0.0;
		while (Math.Abs(num13 - num14) > 1E-12)
		{
			num17 = Math.Cos(2.0 * num7 + num13);
			num15 = Math.Sin(num13);
			num16 = Math.Cos(num13);
			double num18 = num12 * num15 * (num17 + num12 / 4.0 * (num16 * (-1.0 + 2.0 * num17 * num17) - num12 / 6.0 * num17 * (-3.0 + 4.0 * num15 * num15) * (-3.0 + 4.0 * num17 * num17)));
			num14 = num13;
			num13 = distance / (6356752.3142 * num11) + num18;
		}
		double num19 = num6 * num15 - num5 * num16 * num3;
		double rad = Math.Atan2(num6 * num16 + num5 * num15 * num3, 0.99664718933525254 * Math.Sqrt(num8 * num8 + num19 * num19));
		double num20 = Math.Atan2(num15 * num2, num5 * num16 - num6 * num15 * num3);
		double num21 = 0.00020955066654671753 * num9 * (4.0 + 0.0033528106647474805 * (4.0 - 3.0 * num9));
		double num22 = num20 - (1.0 - num21) * 0.0033528106647474805 * num8 * (num13 + num21 * num15 * (num17 + num21 * num16 * (-1.0 + 2.0 * num17 * num17)));
		double rad2 = (ToRad(baseLong) + num22 + Math.PI * 3.0) % (Math.PI * 2.0) - Math.PI;
		double num23 = Todeg(rad);
		double num24 = Todeg(rad2);
		int num25 = (int)num23;
		double num26 = (num23 - (double)num25) * 60.0;
		int num27 = (int)num26;
		double num28 = (num26 - (double)num27) * 60.0;
		double num29 = (double)num25 * 60.0 + (double)num27 + num28 / 60.0;
		int num30 = (int)num24;
		double num31 = (num24 - (double)num30) * 60.0;
		int num32 = (int)num31;
		double num33 = (num31 - (double)num32) * 60.0;
		double num34 = (double)num30 * 60.0 + (double)num32 + num33 / 60.0;
		return new double[2] { num29, num34 };
	}
}
