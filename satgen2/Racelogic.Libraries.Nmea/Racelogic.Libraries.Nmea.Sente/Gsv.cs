using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Racelogic.Libraries.Nmea.Sentences;

public class Gsv : NmeaSentence
{
	public int Sentence { get; set; }

	public Sv[] Svs { get; set; }

	public int SvsInView { get; set; }

	public int TotalSentences { get; set; }

	public Constellation Constellation { get; set; }

	public static Gsv FromNmea(string sentence)
	{
		if (string.IsNullOrEmpty(sentence))
		{
			throw new ArgumentException("Invalid GSV sentence cannot be parsed");
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
		Gsv gsv = new Gsv();
		Constellation constellation = Constellation.GPS;
		if (num > 0 && !string.IsNullOrEmpty(array[0]) && array[0].Length >= 2)
		{
			constellation = (gsv.Constellation = array[0][1] switch
			{
				'P' => Constellation.GPS, 
				'L' => Constellation.GLONASS, 
				'A' => Constellation.GALILEO, 
				'N' => Constellation.Combined, 
				'B' => Constellation.BEIDOU, 
				_ => Constellation.GPS, 
			});
		}
		int result;
		if (num > 1)
		{
			int.TryParse(array[1], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			gsv.TotalSentences = result;
		}
		if (num > 2)
		{
			int.TryParse(array[2], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			gsv.Sentence = result;
		}
		if (num > 3)
		{
			int.TryParse(array[3], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			gsv.SvsInView = result;
		}
		IList<Sv> list = new List<Sv>();
		for (int i = 4; i < array.Length && num > i + 3; i += 4)
		{
			Sv sv = new Sv
			{
				Constellation = constellation
			};
			int.TryParse(array[i], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			sv.Prn = result;
			int.TryParse(array[i + 1], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			sv.Elevation = result;
			int.TryParse(array[i + 2], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			sv.Azimuth = result;
			int.TryParse(array[i + 3], NumberStyles.None, CultureInfo.InvariantCulture, out result);
			sv.Snr = result;
			list.Add(sv);
		}
		gsv.Svs = list.ToArray();
		return gsv;
	}

	public static bool operator ==(Gsv x, Gsv y)
	{
		if (x != null && y != null && x.TotalSentences == y.TotalSentences && x.Sentence == y.Sentence && x.SvsInView == y.SvsInView)
		{
			return x.Svs.SequenceEqual(y.Svs);
		}
		return false;
	}

	public static bool operator !=(Gsv x, Gsv y)
	{
		return !(x == y);
	}

	public override bool Equals(object obj)
	{
		if (obj is Gsv)
		{
			return this == (Gsv)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return TotalSentences.GetHashCode() ^ Sentence.GetHashCode() ^ SvsInView.GetHashCode() ^ Svs.GetHashCode();
	}
}
