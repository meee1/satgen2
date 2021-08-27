using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Racelogic.Geodetics
{
	[JsonObject(MemberSerialization.OptIn)]
	[DebuggerDisplay("North:{North}  East:{East}  Down:{Down}  Mag:{Magnitude()}")]
	public readonly struct LocalTangentPlane : IEquatable<LocalTangentPlane>
	{
		[JsonProperty(PropertyName = "North")]
		public readonly double North;

		[JsonProperty(PropertyName = "East")]
		public readonly double East;

		[JsonProperty(PropertyName = "Down")]
		public readonly double Down;

		[JsonConstructor]
		public LocalTangentPlane(double north, double east, double down)
		{
			North = north;
			East = east;
			Down = down;
		}

		public (double North, double East, double Down) Deconstruct()
		{
			return (North, East, Down);
		}

		public Topocentric ToAzimuthElevation()
		{
			double num = Math.Atan2(East, North);
			double x = FlatMagnitude();
			double elevation = Math.Atan2(0.0 - Down, x);
			if (num < 0.0)
			{
				num += Math.PI * 2.0;
			}
			return new Topocentric(num, elevation);
		}

		public Ecef ToEcef(in Geodetic referenceLocation, in Ecef referenceEcef)
		{
			if (!referenceEcef.IsAbsolute)
			{
				throw new ArgumentException("Relative Ecefs cannot be used as reference origin", "referenceEcef");
			}
			double num = Math.Cos(referenceLocation.Longitude);
			double num2 = Math.Sin(referenceLocation.Longitude);
			double num3 = num2 * East + num * Down;
			double num4 = Math.Cos(referenceLocation.Latitude);
			double num5 = Math.Sin(referenceLocation.Latitude);
			Vector3D position = referenceEcef.Position + new Vector3D(0.0 - North * num5 - num4 * num3, num * East - num2 * Down, num4 * North - num5 * num3);
			return new Ecef(in position);
		}

		public Ecef ToEcef(in Geodetic referenceLocation, Datum datum = null, Geoid geoid = null)
		{
			Ecef referenceEcef = referenceLocation.ToEcef(datum, geoid);
			return ToEcef(in referenceLocation, in referenceEcef);
		}

		public Ecef ToRelativeEcef(in Ecef position)
		{
			Geodetic geodetic = position.ToGeodetic();
			double num = Math.Cos(geodetic.Longitude);
			double num2 = Math.Sin(geodetic.Longitude);
			double num3 = num2 * East + num * Down;
			double num4 = Math.Cos(geodetic.Latitude);
			double num5 = Math.Sin(geodetic.Latitude);
			ref readonly Vector3D position2 = ref position.Position;
			Vector3D velocity = new Vector3D(0.0 - North * num5 - num4 * num3, num * East - num2 * Down, num4 * North - num5 * num3);
			return new Ecef(in position2, in velocity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double FlatMagnitude()
		{
			return Math.Sqrt(North * North + East * East);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Magnitude()
		{
			return Math.Sqrt(North * North + East * East + Down * Down);
		}

		public static bool operator ==(LocalTangentPlane left, LocalTangentPlane right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(LocalTangentPlane left, LocalTangentPlane right)
		{
			return !left.Equals(right);
		}

		public bool Equals(LocalTangentPlane other)
		{
			if (other.North.Equals(North) && other.East.Equals(East))
			{
				return other.Down.Equals(Down);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(LocalTangentPlane))
			{
				return false;
			}
			return Equals((LocalTangentPlane)obj);
		}

		public override int GetHashCode()
		{
			return ((5993773 + North.GetHashCode()) * 9973 + East.GetHashCode()) * 9973 + Down.GetHashCode();
		}
	}
}
