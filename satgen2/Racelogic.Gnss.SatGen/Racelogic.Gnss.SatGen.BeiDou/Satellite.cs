using System;
using System.Diagnostics;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen.BeiDou;

[DebuggerDisplay("ID={Id} {ConstellationType} {OrbitType} {IsEnabled ? string.Empty : \"Disabled\", nq}")]
public sealed class Satellite : SatelliteBase
{
	public sealed override Datum Datum
	{
		[DebuggerStepThrough]
		get
		{
			return Constellation.Datum;
		}
	}

	public sealed override ConstellationType ConstellationType
	{
		[DebuggerStepThrough]
		get
		{
			return ConstellationType.BeiDou;
		}
	}

	public double A0
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A1
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A2
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A0GPS
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A1GPS
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A0Gal
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A1Gal
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A0GLO
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A1GLO
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A0UTC
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double A1UTC
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double ArgumentOfPerigee
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Cic
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Cis
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Crc
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Crs
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Cuc
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Cus
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Eccentricity
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double LongitudeOfAscendingNode
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double MeanAnomaly
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double MeanMotionCorrection
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Inclination
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double RateOfInclination
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double RateOfLongitudeOfAscendingNode
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double SqrtA
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public int TimeOfApplicability
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public int Week
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public override Ecef GetEcef(in GnssTime time, out double eccentricAnomaly)
	{
		GnssTime gnssTime = GnssTime.FromBeiDou(Week, TimeOfApplicability);
		double seconds = (time - gnssTime).Seconds;
		double num = SqrtA * SqrtA;
		double num2 = Datum.SqrtGM / (SqrtA * num) + MeanMotionCorrection;
		double meanAnomaly = FastMath.NormalizeRadiansPi(MeanAnomaly + num2 * seconds);
		double argumentOfPerigee = ArgumentOfPerigee;
		eccentricAnomaly = OrbitalMechanics.InverseKepler(meanAnomaly, Eccentricity);
		double num3 = 2.0 * Math.Atan(Math.Sqrt((1.0 + Eccentricity) / (1.0 - Eccentricity)) * Math.Tan(0.5 * eccentricAnomaly));
		double num4 = num3 + argumentOfPerigee;
		double num5 = num * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));
		double inclination = Inclination;
		double num6 = num5 * Math.Cos(num4);
		double num7 = num5 * Math.Sin(num4);
		double num8 = LongitudeOfAscendingNode - Datum.AngularVelocity * (seconds + (double)TimeOfApplicability) + RateOfLongitudeOfAscendingNode * seconds;
		double num9 = Math.Cos(num8);
		double num10 = Math.Sin(num8);
		double num11 = Math.Cos(inclination);
		double num12 = Math.Sin(inclination);
		double num13 = num7 * num11;
		double num14 = num6 * num9 - num13 * num10;
		double num15 = num6 * num10 + num13 * num9;
		double positionZ = num7 * num12;
		double num16 = Math.Cos(num3);
		double num17 = num5 * (1.0 + Eccentricity * num16);
		double num18 = Math.Sqrt(Datum.GM / num17);
		double num19 = (0.0 - num18) * Math.Sin(num3);
		double num20 = num18 * (Eccentricity + num16);
		double num21 = Math.Cos(argumentOfPerigee);
		double num22 = Math.Sin(argumentOfPerigee);
		double num23 = num22 * num11;
		double num24 = num21 * num11;
		double num25 = num19 * (num9 * num21 - num10 * num23) + num20 * ((0.0 - num9) * num22 - num10 * num24);
		double num26 = num19 * (num10 * num21 + num9 * num23) + num20 * ((0.0 - num10) * num22 + num9 * num24);
		double velocityZ = (num19 * num22 + num20 * num21) * num12;
		double num27 = Datum.AngularVelocity - RateOfLongitudeOfAscendingNode;
		num25 += num27 * num15;
		num26 -= num27 * num14;
		return new Ecef(num14, num15, positionZ, num25, num26, velocityZ);
	}

	public new Satellite Clone()
	{
		return (Satellite)MemberwiseClone();
	}
}
