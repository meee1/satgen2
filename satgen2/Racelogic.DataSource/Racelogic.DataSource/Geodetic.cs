using System;
using System.Runtime.Serialization;
using Racelogic.Geodetics;

namespace Racelogic.DataSource;

[Obsolete("This class is about to be removed.  Please use Geodetic defined in Racelogic.Geodetic.")]
[DataContract]
public class Geodetic : IEquatable<Geodetic>
{
	private const double D2R = Math.PI / 180.0;

	private const double R2D = 180.0 / Math.PI;

	[DataMember]
	public double Altitude { get; private set; }

	[DataMember]
	public double Latitude { get; private set; }

	[DataMember]
	public double Longitude { get; private set; }

	public Geodetic(double latitude, double longitude, double altitude)
	{
		Latitude = latitude;
		Longitude = longitude;
		Altitude = altitude;
	}

	public static implicit operator Racelogic.Geodetics.Geodetic(Geodetic g)
	{
		return new Racelogic.Geodetics.Geodetic(g.Latitude, g.Longitude, g.Altitude);
	}

	public static implicit operator Geodetic(Racelogic.Geodetics.Geodetic g)
	{
		return new Geodetic(g.Latitude, g.Longitude, g.Altitude);
	}

	public static Geodetic FromDegrees(double latitude, double longitude, double altitude)
	{
		return new Geodetic(latitude * (Math.PI / 180.0), longitude * (Math.PI / 180.0), altitude);
	}

	[Obsolete("ToEcef is deprecated, please use ToEcef2 instead.")]
	public Ecef ToEcef()
	{
		double d = 1.0 - 0.0066944781979932861 * (Math.Sin(Latitude) * Math.Sin(Latitude));
		d = Math.Sqrt(d);
		double num = Math.Cos(Latitude);
		double num2 = Math.Sin(Latitude);
		double num3 = Math.Cos(Longitude);
		double num4 = Math.Sin(Longitude);
		return new Ecef((6378137.0 / d + Altitude) * num * num3, (6378137.0 / d + Altitude) * num * num4, (6335438.7009096853 / d + (Altitude - 47.3248)) * num2);
	}

	public Ecef ToEcef2()
	{
		return ToEcef2(Datum.Wgs84, geoidHeight: true);
	}

	public Ecef ToEcef2(Datum datum, bool geoidHeight)
	{
		double num = Math.Sin(Latitude);
		double num2 = Math.Sqrt(1.0 - num * num);
		double num3 = Latitude % Ecef.TwoPI;
		if (num3 > 0.0)
		{
			if (num3 > Ecef.PIByTwo && num3 < Ecef.ThreePIByTwo)
			{
				num2 = 0.0 - num2;
			}
		}
		else if (num3 < 0.0 - Ecef.PIByTwo && num3 > 0.0 - Ecef.ThreePIByTwo)
		{
			num2 = 0.0 - num2;
		}
		double num4 = Math.Sin(Longitude);
		double num5 = Math.Sqrt(1.0 - num4 * num4);
		num3 = Longitude % Ecef.TwoPI;
		if (num3 > 0.0)
		{
			if (num3 > Ecef.PIByTwo && num3 < Ecef.ThreePIByTwo)
			{
				num5 = 0.0 - num5;
			}
		}
		else if (num3 < 0.0 - Ecef.PIByTwo && num3 > 0.0 - Ecef.ThreePIByTwo)
		{
			num5 = 0.0 - num5;
		}
		if (geoidHeight)
		{
			return new Geodetic(Latitude, Longitude, Altitude + 47.3248).ToEcef2(datum, geoidHeight: false);
		}
		double num6 = datum.SemiMajorAxis / Math.Sqrt(datum.SemiMajorAxis * Math.Pow(num2, 2.0) + datum.SemiMinorAxis * Math.Pow(num, 2.0));
		return new Ecef((num6 + Altitude) * num2 * num5, (num6 + Altitude) * num2 * num4, (datum.SemiMinorAxis / datum.SemiMajorAxis * num6 + Altitude) * num);
	}

	public bool Equals(Geodetic other)
	{
		if (other.Latitude.Equals(Latitude) && other.Longitude.Equals(Longitude))
		{
			return other.Altitude.Equals(Altitude);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(Geodetic))
		{
			return false;
		}
		return Equals((Geodetic)obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
