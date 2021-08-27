using System;
using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo
{
	internal abstract class NavigationDataINav : NavigationDataINavFNav
	{
		protected const int frameLength = 720;

		protected const int subFramesPerFrame = 24;

		protected const int subframeLength = 30;

		protected const int pagesPerSubframe = 15;

		protected const int pageLength = 2;

		private static readonly GnssTimeSpan pageLengthSpan = GnssTimeSpan.FromSeconds(2);

		private static readonly byte[] syncBits = new byte[10] { 0, 1, 0, 1, 1, 0, 0, 0, 0, 0 };

		private const byte evenPagePartBit = 0;

		private const byte oddPagePartBit = 1;

		private const byte nominalPageTypeBit = 0;

		private static readonly byte[] timeFlagsBits = new byte[2] { 1, 0 };

		private static readonly BlockInterleaver<byte> blockInterleaver;

		private readonly bool e1BPresent = true;

		private readonly bool e5bPresent = true;

		protected abstract WordType[] PageFormats { get; }

		protected NavigationDataINav(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
			: base(in satIndex, almanac, in interval, simulatedSignals)
		{
		}

		public override byte[] Generate()
		{
			int galileoNavicSecondOfWeek = base.Interval.Start.GalileoNavicSecondOfWeek;
			int galileoNavicWeek = base.Interval.Start.GalileoNavicWeek;
			int subframeIndex = galileoNavicSecondOfWeek / 30 % 24;
			int num4 = galileoNavicSecondOfWeek / 2;
			int pageIndex = num4 % 15;
			int second = num4 * 2;
			GnssTime transmissionTime = GnssTime.FromGalileoNavic(galileoNavicWeek, second);
			int num2 = (int)((base.Interval.End - transmissionTime).Seconds / 2.0).SafeCeiling();
			int index = (int)Math.Round((base.Interval.Start - transmissionTime).Seconds * (double)BitRate);
			List<byte> list = new List<byte>(num2 * 2 * BitRate);
			PagePartParams pagePartParams = null;
			IEnumerable<byte>[] array = null;
			for (int i = 0; i < num2; i++)
			{
				AlmanacBase almanacBase = base.Almanac;
				int satelliteIndex = base.SatelliteIndex;
				Satellite ephemeris = (almanacBase.GetEphemeris(in satelliteIndex, SignalType, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Galileo", base.SatelliteIndex + 1));
				PagePartParams firstPagePartParameters = GetFirstPagePartParameters(pageIndex, subframeIndex, in transmissionTime);
				IEnumerable<byte>[] array2 = ((firstPagePartParameters.PageIndex == (pagePartParams?.PageIndex ?? (-1))) ? array : GetPageParts(firstPagePartParameters, ephemeris));
				list.AddRange(array2[firstPagePartParameters.PagePartIndex]);
				PagePartParams secondPagePartParameters = GetSecondPagePartParameters(in pageIndex, in subframeIndex, in transmissionTime);
				IEnumerable<byte>[] array3;
				if (secondPagePartParameters.PageIndex == firstPagePartParameters.PageIndex)
				{
					array3 = array2;
					pagePartParams = null;
					array = null;
				}
				else
				{
					array3 = GetPageParts(secondPagePartParameters, ephemeris);
					pagePartParams = secondPagePartParameters;
					array = array3;
				}
				list.AddRange(array3[secondPagePartParameters.PagePartIndex]);
				transmissionTime += pageLengthSpan;
				if (++pageIndex >= 15)
				{
					pageIndex = 0;
					if (++subframeIndex >= 24)
					{
						subframeIndex = 0;
					}
				}
			}
			int num3 = (int)Math.Round(base.Interval.Width.Seconds * (double)BitRate);
			byte[] array4 = new byte[num3];
			list.CopyTo(index, array4, 0, num3);
			return array4;
		}

		private IEnumerable<byte>[] GetPageParts(PagePartParams pagePartParams, Satellite ephemeris)
		{
			byte[] array = GetPageData(pagePartParams, ephemeris).ToArray(128);
			byte[] first = Enumerable.Concat(second: new ArraySegment<byte>(array, 0, 112), first: new byte[2]).ToArray(114);
			IEnumerable<byte> inputSequence = first.Concat(NavigationDataINavFNav.tailBits);
			int registerState = 0;
			int captureIndex = 0;
			byte[] data = new ConvolutionalEncoder(inputSequence, in registerState, in captureIndex, ConvolutionalEncoderOptions.NegateG2).ToArray(240);
			IEnumerable<byte> second2 = blockInterleaver.Interleave(data);
			IEnumerable<byte> enumerable = syncBits.Concat(second2);
			IEnumerable<byte> reservedBits = GetReservedBits1(pagePartParams);
			IEnumerable<byte> reservedBits2 = GetReservedBits2();
			byte[] array2 = Enumerable.Concat(second: new ArraySegment<byte>(array, 112, 16), first: new byte[2] { 1, 0 }).Concat(reservedBits).ToArray(82);
			IEnumerable<byte> second3 = CRC24Q.ComputeBytes(first.Concat(array2).ToArray(196));
			IEnumerable<byte> inputSequence2 = array2.Concat(second3).Concat(reservedBits2).Concat(NavigationDataINavFNav.tailBits);
			registerState = 0;
			captureIndex = 0;
			byte[] data2 = new ConvolutionalEncoder(inputSequence2, in registerState, in captureIndex, ConvolutionalEncoderOptions.NegateG2).ToArray(240);
			IEnumerable<byte> second4 = blockInterleaver.Interleave(data2);
			IEnumerable<byte> enumerable2 = syncBits.Concat(second4);
			return new IEnumerable<byte>[2] { enumerable, enumerable2 };
		}

		private IEnumerable<byte> GetPageData(PagePartParams pagePartParams, Satellite ephemeris)
		{
			WordType wordType = PageFormats[pagePartParams.PageIndex];
			if (wordType >= WordType.Unresolved)
			{
				wordType -= 100;
				if (((uint)pagePartParams.SubframeIndex & (true ? 1u : 0u)) != 0)
				{
					wordType += 2;
				}
			}
			uint value = (uint)wordType;
			int bitCount = 6;
			byte[] first = NavigationData.Dec2Bin(in value, in bitCount);
			switch (wordType)
			{
			case WordType.Spare:
			{
				IEnumerable<byte> second66 = NavigationData.ZeroAndOneBits.Take(88);
				GnssTime pageTime = pagePartParams.PageTime;
				bitCount = pageTime.GalileoNavicWeek;
				int bitCount2 = 12;
				byte[] second67 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pageTime.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second68 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				return first.Concat(timeFlagsBits).Concat(second66).Concat(second67)
					.Concat(second68);
			}
			case WordType.Word1:
			{
				int t0e2 = ephemeris.TimeOfApplicability;
				byte[] iodNavBits4 = NavigationDataINavFNav.GetIodNavBits(in t0e2);
				double value6 = (double)t0e2 * 0.016666666666666666;
				bitCount = 14;
				byte[] second69 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second70 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.Eccentricity * 8589934592.0;
				bitCount = 32;
				byte[] second71 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.SqrtA * 524288.0;
				bitCount = 32;
				byte[] second72 = NavigationData.Dec2Bin(in value6, in bitCount);
				IEnumerable<byte> second73 = NavigationData.OneBits.Take(2);
				return first.Concat(iodNavBits4).Concat(second69).Concat(second70)
					.Concat(second71)
					.Concat(second72)
					.Concat(second73);
			}
			case WordType.Word2:
			{
				bitCount = ephemeris.TimeOfApplicability;
				byte[] iodNavBits2 = NavigationDataINavFNav.GetIodNavBits(in bitCount);
				double value6 = ephemeris.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second27 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.Inclination * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second28 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
				bitCount = 32;
				byte[] second29 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.RateOfInclination * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 14;
				byte[] second30 = NavigationData.Dec2Bin(in value6, in bitCount);
				IEnumerable<byte> second31 = NavigationData.ZeroAndOneBits.Take(2);
				return first.Concat(iodNavBits2).Concat(second27).Concat(second28)
					.Concat(second29)
					.Concat(second30)
					.Concat(second31);
			}
			case WordType.Word3:
			{
				bitCount = ephemeris.TimeOfApplicability;
				byte[] iodNavBits3 = NavigationDataINavFNav.GetIodNavBits(in bitCount);
				double value6 = ephemeris.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 24;
				byte[] second32 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.MeanMotionCorrection * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
				bitCount = 16;
				byte[] second34 = NavigationData.Dec2Bin(in value6, in bitCount);
				IEnumerable<byte> second35 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second36 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second37 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second38 = NavigationData.ZeroBits.Take(16);
				byte[] second39 = NavigationDataINavFNav.sisaBits;
				return first.Concat(iodNavBits3).Concat(second32).Concat(second34)
					.Concat(second35)
					.Concat(second36)
					.Concat(second37)
					.Concat(second38)
					.Concat(second39);
			}
			case WordType.Word4:
			{
				int t0e = ephemeris.TimeOfApplicability;
				byte[] iodNavBits = NavigationDataINavFNav.GetIodNavBits(in t0e);
				bitCount = ephemeris.Id;
				int bitCount2 = 6;
				byte[] second18 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				IEnumerable<byte> second19 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second20 = NavigationData.ZeroBits.Take(16);
				double value6 = (double)t0e * 0.016666666666666666;
				bitCount = 14;
				byte[] second21 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.Af0 * 17179869184.0;
				bitCount = 31;
				byte[] second23 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.Af1 * 70368744177664.0;
				bitCount = 21;
				byte[] second24 = NavigationData.Dec2Bin(in value6, in bitCount);
				value6 = ephemeris.Af2 * 5.7646075230342349E+17;
				bitCount = 6;
				byte[] second25 = NavigationData.Dec2Bin(in value6, in bitCount);
				IEnumerable<byte> second26 = NavigationData.ZeroAndOneBits.Take(2);
				return first.Concat(iodNavBits).Concat(second18).Concat(second19)
					.Concat(second20)
					.Concat(second21)
					.Concat(second23)
					.Concat(second24)
					.Concat(second25)
					.Concat(second26);
			}
			case WordType.Word5:
			{
				IEnumerable<byte> second74 = NavigationData.ZeroBits.Take(5);
				IEnumerable<byte> second76 = NavigationData.ZeroBits.Take(10);
				IEnumerable<byte> second77 = NavigationData.ZeroBits.Take(10);
				value = ((!e5bPresent || !ephemeris.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second78 = NavigationData.Dec2Bin(in value, in bitCount);
				value = ((!e1BPresent || !ephemeris.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second79 = NavigationData.Dec2Bin(in value, in bitCount);
				byte b = (byte)((!e5bPresent || !ephemeris.IsHealthy) ? 1 : 0);
				byte[] second80 = new byte[1] { b };
				byte b2 = (byte)((!e1BPresent || !ephemeris.IsHealthy) ? 1 : 0);
				byte[] second81 = new byte[1] { b2 };
				GnssTime pageTime2 = pagePartParams.PageTime;
				bitCount = pageTime2.GalileoNavicWeek;
				int bitCount2 = 12;
				byte[] second82 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pageTime2.GalileoNavicSecondOfWeek;
				bitCount2 = 20;
				byte[] second83 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				IEnumerable<byte> second84 = NavigationData.ZeroAndOneBits.Take(23);
				return first.Concat(NavigationDataINavFNav.ai0Bits).Concat(NavigationDataINavFNav.ai1Bits).Concat(NavigationDataINavFNav.ai2Bits)
					.Concat(second74)
					.Concat(second76)
					.Concat(second77)
					.Concat(second78)
					.Concat(second79)
					.Concat(second80)
					.Concat(second81)
					.Concat(second82)
					.Concat(second83)
					.Concat(second84);
			}
			case WordType.Word6:
			{
				IEnumerable<byte> second85 = NavigationData.ZeroBits.Take(32);
				IEnumerable<byte> second87 = NavigationData.ZeroBits.Take(24);
				int num2 = ephemeris.TransmissionInterval.Start.GalileoNavicDayOfWeek * 86400;
				int value11 = ephemeris.TransmissionInterval.Start.GalileoNavicWeek;
				GnssTime gnssTime2 = GnssTime.FromGalileoNavic(value11, num2);
				DateTime utcTime = gnssTime2.UtcTime;
				LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime);
				int value12 = leapSecond.Seconds;
				LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime);
				GnssTime gnssTime3 = GnssTime.FromUtc(leapSecond2.Utc);
				if ((int)(gnssTime3 - gnssTime2).Seconds > 15552000)
				{
					leapSecond2 = leapSecond;
					gnssTime3 = GnssTime.FromUtc(leapSecond.Utc);
				}
				GnssTime gnssTime4 = gnssTime3 - GnssTimeSpan.FromMinutes(1);
				int value2 = gnssTime4.GalileoNavicDayOfWeek + 1;
				int value3 = gnssTime4.GalileoNavicWeek;
				int value4 = leapSecond2.Seconds;
				bitCount = 8;
				byte[] second88 = NavigationData.Dec2Bin(in value12, in bitCount);
				double value6 = (double)num2 * 0.00027777777777777778;
				bitCount = 8;
				byte[] second89 = NavigationData.Dec2Bin(in value6, in bitCount);
				bitCount = 8;
				byte[] second90 = NavigationData.Dec2Bin(in value11, in bitCount);
				bitCount = 8;
				byte[] second91 = NavigationData.Dec2Bin(in value3, in bitCount);
				bitCount = 3;
				byte[] second92 = NavigationData.Dec2Bin(in value2, in bitCount);
				bitCount = 8;
				byte[] second93 = NavigationData.Dec2Bin(in value4, in bitCount);
				bitCount = pagePartParams.PageTime.GalileoNavicSecondOfWeek;
				int bitCount2 = 20;
				byte[] second94 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				IEnumerable<byte> second95 = NavigationData.ZeroAndOneBits.Take(3);
				return first.Concat(second85).Concat(second87).Concat(second88)
					.Concat(second89)
					.Concat(second90)
					.Concat(second91)
					.Concat(second92)
					.Concat(second93)
					.Concat(second94)
					.Concat(second95);
			}
			case WordType.Word7:
			{
				GnssTime transmissionTime4 = pagePartParams.PageTime;
				AlmanacBase almanacBase8 = base.Almanac;
				bitCount = 0;
				GnssTime timeOfAlmanac2 = almanacBase8.GetTimeOfAlmanac(in transmissionTime4, in bitCount);
				int galileoNavicSecondOfWeek2 = timeOfAlmanac2.GalileoNavicSecondOfWeek;
				int value10 = galileoNavicSecondOfWeek2 / 600;
				bitCount = 4;
				byte[] second53 = NavigationData.Dec2Bin(in value10, in bitCount);
				double value6 = (double)galileoNavicSecondOfWeek2 * 0.0016666666666666668;
				bitCount = 10;
				byte[] second55 = NavigationData.Dec2Bin(in value6, in bitCount);
				bitCount = timeOfAlmanac2.GalileoNavicWeek;
				int bitCount2 = 2;
				byte[] second56 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pagePartParams.SubframeIndex;
				int firstAlmanacIndex4 = GetFirstAlmanacIndex(in bitCount);
				Satellite satellite6 = base.Almanac.GetAlmanac(in firstAlmanacIndex4, in transmissionTime4) as Satellite;
				IEnumerable<byte> second64;
				if (satellite6 != null)
				{
					bitCount = satellite6.Id;
					bitCount2 = 6;
					byte[] first4 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					value6 = satellite6.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second57 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second58 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second59 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second60 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second61 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second62 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite6.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second63 = NavigationData.Dec2Bin(in value6, in bitCount);
					second64 = first4.Concat(second57).Concat(second58).Concat(second59)
						.Concat(second60)
						.Concat(second61)
						.Concat(second62)
						.Concat(second63);
				}
				else
				{
					second64 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(94));
				}
				IEnumerable<byte> second65 = NavigationData.ZeroAndOneBits.Take(6);
				return first.Concat(second53).Concat(second56).Concat(second55)
					.Concat(second64)
					.Concat(second65);
			}
			case WordType.Word8:
			{
				GnssTime transmissionTime3 = pagePartParams.PageTime;
				AlmanacBase almanacBase6 = base.Almanac;
				bitCount = 0;
				int value9 = almanacBase6.GetTimeOfAlmanac(in transmissionTime3, in bitCount).GalileoNavicSecondOfWeek / 600;
				bitCount = 4;
				byte[] second40 = NavigationData.Dec2Bin(in value9, in bitCount);
				bitCount = pagePartParams.SubframeIndex;
				int firstAlmanacIndex3 = GetFirstAlmanacIndex(in bitCount);
				Satellite satellite4 = base.Almanac.GetAlmanac(in firstAlmanacIndex3, in transmissionTime3) as Satellite;
				IEnumerable<byte> second44;
				if (satellite4 != null)
				{
					double value6 = satellite4.Af0 * 524288.0;
					bitCount = 16;
					byte[] first7 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite4.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second41 = NavigationData.Dec2Bin(in value6, in bitCount);
					value = ((!e5bPresent || !satellite4.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					byte[] second42 = NavigationData.Dec2Bin(in value, in bitCount);
					value = ((!e1BPresent || !satellite4.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					second44 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first7.Concat(second41).Concat(second42));
				}
				else
				{
					second44 = NavigationData.ZeroAndOneBits.Take(33);
				}
				AlmanacBase almanacBase7 = base.Almanac;
				bitCount = firstAlmanacIndex3 + 1;
				Satellite satellite5 = almanacBase7.GetAlmanac(in bitCount, in transmissionTime3) as Satellite;
				IEnumerable<byte> second51;
				if (satellite5 != null)
				{
					bitCount = satellite5.Id;
					int bitCount2 = 6;
					byte[] first3 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					double value6 = satellite5.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second45 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite5.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second46 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite5.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second47 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite5.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second48 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite5.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second49 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite5.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second50 = NavigationData.Dec2Bin(in value6, in bitCount);
					second51 = first3.Concat(second45).Concat(second46).Concat(second47)
						.Concat(second48)
						.Concat(second49)
						.Concat(second50);
				}
				else
				{
					second51 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(78));
				}
				IEnumerable<byte> second52 = NavigationData.ZeroAndOneBits.Take(1);
				return first.Concat(second40).Concat(second44).Concat(second51)
					.Concat(second52);
			}
			case WordType.Word9:
			{
				GnssTime transmissionTime2 = pagePartParams.PageTime;
				AlmanacBase almanacBase3 = base.Almanac;
				bitCount = 0;
				GnssTime timeOfAlmanac = almanacBase3.GetTimeOfAlmanac(in transmissionTime2, in bitCount);
				int galileoNavicSecondOfWeek = timeOfAlmanac.GalileoNavicSecondOfWeek;
				int value8 = galileoNavicSecondOfWeek / 600;
				bitCount = 4;
				byte[] second5 = NavigationData.Dec2Bin(in value8, in bitCount);
				double value6 = (double)galileoNavicSecondOfWeek * 0.0016666666666666668;
				bitCount = 10;
				byte[] second6 = NavigationData.Dec2Bin(in value6, in bitCount);
				bitCount = timeOfAlmanac.GalileoNavicWeek;
				int bitCount2 = 2;
				byte[] second7 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				bitCount = pagePartParams.SubframeIndex;
				int firstAlmanacIndex2 = GetFirstAlmanacIndex(in bitCount);
				AlmanacBase almanacBase4 = base.Almanac;
				bitCount = firstAlmanacIndex2 + 1;
				Satellite satellite2 = almanacBase4.GetAlmanac(in bitCount, in transmissionTime2) as Satellite;
				IEnumerable<byte> second12;
				if (satellite2 != null)
				{
					value6 = satellite2.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] first6 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite2.Af0 * 524288.0;
					bitCount = 16;
					byte[] second8 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite2.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second9 = NavigationData.Dec2Bin(in value6, in bitCount);
					value = ((!e5bPresent || !satellite2.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					byte[] second10 = NavigationData.Dec2Bin(in value, in bitCount);
					value = ((!e1BPresent || !satellite2.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					second12 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first6.Concat(second8).Concat(second9).Concat(second10));
				}
				else
				{
					second12 = NavigationData.ZeroAndOneBits.Take(49);
				}
				AlmanacBase almanacBase5 = base.Almanac;
				bitCount = firstAlmanacIndex2 + 2;
				Satellite satellite3 = almanacBase5.GetAlmanac(in bitCount, in transmissionTime2) as Satellite;
				IEnumerable<byte> second17;
				if (satellite3 != null)
				{
					bitCount = satellite3.Id;
					bitCount2 = 6;
					byte[] first2 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
					value6 = satellite3.DeltaSqrtA * 512.0;
					bitCount = 13;
					byte[] second13 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite3.Eccentricity * 65536.0;
					bitCount = 11;
					byte[] second14 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite3.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second15 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite3.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
					bitCount = 11;
					byte[] second16 = NavigationData.Dec2Bin(in value6, in bitCount);
					second17 = first2.Concat(second13).Concat(second14).Concat(second15)
						.Concat(second16);
				}
				else
				{
					second17 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(51));
				}
				return first.Concat(second5).Concat(second7).Concat(second6)
					.Concat(second12)
					.Concat(second17);
			}
			case WordType.Word10:
			{
				GnssTime transmissionTime = pagePartParams.PageTime;
				AlmanacBase almanacBase = base.Almanac;
				bitCount = 0;
				int value5 = almanacBase.GetTimeOfAlmanac(in transmissionTime, in bitCount).GalileoNavicSecondOfWeek / 600;
				bitCount = 4;
				byte[] second = NavigationData.Dec2Bin(in value5, in bitCount);
				bitCount = pagePartParams.SubframeIndex;
				int firstAlmanacIndex = GetFirstAlmanacIndex(in bitCount);
				AlmanacBase almanacBase2 = base.Almanac;
				bitCount = firstAlmanacIndex + 2;
				Satellite satellite = almanacBase2.GetAlmanac(in bitCount, in transmissionTime) as Satellite;
				IEnumerable<byte> second75;
				double value6;
				if (satellite != null)
				{
					value6 = satellite.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] first5 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
					bitCount = 11;
					byte[] second11 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
					bitCount = 16;
					byte[] second22 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite.Af0 * 524288.0;
					bitCount = 16;
					byte[] second33 = NavigationData.Dec2Bin(in value6, in bitCount);
					value6 = satellite.Af1 * 274877906944.0;
					bitCount = 13;
					byte[] second43 = NavigationData.Dec2Bin(in value6, in bitCount);
					value = ((!e5bPresent || !satellite.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					byte[] second54 = NavigationData.Dec2Bin(in value, in bitCount);
					value = ((!e1BPresent || !satellite.IsHealthy) ? 1u : 0u);
					bitCount = 2;
					second75 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first5.Concat(second11).Concat(second22).Concat(second33)
						.Concat(second43)
						.Concat(second54));
				}
				else
				{
					second75 = NavigationData.ZeroAndOneBits.Take(76);
				}
				IEnumerable<byte> second86 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second2 = NavigationData.ZeroBits.Take(12);
				GnssTime gnssTime = ephemeris.TransmissionInterval.Start + GnssTimeSpan.FromDays(1);
				int num3 = gnssTime.GalileoNavicDayOfWeek * 86400;
				int value7 = gnssTime.GalileoNavicWeek;
				value6 = (double)num3 * 0.00027777777777777778;
				bitCount = 8;
				byte[] second3 = NavigationData.Dec2Bin(in value6, in bitCount);
				bitCount = 6;
				byte[] second4 = NavigationData.Dec2Bin(in value7, in bitCount);
				return first.Concat(second).Concat(second75).Concat(second86)
					.Concat(second2)
					.Concat(second3)
					.Concat(second4);
			}
			default:
				return first.Concat(NavigationData.ZeroAndOneBits.Take(122));
			}
		}

		protected abstract PagePartParams GetFirstPagePartParameters(int pageIndex, int subframeIndex, in GnssTime pageTime);

		protected abstract PagePartParams GetSecondPagePartParameters(in int pageIndex, in int subframeIndex, in GnssTime pageTime);

		protected abstract IEnumerable<byte> GetReservedBits1(PagePartParams pageParams);

		private IEnumerable<byte> GetReservedBits2()
		{
			return NavigationData.ZeroBits.Take(8);
		}

		static NavigationDataINav()
		{
			int columns = 30;
			int rows = 8;
			blockInterleaver = new BlockInterleaver<byte>(in columns, in rows);
		}
	}
}
