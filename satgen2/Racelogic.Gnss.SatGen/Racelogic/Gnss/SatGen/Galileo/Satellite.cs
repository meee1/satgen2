using System;
using System.Diagnostics;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen.Galileo
{
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
			double num11 = Datum.SqrtGM / (SqrtA * num) + MeanMotionCorrection;
			double meanAnomaly = FastMath.NormalizeRadiansPi(MeanAnomaly + num11 * seconds);
			double argumentOfPerigee = ArgumentOfPerigee;
			eccentricAnomaly = OrbitalMechanics.InverseKepler(meanAnomaly, Eccentricity);
			double num21 = 2.0 * Math.Atan(Math.Sqrt((1.0 + Eccentricity) / (1.0 - Eccentricity)) * Math.Tan(0.5 * eccentricAnomaly));
			double num22 = num21 + argumentOfPerigee;
			double num25 = num * (1.0 - Eccentricity * Math.Cos(eccentricAnomaly));
			double num23 = Inclination;
			double num24 = num25 * Math.Cos(num22);
			double num26 = num25 * Math.Sin(num22);
			double num27 = LongitudeOfAscendingNode - Datum.AngularVelocity * (seconds + (double)TimeOfApplicability) + RateOfLongitudeOfAscendingNode * seconds;
			double num2 = Math.Cos(num27);
			double num3 = Math.Sin(num27);
			double num4 = Math.Cos(num23);
			double num5 = Math.Sin(num23);
			double num6 = num26 * num4;
			double num7 = num24 * num2 - num6 * num3;
			double num8 = num24 * num3 + num6 * num2;
			double positionZ = num26 * num5;
			double num9 = Math.Cos(num21);
			double num10 = num25 * (1.0 + Eccentricity * num9);
			double num28 = Math.Sqrt(Datum.GM / num10);
			double num12 = (0.0 - num28) * Math.Sin(num21);
			double num13 = num28 * (Eccentricity + num9);
			double num14 = Math.Cos(argumentOfPerigee);
			double num15 = Math.Sin(argumentOfPerigee);
			double num16 = num15 * num4;
			double num17 = num14 * num4;
			double num18 = num12 * (num2 * num14 - num3 * num16) + num13 * ((0.0 - num2) * num15 - num3 * num17);
			double num19 = num12 * (num3 * num14 + num2 * num16) + num13 * ((0.0 - num3) * num15 + num2 * num17);
			double velocityZ = (num12 * num15 + num13 * num14) * num5;
			double num20 = Datum.AngularVelocity - RateOfLongitudeOfAscendingNode;
			num18 += num20 * num8;
			num19 -= num20 * num7;
			return new Ecef(num7, num8, positionZ, num18, num19, velocityZ);
		}

		public new Satellite Clone()
		{
			return (Satellite)MemberwiseClone();
		}
	}
}
