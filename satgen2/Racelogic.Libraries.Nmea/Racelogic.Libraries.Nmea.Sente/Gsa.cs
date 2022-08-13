using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Gsa : NmeaSentence
{
	public int Fix { get; set; }

	public char FixSelection { get; set; }

	public float Hdop { get; set; }

	public float Pdop { get; set; }

	public int[] SVs { get; set; }

	public float Vdop { get; set; }

	public static Gsa FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid GSA sentence cannot be parsed");
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
		Gsa gsa = new Gsa();
		if (num > 1)
		{
			gsa.FixSelection = array[1].FirstOrDefault();
		}
		if (num > 2)
		{
			int.TryParse(array[2], NumberStyles.None, CultureInfo.InvariantCulture, out var result);
			gsa.Fix = result;
		}
		float result2;
		if (num > 15)
		{
			float.TryParse(array[15], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result2);
			gsa.Pdop = result2;
		}
		if (num > 16)
		{
			float.TryParse(array[16], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result2);
			gsa.Hdop = result2;
		}
		if (num > 17)
		{
			float.TryParse(array[17], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result2);
			gsa.Vdop = result2;
		}
		IList<int> list = new List<int>();
		for (int i = 3; i < 15 && num > i; i++)
		{
			int.TryParse(array[i], NumberStyles.None, CultureInfo.InvariantCulture, out var result3);
			if (result3 > 0)
			{
				list.Add(result3);
			}
		}
		gsa.SVs = list.ToArray();
		return gsa;
	}

	public static bool operator ==(Gsa x, Gsa y)
	{
		if (x != null && y != null && x.FixSelection == y.FixSelection && x.Fix == y.Fix && x.SVs.SequenceEqual(y.SVs) && x.Pdop == y.Pdop && x.Hdop == y.Hdop)
		{
			return x.Vdop == y.Vdop;
		}
		return false;
	}

	public static bool operator !=(Gsa x, Gsa y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Gsa)
		{
			return this == (Gsa)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return FixSelection.GetHashCode() ^ Fix.GetHashCode() ^ SVs.GetHashCode() ^ Pdop.GetHashCode() ^ Hdop.GetHashCode() ^ Vdop.GetHashCode();
	}
}
