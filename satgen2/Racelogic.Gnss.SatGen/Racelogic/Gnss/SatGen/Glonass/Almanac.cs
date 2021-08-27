using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Glonass
{
	public sealed class Almanac : AlmanacBase
	{
		private static readonly GnssTimeSpan ephemerisSpan = GnssTimeSpan.FromMinutes(30);

		private static readonly int ephemerisDuration = (int)ephemerisSpan.Seconds;

		private protected override GnssTime GetEphemerisIntervalStart(in GnssTime transmissionTime)
		{
			int num = transmissionTime.GlonassSecondOfDay / ephemerisDuration * ephemerisDuration;
			return GnssTime.FromGlonass(transmissionTime.GlonassFourYearPeriodNumber, transmissionTime.GlonassFourYearPeriodDayNumber, num);
		}

		private protected override GnssTime GetTimeOfEphemeris(in GnssTime transmissionTime, SignalType signalType)
		{
			GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
			if (signalType != SignalType.GlonassL1OF)
			{
				_ = 1024;
			}
			return ephemerisIntervalStart + GnssTimeSpan.FromMinutes(15);
		}

		private protected override SatelliteBase CreateEphemeris(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			Satellite satellite = baselineSat as Satellite;
			if (satellite == null)
			{
				throw new ArgumentException("baselineSat is not a Glonass satellite", "baselineSat");
			}
			int glonassSecondOfDay = referenceTime.GlonassSecondOfDay;
			double eccentricAnomaly;
			Ecef ecef = satellite.GetEcef(in referenceTime, out eccentricAnomaly);
			GnssTime ephemerisIntervalStart = GetEphemerisIntervalStart(in transmissionTime);
			Range<GnssTime, GnssTimeSpan> transmissionInterval = new Range<GnssTime, GnssTimeSpan>(ephemerisIntervalStart, ephemerisSpan);
			return new Satellite
			{
				Id = satellite.Id,
				Slot = satellite.Slot,
				IsHealthy = satellite.IsHealthy,
				TimeOfReceiptOfAlmanac = satellite.TimeOfReceiptOfAlmanac,
				GlonassTimeCorrection = satellite.GlonassTimeCorrection,
				SatelliteClockBiasAtEphemerisTime = satellite.GlonassTimeCorrection,
				TimeOfApplicability = glonassSecondOfDay,
				TransmissionInterval = transmissionInterval,
				Position = ecef.Position,
				Velocity = ecef.Velocity
			};
		}

		internal override GnssTime GetTimeOfAlmanac(in GnssTime transmissionTime, in int satIndex = 0)
		{
			return (base.BaselineSatellites[satIndex] as Satellite)?.TimeOfReceiptOfAlmanac ?? transmissionTime;
		}

		private protected override SatelliteBase CreateAlmanac(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat)
		{
			if (!(baselineSat is Satellite))
			{
				throw new ArgumentException("baselineSat is not a Glonass satellite", "baselineSat");
			}
			return baselineSat;
		}

		public override void UpdateAlmanacForTime(in GnssTime simulationTime)
		{
		}

		internal static Almanac LoadAgl(Stream stream)
		{
			SatelliteBase[] array = new SatelliteBase[50];
			string rawAlmanac;
			using (TextReader textReader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true))
			{
				int num = 0;
				Satellite satellite = null;
				while (true)
				{
					string text = textReader.ReadLine();
					if (text == null)
					{
						break;
					}
					if (!text.Any())
					{
						continue;
					}
					string[] source = text.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					switch (num % 3)
					{
					case 0:
					{
						satellite = new Satellite();
						int.TryParse(source.ElementAtOrDefault(0), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result9);
						int.TryParse(source.ElementAtOrDefault(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result10);
						int.TryParse(source.ElementAtOrDefault(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result11);
						int.TryParse(source.ElementAtOrDefault(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result13);
						DateTime utcDateTime2 = new DateTime(result11, result10, result9) + TimeSpan.FromSeconds(result13);
						satellite.TimeOfReceiptOfAlmanac = GnssTime.FromUtc(utcDateTime2);
						break;
					}
					case 1:
						if (satellite != null)
						{
							int.TryParse(source.ElementAtOrDefault(0), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result18);
							int.TryParse(source.ElementAtOrDefault(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result19);
							int.TryParse(source.ElementAtOrDefault(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result20);
							int.TryParse(source.ElementAtOrDefault(3), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2);
							int.TryParse(source.ElementAtOrDefault(4), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3);
							int.TryParse(source.ElementAtOrDefault(5), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4);
							double.TryParse(source.ElementAtOrDefault(6), NumberStyles.Float, CultureInfo.InvariantCulture, out var result5);
							satellite.Id = result18;
							satellite.Slot = result19;
							satellite.IsHealthy = result20 != 0;
							DateTime utcDateTime = new DateTime(result4, result3, result2) + TimeSpan.FromSeconds(result5) - TimeSpan.FromHours(3.0);
							satellite.TimeOfAscendingNode = GnssTime.FromUtc(utcDateTime);
							double.TryParse(source.ElementAtOrDefault(7), NumberStyles.Float, CultureInfo.InvariantCulture, out var result6);
							double.TryParse(source.ElementAtOrDefault(8), NumberStyles.Float, CultureInfo.InvariantCulture, out var result7);
							double.TryParse(source.ElementAtOrDefault(9), NumberStyles.Float, CultureInfo.InvariantCulture, out var result8);
							satellite.UtcTimeCorrection = result6;
							satellite.GpsTimeCorrection = result7;
							satellite.GlonassTimeCorrection = result8;
						}
						break;
					case 2:
						if (satellite != null)
						{
							double.TryParse(source.ElementAtOrDefault(0), NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
							double.TryParse(source.ElementAtOrDefault(1), NumberStyles.Float, CultureInfo.InvariantCulture, out var result12);
							double.TryParse(source.ElementAtOrDefault(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var result14);
							double.TryParse(source.ElementAtOrDefault(3), NumberStyles.Float, CultureInfo.InvariantCulture, out var result15);
							double.TryParse(source.ElementAtOrDefault(4), NumberStyles.Float, CultureInfo.InvariantCulture, out var result16);
							double.TryParse(source.ElementAtOrDefault(5), NumberStyles.Float, CultureInfo.InvariantCulture, out var result17);
							satellite.LongitudeOfAscendingNode = result * Math.PI;
							satellite.InclinationCorrection = result12 * Math.PI;
							satellite.ArgumentOfPerigee = result14 * Math.PI;
							satellite.Eccentricity = result15;
							satellite.DraconicPeriodCorrection = result16;
							satellite.DraconicPeriodCorrectionRate = result17;
							SatelliteBase[] array2 = array;
							int index = satellite.Index;
							if (array2[index] == null)
							{
								array2[index] = satellite;
							}
						}
						break;
					}
					num++;
				}
				stream.Seek(0L, SeekOrigin.Begin);
				using TextReader textReader2 = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
				rawAlmanac = textReader2.ReadToEnd();
			}
			foreach (Satellite item in array.Where((SatelliteBase s) => s != null))
			{
				item.GlonassTimeCorrection = 0.0;
				item.GpsTimeCorrection = 0.0;
				item.SatelliteClockBiasAtEphemerisTime = 0.0;
				item.UtcTimeCorrection = 0.0;
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
	}
}
