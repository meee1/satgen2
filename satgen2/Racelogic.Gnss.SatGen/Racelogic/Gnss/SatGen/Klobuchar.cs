using System;
using System.Collections.Generic;
using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	internal static class Klobuchar
	{
		private const double Semi2Rad = Math.PI;

		private const double Rad2Semi = 1.0 / Math.PI;

		private const double elevationCutoff = -0.08;

		private const double bumpThreshold = 0.01;

		private static readonly double[] alpha = new double[4] { 1.211E-08, 1.49E-08, -5.96E-08, -1.192E-07 };

		private static readonly double[] beta = new double[4] { 96260.0, 81920.0, -196600.0, -393200.0 };

		private static readonly Range<double> verticalExtent = new Range<double>(80000.0, 620000.0);

		public static IReadOnlyList<double> Alpha
		{
			[DebuggerStepThrough]
			get
			{
				return alpha;
			}
		}

		public static IReadOnlyList<double> Beta
		{
			[DebuggerStepThrough]
			get
			{
				return beta;
			}
		}

		public static double GetIonosphericDelay(in GnssTime time, in Geodetic position, in Topocentric azimuthElevation, in double frequency, in double referenceFrequency = 1575420000.0)
		{
			(double Latitude, double Longitude, double Altitude) tuple = position.Deconstruct();
			double item = tuple.Latitude;
			double item2 = tuple.Longitude;
			double item3 = tuple.Altitude;
			double num = item * (1.0 / Math.PI);
			double num12 = item2 * (1.0 / Math.PI);
			if (item3 >= verticalExtent.Max)
			{
				return 0.0;
			}
			(double Azimuth, double Elevation) tuple2 = azimuthElevation.Deconstruct();
			double item4 = tuple2.Azimuth;
			double num17 = tuple2.Elevation * (1.0 / Math.PI);
			if (num17 < -0.08)
			{
				num17 = -0.08;
			}
			double num18 = 0.0137 / (num17 + 0.11) - 0.022;
			double num19 = Math.Cos(item4);
			double num20 = Math.CopySign(Math.Sqrt(1.0 - num19 * num19), item4);
			double num21 = num + num18 * num19;
			if (num21 > 0.416)
			{
				num21 = 0.416;
			}
			if (num21 < -0.416)
			{
				num21 = -0.416;
			}
			double num22 = num12 + num18 * num20 / Math.Cos(num21 * Math.PI);
			double num23 = num21 + 0.064 * Math.Cos((num22 - 1.617) * Math.PI);
			DateTime utcTime = time.UtcTime;
			int num2 = (int)(utcTime - utcTime.Date).TotalSeconds;
			double num3 = 43200.0 * num22 + (double)num2;
			if (num3 < 0.0)
			{
				num3 += 86400.0;
			}
			else if (num3 >= 86400.0)
			{
				num3 -= 86400.0;
			}
			double num4 = 0.53 - num17;
			double num5 = 1.0 + 16.0 * num4 * num4 * num4;
			double num6 = num23 * num23;
			double num7 = num6 * num23;
			double num8 = beta[0] + beta[1] * num23 + beta[2] * num6 + beta[3] * num7;
			if (num8 < 72000.0)
			{
				num8 = 72000.0;
			}
			double num9 = Math.PI * 2.0 * (num3 - 50400.0) / num8;
			double num10 = alpha[0] + alpha[1] * num23 + alpha[2] * num6 + alpha[3] * num7;
			if (num10 < 0.0)
			{
				num10 = 0.0;
			}
			double num13;
			if (Math.Abs(num9) < 1.57)
			{
				double num11 = num9 * num9;
				num13 = num5 * (5E-09 + num10 * (1.0 - num11 * (0.5 + num11 * 0.041666666666666664)));
			}
			else
			{
				num13 = num5 * 5E-09;
			}
			if (verticalExtent.ContainsExclusive(item3))
			{
				double num14 = (item3 - verticalExtent.Min) / verticalExtent.Width;
				if (num14 < 0.5)
				{
					num13 *= 1.0 - 2.0 * num14 * num14;
				}
				else
				{
					double num15 = 1.0 - num14;
					num13 *= 2.0 * num15 * num15;
				}
				if (num17 < 0.01)
				{
					num13 *= 1.0 + 1.41 * (0.01 - num17) * 11.111111111111111;
				}
			}
			if (frequency != referenceFrequency)
			{
				double num16 = referenceFrequency / frequency;
				num13 *= num16 * num16;
			}
			return num13 * 299792458.0;
		}
	}
}
