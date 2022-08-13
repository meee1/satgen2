using System;
using System.Globalization;
using System.Linq;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Vtg : NmeaSentence
{
	public char FixMode { get; set; }

	public double Heading { get; set; }

	public double Knots { get; set; }

	public double MagneticHeading { get; set; }

	public double Speed { get; set; }

	public static Vtg FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid VTG sentence cannot be parsed");
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
		Vtg vtg = new Vtg();
		double result;
		if (num > 1)
		{
			double.TryParse(array[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			vtg.Heading = result;
		}
		if (num > 3)
		{
			double.TryParse(array[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			vtg.MagneticHeading = result;
		}
		if (num > 5)
		{
			double.TryParse(array[5], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			vtg.Knots = result;
		}
		if (num > 7)
		{
			double.TryParse(array[7], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result);
			vtg.Speed = result;
		}
		if (num > 9)
		{
			vtg.FixMode = array[9].FirstOrDefault();
		}
		return vtg;
	}

	public static bool operator ==(Vtg x, Vtg y)
	{
		if (x != null && y != null && x.Heading == y.Heading && x.MagneticHeading == y.MagneticHeading && x.Knots == y.Knots && x.Speed == y.Speed)
		{
			return x.FixMode == y.FixMode;
		}
		return false;
	}

	public static bool operator !=(Vtg x, Vtg y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Vtg)
		{
			return this == (Vtg)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Heading.GetHashCode() ^ MagneticHeading.GetHashCode() ^ Knots.GetHashCode() ^ Speed.GetHashCode() ^ FixMode.GetHashCode();
	}
}
