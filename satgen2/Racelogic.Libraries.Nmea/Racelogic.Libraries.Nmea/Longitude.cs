using System;
using System.Globalization;

namespace Racelogic.Libraries.Nmea;

public struct Longitude
{
	private const double Epsilon = 1E-15;

	private readonly double absoluteLongitude;

	private readonly double longitude;

	public double DecimalDegrees => longitude;

	public double GPSCoordinate
	{
		get
		{
			double num = absoluteLongitude;
			double num2 = Math.Floor(num);
			double num3 = (num - num2 * 1.0) * 60.0;
			return (num2 + num3 / 100.0) * 100.0;
		}
	}

	public int Degrees => (int)absoluteLongitude;

	public LongitudeHemisphere Hemisphere
	{
		get
		{
			if (!(longitude >= 0.0))
			{
				return LongitudeHemisphere.West;
			}
			return LongitudeHemisphere.East;
		}
	}

	public int Minutes => (int)((absoluteLongitude - (double)Degrees) * 60.0);

	public double Radians => longitude * Math.PI / 180.0;

	public double Seconds => (absoluteLongitude - (double)Degrees - (double)Minutes / 60.0) * 3600.0;

	public Longitude(double degrees)
	{
		longitude = degrees;
		absoluteLongitude = Math.Abs(longitude);
	}

	public Longitude(double degrees, double minutes, double seconds, LongitudeHemisphere hemisphere = LongitudeHemisphere.East)
	{
		longitude = (degrees + minutes / 60.0 + seconds / 3600.0) * (double)((hemisphere != 0) ? 1 : (-1));
		absoluteLongitude = Math.Abs(longitude);
	}

	public static Longitude FromNmea(string value, string hemisphere)
	{
		int result;
		if (value.Length >= 3)
		{
			int.TryParse(value.Substring(0, 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}
		else
		{
			result = 0;
		}
		int result2;
		if (value.Length >= 5)
		{
			int.TryParse(value.Substring(3, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out result2);
		}
		else
		{
			result2 = 0;
		}
		double result3;
		if (value.Length > 5)
		{
			double.TryParse(value.Substring(5), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result3);
		}
		else
		{
			result3 = 0.0;
		}
		LongitudeHemisphere hemisphere2 = ("E".Equals(hemisphere) ? LongitudeHemisphere.East : LongitudeHemisphere.West);
		return new Longitude(result, result2, result3 * 60.0, hemisphere2);
	}

	public static bool operator ==(Longitude x, Longitude y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(Longitude x, Longitude y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Longitude longitude))
		{
			return false;
		}
		return Math.Abs(DecimalDegrees - longitude.DecimalDegrees) < 1E-15;
	}

	public override int GetHashCode()
	{
		return DecimalDegrees.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{0}°{1}'{2}\"{3}", Degrees, Minutes, Seconds, (longitude >= 0.0) ? "E" : "W");
	}
}
