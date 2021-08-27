using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen.Glonass
{
	[DebuggerDisplay("ID={Id} {ConstellationType} {OrbitType} Slot = {Slot} {IsEnabled ? string.Empty : \"Disabled\", nq}")]
	public sealed class Satellite : SatelliteBase
	{
		private const double J20 = 0.00108262575;

		private const double StandardDraconicPeriod = 43200.0;

		private const double StandardInclination = Math.PI * 7.0 / 20.0;

		private bool isGlonassMSatellite = true;

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
				return ConstellationType.Glonass;
			}
		}

		public double ArgumentOfPerigee
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double DraconicPeriodCorrection
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double DraconicPeriodCorrectionRate
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double DraconicPeriod => 43200.0 + DraconicPeriodCorrection;

		public double Eccentricity
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double InclinationCorrection
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double Inclination => Math.PI * 7.0 / 20.0 + InclinationCorrection;

		public double LongitudeOfAscendingNode
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public int Slot
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public GnssTime TimeOfAscendingNode
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public GnssTime TimeOfReceiptOfAlmanac
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public Vector3D Position
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public Vector3D Velocity
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public Vector3D Acceleration
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double UtcTimeCorrection
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double GpsTimeCorrection
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double GlonassTimeCorrection
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public double SatelliteClockBiasAtEphemerisTime
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		internal int TimeOfApplicability
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			set;
		}

		public bool IsGlonassMSatellite
		{
			[DebuggerStepThrough]
			get
			{
				return isGlonassMSatellite;
			}
			[DebuggerStepThrough]
			set
			{
				isGlonassMSatellite = value;
			}
		}

		public override Ecef GetEcef(in GnssTime time, out double eccentricAnomaly)
		{
			double deltaT = (time - TimeOfAscendingNode).Seconds;
			double num = (deltaT / DraconicPeriod).SafeFloor();
			double draconicPeriod = DraconicPeriod + (num + 0.5) * DraconicPeriodCorrectionRate;
			double meanMotion = (Datum.PI + Datum.PI) / draconicPeriod;
			double eccentricity = Eccentricity;
			double inclination = Inclination;
			double sinI = Math.Sin(inclination);
			double cosI = Math.Cos(inclination);
			double sinISquared = sinI * sinI;
			double cosISquared = cosI * cosI;
			double a = GetSemiMajorAxis(in draconicPeriod, in sinISquared, out var apSquared);
			double longitudeOfAscendingNode = GetLongitudeOfAscendingNode(in deltaT, in meanMotion, in apSquared, in cosI);
			double argumentOfPerigee = GetArgumentOfPerigee(in deltaT, in meanMotion, in apSquared, in cosISquared);
			double value = GetMeanLongitudeOfAscendingNode(in argumentOfPerigee);
			double value2 = value + meanMotion * (deltaT - num * (DraconicPeriod + DraconicPeriodCorrectionRate * num));
			SinCosStore[] meanLongitudeStore = new SinCosStore[2]
			{
				new SinCosStore(in value),
				new SinCosStore(in value2)
			};
			double h = eccentricity * Math.Sin(argumentOfPerigee);
			double i = eccentricity * Math.Cos(argumentOfPerigee);
			double num11 = Datum.SemiMajorAxis / a;
			double B = 0.0016239386250000002 * num11 * num11;
			double delta_a = GetDelta_a(in B, in a, in sinISquared, in i, in h, meanLongitudeStore);
			double delta_h = GetDelta_h(in B, in sinISquared, in cosISquared, in i, in h, meanLongitudeStore);
			double delta_l = GetDelta_l(in B, in sinISquared, in cosISquared, in i, in h, meanLongitudeStore);
			double delta_lambda = GetDelta_lambda(in B, in cosI, in i, in h, meanLongitudeStore);
			double delta_i = GetDelta_i(in B, in sinI, in cosI, in i, in h, meanLongitudeStore);
			double delta_L = GetDelta_L(in B, in sinISquared, in cosISquared, in i, in h, meanLongitudeStore);
			a += delta_a;
			longitudeOfAscendingNode += delta_lambda;
			value2 += delta_L;
			inclination += delta_i;
			sinI = Math.Sin(inclination);
			cosI = Math.Cos(inclination);
			h += delta_h;
			i += delta_l;
			eccentricity = Math.Sqrt(h * h + i * i);
			argumentOfPerigee = Math.Atan2(h, i);
			double num14 = a * (1.0 - eccentricity * eccentricity);
			eccentricAnomaly = OrbitalMechanics.InverseKepler(value2 - argumentOfPerigee, eccentricity);
			double trueAnomaly = GetTrueAnomaly(in eccentricity, in eccentricAnomaly);
			double num15 = Math.Sin(trueAnomaly);
			double num16 = Math.Cos(trueAnomaly);
			double num20 = trueAnomaly + argumentOfPerigee;
			double num17 = num14 / (1.0 + eccentricity * num16);
			double num18 = Math.Cos(longitudeOfAscendingNode);
			double num19 = Math.Sin(longitudeOfAscendingNode);
			double num2 = Math.Cos(num20);
			double num3 = Math.Sin(num20);
			double num4 = num3 * sinI;
			double num5 = num18 * num2;
			double num6 = num19 * num2;
			double num7 = num18 * num3;
			double num8 = num19 * num3;
			double num9 = num17 * (num5 - num8 * cosI);
			double num10 = num17 * (num6 + num7 * cosI);
			double positionZ = num17 * num4;
			double num21 = Math.Sqrt(Datum.GM / num14);
			double num12 = num21 * eccentricity * num15;
			double num13 = num21 * (1.0 + eccentricity * num16);
			double velocityX = num12 * (num5 - num8 * cosI) - num13 * (num7 + num6 * cosI) + Datum.AngularVelocity * num10;
			double velocityY = num12 * (num6 + num7 * cosI) - num13 * (num8 - num5 * cosI) - Datum.AngularVelocity * num9;
			double velocityZ = num12 * num4 + num13 * num2 * sinI;
			return new Ecef(num9, num10, positionZ, velocityX, velocityY, velocityZ);
		}

		public new Satellite Clone()
		{
			return (Satellite)MemberwiseClone();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetLongitudeOfAscendingNode(in double deltaT, in double meanMotion, in double apSquared, in double cosI)
		{
			return LongitudeOfAscendingNode - (Datum.AngularVelocity + 0.0016239386250000002 * meanMotion * apSquared * cosI) * deltaT;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetArgumentOfPerigee(in double deltaT, in double meanMotion, in double apSquared, in double cosISquared)
		{
			return ArgumentOfPerigee - 0.00081196931250000011 * meanMotion * apSquared * (1.0 - 5.0 * cosISquared) * deltaT;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetMeanLongitudeOfAscendingNode(in double argumentOfPerigee)
		{
			double num = -2.0 * Math.Atan(Math.Sqrt((1.0 - Eccentricity) / (1.0 + Eccentricity)) * Math.Tan(0.5 * argumentOfPerigee));
			return argumentOfPerigee + num - Eccentricity * Math.Sin(num);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private double GetSemiMajorAxis(in double draconicPeriod, in double sinISquared, out double apSquared)
		{
			double num = 1.0 - Eccentricity * Eccentricity;
			double num6 = 1.0 + Eccentricity * Math.Cos(ArgumentOfPerigee);
			double num7 = num6 * num6;
			double num13 = 2.0 - 2.5 * sinISquared;
			double num8 = Math.Sqrt(num * num * num);
			double num9 = num13 * num8 / num7 + num7 * num6 / num;
			double num10 = 0.0016239386250000002;
			double num11 = draconicPeriod;
			double num12 = 1.0;
			double num2 = num12;
			while (true)
			{
				double num14 = num11 * 0.15915494309189535;
				num12 = Math.Cbrt(num14 * num14 * Datum.GM);
				double num3 = num12 * num;
				double num4 = Datum.SemiMajorAxis / num3;
				apSquared = num4 * num4;
				if (Math.Abs(num12 - num2) <= 1E-06)
				{
					break;
				}
				num2 = num12;
				double num5 = num10 * apSquared;
				num11 = draconicPeriod / (1.0 - num5 * num9);
			}
			return num12;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetTrueAnomaly(in double eccentricity, in double eccentricAnomaly)
		{
			double num = Math.Atan(Math.Sqrt((1.0 + eccentricity) / (1.0 - eccentricity)) * Math.Tan(0.5 * eccentricAnomaly));
			return num + num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_a(in double B, in double a, in double sinISquared, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			double num7 = 1.0 - 1.5 * sinISquared;
			double num2 = num7 + num7;
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				double num3 = l * sinCosStore.Cos;
				double num4 = h * sinCosStore.Sin;
				double num5 = num2 * (num3 + num4);
				double num6 = sinISquared * (0.5 * (num4 - num3) + sinCosStore.Cos2 + 3.5 * (l * sinCosStore.Cos3 + h * sinCosStore.Sin3));
				array[i] = num5 + num6;
			}
			return a * B * (array[1] - array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_h(in double B, in double sinISquared, in double cosISquared, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			double num = 1.0 - 1.5 * sinISquared;
			double num2 = -0.25 * sinISquared;
			double num3 = -0.5 * cosISquared;
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				double num4 = l * sinCosStore.Sin2;
				double num5 = h * sinCosStore.Cos2;
				double num6 = num * (sinCosStore.Sin + 1.5 * (num4 - num5));
				double num7 = num2 * (sinCosStore.Sin - 2.3333333333333335 * sinCosStore.Sin3 + 5.0 * num4 - 8.5 * (l * sinCosStore.Sin4 - h * sinCosStore.Cos4) + num5);
				double num8 = num3 * num4;
				array[i] = num6 + num7 + num8;
			}
			return B * (array[1] - array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_l(in double B, in double sinISquared, in double cosISquared, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			double num = 1.0 - 1.5 * sinISquared;
			double num2 = -0.25 * sinISquared;
			double num3 = 0.5 * cosISquared;
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				double num4 = h * sinCosStore.Sin2;
				double num5 = l * sinCosStore.Cos2;
				double num6 = num * (sinCosStore.Cos + 1.5 * (num5 + num4));
				double num7 = num2 * (0.0 - sinCosStore.Cos - 2.3333333333333335 * sinCosStore.Cos3 - 5.0 * num4 - 8.5 * (l * sinCosStore.Cos4 + h * sinCosStore.Sin4) + num5);
				double num8 = num3 * num4;
				array[i] = num6 + num7 + num8;
			}
			return B * (array[1] - array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_lambda(in double B, in double cosi, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				array[i] = 3.5 * l * sinCosStore.Sin - 2.5 * h * sinCosStore.Cos - 0.5 * sinCosStore.Sin2 - 1.1666666666666667 * (l * sinCosStore.Sin3 - h * sinCosStore.Cos3);
			}
			return (0.0 - B) * cosi * (array[1] - array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_i(in double B, in double sinI, in double cosI, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				array[i] = (0.0 - l) * sinCosStore.Cos + h * sinCosStore.Sin + sinCosStore.Cos2 + 2.3333333333333335 * (l * sinCosStore.Cos3 + h * sinCosStore.Sin3);
			}
			return 0.5 * B * sinI * cosI * (array[1] - array[0]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelta_L(in double B, in double sinISquared, in double cosISquared, in double l, in double h, SinCosStore[] meanLongitudeStore)
		{
			double[] array = new double[2];
			double num = 3.5 * (1.0 - 1.5 * sinISquared);
			for (int i = 0; i <= 1; i++)
			{
				SinCosStore sinCosStore = meanLongitudeStore[i];
				double num2 = h * sinCosStore.Cos;
				double num3 = l * sinCosStore.Sin;
				double num4 = h * sinCosStore.Cos3;
				double num5 = l * sinCosStore.Sin3;
				double num6 = num * (num3 - num2);
				double num7 = 3.0 * sinISquared * (-7.0 / 24.0 * (num2 + num3) + 0.25 * sinCosStore.Sin2 - 49.0 / 72.0 * (num4 - num5));
				double num8 = cosISquared * (3.5 * num3 - 2.5 * num2 - 0.5 * sinCosStore.Sin2 + 1.1666666666666667 * (num5 + num4));
				array[i] = num6 + num7 + num8;
			}
			return B * (array[1] - array[0]);
		}
	}
}
