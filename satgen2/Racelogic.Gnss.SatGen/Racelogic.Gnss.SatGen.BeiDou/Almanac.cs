using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.BeiDou;

public sealed class Almanac : AlmanacBase
{
	private static readonly GnssTimeSpan ephemerisSpan = GnssTimeSpan.FromHours(1);

	private static readonly int ephemerisDuration = (int)ephemerisSpan.Seconds;

	private static readonly GnssTimeSpan almanacDelay = GnssTimeSpan.FromDays(3);

	private const double nominalInclination = 0.95993108859688125;

	private static readonly double sinTilt = Math.Sin(Math.PI / 36.0);

	private static readonly double cosTilt = Math.Cos(Math.PI / 36.0);

	private static readonly double cotTilt = 1.0 / Math.Tan(Math.PI / 36.0);

	private protected override GnssTime GetEphemerisIntervalStart(in GnssTime transmissionTime)
	{
		int second = transmissionTime.BeiDouSecondOfWeek / ephemerisDuration * ephemerisDuration;
		return GnssTime.FromBeiDou(transmissionTime.BeiDouWeek, second);
	}

	private protected override GnssTime GetTimeOfEphemeris(in GnssTime transmissionTime, SignalType signalType)
	{
		GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
		if (signalType != SignalType.BeiDouB1I && signalType != SignalType.BeiDouB2I)
		{
			_ = 16384;
		}
		return ephemerisIntervalStart;
	}

