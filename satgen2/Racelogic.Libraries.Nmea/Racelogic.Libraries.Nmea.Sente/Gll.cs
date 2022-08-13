using System;
using System.Linq;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Gll : NmeaSentence
{
	public bool Active { get; set; }

	public char FixMode { get; set; }

	public Latitude Latitude { get; set; }

	public Longitude Longitude { get; set; }

	public TimeSpan Time { get; set; }

	public static Gll FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid GLL sentence cannot be parsed");
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
		Gll gll = new Gll();
		if (num > 2)
		{
			gll.Latitude = Latitude.FromNmea(array[1], array[2]);
		}
		if (num > 4)
		{
			gll.Longitude = Longitude.FromNmea(array[3], array[4]);
		}
		if (num > 5)
		{
			gll.Time = NmeaSentence.ParseTime(array[5]);
		}
		if (num > 6)
		{
			gll.Active = "A".Equals(array[6]);
		}
		if (num > 7)
		{
			gll.FixMode = array[7].FirstOrDefault();
		}
		return gll;
	}

	public static bool operator ==(Gll x, Gll y)
	{
		if (x != null && y != null && x.Latitude == y.Latitude && x.Longitude == y.Longitude && x.Time == y.Time && x.Active == y.Active)
		{
			return x.FixMode == y.FixMode;
		}
		return false;
	}

	public static bool operator !=(Gll x, Gll y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Gll)
		{
			return this == (Gll)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Latitude.GetHashCode() ^ Longitude.GetHashCode() ^ Time.GetHashCode() ^ Active.GetHashCode() ^ FixMode.GetHashCode();
	}
}
