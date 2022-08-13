using System;
using System.Globalization;
using System.Linq;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Rmc : NmeaSentence
{
	private const double KnotsToKph = 1.852;

	public DateTime Date { get; set; }

	public char FixMode { get; set; }

	public double Heading { get; set; }

	public Latitude Latitude { get; set; }

	public Longitude Longitude { get; set; }

	public float MagneticVariation { get; set; }

	public double Speed { get; set; }

	public char Status { get; set; }

	public TimeSpan Time { get; set; }

	public double Knots { get; set; }

	public static Rmc FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid RMC sentence cannot be parsed");
		}
		if (sentence.IndexOf("$", StringComparison.Ordinal) >= 0)
		{
			sentence = sentence.Substring(sentence.IndexOf("$", StringComparison.Ordinal) + 1);
		}
		if (sentence.IndexOf("*", StringComparison.Ordinal) >= 0)
		{
			sentence = sentence.Substring(0, sentence.IndexOf("*", StringComparison.Ordinal));
		}
		string[] array = sentence.Split(',');
		int num = array.Length;
		Rmc rmc = new Rmc();
		if (num > 1)
		{
			rmc.Time = NmeaSentence.ParseTime(array[1]);
		}
		if (num > 2)
		{
			rmc.Status = array[2].FirstOrDefault();
		}
		if (num > 4)
		{
			rmc.Latitude = Latitude.FromNmea(array[3], array[4]);
		}
		if (num > 6)
		{
			rmc.Longitude = Longitude.FromNmea(array[5], array[6]);
		}
		double result;
		if (num > 7)
		{
			double.TryParse(array[7], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			rmc.Speed = result * 1.852;
			rmc.Knots = result;
		}
		if (num > 8)
		{
			double.TryParse(array[8], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			rmc.Heading = result;
		}
		if (num > 9)
		{
			rmc.Date = NmeaSentence.ParseDate(array[9]);
		}
		if (num > 10)
		{
			float.TryParse(array[10], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result2);
			rmc.MagneticVariation = result2;
		}
		if (num > 11 && array[11] == "W")
		{
			rmc.MagneticVariation *= -1f;
		}
		if (num > 12)
		{
			rmc.FixMode = array[12].FirstOrDefault();
		}
		return rmc;
	}

	public static bool operator ==(Rmc x, Rmc y)
	{
		if (x != null && y != null && x.Time == y.Time && x.Status == y.Status && x.Latitude == y.Latitude && x.Longitude == y.Longitude && x.Speed == y.Speed && x.Heading == y.Heading && x.Date == y.Date && x.MagneticVariation == y.MagneticVariation)
		{
			return x.FixMode == y.FixMode;
		}
		return false;
	}

	public static bool operator !=(Rmc x, Rmc y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Rmc)
		{
			return this == (Rmc)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Time.GetHashCode() ^ Status.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode() ^ Speed.GetHashCode() ^ Heading.GetHashCode() ^ Date.GetHashCode() ^ MagneticVariation.GetHashCode() ^ FixMode.GetHashCode();
	}
}
