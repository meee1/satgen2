using System;
using System.Diagnostics;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen.BeiDou
{
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
			double num11 = Datum.SqrtGM / (SqrtA * num) + MeanMotionCorrection;
			double meanAnomaly = FastMath.NormalizeRadiansPi(MeanAnomaly + num11 * seconds);
			double argumentOfPerigee = ArgumentOfPerigee;
			eccentricAnomaly = OrbitalMechanics.InverseKepler(meanAnomaly, Eccentricity);
			double num20 = 2.0 * Math.Atan(Math.Sqrt((1.0 + Eccentricity) / (1.0 - Eccentricity)) * Math.Tan(0.5 * eccentricAnomaly));
			double num21 = num20 + argumentOfPerigee;
			double num24 = num * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));
			double inclination = Inclination;
			double num22 = num24 * Math.Cos(num21);
			double num25 = num24 * Math.Sin(num21);
			double num26 = LongitudeOfAscendingNode - Datum.AngularVelocity * (seconds + (double)TimeOfApplicability) + RateOfLongitudeOfAscendingNode * seconds;
			double num23 = Math.Cos(num26);
			double num2 = Math.Sin(num26);
			double num3 = Math.Cos(inclination);
			double num4 = Math.Sin(inclination);
			double num5 = num25 * num3;
			double num6 = num22 * num23 - num5 * num2;
			double num7 = num22 * num2 + num5 * num23;
			double positionZ = num25 * num4;
			double num8 = Math.Cos(num20);
			double num9 = num24 * (1.0 + Eccentricity * num8);
			double num27 = Math.Sqrt(Datum.GM / num9);
			double num10 = (0.0 - num27) * Math.Sin(num20);
			double num12 = num27 * (Eccentricity + num8);
			double num13 = Math.Cos(argumentOfPerigee);
			double num14 = Math.Sin(argumentOfPerigee);
			double num15 = num14 * num3;
			double num16 = num13 * num3;
			double num17 = num10 * (num23 * num13 - num2 * num15) + num12 * ((0.0 - num23) * num14 - num2 * num16);
			double num18 = num10 * (num2 * num13 + num23 * num15) + num12 * ((0.0 - num2) * num14 + num23 * num16);
			double velocityZ = (num10 * num14 + num12 * num13) * num4;
			double num19 = Datum.AngularVelocity - RateOfLongitudeOfAscendingNode;
			num17 += num19 * num7;
			num18 -= num19 * num6;
			return new Ecef(num6, num7, positionZ, num17, num18, velocityZ);
		}

		public new Satellite Clone()
		{
			return (Satellite)MemberwiseClone();
		}
	}
}
