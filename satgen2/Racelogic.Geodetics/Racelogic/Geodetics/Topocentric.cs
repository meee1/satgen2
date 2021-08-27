using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics
{
	[JsonObject(MemberSerialization.OptIn)]
	[DebuggerDisplay("Azimuth: {Azimuth / System.Math.PI * 180.0}°   Elevation: {Elevation / System.Math.PI * 180.0}°")]
	public readonly struct Topocentric : IEquatable<Topocentric>
	{
		[JsonProperty(PropertyName = "Azimuth")]
		public readonly double Azimuth;

		[JsonProperty(PropertyName = "Elevation")]
		public readonly double Elevation;

		[JsonConstructor]
		public Topocentric(double azimuth, double elevation)
		{
			Azimuth = azimuth;
			Elevation = elevation;
		}

		public (double Azimuth, double Elevation) Deconstruct()
		{
			return (Azimuth, Elevation);
		}

		public static bool operator ==(Topocentric left, Topocentric right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Topocentric left, Topocentric right)
		{
			return !left.Equals(right);
		}

		public bool Equals(Topocentric other)
		{
			if (other.Azimuth.Equals(Azimuth))
			{
				return other.Elevation.Equals(Elevation);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Topocentric))
			{
				return false;
			}
			return Equals((Topocentric)obj);
		}

		public override int GetHashCode()
		{
			return (5993773 + Azimuth.GetHashCode()) * 9973 + Elevation.GetHashCode();
		}
	}
}
