using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo
{
	public sealed class Almanac : AlmanacBase
	{
		private static readonly GnssTimeSpan ephemerisSpan = GnssTimeSpan.FromMinutes(10);

		private static readonly int ephemerisDuration = (int)ephemerisSpan.Seconds;

		private static readonly int iodPeriod = ephemerisDuration;

		private static readonly GnssTimeSpan almanacSpan = GnssTimeSpan.FromMinutes(10);

		private static readonly int almanacDuration = (int)almanacSpan.Seconds;

		private static readonly double Semi2Rad = Constellation.Datum.PI;

		private protected override GnssTime GetEphemerisIntervalStart(in GnssTime transmissionTime)
		{
			int second = transmissionTime.GalileoNavicSecondOfWeek / ephemerisDuration * ephemerisDuration;
			return GnssTime.FromGalileoNavic(transmissionTime.GalileoNavicWeek, second);
		}

		private protected override GnssTime GetTimeOfEphemeris(in GnssTime transmissionTime, SignalType signalType)
		{
			GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
			if (signalType <= SignalType.GalileoE5bI)
			{
				switch (signalType)
				{
				}
			}
			else if (signalType != SignalType.GalileoE5AltBocI && signalType != SignalType.GalileoE5AltBocQ)
			{
				_ = 4194304;
			}
			return ephemerisIntervalStart - ephemerisSpan;
		}

		private protected override SatelliteBase CreateEphemeris(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			Satellite satellite = baselineSat as Satellite;
			if (satellite == null)
			{
				throw new ArgumentException("baselineSat is not a Galileo satellite", "baselineSat");
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
			int second = transmissionTime.GalileoNavicSecondOfWeek / almanacDuration * almanacDuration;
			return GnssTime.FromGalileoNavic(transmissionTime.GalileoNavicWeek, second) - almanacSpan;
		}

		private protected override SatelliteBase CreateAlmanac(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			Satellite satellite = baselineSat as Satellite;
			if (satellite == null)
			{
				throw new ArgumentException("baselineSat is not a Galileo satellite", "baselineSat");
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
				TransmissionInterval = transmissionInterval,
				TimeOfApplicability = galileoNavicSecondOfWeek,
				Week = galileoNavicWeek
			};
		}

		public override void UpdateAlmanacForTime(in GnssTime simulationTime)
		{
			int satIndex = 0;
			GnssTime timeOfAlmanac = GetTimeOfAlmanac(in simulationTime, in satIndex);
			int galileoNavicWeek = timeOfAlmanac.GalileoNavicWeek;
			int galileoNavicSecondOfWeek = timeOfAlmanac.GalileoNavicSecondOfWeek;
			GnssTime gnssTime = GnssTime.FromGalileoNavic(galileoNavicWeek, 0);
			int iod = (galileoNavicSecondOfWeek / iodPeriod) & 0xF;
			base.BaselineSatellites = base.OriginalSatellites.Select((SatelliteBase s) => s?.Clone()).ToArray(base.OriginalSatellites.Count);
			foreach (Satellite item in from s in base.BaselineSatellites
				select s as Satellite into s
				where s != null
				select (s))
			{
				int week = item.Week;
				int timeOfApplicability = item.TimeOfApplicability;
				GnssTime gnssTime2 = GnssTime.FromGalileoNavic(week, timeOfApplicability);
				GnssTime gnssTime3 = GnssTime.FromGalileoNavic(week, 0);
				double seconds = (timeOfAlmanac - gnssTime2).Seconds;
				double seconds2 = (gnssTime - gnssTime3).Seconds;
				item.Week = galileoNavicWeek;
				item.TimeOfApplicability = galileoNavicSecondOfWeek;
				item.Iod = iod;
				double sqrtA = item.SqrtA;
				double num = item.Datum.SqrtGM / (sqrtA * sqrtA * sqrtA) + item.MeanMotionCorrection;
				item.MeanAnomaly = FastMath.NormalizeRadiansPi(item.MeanAnomaly + num * seconds);
				double num2 = ((Math.Abs(seconds2) <= 604800.0) ? item.RateOfLongitudeOfAscendingNode : 0.0);
				double radians = item.LongitudeOfAscendingNode + seconds2 * (num2 - item.Datum.AngularVelocity);
				item.LongitudeOfAscendingNode = FastMath.NormalizeRadiansPi(radians);
			}
		}

		internal static Almanac LoadXml(Stream stream, in GnssTime simulationTime)
		{
			SatelliteBase[] array = new SatelliteBase[50];
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(stream);
			}
			catch (XmlException)
			{
				return new Almanac();
			}
			try
			{
				DateTime result;
				GnssTime gnssTime = (DateTime.TryParseExact(xmlDocument.DocumentElement.SelectSingleNode("header")?.SelectSingleNode("GAL-header")?.SelectSingleNode("issueDate")?.FirstChild?.Value, "yyyy-MM-ddTHH:mm:ss.fZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result) ? GnssTime.FromUtc(result) : simulationTime);
				XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("body")?.SelectSingleNode("Almanacs");
				if (xmlNode != null)
				{
					foreach (XmlNode item in from XmlNode n in xmlNode.ChildNodes
						where n.Name == "svAlmanac"
						select n)
					{
						try
						{
							int? num = (int?)ParseChild(item, "SVID");
							if (!num.HasValue)
							{
								continue;
							}
							XmlNode xmlNode2 = item.SelectSingleNode("almanac");
							Satellite satellite;
							if (xmlNode2 != null)
							{
								satellite = new Satellite
								{
									Id = num.Value,
									DeltaSqrtA = ParseChild(xmlNode2, "aSqRoot").GetValueOrDefault(),
									Eccentricity = ParseChild(xmlNode2, "ecc").GetValueOrDefault(),
									DeltaInclination = ParseChild(xmlNode2, "deltai").GetValueOrDefault() * Semi2Rad,
									LongitudeOfAscendingNode = ParseChild(xmlNode2, "omega0").GetValueOrDefault() * Semi2Rad,
									RateOfLongitudeOfAscendingNode = ParseChild(xmlNode2, "omegaDot").GetValueOrDefault() * Semi2Rad,
									ArgumentOfPerigee = ParseChild(xmlNode2, "w").GetValueOrDefault() * Semi2Rad,
									MeanAnomaly = ParseChild(xmlNode2, "m0").GetValueOrDefault() * Semi2Rad,
									Af0 = ParseChild(xmlNode2, "af0").GetValueOrDefault(),
									Af1 = ParseChild(xmlNode2, "af1").GetValueOrDefault(),
									Iod = (int)ParseChild(xmlNode2, "iod").GetValueOrDefault(),
									TimeOfApplicability = (int)ParseChild(xmlNode2, "t0a").GetValueOrDefault(),
									Week = (int)ParseChild(xmlNode2, "wna").GetValueOrDefault(),
									StatusE5a = (SatelliteHealth)ParseChild(item.SelectSingleNode("svFNavSignalStatus"), "statusE5a").GetValueOrDefault(),
									StatusE5b = (SatelliteHealth)ParseChild(item.SelectSingleNode("svINavSignalStatus"), "statusE5b").GetValueOrDefault(),
									StatusE1B = (SatelliteHealth)ParseChild(item.SelectSingleNode("svINavSignalStatus"), "statusE1B").GetValueOrDefault()
								};
								int week = satellite.Week;
								int num2 = gnssTime.GalileoNavicWeek & 3;
								satellite.Week = gnssTime.GalileoNavicWeek;
								if (week == num2)
								{
									goto IL_03cf;
								}
								if (((num2 + 1) & 3) == satellite.Week)
								{
									satellite.Week++;
									goto IL_03cf;
								}
								if (((num2 + 3) & 3) == satellite.Week)
								{
									satellite.Week--;
									goto IL_03cf;
								}
								RLLogger.GetLogger().LogMessage($"WARNING: The difference between almanac wna and issue week is 2, we don't know which way to adjust, there is something wrong with the Galileo almanac for satellite {satellite.Id}");
							}
							goto end_IL_0103;
							IL_03cf:
							try
							{
								array[satellite.Index] = satellite;
							}
							catch (IndexOutOfRangeException)
							{
							}
							end_IL_0103:;
						}
						catch (ArgumentException)
						{
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
			}
			catch (XPathException)
			{
				return new Almanac();
			}
			stream.Seek(0L, SeekOrigin.Begin);
			string rawAlmanac;
			using (TextReader textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
			{
				rawAlmanac = textReader.ReadToEnd();
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

		private static double? ParseChild(XmlNode node, string childName)
		{
			if (double.TryParse(node?.SelectSingleNode(childName)?.FirstChild?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				return result;
			}
			return null;
		}
	}
}
