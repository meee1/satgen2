using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo
{
	internal sealed class NavigationDataE5a : NavigationDataINavFNav
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE5a);

		private static readonly SignalType signalType = Signal.AllSignals.First((Signal s) => s.NavigationDataInfos.Any((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE5a)).SignalType;

		private const int frameLength = 600;

		private const int subFramesPerFrame = 12;

		private const int subframeLength = 50;

		private const int pagesPerSubframe = 5;

		private const int pageLength = 10;

		private static readonly GnssTimeSpan pageLengthSpan = GnssTimeSpan.FromSeconds(10);

		private static readonly byte[] syncBits = new byte[12]
		{
			1, 0, 1, 1, 0, 1, 1, 1, 0, 0,
			0, 0
		};

		private static readonly BlockInterleaver<byte> blockInterleaver;

		public static NavigationDataInfo Info
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo;
			}
		}

		protected override SignalType SignalType
		{
			[DebuggerStepThrough]
			get
			{
				return signalType;
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

		public NavigationDataE5a(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
			: base(in satIndex, almanac, in interval, simulatedSignals)
		{
		}

		public override byte[] Generate()
		{
			int galileoNavicSecondOfWeek = base.Interval.Start.GalileoNavicSecondOfWeek;
			int galileoNavicWeek = base.Interval.Start.GalileoNavicWeek;
			int subframeIndex = (galileoNavicSecondOfWeek / 50 + (base.SatelliteIndex & 1)) % 12;
			int num5 = galileoNavicSecondOfWeek / 10;
			int num2 = num5 % 5;
			int second = num5 * 10;
			GnssTime transmissionTime = GnssTime.FromGalileoNavic(galileoNavicWeek, second);
			int num3 = (int)((base.Interval.End - transmissionTime).Seconds / 10.0).SafeCeiling();
			int index = (int)Math.Round((base.Interval.Start - transmissionTime).Seconds * (double)BitRate);
			List<byte> list = new List<byte>(num3 * 10 * BitRate);
			for (int i = 0; i < num3; i++)
			{
				AlmanacBase almanacBase = base.Almanac;
				int satelliteIndex = base.SatelliteIndex;
				Satellite ephemeris = (almanacBase.GetEphemeris(in satelliteIndex, SignalType, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Galileo", base.SatelliteIndex + 1));
				PageId pageId = (PageId)(num2 + 1);
				if (pageId == PageId.Page5 && ((uint)subframeIndex & (true ? 1u : 0u)) != 0)
				{
					pageId = PageId.Page6;
				}
				byte[] array3 = GetPageData(in transmissionTime, pageId, in subframeIndex, ephemeris).ToArray(214);
				IEnumerable<byte> second2 = CRC24Q.ComputeBytes(array3);
				IEnumerable<byte> inputSequence = array3.Concat(second2).Concat(NavigationDataINavFNav.tailBits);
				satelliteIndex = 0;
				int captureIndex = 0;
				byte[] data = new ConvolutionalEncoder(inputSequence, in satelliteIndex, in captureIndex, ConvolutionalEncoderOptions.NegateG2).ToArray(488);
				IEnumerable<byte> second3 = blockInterleaver.Interleave(data);
				list.AddRange(syncBits.Concat(second3));
				transmissionTime += pageLengthSpan;
				if (++num2 >= 5)
				{
					num2 = 0;
					if (++subframeIndex >= 12)
					{
						subframeIndex = 0;
					}
				}
			}
			int num4 = (int)Math.Round(base.Interval.Width.Seconds * (double)BitRate);
			byte[] array2 = new byte[num4];
			list.CopyTo(index, array2, 0, num4);
			return array2;
		}

		private IEnumerable<byte> GetPageData(in GnssTime pageTime, PageId pageId, in int subframeIndex, Satellite ephemeris)
		{
			uint value = (uint)pageId;
			int bitCount = 6;
			byte[] first = NavigationData.Dec2Bin(in value, in bitCount);
			switch (pageId)
			{
			case PageId.Page1:
			{
				bitCount = base.SatelliteId;
				int bitCount2 = 6;
				byte[] second33 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = ephemeris.TimeOfApplicability;
				byte[] iodNavBits = NavigationDataINavFNav.GetIodNavBits(in bitCount);
				double value4 = (double)ephemeris.TimeOfApplicability * 0.016666666666666666;
				bitCount = 14;
				byte[] second35 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.Af0 * 17179869184.0;
				bitCount = 31;
				byte[] second36 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.Af1 * 70368744177664.0;
				bitCount = 21;
				byte[] second37 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.Af2 * 5.7646075230342349E+17;
				bitCount = 6;
				byte[] second38 = NavigationData.Dec2Bin(in value4, in bitCount);
				byte[] second39 = NavigationDataINavFNav.sisaBits;
				IEnumerable<byte> second40 = NavigationData.ZeroBits.Take(5);
				IEnumerable<byte> second41 = NavigationData.ZeroBits.Take(10);
				value = ((!ephemeris.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second42 = NavigationData.Dec2Bin(in value, in bitCount);
				bitCount = pageTime.GalileoNavicWeek;
				bitCount2 = 12;
				byte[] second43 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pageTime.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second44 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				byte b = (byte)((!ephemeris.IsHealthy) ? 1 : 0);
				byte[] second46 = new byte[1] { b };
				IEnumerable<byte> second47 = NavigationData.OneAndZeroBits.Take(26);
				return first.Concat(second33).Concat(iodNavBits).Concat(second35)
					.Concat(second36)
					.Concat(second37)
					.Concat(second38)
					.Concat(second39)
					.Concat(NavigationDataINavFNav.ai0Bits)
					.Concat(NavigationDataINavFNav.ai1Bits)
					.Concat(NavigationDataINavFNav.ai2Bits)
					.Concat(second40)
					.Concat(second41)
					.Concat(second42)
					.Concat(second43)
					.Concat(second44)
					.Concat(second46)
					.Concat(second47);
			}
			case PageId.Page2:
			{
				bitCount = ephemeris.TimeOfApplicability;
				byte[] iodNavBits2 = NavigationDataINavFNav.GetIodNavBits(in bitCount);
				double value4 = ephemeris.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second48 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 24;
				byte[] second49 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.Eccentricity * 8589934592.0;
				bitCount = 32;
				byte[] second50 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.SqrtA * 524288.0;
				bitCount = 32;
				byte[] second51 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second52 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.RateOfInclination * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 14;
				byte[] second53 = NavigationData.Dec2Bin(in value4, in bitCount);
				bitCount = pageTime.GalileoNavicWeek;
				int bitCount2 = 12;
				byte[] second54 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pageTime.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second55 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				return first.Concat(iodNavBits2).Concat(second48).Concat(second49)
					.Concat(second50)
					.Concat(second51)
					.Concat(second52)
					.Concat(second53)
					.Concat(second54)
					.Concat(second55);
			}
			case PageId.Page3:
			{
				int t0e = ephemeris.TimeOfApplicability;
				byte[] iodNavBits3 = NavigationDataINavFNav.GetIodNavBits(in t0e);
				double value4 = ephemeris.Inclination * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second56 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second57 = NavigationData.Dec2Bin(in value4, in bitCount);
				value4 = ephemeris.MeanMotionCorrection * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 16;
				byte[] second58 = NavigationData.Dec2Bin(in value4, in bitCount);
				IEnumerable<byte> second59 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second60 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second61 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second62 = NavigationData.ZeroBits.Take(16);
				value4 = (double)t0e * 0.016666666666666666;
				bitCount = 14;
				byte[] second63 = NavigationData.Dec2Bin(in value4, in bitCount);
				bitCount = pageTime.GalileoNavicWeek;
				int bitCount2 = 12;
				byte[] second64 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pageTime.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second65 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				IEnumerable<byte> second67 = NavigationData.ZeroAndOneBits.Take(8);
				return first.Concat(iodNavBits3).Concat(second56).Concat(second57)
					.Concat(second58)
					.Concat(second59)
					.Concat(second60)
					.Concat(second61)
					.Concat(second62)
					.Concat(second63)
					.Concat(second64)
					.Concat(second65)
					.Concat(second67);
			}
			case PageId.Page4:
			{
				bitCount = ephemeris.TimeOfApplicability;
				byte[] iodNavBits4 = NavigationDataINavFNav.GetIodNavBits(in bitCount);
				IEnumerable<byte> second68 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second69 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second70 = NavigationData.ZeroBits.Take(32);
				IEnumerable<byte> second71 = NavigationData.ZeroBits.Take(24);
				GnssTime start = ephemeris.TransmissionInterval.Start;
				DateTime utcTime = start.UtcTime;
				LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime);
				int value7 = leapSecond.Seconds;
				LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime);
				GnssTime gnssTime = GnssTime.FromUtc(leapSecond2.Utc);
				if ((int)(gnssTime - start).Seconds > 15552000)
				{
					leapSecond2 = leapSecond;
					gnssTime = GnssTime.FromUtc(leapSecond.Utc);
				}
				GnssTime gnssTime2 = gnssTime - GnssTimeSpan.FromMinutes(1);
				int value8 = gnssTime2.GalileoNavicDayOfWeek + 1;
				int value9 = gnssTime2.GalileoNavicWeek;
				int value10 = leapSecond2.Seconds;
				bitCount = 8;
				byte[] second72 = NavigationData.Dec2Bin(in value7, in bitCount);
				double value4 = (double)(start.GalileoNavicDayOfWeek * 86400) * 0.00027777777777777778;
				bitCount = 8;
				byte[] second73 = NavigationData.Dec2Bin(in value4, in bitCount);
				bitCount = start.GalileoNavicWeek;
				int bitCount2 = 8;
				byte[] second74 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = 8;
				byte[] second75 = NavigationData.Dec2Bin(in value9, in bitCount);
				bitCount = 3;
				byte[] second76 = NavigationData.Dec2Bin(in value8, in bitCount);
				bitCount = 8;
				byte[] second78 = NavigationData.Dec2Bin(in value10, in bitCount);
				IEnumerable<byte> second79 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second80 = NavigationData.ZeroBits.Take(12);
				GnssTime gnssTime3 = ephemeris.TransmissionInterval.Start + GnssTimeSpan.FromDays(1);
				int num = gnssTime3.GalileoNavicDayOfWeek * 86400;
				int value2 = gnssTime3.GalileoNavicWeek;
				value4 = (double)num * 0.00027777777777777778;
				bitCount = 8;
				byte[] second81 = NavigationData.Dec2Bin(in value4, in bitCount);
				bitCount = 6;
				byte[] second82 = NavigationData.Dec2Bin(in value2, in bitCount);
				bitCount = pageTime.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second83 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				IEnumerable<byte> second84 = NavigationData.ZeroAndOneBits.Take(5);
				return first.Concat(iodNavBits4).Concat(second68).Concat(second69)
					.Concat(second70)
					.Concat(second71)
					.Concat(second72)
					.Concat(second73)
					.Concat(second74)
					.Concat(second75)
					.Concat(second76)
					.Concat(second78)
					.Concat(second81)
					.Concat(second79)
					.Concat(second80)
					.Concat(second82)
					.Concat(second83)
					.Concat(second84);
			}
			case PageId.Page5:
			{
				AlmanacBase almanacBase4 = base.Almanac;
				bitCount = 0;
				GnssTime timeOfAlmanac = almanacBase4.GetTimeOfAlmanac(in pageTime, in bitCount);
				int galileoNavicSecondOfWeek = timeOfAlmanac.GalileoNavicSecondOfWeek;
				int value5 = timeOfAlmanac.GalileoNavicWeek;
				int value6 = galileoNavicSecondOfWeek / 600;
				bitCount = 4;
				byte[] second13 = NavigationData.Dec2Bin(in value6, in bitCount);
				bitCount = 2;
				byte[] second14 = NavigationData.Dec2Bin(in value5, in bitCount);
				double value4 = (double)galileoNavicSecondOfWeek * 0.0016666666666666668;
				bitCount = 10;
				byte[] second15 = NavigationData.Dec2Bin(in value4, in bitCount);
				int firstAlmanacIndex2 = GetFirstAlmanacIndex(in subframeIndex);
				Satellite satellite3 = base.Almanac.GetAlmanac(in firstAlmanacIndex2, in pageTime) as Satellite;
				IEnumerable<byte> second27;
				if (satellite3 != null)
				{
					bitCount = satellite3.Id;
					int bitCount2 = 6;
					byte[] first3 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					value4 = satellite3.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second16 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second17 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second18 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second19 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second20 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second21 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second22 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.Af0 * 524288.0;
					bitCount = 16;
					byte[] second24 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite3.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second25 = NavigationData.Dec2Bin(in value4, in bitCount);
					value = ((!satellite3.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					byte[] second26 = NavigationData.Dec2Bin(in value, in bitCount);
					second27 = first3.Concat(second16).Concat(second17).Concat(second18)
						.Concat(second19)
						.Concat(second20)
						.Concat(second21)
						.Concat(second22)
						.Concat(second24)
						.Concat(second25)
						.Concat(second26);
				}
				else
				{
					second27 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(125));
				}
				AlmanacBase almanacBase5 = base.Almanac;
				bitCount = firstAlmanacIndex2 + 1;
				Satellite satellite4 = almanacBase5.GetAlmanac(in bitCount, in pageTime) as Satellite;
				IEnumerable<byte> second32;
				if (satellite4 != null)
				{
					bitCount = satellite4.Id;
					int bitCount2 = 6;
					byte[] first4 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					value4 = satellite4.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second28 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite4.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second29 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite4.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second30 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite4.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second31 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite4.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] source2 = NavigationData.Dec2Bin(in value4, in bitCount);
					second32 = first4.Concat(second28).Concat(second29).Concat(second30)
						.Concat(second31)
						.Concat(source2.Take(4));
				}
				else
				{
					second32 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(55));
				}
				return first.Concat(second13).Concat(second14).Concat(second15)
					.Concat(second27)
					.Concat(second32);
			}
			case PageId.Page6:
			{
				AlmanacBase almanacBase = base.Almanac;
				bitCount = 0;
				int value3 = almanacBase.GetTimeOfAlmanac(in pageTime, in bitCount).GalileoNavicSecondOfWeek / 600;
				bitCount = 4;
				byte[] second = NavigationData.Dec2Bin(in value3, in bitCount);
				int firstAlmanacIndex = GetFirstAlmanacIndex(in subframeIndex);
				AlmanacBase almanacBase2 = base.Almanac;
				bitCount = firstAlmanacIndex + 1;
				Satellite satellite = almanacBase2.GetAlmanac(in bitCount, in pageTime) as Satellite;
				IEnumerable<byte> second66;
				if (satellite != null)
				{
					double value4 = satellite.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] source3 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second12 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second23 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite.Af0 * 524288.0;
					bitCount = 16;
					byte[] second34 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second45 = NavigationData.Dec2Bin(in value4, in bitCount);
					value = ((!satellite.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					second66 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: source3.Skip(4).Take(12).Concat(second12)
						.Concat(second23)
						.Concat(second34)
						.Concat(second45));
				}
				else
				{
					second66 = NavigationData.ZeroAndOneBits.Take(70);
				}
				AlmanacBase almanacBase3 = base.Almanac;
				bitCount = firstAlmanacIndex + 2;
				Satellite satellite2 = almanacBase3.GetAlmanac(in bitCount, in pageTime) as Satellite;
				IEnumerable<byte> second10;
				if (satellite2 != null)
				{
					bitCount = satellite2.Id;
					int bitCount2 = 6;
					byte[] first2 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					double value4 = satellite2.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second77 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second85 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second2 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second3 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second4 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second5 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second6 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.Af0 * 524288.0;
					bitCount = 16;
					byte[] second7 = NavigationData.Dec2Bin(in value4, in bitCount);
					value4 = satellite2.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second8 = NavigationData.Dec2Bin(in value4, in bitCount);
					value = ((!satellite2.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					byte[] second9 = NavigationData.Dec2Bin(in value, in bitCount);
					second10 = first2.Concat(second77).Concat(second85).Concat(second2)
						.Concat(second3)
						.Concat(second4)
						.Concat(second5)
						.Concat(second6)
						.Concat(second7)
						.Concat(second8)
						.Concat(second9);
				}
				else
				{
					second10 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(125));
				}
				IEnumerable<byte> second11 = NavigationData.ZeroAndOneBits.Take(3);
				return first.Concat(second).Concat(second66).Concat(second10)
					.Concat(second11);
			}
			default:
				return first.Concat(NavigationData.ZeroAndOneBits.Take(208));
			}
		}

		protected override int GetFirstAlmanacIndex(in int subframeIndex)
		{
			int num = (base.SatelliteIndex / 3 + (subframeIndex >> 1)) * 3;
			if (num > 33)
			{
				num -= 36;
			}
			return num;
		}

		static NavigationDataE5a()
		{
			int columns = 61;
			int rows = 8;
			blockInterleaver = new BlockInterleaver<byte>(in columns, in rows);
		}
	}
}