	private protected override SatelliteBase CreateEphemeris(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
	{
		if (!(baselineSat is Satellite satellite))
		{
			throw new ArgumentException("baselineSat is not a BeiDou satellite", "baselineSat");
		}
		int beiDouWeek = referenceTime.BeiDouWeek;
		int beiDouSecondOfWeek = referenceTime.BeiDouSecondOfWeek;
		int week = satellite.Week;
		int timeOfApplicability = satellite.TimeOfApplicability;
		GnssTime gnssTime = GnssTime.FromBeiDou(week, timeOfApplicability);
		double seconds = (referenceTime - gnssTime).Seconds;
		int num = (beiDouWeek - week) * 604800;
		double sqrtA = satellite.SqrtA;
		double num2 = satellite.Datum.SqrtGM / (sqrtA * sqrtA * sqrtA) + satellite.MeanMotionCorrection;
		double meanAnomaly = FastMath.NormalizeRadiansPi(satellite.MeanAnomaly + num2 * seconds);
		double num3 = satellite.LongitudeOfAscendingNode + seconds * satellite.RateOfLongitudeOfAscendingNode - (double)num * satellite.Datum.AngularVelocity;
		double inclination;
		double argumentOfPerigee;
		if (satellite.OrbitType == OrbitType.GEO)
		{
			double num4 = satellite.Datum.AngularVelocity * (double)beiDouSecondOfWeek;
			double num5 = num3 - num4;
			double y = Math.Sin(num5);
			double num6 = Math.Cos(num5);
			double num7 = Math.Sin(satellite.Inclination);
			double num8 = Math.Cos(satellite.Inclination);
			double num9 = Math.Tan(satellite.Inclination);
			inclination = Math.Acos(cosTilt * num8 + sinTilt * num7 * num6);
			num3 = Math.Atan2(y, num6 * cosTilt - sinTilt / num9) + num4;
			double num10 = Math.Atan2(y, (0.0 - num6) * num8 + num7 * cotTilt);
			argumentOfPerigee = FastMath.NormalizeRadiansPi(satellite.ArgumentOfPerigee - num10);
		}
		else
		{
			inclination = FastMath.NormalizeRadiansPi(satellite.Inclination + satellite.RateOfInclination * seconds);
			argumentOfPerigee = satellite.ArgumentOfPerigee;
		}
		num3 = FastMath.NormalizeRadiansPi(num3);
		GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
		Range<GnssTime, GnssTimeSpan> transmissionInterval = new Range<GnssTime, GnssTimeSpan>(ephemerisIntervalStart, ephemerisSpan);
		return new Satellite
		{
			Eccentricity = satellite.Eccentricity,
			Id = satellite.Id,
			IsEnabled = satellite.IsEnabled,
			IsHealthy = satellite.IsHealthy,
			MeanMotionCorrection = satellite.MeanMotionCorrection,
			OrbitType = satellite.OrbitType,
			RateOfInclination = satellite.RateOfInclination,
			RateOfLongitudeOfAscendingNode = satellite.RateOfLongitudeOfAscendingNode,
			SqrtA = satellite.SqrtA,
			ArgumentOfPerigee = argumentOfPerigee,
			Inclination = inclination,
			LongitudeOfAscendingNode = num3,
			MeanAnomaly = meanAnomaly,
			TimeOfApplicability = beiDouSecondOfWeek,
			TransmissionInterval = transmissionInterval,
			Week = beiDouWeek
		};
	}

	internal override GnssTime GetTimeOfAlmanac(in GnssTime transmissionTime, in int satIndex = 0)
	{
		GnssTime gnssTime = transmissionTime - almanacDelay;
		int beiDouWeek = gnssTime.BeiDouWeek;
		int beiDouSecondOfWeek = gnssTime.BeiDouSecondOfWeek;
		beiDouSecondOfWeek >>= 12;
		beiDouSecondOfWeek <<= 12;
		return GnssTime.FromBeiDou(beiDouWeek, beiDouSecondOfWeek);
	}

	private protected override SatelliteBase CreateAlmanac(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
	{
		if (!(baselineSat is Satellite satellite))
		{
			throw new ArgumentException("baselineSat is not a BeiDou satellite", "baselineSat");
		}
		int beiDouWeek = referenceTime.BeiDouWeek;
		int beiDouSecondOfWeek = referenceTime.BeiDouSecondOfWeek;
		int week = satellite.Week;
		int timeOfApplicability = satellite.TimeOfApplicability;
		GnssTime gnssTime = GnssTime.FromBeiDou(week, timeOfApplicability);
		double seconds = (referenceTime - gnssTime).Seconds;
		int num = (beiDouWeek - week) * 604800;
		double sqrtA = satellite.SqrtA;
		double num2 = satellite.Datum.SqrtGM / (sqrtA * sqrtA * sqrtA) + satellite.MeanMotionCorrection;
		double meanAnomaly = FastMath.NormalizeRadiansPi(satellite.MeanAnomaly + num2 * seconds);
		double radians = satellite.LongitudeOfAscendingNode + (double)num * (satellite.RateOfLongitudeOfAscendingNode - satellite.Datum.AngularVelocity);
		radians = FastMath.NormalizeRadiansPi(radians);
		GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
		Range<GnssTime, GnssTimeSpan> transmissionInterval = new Range<GnssTime, GnssTimeSpan>(ephemerisIntervalStart, ephemerisSpan);
		return new Satellite
		{
			ArgumentOfPerigee = satellite.ArgumentOfPerigee,
			Eccentricity = satellite.Eccentricity,
			Id = satellite.Id,
			Inclination = satellite.Inclination,
			IsEnabled = satellite.IsEnabled,
			IsHealthy = satellite.IsHealthy,
			MeanMotionCorrection = satellite.MeanMotionCorrection,
			OrbitType = satellite.OrbitType,
			RateOfInclination = satellite.RateOfInclination,
			RateOfLongitudeOfAscendingNode = satellite.RateOfLongitudeOfAscendingNode,
			SqrtA = satellite.SqrtA,
			LongitudeOfAscendingNode = radians,
			MeanAnomaly = meanAnomaly,
			TimeOfApplicability = beiDouSecondOfWeek,
			TransmissionInterval = transmissionInterval,
			Week = beiDouWeek
		};
	}

	public override void UpdateAlmanacForTime(in GnssTime simulationTime)
	{
		int satIndex = 0;
		GnssTime timeOfAlmanac = GetTimeOfAlmanac(in simulationTime, in satIndex);
		int beiDouWeek = timeOfAlmanac.BeiDouWeek;
		int beiDouSecondOfWeek = timeOfAlmanac.BeiDouSecondOfWeek;
		GnssTime.FromBeiDou(beiDouWeek, 0);
		base.BaselineSatellites = base.OriginalSatellites.Select((SatelliteBase s) => s?.Clone()).ToArray(base.OriginalSatellites.Count);
		foreach (Satellite item in from s in base.BaselineSatellites
			select s as Satellite into s
			where s != null
			select (s))
		{
			int week = item.Week;
			int timeOfApplicability = item.TimeOfApplicability;
			if (beiDouSecondOfWeek != timeOfApplicability || beiDouWeek != week)
			{
				GnssTime gnssTime = GnssTime.FromBeiDou(week, timeOfApplicability);
				double seconds = (timeOfAlmanac - gnssTime).Seconds;
				int num = (beiDouWeek - week) * 604800;
				item.Week = beiDouWeek;
				item.TimeOfApplicability = beiDouSecondOfWeek;
				double sqrtA = item.SqrtA;
				double num2 = item.Datum.SqrtGM / (sqrtA * sqrtA * sqrtA) + item.MeanMotionCorrection;
				item.MeanAnomaly = FastMath.NormalizeRadiansPi(item.MeanAnomaly + num2 * seconds);
				double num3 = ((Math.Abs(num) <= 604800) ? item.RateOfLongitudeOfAscendingNode : 0.0);
				double radians = item.LongitudeOfAscendingNode + (double)num * (num3 - item.Datum.AngularVelocity);
				item.LongitudeOfAscendingNode = FastMath.NormalizeRadiansPi(radians);
			}
		}
	}

	internal static Almanac LoadYuma(Stream stream, in GnssTime simulationTime)
	{
		SatelliteBase[] array = new SatelliteBase[50];
		string rawAlmanac;
		using (TextReader textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string text;
			while ((text = textReader.ReadLine()) != null || dictionary.Any())
			{
				if (!string.IsNullOrWhiteSpace(text?.Trim()))
				{
					string[] array2 = text.Split(':');
					if (array2.Length == 2)
					{
						dictionary.Add(array2[0].Trim().ToLowerInvariant(), array2[1].Trim());
					}
				}
				else
				{
					if (dictionary.Count <= 0)
					{
						continue;
					}
					Satellite satellite = new Satellite();
					if (ReadValue("id", dictionary, out var value))
					{
						satellite.Id = (int)value;
					}
					if (ReadValue("health", dictionary, out value))
					{
						satellite.IsHealthy = value == 0.0;
					}
					if (ReadValue("eccentricity", dictionary, out value))
					{
						satellite.Eccentricity = value;
					}
					if (ReadValue("time of applicability", dictionary, out value))
					{
						satellite.TimeOfApplicability = (int)value;
					}
					if (ReadValue("orbital inclination", dictionary, out value))
					{
						satellite.Inclination = value;
					}
					if (ReadValue("rate of right ascen", dictionary, out value))
					{
						satellite.RateOfLongitudeOfAscendingNode = value;
					}
					if (ReadValue("sqrt(a)", dictionary, out value))
					{
						satellite.SqrtA = value;
					}
					if (ReadValue("right ascen", dictionary, out value))
					{
						satellite.LongitudeOfAscendingNode = value;
					}
					if (ReadValue("argument of perigee", dictionary, out value))
					{
						satellite.ArgumentOfPerigee = value;
					}
					if (ReadValue("mean anom", dictionary, out value))
					{
						satellite.MeanAnomaly = value;
					}
					if (ReadValue("af0", dictionary, out value))
					{
						satellite.A0 = value;
					}
					if (ReadValue("af1", dictionary, out value))
					{
						satellite.A1 = value;
					}
					if (ReadValue("week", dictionary, out value))
					{
						int i = (int)value;
						for (int beiDouWeek = simulationTime.BeiDouWeek; i < beiDouWeek - 4096; i += 8192)
						{
						}
						satellite.Week = i;
					}
					if (satellite.Index >= 0)
					{
						double num = 0.95993108859688125;
						if (satellite.SqrtA > AlmanacBase.SqrtAThreshold)
						{
							if (satellite.Id <= 5)
							{
								satellite.OrbitType = OrbitType.GEO;
								num = 0.0;
								satellite.RateOfLongitudeOfAscendingNode = 0.0;
								satellite.RateOfInclination = 0.0;
							}
							else
							{
								satellite.OrbitType = OrbitType.IGSO;
							}
						}
						else
						{
							satellite.OrbitType = OrbitType.MEO;
						}
						if (satellite.Inclination < 0.47996554429844063)
						{
							satellite.Inclination += num;
						}
						array[satellite.Index] = satellite;
					}
					dictionary.Clear();
				}
			}
			stream.Seek(0L, SeekOrigin.Begin);
			using TextReader textReader2 = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
			rawAlmanac = textReader2.ReadToEnd();
		}
		if (array.Any((SatelliteBase s) => s != null))
		{
			SatelliteBase[] baselineSatellites = array.Select((SatelliteBase s) => s?.Clone()).ToArray(array.Length);
			return new Almanac
			{
				OriginalSatellites = array,
				BaselineSatellites = baselineSatellites,
				RawAlmanac = rawAlmanac
			};
		}
		return new Almanac();
	}

	private static bool ReadValue(string fieldNameBeginning, IDictionary<string, string> fields, out double value)
	{
		string fieldNameBeginning2 = fieldNameBeginning;
		double val = 0.0;
		bool result = fields.Any<KeyValuePair<string, string>>((KeyValuePair<string, string> kvp) => kvp.Key.StartsWith(fieldNameBeginning2) && double.TryParse(kvp.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out val));
		value = val;
		return result;
	}
}
