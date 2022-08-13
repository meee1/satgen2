using System;

namespace Racelogic.DataSource;

[Obsolete("This struct is about to be removed.  Please use Topocentric defined in Racelogic.Geodetic.")]
public struct Topocentric : IEquatable<Topocentric>
{
	private readonly double azimuth;

	private readonly double elevation;

	public double Azimuth => azimuth;

	public double Elevation => elevation;

	public Topocentric(double azimuth, double elevation)
	{
		this.azimuth = azimuth;
		this.elevation = elevation;
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
		double num = other.azimuth;
		if (num.Equals(azimuth))
		{
			num = other.elevation;
			return num.Equals(elevation);
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
		double num = azimuth;
		int num2 = num.GetHashCode() * 397;
		num = elevation;
		return num2 ^ num.GetHashCode();
	}
}
