using System;
using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo;

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
		int num = galileoNavicSecondOfWeek / 2;
		int pageIndex = num % 15;
		int second = num * 2;
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
		IEnumerable<byte> second4 = CRC24Q.ComputeBytes(first.Concat(array2).ToArray(196));
		IEnumerable<byte> inputSequence2 = array2.Concat(second4).Concat(reservedBits2).Concat(NavigationDataINavFNav.tailBits);
		registerState = 0;
		captureIndex = 0;
		byte[] data2 = new ConvolutionalEncoder(inputSequence2, in registerState, in captureIndex, ConvolutionalEncoderOptions.NegateG2).ToArray(240);
		IEnumerable<byte> second5 = blockInterleaver.Interleave(data2);
		IEnumerable<byte> enumerable2 = syncBits.Concat(second5);
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
			IEnumerable<byte> second68 = NavigationData.ZeroAndOneBits.Take(88);
			GnssTime pageTime = pagePartParams.PageTime;
			bitCount = pageTime.GalileoNavicWeek;
			int bitCount2 = 12;
			byte[] second69 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			bitCount = pageTime.GalileoNavicSecondOfWeek;
			bitCount2 = 20;
			byte[] second70 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			return first.Concat(timeFlagsBits).Concat(second68).Concat(second69)
				.Concat(second70);
		}
		case WordType.Word1:
		{
			int t0e2 = ephemeris.TimeOfApplicability;
			byte[] iodNavBits4 = NavigationDataINavFNav.GetIodNavBits(in t0e2);
			double value3 = (double)t0e2 * (1.0 / 60.0);
			bitCount = 14;
			byte[] second81 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
			bitCount = 32;
			byte[] second82 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.Eccentricity * 8589934592.0;
			bitCount = 32;
			byte[] second83 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.SqrtA * 524288.0;
			bitCount = 32;
			byte[] second84 = NavigationData.Dec2Bin(in value3, in bitCount);
			IEnumerable<byte> second85 = NavigationData.OneBits.Take(2);
			return first.Concat(iodNavBits4).Concat(second81).Concat(second82)
				.Concat(second83)
				.Concat(second84)
				.Concat(second85);
		}
		case WordType.Word2:
		{
			bitCount = ephemeris.TimeOfApplicability;
			byte[] iodNavBits3 = NavigationDataINavFNav.GetIodNavBits(in bitCount);
			double value3 = ephemeris.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
			bitCount = 32;
			byte[] second38 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.Inclination * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
			bitCount = 32;
			byte[] second39 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 2147483648.0;
			bitCount = 32;
			byte[] second40 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.RateOfInclination * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
			bitCount = 14;
			byte[] second41 = NavigationData.Dec2Bin(in value3, in bitCount);
			IEnumerable<byte> second42 = NavigationData.ZeroAndOneBits.Take(2);
			return first.Concat(iodNavBits3).Concat(second38).Concat(second39)
				.Concat(second40)
				.Concat(second41)
				.Concat(second42);
		}
		case WordType.Word3:
		{
			bitCount = ephemeris.TimeOfApplicability;
			byte[] iodNavBits = NavigationDataINavFNav.GetIodNavBits(in bitCount);
			double value3 = ephemeris.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
			bitCount = 24;
			byte[] second13 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.MeanMotionCorrection * NavigationDataINavFNav.Rad2Semi * 8796093022208.0;
			bitCount = 16;
			byte[] second14 = NavigationData.Dec2Bin(in value3, in bitCount);
			IEnumerable<byte> second15 = NavigationData.ZeroBits.Take(16);
			IEnumerable<byte> second16 = NavigationData.ZeroBits.Take(16);
			IEnumerable<byte> second17 = NavigationData.ZeroBits.Take(16);
			IEnumerable<byte> second18 = NavigationData.ZeroBits.Take(16);
			byte[] second19 = NavigationDataINavFNav.sisaBits;
			return first.Concat(iodNavBits).Concat(second13).Concat(second14)
				.Concat(second15)
				.Concat(second16)
				.Concat(second17)
				.Concat(second18)
				.Concat(second19);
		}
		case WordType.Word4:
		{
			int t0e = ephemeris.TimeOfApplicability;
			byte[] iodNavBits2 = NavigationDataINavFNav.GetIodNavBits(in t0e);
			bitCount = ephemeris.Id;
			int bitCount2 = 6;
			byte[] second20 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			IEnumerable<byte> second21 = NavigationData.ZeroBits.Take(16);
			IEnumerable<byte> second22 = NavigationData.ZeroBits.Take(16);
			double value3 = (double)t0e * (1.0 / 60.0);
			bitCount = 14;
			byte[] second23 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.Af0 * 17179869184.0;
			bitCount = 31;
			byte[] second24 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.Af1 * 70368744177664.0;
			bitCount = 21;
			byte[] second25 = NavigationData.Dec2Bin(in value3, in bitCount);
			value3 = ephemeris.Af2 * 5.7646075230342349E+17;
			bitCount = 6;
			byte[] second26 = NavigationData.Dec2Bin(in value3, in bitCount);
			IEnumerable<byte> second27 = NavigationData.ZeroAndOneBits.Take(2);
			return first.Concat(iodNavBits2).Concat(second20).Concat(second21)
				.Concat(second22)
				.Concat(second23)
				.Concat(second24)
				.Concat(second25)
				.Concat(second26)
				.Concat(second27);
		}
		case WordType.Word5:
		{
			IEnumerable<byte> second71 = NavigationData.ZeroBits.Take(5);
			IEnumerable<byte> second72 = NavigationData.ZeroBits.Take(10);
			IEnumerable<byte> second73 = NavigationData.ZeroBits.Take(10);
			value = ((!e5bPresent || !ephemeris.IsHealthy) ? 1u : 0u);
			bitCount = 2;
			byte[] second74 = NavigationData.Dec2Bin(in value, in bitCount);
			value = ((!e1BPresent || !ephemeris.IsHealthy) ? 1u : 0u);
			bitCount = 2;
			byte[] second75 = NavigationData.Dec2Bin(in value, in bitCount);
			byte b = (byte)((!e5bPresent || !ephemeris.IsHealthy) ? 1 : 0);
			byte[] second76 = new byte[1] { b };
			byte b2 = (byte)((!e1BPresent || !ephemeris.IsHealthy) ? 1 : 0);
			byte[] second77 = new byte[1] { b2 };
			GnssTime pageTime2 = pagePartParams.PageTime;
			bitCount = pageTime2.GalileoNavicWeek;
			int bitCount2 = 12;
			byte[] second78 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			bitCount = pageTime2.GalileoNavicSecondOfWeek;
			bitCount2 = 20;
			byte[] second79 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			IEnumerable<byte> second80 = NavigationData.ZeroAndOneBits.Take(23);
			return first.Concat(NavigationDataINavFNav.ai0Bits).Concat(NavigationDataINavFNav.ai1Bits).Concat(NavigationDataINavFNav.ai2Bits)
				.Concat(second71)
				.Concat(second72)
				.Concat(second73)
				.Concat(second74)
				.Concat(second75)
				.Concat(second76)
				.Concat(second77)
				.Concat(second78)
				.Concat(second79)
				.Concat(second80);
		}
		case WordType.Word6:
		{
			IEnumerable<byte> second28 = NavigationData.ZeroBits.Take(32);
			IEnumerable<byte> second29 = NavigationData.ZeroBits.Take(24);
			int num2 = ephemeris.TransmissionInterval.Start.GalileoNavicDayOfWeek * 86400;
			int value5 = ephemeris.TransmissionInterval.Start.GalileoNavicWeek;
			GnssTime gnssTime2 = GnssTime.FromGalileoNavic(value5, num2);
			DateTime utcTime = gnssTime2.UtcTime;
			LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime);
			int value6 = leapSecond.Seconds;
			LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime);
			GnssTime gnssTime3 = GnssTime.FromUtc(leapSecond2.Utc);
			if ((int)(gnssTime3 - gnssTime2).Seconds > 15552000)
			{
				leapSecond2 = leapSecond;
				gnssTime3 = GnssTime.FromUtc(leapSecond.Utc);
			}
			GnssTime gnssTime4 = gnssTime3 - GnssTimeSpan.FromMinutes(1);
			int value7 = gnssTime4.GalileoNavicDayOfWeek + 1;
			int value8 = gnssTime4.GalileoNavicWeek;
			int value9 = leapSecond2.Seconds;
			bitCount = 8;
			byte[] second30 = NavigationData.Dec2Bin(in value6, in bitCount);
			double value3 = (double)num2 * 0.00027777777777777778;
			bitCount = 8;
			byte[] second31 = NavigationData.Dec2Bin(in value3, in bitCount);
			bitCount = 8;
			byte[] second32 = NavigationData.Dec2Bin(in value5, in bitCount);
			bitCount = 8;
			byte[] second33 = NavigationData.Dec2Bin(in value8, in bitCount);
			bitCount = 3;
			byte[] second34 = NavigationData.Dec2Bin(in value7, in bitCount);
			bitCount = 8;
			byte[] second35 = NavigationData.Dec2Bin(in value9, in bitCount);
			bitCount = pagePartParams.PageTime.GalileoNavicSecondOfWeek;
			int bitCount2 = 20;
			byte[] second36 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			IEnumerable<byte> second37 = NavigationData.ZeroAndOneBits.Take(3);
			return first.Concat(second28).Concat(second29).Concat(second30)
				.Concat(second31)
				.Concat(second32)
				.Concat(second33)
				.Concat(second34)
				.Concat(second35)
				.Concat(second36)
				.Concat(second37);
		}
		case WordType.Word7:
		{
			GnssTime transmissionTime3 = pagePartParams.PageTime;
			AlmanacBase almanacBase5 = base.Almanac;
			bitCount = 0;
			GnssTime timeOfAlmanac = almanacBase5.GetTimeOfAlmanac(in transmissionTime3, in bitCount);
			int galileoNavicSecondOfWeek = timeOfAlmanac.GalileoNavicSecondOfWeek;
			int value11 = galileoNavicSecondOfWeek / 600;
			bitCount = 4;
			byte[] second56 = NavigationData.Dec2Bin(in value11, in bitCount);
			double value3 = (double)galileoNavicSecondOfWeek * (1.0 / 600.0);
			bitCount = 10;
			byte[] second57 = NavigationData.Dec2Bin(in value3, in bitCount);
			bitCount = timeOfAlmanac.GalileoNavicWeek;
			int bitCount2 = 2;
			byte[] second58 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			bitCount = pagePartParams.SubframeIndex;
			int firstAlmanacIndex3 = GetFirstAlmanacIndex(in bitCount);
			IEnumerable<byte> second66;
			if (base.Almanac.GetAlmanac(in firstAlmanacIndex3, in transmissionTime3) is Satellite satellite4)
			{
				bitCount = satellite4.Id;
				bitCount2 = 6;
				byte[] first5 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				value3 = satellite4.DeltaSqrtA * 512.0;
				bitCount = 13;
				byte[] second59 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.Eccentricity * 65536.0;
				bitCount = 11;
				byte[] second60 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second61 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
				bitCount = 11;
				byte[] second62 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second63 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
				bitCount = 11;
				byte[] second64 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite4.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second65 = NavigationData.Dec2Bin(in value3, in bitCount);
				second66 = first5.Concat(second59).Concat(second60).Concat(second61)
					.Concat(second62)
					.Concat(second63)
					.Concat(second64)
					.Concat(second65);
			}
			else
			{
				second66 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(94));
			}
			IEnumerable<byte> second67 = NavigationData.ZeroAndOneBits.Take(6);
			return first.Concat(second56).Concat(second58).Concat(second57)
				.Concat(second66)
				.Concat(second67);
		}
		case WordType.Word8:
		{
			GnssTime transmissionTime2 = pagePartParams.PageTime;
			AlmanacBase almanacBase3 = base.Almanac;
			bitCount = 0;
			int value10 = almanacBase3.GetTimeOfAlmanac(in transmissionTime2, in bitCount).GalileoNavicSecondOfWeek / 600;
			bitCount = 4;
			byte[] second43 = NavigationData.Dec2Bin(in value10, in bitCount);
			bitCount = pagePartParams.SubframeIndex;
			int firstAlmanacIndex2 = GetFirstAlmanacIndex(in bitCount);
			IEnumerable<byte> second47;
			if (base.Almanac.GetAlmanac(in firstAlmanacIndex2, in transmissionTime2) is Satellite satellite2)
			{
				double value3 = satellite2.Af0 * 524288.0;
				bitCount = 16;
				byte[] first3 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite2.Af1 * 274877906944.0;
				bitCount = 13;
				byte[] second44 = NavigationData.Dec2Bin(in value3, in bitCount);
				value = ((!e5bPresent || !satellite2.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second45 = NavigationData.Dec2Bin(in value, in bitCount);
				value = ((!e1BPresent || !satellite2.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				second47 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first3.Concat(second44).Concat(second45));
			}
			else
			{
				second47 = NavigationData.ZeroAndOneBits.Take(33);
			}
			AlmanacBase almanacBase4 = base.Almanac;
			bitCount = firstAlmanacIndex2 + 1;
			IEnumerable<byte> second54;
			if (almanacBase4.GetAlmanac(in bitCount, in transmissionTime2) is Satellite satellite3)
			{
				bitCount = satellite3.Id;
				int bitCount2 = 6;
				byte[] first4 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				double value3 = satellite3.DeltaSqrtA * 512.0;
				bitCount = 13;
				byte[] second48 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite3.Eccentricity * 65536.0;
				bitCount = 11;
				byte[] second49 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite3.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second50 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite3.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
				bitCount = 11;
				byte[] second51 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite3.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second52 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite3.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
				bitCount = 11;
				byte[] second53 = NavigationData.Dec2Bin(in value3, in bitCount);
				second54 = first4.Concat(second48).Concat(second49).Concat(second50)
					.Concat(second51)
					.Concat(second52)
					.Concat(second53);
			}
			else
			{
				second54 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(78));
			}
			IEnumerable<byte> second55 = NavigationData.ZeroAndOneBits.Take(1);
			return first.Concat(second43).Concat(second47).Concat(second54)
				.Concat(second55);
		}
		case WordType.Word9:
		{
			GnssTime transmissionTime4 = pagePartParams.PageTime;
			AlmanacBase almanacBase6 = base.Almanac;
			bitCount = 0;
			GnssTime timeOfAlmanac2 = almanacBase6.GetTimeOfAlmanac(in transmissionTime4, in bitCount);
			int galileoNavicSecondOfWeek2 = timeOfAlmanac2.GalileoNavicSecondOfWeek;
			int value12 = galileoNavicSecondOfWeek2 / 600;
			bitCount = 4;
			byte[] second86 = NavigationData.Dec2Bin(in value12, in bitCount);
			double value3 = (double)galileoNavicSecondOfWeek2 * (1.0 / 600.0);
			bitCount = 10;
			byte[] second87 = NavigationData.Dec2Bin(in value3, in bitCount);
			bitCount = timeOfAlmanac2.GalileoNavicWeek;
			int bitCount2 = 2;
			byte[] second88 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
			bitCount = pagePartParams.SubframeIndex;
			int firstAlmanacIndex4 = GetFirstAlmanacIndex(in bitCount);
			AlmanacBase almanacBase7 = base.Almanac;
			bitCount = firstAlmanacIndex4 + 1;
			IEnumerable<byte> second93;
			if (almanacBase7.GetAlmanac(in bitCount, in transmissionTime4) is Satellite satellite5)
			{
				value3 = satellite5.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] first6 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite5.Af0 * 524288.0;
				bitCount = 16;
				byte[] second89 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite5.Af1 * 274877906944.0;
				bitCount = 13;
				byte[] second90 = NavigationData.Dec2Bin(in value3, in bitCount);
				value = ((!e5bPresent || !satellite5.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second91 = NavigationData.Dec2Bin(in value, in bitCount);
				value = ((!e1BPresent || !satellite5.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				second93 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first6.Concat(second89).Concat(second90).Concat(second91));
			}
			else
			{
				second93 = NavigationData.ZeroAndOneBits.Take(49);
			}
			AlmanacBase almanacBase8 = base.Almanac;
			bitCount = firstAlmanacIndex4 + 2;
			IEnumerable<byte> second98;
			if (almanacBase8.GetAlmanac(in bitCount, in transmissionTime4) is Satellite satellite6)
			{
				bitCount = satellite6.Id;
				bitCount2 = 6;
				byte[] first7 = NavigationData.Dec2Bin(in bitCount, in bitCount2);
				value3 = satellite6.DeltaSqrtA * 512.0;
				bitCount = 13;
				byte[] second94 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite6.Eccentricity * 65536.0;
				bitCount = 11;
				byte[] second95 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite6.ArgumentOfPerigee * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second96 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite6.DeltaInclination * NavigationDataINavFNav.Rad2Semi * 16384.0;
				bitCount = 11;
				byte[] second97 = NavigationData.Dec2Bin(in value3, in bitCount);
				second98 = first7.Concat(second94).Concat(second95).Concat(second96)
					.Concat(second97);
			}
			else
			{
				second98 = NavigationData.ZeroBits.Take(6).Concat(NavigationData.ZeroAndOneBits.Take(51));
			}
			return first.Concat(second86).Concat(second88).Concat(second87)
				.Concat(second93)
				.Concat(second98);
		}
		case WordType.Word10:
		{
			GnssTime transmissionTime = pagePartParams.PageTime;
			AlmanacBase almanacBase = base.Almanac;
			bitCount = 0;
			int value2 = almanacBase.GetTimeOfAlmanac(in transmissionTime, in bitCount).GalileoNavicSecondOfWeek / 600;
			bitCount = 4;
			byte[] second = NavigationData.Dec2Bin(in value2, in bitCount);
			bitCount = pagePartParams.SubframeIndex;
			int firstAlmanacIndex = GetFirstAlmanacIndex(in bitCount);
			AlmanacBase almanacBase2 = base.Almanac;
			bitCount = firstAlmanacIndex + 2;
			IEnumerable<byte> second8;
			double value3;
			if (almanacBase2.GetAlmanac(in bitCount, in transmissionTime) is Satellite satellite)
			{
				value3 = satellite.LongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] first2 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite.RateOfLongitudeOfAscendingNode * NavigationDataINavFNav.Rad2Semi * 8589934592.0;
				bitCount = 11;
				byte[] second2 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite.MeanAnomaly * NavigationDataINavFNav.Rad2Semi * 32768.0;
				bitCount = 16;
				byte[] second3 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite.Af0 * 524288.0;
				bitCount = 16;
				byte[] second4 = NavigationData.Dec2Bin(in value3, in bitCount);
				value3 = satellite.Af1 * 274877906944.0;
				bitCount = 13;
				byte[] second5 = NavigationData.Dec2Bin(in value3, in bitCount);
				value = ((!e5bPresent || !satellite.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				byte[] second6 = NavigationData.Dec2Bin(in value, in bitCount);
				value = ((!e1BPresent || !satellite.IsHealthy) ? 1u : 0u);
				bitCount = 2;
				second8 = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in bitCount), first: first2.Concat(second2).Concat(second3).Concat(second4)
					.Concat(second5)
					.Concat(second6));
			}
			else
			{
				second8 = NavigationData.ZeroAndOneBits.Take(76);
			}
			IEnumerable<byte> second9 = NavigationData.ZeroBits.Take(16);
			IEnumerable<byte> second10 = NavigationData.ZeroBits.Take(12);
			GnssTime gnssTime = ephemeris.TransmissionInterval.Start + GnssTimeSpan.FromDays(1);
			int num = gnssTime.GalileoNavicDayOfWeek * 86400;
			int value4 = gnssTime.GalileoNavicWeek;
			value3 = (double)num * 0.00027777777777777778;
			bitCount = 8;
			byte[] second11 = NavigationData.Dec2Bin(in value3, in bitCount);
			bitCount = 6;
			byte[] second12 = NavigationData.Dec2Bin(in value4, in bitCount);
			return first.Concat(second).Concat(second8).Concat(second9)
				.Concat(second10)
				.Concat(second11)
				.Concat(second12);
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
