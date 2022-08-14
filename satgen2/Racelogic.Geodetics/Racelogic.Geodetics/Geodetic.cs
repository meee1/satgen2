using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Flitesys.GeographicLib;
using Newtonsoft.Json;
using Racelogic.Maths;

namespace Racelogic.Geodetics;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("Lat: {Latitude / System.Math.PI * 180.0}°   Long: {Longitude / System.Math.PI * 180.0}°   Alt: {Altitude}m")]
public readonly struct Geodetic
{
	private sealed class CoordinateNumber
	{
		private readonly bool isNegative;

		private decimal degrees;

		private decimal minutes;

		private decimal seconds;

		private decimal milliseconds;

		internal CoordinateNumber(IEnumerable<double> coordinateNumbers)
		{
			double[] array = new double[3];
			IEnumerator<double> enumerator = coordinateNumbers.GetEnumerator();
			int num = 0;
			while (enumerator.MoveNext() && num < array.Length)
			{
				array[num] = enumerator.Current;
				num++;
			}
			isNegative = array[0] < 0.0;
			degrees = (decimal)Math.Abs(array[0]);
			minutes = (decimal)array[1];
			seconds = (decimal)array[2];
			DetectSpecialFormats();
		}

		private void DetectSpecialFormats()
		{
			if (DegreesCanBeSpecial())
			{
				if (DegreesCanBeMilliseconds())
				{
					DegreesAsMilliseconds();
				}
				else if (DegreesCanBeDegreesMinutesAndSeconds())
				{
					DegreesAsDegreesMinutesAndSeconds();
				}
				else if (DegreesCanBeDegreesAndMinutes())
				{
					DegreesAsDegreesAndMinutes();
				}
			}
		}

		private bool DegreesCanBeSpecial()
		{
			if (minutes == 0.0m)
			{
				return seconds == 0.0m;
			}
			return false;
		}

		private bool DegreesCanBeMilliseconds()
		{
			return degrees > 909090.0m;
		}

		private void DegreesAsMilliseconds()
		{
			milliseconds = degrees;
			degrees = 0.0m;
		}

		private bool DegreesCanBeDegreesMinutesAndSeconds()
		{
			return degrees > 9090.0m;
		}

		private void DegreesAsDegreesMinutesAndSeconds()
		{
			decimal num = (degrees * 0.0001m).SafeFloor();
			minutes = ((degrees - num * 100000.0m) * 0.01m).SafeFloor();
			seconds = (degrees - num * 10000.0m - minutes * 100.0m).SafeFloor();
			degrees = num;
		}

		private bool DegreesCanBeDegreesAndMinutes()
		{
			return degrees > 360.0m;
		}

		private void DegreesAsDegreesAndMinutes()
		{
			decimal num = (degrees * 0.01m).SafeFloor();
			minutes = degrees - num * 100.0m;
			degrees = num;
		}

		internal double ToDegrees()
		{
			double num = (double)(degrees + minutes * 0.0166666666666666666666666667m + seconds * 0.0002777777777777777777777778m + milliseconds * 0.0000002777777777777777777778m);
			if (!isNegative)
			{
				return num;
			}
			return 0.0 - num;
		}

		internal double ToRadians()
		{
			double num = (double)((degrees + minutes * 0.0166666666666666666666666667m + seconds * 0.0002777777777777777777777778m + milliseconds * 0.0000002777777777777777777778m) * 0.0174532925199432957692369077m);
			if (!isNegative)
			{
				return num;
			}
			return 0.0 - num;
		}
	}

	private const decimal PIm = 3.1415926535897932384626433833m;

	internal const decimal D2Rm = 0.0174532925199432957692369077m;

	internal const decimal R2Dm = 57.295779513082320876798154814m;

	internal const double D2R = Math.PI / 180.0;

	internal const double R2D = 57.295779513082316;

	private static readonly CultureInfo commaCulture = CultureInfo.GetCultureInfo("de");

	private static readonly Dictionary<Datum, Geodesic> geodesicLibrary = new Dictionary<Datum, Geodesic>();

	[JsonProperty(PropertyName = "Latitude")]
	public readonly double Latitude;

	[JsonProperty(PropertyName = "Longitude")]
	public readonly double Longitude;

	[JsonProperty(PropertyName = "Altitude")]
	public readonly double Altitude;

	[JsonConstructor]
	public Geodetic(double latitude, double longitude, double altitude)
	{
		Latitude = latitude;
		Longitude = FastMath.NormalizeRadiansPi(longitude);
		Altitude = altitude;
	}

	public Geodetic(decimal latitude, decimal longitude, double altitude)
	{
		Latitude = (double)latitude;
		Longitude = (double)FastMath.NormalizeRadiansPi(longitude);
		Altitude = altitude;
	}

	public (double Latitude, double Longitude, double Altitude) Deconstruct()
	{
		return (Latitude, Longitude, Altitude);
	}

	public Geodetic SurfaceTravel(double initialHeading, double distance, Datum datum = null)
	{
		GeodesicData geodesicData = GeodesicForDatum(datum).Direct(Latitude * 57.295779513082316, Longitude * 57.295779513082316, initialHeading * 57.295779513082316, distance);
		return new Geodetic(geodesicData.Latitude2 * (Math.PI / 180.0), geodesicData.Longitude2 * (Math.PI / 180.0), Altitude);
	}

	public double SurfaceDistance(in Geodetic destination, Datum datum = null)
	{
		return GeodesicForDatum(datum).Inverse(Latitude * 57.295779513082316, Longitude * 57.295779513082316, destination.Latitude * 57.295779513082316, destination.Longitude * 57.295779513082316).Distance;
	}

	public double SurfaceDistance(in Geodetic destination, out double initialHeading, Datum datum = null)
	{
		GeodesicData geodesicData = GeodesicForDatum(datum).Inverse(Latitude * 57.295779513082316, Longitude * 57.295779513082316, destination.Latitude * 57.295779513082316, destination.Longitude * 57.295779513082316);
		initialHeading = geodesicData.InitialAzimuth * (Math.PI / 180.0);
		return geodesicData.Distance;
	}

	public Geodetic VectorTravel(double initialHeading, double vectorDistance, Datum datum = null)
	{
		if ((object)datum == null)
		{
			datum = Datum.WGS84;
		}
		Geodetic geodetic = new Geodetic(Latitude, Longitude, 0.0);
		Ecef ecef = geodetic.ToEcef(datum);
		Geodetic result = geodetic;
		double num = vectorDistance;
		double num2 = double.MaxValue;
		while (Math.Abs(num2) >= 1E-08)
		{
			result = SurfaceTravel(initialHeading, num, datum).SetAltitude(0.0);
			Ecef other = result.ToEcef(datum);
			num2 = vectorDistance - ecef.DistanceFrom(in other);
			num += num2;
		}
		return result;
	}

	private static Geodesic GeodesicForDatum(Datum datum)
	{
		if ((object)datum == null)
		{
			datum = Datum.WGS84;
		}
		if (!geodesicLibrary.TryGetValue(datum, out var value))
		{
			value = new Geodesic(datum.SemiMajorAxis, datum.Flattening);
			geodesicLibrary[datum] = value;
		}
		return value;
	}

	public Geodetic SetAltitude(double altitude)
	{
		return new Geodetic(Latitude, Longitude, altitude);
	}

	public double GetGeocentricRadius(Datum datum = null)
	{
		if ((object)datum == null)
		{
			datum = Datum.WGS84;
		}
		double num = datum.SemiMajorAxis * datum.SemiMajorAxis;
		double num2 = Math.Cos(Latitude);
		double num3 = num * num2 * num2;
		double num4 = datum.SemiMinorAxis * datum.SemiMinorAxis;
		double num5 = Math.Sin(Latitude);
		double num6 = num4 * num5 * num5;
		return Math.Sqrt((num * num3 + num4 * num6) / (num3 + num6));
	}

	public string ToGGALine(DateTime time)
	{
		double num = Math.Abs(Latitude * 57.295779513082316);
		int num2 = (int)num;
		double num3 = (num - (double)num2) * 60.0;
		char c = ((Latitude >= 0.0) ? 'N' : 'S');
		double num4 = Math.Abs(Longitude * 57.295779513082316);
		int num5 = (int)num4;
		double num6 = (num4 - (double)num5) * 60.0;
		char c2 = ((Longitude > 0.0) ? 'E' : 'W');
		string text = $"GPGGA,{time.Hour:00}{time.Minute:00}{time.Second:00}.{time.Millisecond:00},{num2:00}{num3:00.000000000},{c},{num5:000}{num6:00.000000000},{c2},8,12,,{Altitude:0.000000},M,,,,";
		int num7 = 0;
		string text2 = text;
		foreach (int num8 in text2)
		{
			num7 ^= num8;
		}
		return $"${text}*{num7:X2}";
	}

	public static Geodetic FromDegrees(double latitude, double longitude, double altitude)
	{
		return new Geodetic((decimal)latitude * 0.0174532925199432957692369077m, (decimal)longitude * 0.0174532925199432957692369077m, altitude);
	}

	public Ecef ToEcef(Datum datum = null, Geoid geoid = null)
	{
		if ((object)datum == null)
		{
			datum = Datum.WGS84;
		}
		if (geoid != null)
		{
			double separation = geoid.GetSeparation(in this);
			return new Geodetic(Latitude, Longitude, Altitude + separation).ToEcef(datum);
		}
		double num = Math.Cos(Latitude);
		double num2 = Math.Sin(Latitude);
		double num3 = Math.Cos(Longitude);
		double num4 = Math.Sin(Longitude);
		double num5 = datum.SemiMajorAxis / Math.Sqrt(1.0 - datum.FirstEccentricitySquared * num2 * num2);
		double num6 = (num5 + Altitude) * num;
		double positionX = num6 * num3;
		double positionY = num6 * num4;
		double positionZ = ((1.0 - datum.FirstEccentricitySquared) * num5 + Altitude) * num2;
		return new Ecef(positionX, positionY, positionZ);
	}

	public LocalTangentPlane ToNed(in Geodetic referenceLocation, Datum datum = null, Geoid geoid = null)
	{
		Ecef referenceEcef = referenceLocation.ToEcef(datum, geoid);
		return ToNed(in referenceLocation, in referenceEcef, datum, geoid);
	}

	public LocalTangentPlane ToNed(in Geodetic referenceLocation, in Ecef referenceEcef, Datum datum = null, Geoid geoid = null)
	{
		if (!referenceEcef.IsAbsolute)
		{
			throw new ArgumentException("Relative Ecefs cannot be used as reference origin", "referenceEcef");
		}
		return (ToEcef(datum, geoid) - referenceEcef).ToNed(in referenceLocation);
	}

	public override string ToString()
	{
		return $"Lat:{Latitude * 57.295779513082316}° Long:{Longitude * 57.295779513082316}° Alt:{Altitude}m";
	}

	public string ToCoordinateString(int secondDecimalPlaces = 5)
	{
		string text = ((Latitude >= 0.0) ? "N" : "S");
		(int, int, double) tuple = GetDMS(Latitude);
		int item = tuple.Item1;
		int item2 = tuple.Item2;
		double item3 = tuple.Item3;
		string text2 = ((Longitude > 0.0) ? "E" : "W");
		(int, int, double) tuple2 = GetDMS(Longitude);
		int item4 = tuple2.Item1;
		int item5 = tuple2.Item2;
		double item6 = tuple2.Item3;
		string text3 = "00." + new string('#', secondDecimalPlaces);
		string text4 = item3.ToString(text3);
		string text5 = item6.ToString(text3);
		return $"{text} {item:00}° {item2:00}' {text4}\", {text2} {item4:000}° {item5:00}' {text5}\"";
		(int, int, double) GetDMS(double radians)
		{
			decimal num = decimal.Round(Math.Abs(206264.80624709635515647335733m * (decimal)radians), secondDecimalPlaces);
			int num2 = (int)(num * 0.0166666666666666666666666667m);
			int result;
			int item7 = Math.DivRem(num2, 60, out result);
			double num3 = (double)(num - (decimal)(60 * num2));
			if (num3.IsNegativeZero())
			{
				num3 = 0.0;
			}
			return (item7, result, num3);
		}
	}

	public static Geodetic? Parse(string coordinateString, double altitude = 0.0)
	{
		if (string.IsNullOrWhiteSpace(coordinateString))
		{
			return null;
		}
		double[] array = ParseNumbers(coordinateString);
		if (!ValidateCoordinateString(coordinateString, array.Length))
		{
			return null;
		}
		int count = array.Length / 2;
		double latitude = new CoordinateNumber(array.Take(count)).ToRadians() * ParseLatitudeSign(coordinateString);
		double num = new CoordinateNumber(array.Skip(count).Take(count)).ToRadians();
		num *= ParseLongitudeSign(coordinateString);
		return new Geodetic(latitude, num, altitude);
	}

	private static double[] ParseNumbers(string coordinateString)
	{
		if (string.IsNullOrWhiteSpace(coordinateString))
		{
			return Array.Empty<double>();
		}
		CultureInfo culture = CultureInfo.CurrentCulture;
		string numberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
		string pattern = ((numberDecimalSeparator == "." || numberDecimalSeparator == ",") ? "-?\\d+([\\.\\,]\\d+)?" : ("-?\\d+([\\.\\,\\" + numberDecimalSeparator + "]\\d+)?"));
		return Regex.Matches(coordinateString, pattern).Cast<Match>().Select(delegate(Match m)
		{
			if (double.TryParse(m.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result))
			{
				return result;
			}
			if (double.TryParse(m.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, commaCulture, out result))
			{
				return result;
			}
			return double.TryParse(m.Value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, culture, out result) ? result : double.NaN;
		})
			.ToArray();
	}

	private static bool ValidateCoordinateString(string coordinateString, int numberGroupCount)
	{
		if (string.IsNullOrWhiteSpace(coordinateString))
		{
			return false;
		}
		if (Regex.Match(coordinateString, "(?![neswd])[a-z]", RegexOptions.IgnoreCase).Success)
		{
			return false;
		}
		if (!Regex.Match(coordinateString, "^[^nsew]*[ns]?[^nsew]*[ew]?[^nsew]*$", RegexOptions.IgnoreCase).Success)
		{
			return false;
		}
		if (numberGroupCount != 2 && numberGroupCount != 4)
		{
			return numberGroupCount == 6;
		}
		return true;
	}

	private static double ParseLatitudeSign(string coordinateString)
	{
		if (!Regex.Match(coordinateString, "s", RegexOptions.IgnoreCase).Success)
		{
			return 1.0;
		}
		return -1.0;
	}

	private static double ParseLongitudeSign(string coordinateString)
	{
		if (!Regex.Match(coordinateString, "w", RegexOptions.IgnoreCase).Success)
		{
			return 1.0;
		}
		return -1.0;
	}

	public static bool operator ==(Geodetic left, Geodetic right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Geodetic left, Geodetic right)
	{
		return !(left == right);
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

	public bool Equals(Geodetic other)
	{
		double latitude = other.Latitude;
		if (latitude.Equals(Latitude))
		{
			latitude = other.Longitude;
			if (latitude.Equals(Longitude))
			{
				latitude = other.Altitude;
				return latitude.Equals(Altitude);
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		double latitude = Latitude;
		int num = (5993773 + latitude.GetHashCode()) * 9973;
		latitude = Longitude;
		int num2 = (num + latitude.GetHashCode()) * 9973;
		latitude = Altitude;
		return num2 + latitude.GetHashCode();
	}
}
