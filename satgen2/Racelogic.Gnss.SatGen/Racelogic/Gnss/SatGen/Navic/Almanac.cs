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

namespace Racelogic.Gnss.SatGen.Navic
{
	public sealed class Almanac : AlmanacBase
	{
		private static readonly GnssTimeSpan ephemerisSpan = GnssTimeSpan.FromHours(2);

		private static readonly int ephemerisDuration = (int)ephemerisSpan.Seconds;

		private protected override GnssTime GetEphemerisIntervalStart(in GnssTime transmissionTime)
		{
			int second = transmissionTime.GalileoNavicSecondOfWeek / ephemerisDuration * ephemerisDuration;
			return GnssTime.FromGalileoNavic(transmissionTime.GalileoNavicWeek, second);
		}

		private protected override GnssTime GetTimeOfEphemeris(in GnssTime transmissionTime, SignalType signalType)
		{
			return GetEphemerisIntervalStart(in transmissionTime);
		}

		private protected override SatelliteBase CreateEphemeris(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			Satellite satellite = baselineSat as Satellite;
			if (satellite == null)
			{
				throw new ArgumentException("baselineSat is not a Navic satellite", "baselineSat");
			}
			int galileoNavicWeek = referenceTime.GalileoNavicWeek;
			int galileoNavicSecondOfWeek = referenceTime.GalileoNavicSecondOfWeek;
			int week = satellite.Week;
			int timeOfApplicability = satellite.TimeOfApplicability;
			GnssTime gnssTime = GnssTime.FromGalileoNavic(week, timeOfApplicability);
			double seconds = (referenceTime - gnssTime).Seconds;
			int num = (galileoNavicWeek - week) * 604800;
			double sqrtA = satellite.SqrtA;
			double num2 = satellite.Datum.SqrtGM / (sqrtA * sqrtA * sqrtA) + satellite.MeanMotionCorrection;
			double meanAnomaly = FastMath.NormalizeRadiansPi(satellite.MeanAnomaly + num2 * seconds);
			double radians = satellite.LongitudeOfAscendingNode + seconds * satellite.RateOfLongitudeOfAscendingNode - (double)num * satellite.Datum.AngularVelocity;
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
				TimeOfApplicability = galileoNavicSecondOfWeek,
				TransmissionInterval = transmissionInterval,
				Week = galileoNavicWeek
			};
		}

		internal override GnssTime GetTimeOfAlmanac(in GnssTime transmissionTime, in int satIndex = 0)
		{
			return GnssTime.FromGalileoNavic(transmissionTime.GalileoNavicWeek, transmissionTime.GalileoNavicDayOfWeek * 86400);
		}

		private protected override SatelliteBase CreateAlmanac(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			Satellite satellite = baselineSat as Satellite;
			if (satellite == null)
			{
				throw new ArgumentException("baselineSat is not a Navic satellite", "baselineSat");
			}
			int galileoNavicWeek = referenceTime.GalileoNavicWeek;
			int galileoNavicSecondOfWeek = referenceTime.GalileoNavicSecondOfWeek;
			int week = satellite.Week;
			int timeOfApplicability = satellite.TimeOfApplicability;
			GnssTime gnssTime = GnssTime.FromGalileoNavic(week, timeOfApplicability);
			double seconds = (referenceTime - gnssTime).Seconds;
			int num = (galileoNavicWeek - week) * 604800;
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
				TimeOfApplicability = galileoNavicSecondOfWeek,
				TransmissionInterval = transmissionInterval,
				Week = galileoNavicWeek
			};
		}

		public override void UpdateAlmanacForTime(in GnssTime simulationTime)
		{
			int satIndex = 0;
			GnssTime timeOfAlmanac = GetTimeOfAlmanac(in simulationTime, in satIndex);
			int galileoNavicWeek = timeOfAlmanac.GalileoNavicWeek;
			int galileoNavicSecondOfWeek = timeOfAlmanac.GalileoNavicSecondOfWeek;
			GnssTime.FromGalileoNavic(galileoNavicWeek, 0);
			base.BaselineSatellites = base.OriginalSatellites.Select((SatelliteBase s) => s?.Clone()).ToArray(base.OriginalSatellites.Count);
			foreach (Satellite item in from s in base.BaselineSatellites
				select s as Satellite into s
				where s != null
				select (s))
			{
				int week = item.Week;
				int timeOfApplicability = item.TimeOfApplicability;
				if (galileoNavicSecondOfWeek != timeOfApplicability || galileoNavicWeek != week)
				{
					GnssTime gnssTime = GnssTime.FromGalileoNavic(week, timeOfApplicability);
					double seconds = (timeOfAlmanac - gnssTime).Seconds;
					int num = (galileoNavicWeek - week) * 604800;
					item.Week = galileoNavicWeek;
					item.TimeOfApplicability = galileoNavicSecondOfWeek;
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
							satellite.Af0 = value;
						}
						if (ReadValue("af1", dictionary, out value))
						{
							satellite.Af1 = value;
						}
						if (ReadValue("week", dictionary, out value))
						{
							int i = (int)value;
							for (int galileoNavicWeek = simulationTime.GalileoNavicWeek; i < galileoNavicWeek - 512; i += 1024)
							{
							}
							satellite.Week = i;
						}
						if (satellite.Index >= 0)
						{
							if (satellite.SqrtA > AlmanacBase.SqrtAThreshold)
							{
								if (satellite.Inclination < Math.PI / 18.0)
								{
									satellite.OrbitType = OrbitType.GEO;
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
							array[satellite.Index] = satellite;
							if (satellite.Index >= 0)
							{
								array[satellite.Index] = satellite;
							}
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
}
