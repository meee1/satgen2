using System;
using System.Diagnostics;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen.Galileo;

[DebuggerDisplay("ID={Id} {ConstellationType} {OrbitType} {IsEnabled ? string.Empty : \"Disabled\", nq}")]
public sealed class Satellite : SatelliteBase
{
	private static readonly double NominalSqrtA = Math.Sqrt(29600000.0);

	private const double NominalInclination = Math.PI * 14.0 / 45.0;

	private double deltaSqrtA;

	private double sqrtA;

	private double deltaInclination;

	private double inclination;

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
			return ConstellationType.Galileo;
		}
	}

	public double DeltaSqrtA
	{
		[DebuggerStepThrough]
		get
		{
			return deltaSqrtA;
		}
		[DebuggerStepThrough]
		set
		{
			deltaSqrtA = value;
			sqrtA = NominalSqrtA + deltaSqrtA;
		}
	}

	public double SqrtA
	{
		[DebuggerStepThrough]
		get
		{
			return sqrtA;
		}
		[DebuggerStepThrough]
		set
		{
			sqrtA = value;
			deltaSqrtA = sqrtA - NominalSqrtA;
		}
	}

	public double Eccentricity
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double DeltaInclination
	{
		[DebuggerStepThrough]
		get
		{
			return deltaInclination;
		}
		[DebuggerStepThrough]
		set
		{
			deltaInclination = value;
			inclination = Math.PI * 14.0 / 45.0 + deltaInclination;
		}
	}

	public double Inclination
	{
		[DebuggerStepThrough]
		get
		{
			return inclination;
		}
		[DebuggerStepThrough]
		set
		{
			inclination = value;
			deltaInclination = inclination - Math.PI * 14.0 / 45.0;
		}
	}

	public double RateOfInclination
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

	public double RateOfLongitudeOfAscendingNode
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

	public double Af0
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Af1
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public double Af2
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public int Iod
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

	internal SatelliteHealth StatusE1B
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	internal SatelliteHealth StatusE5a
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	internal SatelliteHealth StatusE5b
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public override Ecef GetEcef(in GnssTime time, out double eccentricAnomaly)
	{
		GnssTime gnssTime = GnssTime.FromGalileoNavic(Week, TimeOfApplicability);
		double seconds = (time - gnssTime).Seconds;
		double num = SqrtA * SqrtA;
		double num2 = Datum.SqrtGM / (SqrtA * num) + MeanMotionCorrection;
		double meanAnomaly = FastMath.NormalizeRadiansPi(MeanAnomaly + num2 * seconds);
		double argumentOfPerigee = ArgumentOfPerigee;
		eccentricAnomaly = OrbitalMechanics.InverseKepler(meanAnomaly, Eccentricity);
		double num3 = 2.0 * Math.Atan(Math.Sqrt((1.0 + Eccentricity) / (1.0 - Eccentricity)) * Math.Tan(0.5 * eccentricAnomaly));
		double num4 = num3 + argumentOfPerigee;
		double num5 = num * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));
		double num6 = Inclination;
		double num7 = num5 * Math.Cos(num4);
		double num8 = num5 * Math.Sin(num4);
		double num9 = LongitudeOfAscendingNode - Datum.AngularVelocity * (seconds + (double)TimeOfApplicability) + RateOfLongitudeOfAscendingNode * seconds;
		double num10 = Math.Cos(num9);
		double num11 = Math.Sin(num9);
		double num12 = Math.Cos(num6);
		double num13 = Math.Sin(num6);
		double num14 = num8 * num12;
		double num15 = num7 * num10 - num14 * num11;
		double num16 = num7 * num11 + num14 * num10;
		double positionZ = num8 * num13;
		double num17 = Math.Cos(num3);
		double num18 = num5 * (1.0 + Eccentricity * num17);
		double num19 = Math.Sqrt(Datum.GM / num18);
		double num20 = (0.0 - num19) * Math.Sin(num3);
		double num21 = num19 * (Eccentricity + num17);
		double num22 = Math.Cos(argumentOfPerigee);
		double num23 = Math.Sin(argumentOfPerigee);
		double num24 = num23 * num12;
		double num25 = num22 * num12;
		double num26 = num20 * (num10 * num22 - num11 * num24) + num21 * ((0.0 - num10) * num23 - num11 * num25);
		double num27 = num20 * (num11 * num22 + num10 * num24) + num21 * ((0.0 - num11) * num23 + num10 * num25);
		double velocityZ = (num20 * num23 + num21 * num22) * num13;
		double num28 = Datum.AngularVelocity - RateOfLongitudeOfAscendingNode;
		num26 += num28 * num16;
		num27 -= num28 * num15;
		return new Ecef(num15, num16, positionZ, num26, num27, velocityZ);
	}

	public new Satellite Clone()
	{
		return (Satellite)MemberwiseClone();
	}
}
