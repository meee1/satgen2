using System;
using System.Globalization;

namespace Racelogic.Libraries.Nmea;

public struct Latitude
{
	private const double Epsilon = 1E-15;

	private readonly double absoluteLatitude;

	private readonly double latitude;

	public double DecimalDegrees => latitude;

	public int Degrees => (int)absoluteLatitude;

	public LatitudeHemisphere Hemisphere
	{
		get
		{
			if (!(latitude >= 0.0))
			{
				return LatitudeHemisphere.South;
			}
			return LatitudeHemisphere.North;
		}
	}

	public int Minutes => (int)((absoluteLatitude - (double)Degrees) * 60.0);

	public double Radians => latitude * Math.PI / 180.0;

	public double Seconds => (absoluteLatitude - (double)Degrees - (double)Minutes / 60.0) * 3600.0;

	public double GPSCoordinate
	{
		get
		{
			double num = absoluteLatitude;
			double num2 = Math.Floor(num);
			double num3 = (num - num2 * 1.0) * 60.0;
			return (num2 + num3 / 100.0) * 100.0;
		}
	}

	public Latitude(double degrees)
	{
		latitude = degrees;
		absoluteLatitude = Math.Abs(latitude);
	}

	public Latitude(double degrees, double minutes, double seconds, LatitudeHemisphere hemisphere = LatitudeHemisphere.North)
	{
		latitude = (degrees + minutes / 60.0 + seconds / 3600.0) * (double)((hemisphere != LatitudeHemisphere.South) ? 1 : (-1));
		absoluteLatitude = Math.Abs(latitude);
	}

	public static Latitude FromNmea(string value, string hemisphere)
	{
		int result;
		if (value.Length >= 2)
		{
			int.TryParse(value.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}
		else
		{
			result = 0;
		}
		int result2;
		if (value.Length >= 4)
		{
			int.TryParse(value.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out result2);
		}
		else
		{
			result2 = 0;
		}
		double result3;
		if (value.Length > 4)
		{
			double.TryParse(value.Substring(4), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result3);
		}
		else
		{
			result3 = 0.0;
		}
		LatitudeHemisphere hemisphere2 = ((!"N".Equals(hemisphere)) ? LatitudeHemisphere.South : LatitudeHemisphere.North);
		return new Latitude(result, result2, result3 * 60.0, hemisphere2);
	}

	public static bool operator ==(Latitude x, Latitude y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(Latitude x, Latitude y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Latitude latitude))
		{
			return false;
		}
		return Math.Abs(DecimalDegrees - latitude.DecimalDegrees) < 1E-15;
	}

	public override int GetHashCode()
	{
		return DecimalDegrees.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{0}°{1}'{2}\"{3}", Degrees, Minutes, Seconds, (latitude >= 0.0) ? "N" : "S");
	}
}
