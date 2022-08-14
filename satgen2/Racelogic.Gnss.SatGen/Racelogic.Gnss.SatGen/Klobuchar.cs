using System;
using System.Collections.Generic;
using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

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
		double num2 = item2 * (1.0 / Math.PI);
		if (item3 >= verticalExtent.Max)
		{
			return 0.0;
		}
		(double Azimuth, double Elevation) tuple2 = azimuthElevation.Deconstruct();
		double item4 = tuple2.Azimuth;
		double num3 = tuple2.Elevation * (1.0 / Math.PI);
		if (num3 < -0.08)
		{
			num3 = -0.08;
		}
		double num4 = 0.0137 / (num3 + 0.11) - 0.022;
		double num5 = Math.Cos(item4);
		double num6 = Math.CopySign(Math.Sqrt(1.0 - num5 * num5), item4);
		double num7 = num + num4 * num5;
		if (num7 > 0.416)
		{
			num7 = 0.416;
		}
		if (num7 < -0.416)
		{
			num7 = -0.416;
		}
		double num8 = num2 + num4 * num6 / Math.Cos(num7 * Math.PI);
		double num9 = num7 + 0.064 * Math.Cos((num8 - 1.617) * Math.PI);
		DateTime utcTime = time.UtcTime;
		int num10 = (int)(utcTime - utcTime.Date).TotalSeconds;
		double num11 = 43200.0 * num8 + (double)num10;
		if (num11 < 0.0)
		{
			num11 += 86400.0;
		}
		else if (num11 >= 86400.0)
		{
			num11 -= 86400.0;
		}
		double num12 = 0.53 - num3;
		double num13 = 1.0 + 16.0 * num12 * num12 * num12;
		double num14 = num9 * num9;
		double num15 = num14 * num9;
		double num16 = beta[0] + beta[1] * num9 + beta[2] * num14 + beta[3] * num15;
		if (num16 < 72000.0)
		{
			num16 = 72000.0;
		}
		double num17 = Math.PI * 2.0 * (num11 - 50400.0) / num16;
		double num18 = alpha[0] + alpha[1] * num9 + alpha[2] * num14 + alpha[3] * num15;
		if (num18 < 0.0)
		{
			num18 = 0.0;
		}
		double num20;
		if (Math.Abs(num17) < 1.57)
		{
			double num19 = num17 * num17;
			num20 = num13 * (5E-09 + num18 * (1.0 - num19 * (0.5 + num19 * (1.0 / 24.0))));
		}
		else
		{
			num20 = num13 * 5E-09;
		}
		if (verticalExtent.ContainsExclusive(item3))
		{
			double num21 = (item3 - verticalExtent.Min) / verticalExtent.Width;
			if (num21 < 0.5)
			{
				num20 *= 1.0 - 2.0 * num21 * num21;
			}
			else
			{
				double num22 = 1.0 - num21;
				num20 *= 2.0 * num22 * num22;
			}
			if (num3 < 0.01)
			{
				num20 *= 1.0 + 1.41 * (0.01 - num3) * 11.111111111111111;
			}
		}
		if (frequency != referenceFrequency)
		{
			double num23 = referenceFrequency / frequency;
			num20 *= num23 * num23;
		}
		return num20 * 299792458.0;
	}
}
