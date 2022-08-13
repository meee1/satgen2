using System;
using System.Globalization;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Gga : NmeaSentence
{
	public int FixQuality { get; set; }

	public double GeoidHeight { get; set; }

	public float Hdop { get; set; }

	public double Height { get; set; }

	public Latitude Latitude { get; set; }

	public Longitude Longitude { get; set; }

	public int Satellites { get; set; }

	public TimeSpan Time { get; set; }

	public static Gga FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid GGA sentence cannot be parsed");
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
		Gga gga = new Gga();
		if (num > 0)
		{
			gga.Time = NmeaSentence.ParseTime(array[1]);
		}
		if (num > 3)
		{
			gga.Latitude = Latitude.FromNmea(array[2], array[3]);
		}
		if (num > 5)
		{
			gga.Longitude = Longitude.FromNmea(array[4], array[5]);
		}
		int result;
		if (num > 6)
		{
			int.TryParse(array[6], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			gga.FixQuality = result;
		}
		if (num > 7)
		{
			int.TryParse(array[7], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			gga.Satellites = result;
		}
		if (num > 8)
		{
			float.TryParse(array[8], NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
			gga.Hdop = result2;
		}
		double result3;
		if (num > 9)
		{
			double.TryParse(array[9], NumberStyles.Float, CultureInfo.InvariantCulture, out result3);
			gga.Height = result3;
		}
		if (num > 11)
		{
			double.TryParse(array[11], NumberStyles.Float, CultureInfo.InvariantCulture, out result3);
			gga.GeoidHeight = result3;
		}
		return gga;
	}

	public static bool operator ==(Gga x, Gga y)
	{
		if (x != null && y != null && x.Time == y.Time && x.Latitude == y.Latitude && x.Longitude == y.Longitude && x.FixQuality == y.FixQuality && x.Satellites == y.Satellites && x.Hdop == y.Hdop && x.Height == y.Height)
		{
			return x.GeoidHeight == y.GeoidHeight;
		}
		return false;
	}

	public static bool operator !=(Gga x, Gga y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Gga)
		{
			return this == (Gga)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Time.GetHashCode() ^ Latitude.GetHashCode() ^ Longitude.GetHashCode() ^ FixQuality.GetHashCode() ^ Satellites.GetHashCode() ^ Hdop.GetHashCode() ^ Height.GetHashCode() ^ GeoidHeight.GetHashCode();
	}
}
