using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Glonass;

namespace Racelogic.Gnss.SatGen.Glonass
{
	internal sealed class NavigationDataL1OF : NavigationData
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GlonassL1OF);

		private static readonly byte[] timeMarkBits = new byte[30]
		{
			1, 1, 1, 1, 1, 0, 0, 0, 1, 1,
			0, 1, 1, 1, 0, 1, 0, 1, 0, 0,
			0, 0, 1, 0, 0, 1, 0, 1, 1, 0
		};

		private const int frameLength = 30;

		private const int superFrameLength = 150;

		private const int stringsPerFrame = 15;

		private const int framesPerSuperFrame = 5;

		private const int stringLength = 2;

		private static readonly GnssTimeSpan frameLengthTimeSpan = GnssTimeSpan.FromSeconds(30);

		private bool isLeapSecondInsertedNow;

		private const double scaleGamma = 1099511627776.0;

		private const double scaleXYZ = 2.048;

		private const double scaleXYZdot = 1048.576;

		private const double scaleXYZdotdot = 1073741.824;

		private const double scaleTauN = 1073741824.0;

		private const double scaleDeltaTauN = 1073741824.0;

		private const double scaleTauC = 2147483648.0;

		private const double scaleTauGPS = 1073741824.0;

		private const double scaleTauA = 262144.0;

		private const double scaleLambdaA = 1048576.0;

		private const double scaleDeltaiA = 1048576.0;

		private const double scaleEpsilonA = 1048576.0;

		private const double scaleOmegaA = 32768.0;

		private const double scaleTLambdaA = 32.0;

		private const double scaleDeltaTA = 512.0;

		private const double scaleDeltaTDotA = 16384.0;

		private const double scaleB1 = 1024.0;

		private const double scaleB2 = 65536.0;

		private const double scaleTb = 0.066666666666666666;

		private readonly double rad2Semi = 1.0 / Constellation.Datum.PI;

		public static NavigationDataInfo Info
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo;
			}
		}

		public override int BitRate
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo.BitRate;
			}
		}

		public NavigationDataL1OF(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> allSignals)
			: base(in satIndex, almanac, in interval, allSignals)
		{
		}

		public sealed override byte[] Generate()
		{
			DateTime glonassTime = base.Interval.Start.GlonassTime;
			DateTime dateTime = new DateTime(glonassTime.Year, glonassTime.Month, glonassTime.Day);
			int glonassSecondOfDay = base.Interval.Start.GlonassSecondOfDay;
			int num = glonassSecondOfDay / 150 * 150;
			int num5 = (glonassSecondOfDay - num) / 30;
			int num6 = num + num5 * 30;
			GnssTime transmissionTime = GnssTime.FromGlonass(dateTime.AddSeconds(num6));
			int num7 = (glonassSecondOfDay - num6) / 2;
			int num8 = num6 + num7 * 2;
			GnssTime gnssTime = GnssTime.FromGlonass(dateTime.AddSeconds(num8));
			double seconds = (base.Interval.Start - gnssTime).Seconds;
			int num9 = (int)(base.Interval.End - gnssTime).Seconds / 2 + 1;
			int num10 = num5 + 1;
			int value = num7 + 1;
			int num11 = 0;
			int num12 = 0;
			AlmanacBase almanacBase = base.Almanac;
			int satelliteIndex = base.SatelliteIndex;
			Satellite satellite = (almanacBase.GetEphemeris(in satelliteIndex, SignalType.GlonassL1OF, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Glonass", base.SatelliteIndex + 1));
			byte[] second = new byte[1] { (byte)((!satellite.IsHealthy) ? 1 : 0) };
			List<byte> list = new List<byte>(num9 * 2 * BitRate);
			for (int i = 0; i < num9; i++)
			{
				if (num11 != num12)
				{
					AlmanacBase almanacBase2 = base.Almanac;
					satelliteIndex = base.SatelliteIndex;
					satellite = (almanacBase2.GetEphemeris(in satelliteIndex, SignalType.GlonassL1OF, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Glonass", base.SatelliteIndex + 1));
					second = new byte[1] { (byte)((!satellite.IsHealthy) ? 1 : 0) };
					num12 = num11;
				}
				switch (value)
				{
				case 1:
				{
					satelliteIndex = 1;
					byte[] first3 = GloDec2Bin(in satelliteIndex, 4);
					IEnumerable<byte> second13 = NavigationData.ZeroBits.Take(2);
					int tbInterval = 0;
					byte[] p = GetP1(in tbInterval);
					DateTime glonassTime2 = transmissionTime.GlonassTime;
					satelliteIndex = glonassTime2.Hour;
					byte[] second14 = GloDec2Bin(in satelliteIndex, 5);
					satelliteIndex = glonassTime2.Minute;
					byte[] second15 = GloDec2Bin(in satelliteIndex, 6);
					satelliteIndex = glonassTime2.Second / 30;
					byte[] second16 = GloDec2Bin(in satelliteIndex, 1);
					double value2 = satellite.Velocity.X * 1048.576;
					byte[] second17 = GloDec2Bin(in value2, 24);
					value2 = satellite.Acceleration.X * 1073741.824;
					byte[] second18 = GloDec2Bin(in value2, 5);
					value2 = satellite.Position.X * 2.048;
					byte[] second19 = GloDec2Bin(in value2, 27);
					AddFirstWord(list, first3.Concat(second13).Concat(p).Concat(second14)
						.Concat(second15)
						.Concat(second16)
						.Concat(second17)
						.Concat(second18)
						.Concat(second19));
					break;
				}
				case 2:
				{
					satelliteIndex = 2;
					byte[] first2 = GloDec2Bin(in satelliteIndex, 4);
					byte[] second6 = new byte[3]
					{
						(byte)((!satellite.IsHealthy) ? 1 : 0),
						0,
						0
					};
					double num13 = satellite.TimeOfApplicability / 60;
					byte element2 = (byte)((int)num13 % 2);
					double value2 = num13 * 0.066666666666666666;
					byte[] second7 = GloDec2Bin(in value2, 7);
					IEnumerable<byte> second8 = NavigationData.ZeroBits.Take(5);
					value2 = satellite.Velocity.Y * 1048.576;
					byte[] second9 = GloDec2Bin(in value2, 24);
					value2 = satellite.Acceleration.Y * 1073741.824;
					byte[] second10 = GloDec2Bin(in value2, 5);
					value2 = satellite.Position.Y * 2.048;
					byte[] second11 = GloDec2Bin(in value2, 27);
					AddWord(list, first2.Concat(second6).Append(element2).Concat(second7)
						.Concat(second8)
						.Concat(second9)
						.Concat(second10)
						.Concat(second11));
					break;
				}
				case 3:
				{
					satelliteIndex = 3;
					byte[] source2 = GloDec2Bin(in satelliteIndex, 4);
					byte element5 = (byte)((num10 != 5) ? 1 : 0);
					double value2 = 0.0 * 1099511627776.0;
					byte[] second40 = GloDec2Bin(in value2, 11);
					byte element6 = 0;
					byte[] second41 = new byte[2] { 1, 1 };
					value2 = satellite.Velocity.Z * 1048.576;
					byte[] second42 = GloDec2Bin(in value2, 24);
					value2 = satellite.Acceleration.Z * 1073741.824;
					byte[] second43 = GloDec2Bin(in value2, 5);
					value2 = satellite.Position.Z * 2.048;
					byte[] second44 = GloDec2Bin(in value2, 27);
					AddWord(list, source2.Append(element5).Concat(second40).Append(element6)
						.Concat(second41)
						.Concat(second)
						.Concat(second42)
						.Concat(second43)
						.Concat(second44));
					break;
				}
				case 4:
				{
					satelliteIndex = 4;
					byte[] first7 = GloDec2Bin(in satelliteIndex, 4);
					double value2 = satellite.SatelliteClockBiasAtEphemerisTime * 1073741824.0;
					byte[] second46 = GloDec2Bin(in value2, 22);
					value2 = 0.0 * 1073741824.0;
					byte[] second47 = GloDec2Bin(in value2, 5);
					double value7 = 0.0;
					byte[] second48 = GloDec2Bin(in value7, 5);
					IEnumerable<byte> second49 = NavigationData.ZeroBits.Take(14);
					byte element7 = 1;
					value2 = 2.0;
					satelliteIndex = GetFt(in value2);
					byte[] second50 = GloDec2Bin(in satelliteIndex, 4);
					IEnumerable<byte> second51 = NavigationData.ZeroBits.Take(3);
					satelliteIndex = transmissionTime.GlonassFourYearPeriodDayNumber;
					byte[] second52 = GloDec2Bin(in satelliteIndex, 11);
					satelliteIndex = satellite.Id;
					byte[] second53 = GloDec2Bin(in satelliteIndex, 5);
					byte[] second54 = new byte[2]
					{
						0,
						(byte)(satellite.IsGlonassMSatellite ? 1 : 0)
					};
					AddWord(list, first7.Concat(second46).Concat(second47).Concat(second48)
						.Concat(second49)
						.Append(element7)
						.Concat(second50)
						.Concat(second51)
						.Concat(second52)
						.Concat(second53)
						.Concat(second54));
					break;
				}
				case 5:
				{
					satelliteIndex = 5;
					byte[] first6 = GloDec2Bin(in satelliteIndex, 4);
					Satellite satellite6 = base.Almanac.BaselineSatellites.Select((SatelliteBase s) => s as Satellite).FirstOrDefault((Satellite s) => s?.IsHealthy ?? false);
					satelliteIndex = satellite6?.TimeOfAscendingNode.GlonassFourYearPeriodDayNumber ?? 0;
					byte[] second36 = GloDec2Bin(in satelliteIndex, 11);
					double value2 = (satellite6?.UtcTimeCorrection ?? 0.0) * 2147483648.0;
					byte[] second37 = GloDec2Bin(in value2, 32);
					byte element4 = 0;
					satelliteIndex = transmissionTime.GlonassFourYearPeriodNumber;
					byte[] second38 = GloDec2Bin(in satelliteIndex, 5);
					value2 = (satellite6?.GpsTimeCorrection ?? 0.0) * 1073741824.0;
					byte[] second39 = GloDec2Bin(in value2, 22);
					AddWord(list, first6.Concat(second36).Concat(second37).Append(element4)
						.Concat(second38)
						.Concat(second39)
						.Concat(second));
					break;
				}
				case 6:
				case 8:
				case 10:
				case 12:
				{
					int value4 = (int)(((long)num10 - 1L) * 5 + ((long)value - 6L) / 2 + 1);
					AlmanacBase almanacBase4 = base.Almanac;
					satelliteIndex = value4 - 1;
					Satellite satellite3 = (almanacBase4.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
					{
						Id = value4,
						IsHealthy = false
					};
					byte[] source = GloDec2Bin(in value, 4);
					byte element = (byte)(satellite3.IsHealthy ? 1 : 0);
					byte[] second57 = new byte[2]
					{
						0,
						(byte)(satellite3.IsGlonassMSatellite ? 1 : 0)
					};
					byte[] second58 = GloDec2Bin(in value4, 5);
					double value2 = satellite3.GlonassTimeCorrection * 262144.0;
					byte[] second2 = GloDec2Bin(in value2, 10);
					value2 = satellite3.LongitudeOfAscendingNode * rad2Semi * 1048576.0;
					byte[] second3 = GloDec2Bin(in value2, 21);
					value2 = satellite3.InclinationCorrection * rad2Semi * 1048576.0;
					byte[] second4 = GloDec2Bin(in value2, 18);
					value2 = satellite3.Eccentricity * 1048576.0;
					byte[] second5 = GloDec2Bin(in value2, 15);
					AddWord(list, source.Append(element).Concat(second57).Concat(second58)
						.Concat(second2)
						.Concat(second3)
						.Concat(second4)
						.Concat(second5));
					break;
				}
				case 7:
				case 9:
				case 11:
				case 13:
				{
					int num3 = (int)(((long)num10 - 1L) * 5 + ((long)value - 7L) / 2 + 1);
					AlmanacBase almanacBase6 = base.Almanac;
					satelliteIndex = num3 - 1;
					Satellite satellite5 = (almanacBase6.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
					{
						Id = num3,
						IsHealthy = false
					};
					byte[] first5 = GloDec2Bin(in value, 4);
					double value2 = satellite5.ArgumentOfPerigee * rad2Semi * 32768.0;
					byte[] second30 = GloDec2Bin(in value2, 16);
					value2 = (satellite5.IsHealthy ? ((double)satellite5.TimeOfAscendingNode.GlonassSecondOfDayDecimal) : 0.0) * 32.0;
					byte[] second31 = GloDec2Bin(in value2, 21);
					value2 = satellite5.DraconicPeriodCorrection * 512.0;
					byte[] second32 = GloDec2Bin(in value2, 22);
					value2 = satellite5.DraconicPeriodCorrectionRate * 16384.0;
					byte[] second33 = GloDec2Bin(in value2, 7);
					int value6 = ((satellite5.Slot < 0) ? (32 + satellite5.Slot) : satellite5.Slot);
					byte[] second35 = GloDec2Bin(in value6, 5);
					AddWord(list, first5.Concat(second30).Concat(second31).Concat(second32)
						.Concat(second33)
						.Concat(second35)
						.Concat(second));
					break;
				}
				case 14:
				{
					satelliteIndex = 14;
					byte[] first4 = GloDec2Bin(in satelliteIndex, 4);
					switch (num10)
					{
					case 1:
					case 2:
					case 3:
					case 4:
					{
						int value5 = (int)(((long)num10 - 1L) * 5 + ((long)value - 6L) / 2 + 1);
						AlmanacBase almanacBase5 = base.Almanac;
						satelliteIndex = value5 - 1;
						Satellite satellite4 = (almanacBase5.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
						{
							Id = value5,
							IsHealthy = false
						};
						first4 = GloDec2Bin(in value, 4);
						byte element3 = 1;
						byte[] second24 = new byte[2]
						{
							0,
							(byte)(satellite4.IsGlonassMSatellite ? 1 : 0)
						};
						byte[] second25 = GloDec2Bin(in value5, 5);
						double value2 = satellite4.GlonassTimeCorrection * 262144.0;
						byte[] second26 = GloDec2Bin(in value2, 10);
						value2 = satellite4.LongitudeOfAscendingNode * rad2Semi * 1048576.0;
						byte[] second27 = GloDec2Bin(in value2, 21);
						value2 = satellite4.InclinationCorrection * rad2Semi * 1048576.0;
						byte[] second28 = GloDec2Bin(in value2, 18);
						value2 = satellite4.Eccentricity * 1048576.0;
						byte[] second29 = GloDec2Bin(in value2, 15);
						AddWord(list, first4.Append(element3).Concat(second24).Concat(second25)
							.Concat(second26)
							.Concat(second27)
							.Concat(second28)
							.Concat(second29));
						break;
					}
					case 5:
					{
						double value2 = 0.0 * 1024.0;
						byte[] second20 = GloDec2Bin(in value2, 11);
						value2 = 0.0 * 65536.0;
						byte[] second21 = GloDec2Bin(in value2, 10);
						byte[] leapSecondPredictionBits = GetLeapSecondPredictionBits(in transmissionTime);
						IEnumerable<byte> second22 = NavigationData.ZeroBits.Take(49);
						AddWord(list, first4.Concat(second20).Concat(second21).Concat(leapSecondPredictionBits)
							.Concat(second22));
						break;
					}
					}
					break;
				}
				case 15:
				{
					GnssTime gnssTime2 = transmissionTime + GnssTimeSpan.FromSeconds((value - 1) % 15 * 2);
					LeapSecond leapSecond = LeapSecond.NextLeapSecondsAfterDate(gnssTime2.UtcTime);
					if (gnssTime2.UtcTime == leapSecond.Utc - TimeSpan.FromSeconds(2.0))
					{
						isLeapSecondInsertedNow = true;
					}
					satelliteIndex = 15;
					byte[] first = GloDec2Bin(in satelliteIndex, 4);
					switch (num10)
					{
					case 1:
					case 2:
					case 3:
					case 4:
					{
						int num2 = (int)(((long)num10 - 1L) * 5 + ((long)value - 7L) / 2 + 1);
						AlmanacBase almanacBase3 = base.Almanac;
						satelliteIndex = num2 - 1;
						Satellite satellite2 = (almanacBase3.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
						{
							Id = num2,
							IsHealthy = false
						};
						double value2 = satellite2.ArgumentOfPerigee * rad2Semi * 32768.0;
						byte[] second23 = GloDec2Bin(in value2, 16);
						value2 = (satellite2.IsHealthy ? ((double)satellite2.TimeOfAscendingNode.GlonassSecondOfDayDecimal) : 0.0) * 32.0;
						byte[] second34 = GloDec2Bin(in value2, 21);
						value2 = satellite2.DraconicPeriodCorrection * 512.0;
						byte[] second45 = GloDec2Bin(in value2, 22);
						value2 = satellite2.DraconicPeriodCorrectionRate * 16384.0;
						byte[] second55 = GloDec2Bin(in value2, 7);
						int value3 = ((satellite2.Slot < 0) ? (32 + satellite2.Slot) : satellite2.Slot);
						byte[] second56 = GloDec2Bin(in value3, 5);
						AddWord(list, first.Concat(second23).Concat(second34).Concat(second45)
							.Concat(second55)
							.Concat(second56)
							.Concat(second));
						break;
					}
					case 5:
					{
						IEnumerable<byte> second12 = NavigationData.ZeroBits.Take(71);
						AddWord(list, first.Concat(second12).Concat(second));
						break;
					}
					}
					isLeapSecondInsertedNow = false;
					break;
				}
				}
				value++;
				if (value > 15)
				{
					value = 1;
					num10++;
					if (num10 > 5)
					{
						num10 = 1;
						num11++;
					}
					transmissionTime += frameLengthTimeSpan;
				}
			}
			int index = (int)Math.Round(seconds * (double)Info.BitRate);
			int num4 = (int)Math.Round(base.Interval.Width.Seconds * (double)Info.BitRate);
			byte[] array = new byte[num4];
			list.CopyTo(index, array, 0, num4);
			return array;
		}

		private static byte[] GetLeapSecondPredictionBits(in GnssTime currentTime)
		{
			LeapSecond leapSecond = LeapSecond.NextLeapSecondsAfterDate(currentTime.UtcTime);
			double totalDays = (leapSecond.Utc - currentTime.UtcTime).TotalDays;
			if (totalDays > 0.0 && ((leapSecond.Utc.Month == 1 && totalDays <= 92.0) || (leapSecond.Utc.Month == 7 && totalDays <= 91.0)))
			{
				if (totalDays <= 56.0)
				{
					if (leapSecond.Seconds - LeapSecond.LeapSecondsForDate(currentTime.UtcTime).Seconds > 0)
					{
						return new byte[2] { 0, 1 };
					}
					return new byte[2] { 1, 1 };
				}
				return new byte[2] { 1, 0 };
			}
			return new byte[2];
		}

		private static int GetFt(in double measurementAccuracy)
		{
			if (Math.Abs(measurementAccuracy - 1.0) < 1E-09)
			{
				return 0;
			}
			if (Math.Abs(measurementAccuracy - 2.0) < 1E-09)
			{
				return 1;
			}
			if (Math.Abs(measurementAccuracy - 2.5) < 1E-09)
			{
				return 2;
			}
			if (Math.Abs(measurementAccuracy - 4.0) < 1E-09)
			{
				return 3;
			}
			if (Math.Abs(measurementAccuracy - 5.0) < 1E-09)
			{
				return 4;
			}
			if (Math.Abs(measurementAccuracy - 7.0) < 1E-09)
			{
				return 5;
			}
			if (Math.Abs(measurementAccuracy - 10.0) < 1E-09)
			{
				return 6;
			}
			if (Math.Abs(measurementAccuracy - 12.0) < 1E-09)
			{
				return 7;
			}
			if (Math.Abs(measurementAccuracy - 14.0) < 1E-09)
			{
				return 8;
			}
			if (Math.Abs(measurementAccuracy - 16.0) < 1E-09)
			{
				return 9;
			}
			if (Math.Abs(measurementAccuracy - 32.0) < 1E-09)
			{
				return 10;
			}
			if (Math.Abs(measurementAccuracy - 64.0) < 1E-09)
			{
				return 11;
			}
			if (Math.Abs(measurementAccuracy - 128.0) < 1E-09)
			{
				return 12;
			}
			if (Math.Abs(measurementAccuracy - 256.0) < 1E-09)
			{
				return 13;
			}
			if (Math.Abs(measurementAccuracy - 512.0) < 1E-09)
			{
				return 14;
			}
			return 15;
		}

		private static byte[] GetP1(in int tbInterval)
		{
			if (tbInterval == 30)
			{
				return new byte[2] { 0, 1 };
			}
			if (tbInterval == 45)
			{
				return new byte[2] { 1, 0 };
			}
			if (tbInterval == 60)
			{
				return new byte[2] { 1, 1 };
			}
			return new byte[2];
		}

		protected sealed override IEnumerable<byte> EncodeWord(IEnumerable<byte> rawWord)
		{
			byte[] array = new byte[77];
			byte[] array2 = new byte[77];
			using (IEnumerator<byte> enumerator = rawWord.GetEnumerator())
			{
				for (int i = 1; i < 77; i++)
				{
					enumerator.MoveNext();
					array2[76 - i] = (array[i] = enumerator.Current);
				}
			}
			IEnumerable<byte> second = HammingCoding.GetCheckBits(array2).Reverse();
			IEnumerable<byte> first = RelativeBiBinaryCoding.Encode(array.Concat(second));
			if (isLeapSecondInsertedNow)
			{
				first = first.Concat(NavigationData.ZeroBits.Take(100));
			}
			return first.Concat(timeMarkBits);
		}

		private static byte[] GloDec2Bin(in int value, int bits)
		{
			byte[] array = new byte[bits];
			uint num = (uint)Math.Abs(value);
			for (int num2 = bits; num2 > 0; num2--)
			{
				array[--bits] = (byte)(num & 1u);
				num >>= 1;
			}
			if (value < 0)
			{
				array[0] = 1;
			}
			return array;
		}

		private static byte[] GloDec2Bin(in double value, int bits)
		{
			byte[] array = new byte[bits];
			uint num = (uint)Math.Abs(value);
			for (int num2 = bits; num2 > 0; num2--)
			{
				array[--bits] = (byte)(num & 1u);
				num >>= 1;
			}
			if (value < 0.0)
			{
				array[0] = 1;
			}
			return array;
		}
	}
}
