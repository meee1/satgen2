using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics;

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
		double azimuth = other.Azimuth;
		if (azimuth.Equals(Azimuth))
		{
			azimuth = other.Elevation;
			return azimuth.Equals(Elevation);
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
		double azimuth = Azimuth;
		int num = (5993773 + azimuth.GetHashCode()) * 9973;
		azimuth = Elevation;
		return num + azimuth.GetHashCode();
	}
}
