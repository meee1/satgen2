using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Navic;

internal sealed class NavigationDataSPS : NavigationData
{
	private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.NavicSPS);

	private static readonly SignalType signalType = Signal.AllSignals.First((Signal s) => s.NavigationDataInfos.Any((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.NavicSPS)).SignalType;

	private const double Rad2Semi = 1.0 / Math.PI;

	private const int subframeLength = 12;

	private const int subframeLengthBits = 600;

	private const int subframesPerFrame = 4;

	private static readonly byte[] syncBits = Code.HexStringToBits("EB90");

	private static readonly IEnumerable<byte> tailBits = NavigationData.ZeroBits.Take(6);

	private static readonly BlockInterleaver<byte> blockInterleaver;

	private static readonly byte[] tlmWord;

	private static readonly int[] evenMessageTypeSequence;

	private static readonly int[] oddMessageTypeSequence;

	private static readonly int[][] messageTypeSequences;

	private static readonly TimeSystem[] timescaleSequence;

	private const byte alertFlag = 0;

	private const byte autoNavFlag = 0;

	private const byte messagingFlag = 0;

	private static readonly IReadOnlyList<double> alphaParameters;

	private static readonly byte[] alpha0Bits;

	private static readonly byte[] alpha1Bits;

	private static readonly byte[] alpha2Bits;

	private static readonly byte[] alpha3Bits;

	private static readonly IReadOnlyList<double> betaParameters;

	private static readonly byte[] beta0Bits;

	private static readonly byte[] beta1Bits;

	private static readonly byte[] beta2Bits;

	private static readonly byte[] beta3Bits;

	private const double scaleT0c = 0.0625;

	private const double scaleDeltaN = 2199023255552.0;

	private const double scaleM0 = 2147483648.0;

	private const double scaleT0e = 0.0625;

	private const double scaleE = 8589934592.0;

	private const double scaleSqrtA = 524288.0;

	private const double scaleOmega0 = 2147483648.0;

	private const double scaleOmega = 2147483648.0;

	private const double scaleOmegaDot = 2199023255552.0;

	private const double scaleI0 = 2147483648.0;

	private const double scaleAlmanacE = 2097152.0;

	private const double scaleT0a = 0.0625;

	private const double scaleAlmanacI0 = 8388608.0;

	private const double scaleAlmanacOmegaDot = 274877906944.0;

	private const double scaleAlmanacSqrtA = 2048.0;

	private const double scaleAlmanacOmega0 = 8388608.0;

	private const double scaleAlmanacOmega = 8388608.0;

	private const double scaleAlmanacM0 = 8388608.0;

	private const double scaleT0Utc = 0.0625;

	private const double scaleT0t = 0.0625;

	private const double scaleTeop = 0.0625;

	private const double scaleDeltaUT1 = 16777216.0;

	private const double scaleAlpha0 = 1073741824.0;

	private const double scaleAlpha1 = 134217728.0;

	private const double scaleAlpha2 = 16777216.0;

	private const double scaleAlpha3 = 16777216.0;

	private const double scaleBeta0 = 0.00048828125;

	private const double scaleBeta1 = 6.103515625E-05;

	private const double scaleBeta2 = 1.52587890625E-05;

	private const double scaleBeta3 = 1.52587890625E-05;

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

	public NavigationDataSPS(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> signals)
		: base(in satIndex, almanac, in interval, signals)
	{
	}

	public override byte[] Generate()
	{
		int galileoNavicWeek = base.Interval.Start.GalileoNavicWeek;
		int num = base.Interval.Start.GalileoNavicSecondOfWeek / 12;
		int value = num % 4;
		int second = num * 12;
		GnssTime transmissionTime = GnssTime.FromGalileoNavic(galileoNavicWeek, second);
		int num2 = (int)((base.Interval.End - transmissionTime).Seconds / 12.0).SafeCeiling();
		int index = (int)Math.Round((base.Interval.Start - transmissionTime).Seconds * (double)BitRate);
		int[] array = (from s in base.Almanac.BaselineSatellites
			where s != null
			select s.Index).ToArray();
		List<byte> list = new List<byte>(num2 * 600);
		for (int i = 0; i < num2; i++)
		{
			AlmanacBase almanacBase = base.Almanac;
			int satelliteIndex = base.SatelliteIndex;
			Satellite satellite = (almanacBase.GetEphemeris(in satelliteIndex, signalType, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Navic", base.SatelliteIndex + 1));
			int value2 = num + 1;
			satelliteIndex = 17;
			byte[] second2 = NavigationData.Dec2Bin(in value2, in satelliteIndex);
			satelliteIndex = 2;
			byte[] second3 = NavigationData.Dec2Bin(in value, in satelliteIndex);
			IEnumerable<byte> second10;
			int bitCount;
			switch (value)
			{
			case 0:
			{
				satelliteIndex = satellite.Week;
				bitCount = 10;
				byte[] first3 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
				IEnumerable<byte> second54 = NavigationData.ZeroBits.Take(22);
				IEnumerable<byte> second55 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second56 = NavigationData.ZeroBits.Take(8);
				IEnumerable<byte> second57 = NavigationData.ZeroBits.Take(4);
				double value4 = (double)satellite.TimeOfApplicability * 0.0625;
				satelliteIndex = 16;
				byte[] second58 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				IEnumerable<byte> second59 = NavigationData.ZeroBits.Take(8);
				value4 = satellite.MeanMotionCorrection * (1.0 / Math.PI) * 2199023255552.0;
				satelliteIndex = 22;
				byte[] second60 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				int num8 = (int)satellite.TransmissionInterval.Width.Seconds;
				int num9 = 86400 / num8;
				int value11 = satellite.TransmissionInterval.Start.GalileoNavicSecondOfWeek / num8 % num9;
				satelliteIndex = 8;
				byte[] second61 = NavigationData.Dec2Bin(in value11, in satelliteIndex);
				byte[] second62 = new byte[10] { 1, 1, 1, 1, 1, 0, 0, 1, 0, 0 };
				byte element = (byte)((!base.SimulatedSignalTypes.Contains(SignalType.NavicL5SPS)) ? 1 : 0);
				byte element2 = (byte)((!base.SimulatedSignalTypes.Contains(SignalType.NavicSSPS)) ? 1 : 0);
				IEnumerable<byte> second63 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second64 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second65 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second66 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second67 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second68 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second69 = NavigationData.ZeroBits.Take(14);
				IEnumerable<byte> second70 = NavigationData.ZeroBits.Take(2);
				second10 = first3.Concat(second54).Concat(second55).Concat(second56)
					.Concat(second57)
					.Concat(second58)
					.Concat(second59)
					.Concat(second60)
					.Concat(second61)
					.Concat(second62)
					.Append(element)
					.Append(element2)
					.Concat(second63)
					.Concat(second64)
					.Concat(second65)
					.Concat(second66)
					.Concat(second67)
					.Concat(second68)
					.Concat(second69)
					.Concat(second70);
				break;
			}
			case 1:
			{
				double value4 = satellite.MeanAnomaly * (1.0 / Math.PI) * 2147483648.0;
				satelliteIndex = 32;
				byte[] first2 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = (double)satellite.TimeOfApplicability * 0.0625;
				satelliteIndex = 16;
				byte[] second46 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.Eccentricity * 8589934592.0;
				satelliteIndex = 32;
				byte[] second47 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.SqrtA * 524288.0;
				satelliteIndex = 32;
				byte[] second48 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.LongitudeOfAscendingNode * (1.0 / Math.PI) * 2147483648.0;
				satelliteIndex = 32;
				byte[] second49 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.ArgumentOfPerigee * (1.0 / Math.PI) * 2147483648.0;
				satelliteIndex = 32;
				byte[] second50 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.RateOfLongitudeOfAscendingNode * (1.0 / Math.PI) * 2199023255552.0;
				satelliteIndex = 22;
				byte[] second51 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.Inclination * (1.0 / Math.PI) * 2147483648.0;
				satelliteIndex = 32;
				byte[] second52 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				second10 = Enumerable.Concat(second: NavigationData.ZeroBits.Take(2), first: first2.Concat(second46).Concat(second47).Concat(second48)
					.Concat(second49)
					.Concat(second50)
					.Concat(second51)
					.Concat(second52));
				break;
			}
			case 2:
			case 3:
			{
				int num3 = num / 4;
				int num4 = (base.SatelliteIndex + num3) % evenMessageTypeSequence.Length;
				int value3 = messageTypeSequences[value & 1][num4];
				satelliteIndex = 6;
				byte[] first = NavigationData.Dec2Bin(in value3, in satelliteIndex);
				satelliteIndex = base.SatelliteId;
				bitCount = 6;
				byte[] second4 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
				switch (value3)
				{
				case 0:
					second10 = NavigationData.ZeroAndOneBits.Take(232);
					break;
				case 5:
				{
					satelliteIndex = 6;
					bitCount = 10;
					byte[] second11 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					int value5 = num % 6;
					satelliteIndex = 4;
					byte[] second12 = NavigationData.Dec2Bin(in value5, in satelliteIndex);
					IEnumerable<byte> second13 = NavigationData.OneBits.Take(195);
					int num6 = (int)satellite.TransmissionInterval.Width.Seconds;
					int value6 = satellite.TransmissionInterval.Start.GalileoNavicSecondOfWeek / num6 % 8;
					satelliteIndex = 3;
					byte[] second14 = NavigationData.Dec2Bin(in value6, in satelliteIndex);
					IEnumerable<byte> second15 = NavigationData.ZeroBits.Take(8);
					second10 = first.Concat(second11).Concat(second12).Concat(second13)
						.Concat(second14)
						.Concat(second15)
						.Concat(second4);
					break;
				}
				case 7:
				{
					int num7 = array[(num3 + base.SatelliteIndex) % array.Length];
					Satellite obj = (base.Almanac.GetAlmanac(in num7, in transmissionTime) as Satellite) ?? base.Almanac.BaselineSatellites.Select((SatelliteBase s) => s as Satellite).FirstOrDefault((Satellite s) => s?.IsHealthy ?? false) ?? new Satellite();
					satelliteIndex = obj.Week;
					bitCount = 10;
					byte[] second32 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					double value4 = obj.Eccentricity * 2097152.0;
					satelliteIndex = 16;
					byte[] second33 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = (double)obj.TimeOfApplicability * 0.0625;
					satelliteIndex = 16;
					byte[] second34 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.Inclination * (1.0 / Math.PI) * 8388608.0;
					satelliteIndex = 24;
					byte[] second35 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.RateOfLongitudeOfAscendingNode * (1.0 / Math.PI) * 274877906944.0;
					satelliteIndex = 16;
					byte[] second36 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.SqrtA * 2048.0;
					satelliteIndex = 24;
					byte[] second37 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.LongitudeOfAscendingNode * (1.0 / Math.PI) * 8388608.0;
					satelliteIndex = 24;
					byte[] second38 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.ArgumentOfPerigee * (1.0 / Math.PI) * 8388608.0;
					satelliteIndex = 24;
					byte[] second39 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = obj.MeanAnomaly * (1.0 / Math.PI) * 8388608.0;
					satelliteIndex = 24;
					byte[] second40 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					IEnumerable<byte> second41 = NavigationData.ZeroBits.Take(11);
					IEnumerable<byte> second42 = NavigationData.ZeroBits.Take(11);
					satelliteIndex = obj.Id;
					bitCount = 6;
					byte[] second43 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					IEnumerable<byte> second44 = NavigationData.ZeroBits.Take(8);
					IEnumerable<byte> second45 = NavigationData.ZeroBits.Take(6);
					second10 = first.Concat(second32).Concat(second33).Concat(second34)
						.Concat(second35)
						.Concat(second36)
						.Concat(second37)
						.Concat(second38)
						.Concat(second39)
						.Concat(second40)
						.Concat(second41)
						.Concat(second42)
						.Concat(second43)
						.Concat(second44)
						.Concat(second45)
						.Concat(second4);
					break;
				}
				case 9:
				case 26:
				{
					IEnumerable<byte> second16 = NavigationData.ZeroBits.Take(16);
					IEnumerable<byte> second17 = NavigationData.ZeroBits.Take(13);
					IEnumerable<byte> second18 = NavigationData.ZeroBits.Take(7);
					DateTime utcTime2 = satellite.TransmissionInterval.Start.UtcTime;
					LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime2);
					int value7 = leapSecond.Seconds;
					LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime2);
					GnssTime gnssTime = GnssTime.FromUtc(leapSecond2.Utc);
					if ((int)(gnssTime - satellite.TransmissionInterval.Start).Seconds > 15552000)
					{
						leapSecond2 = leapSecond;
						gnssTime = GnssTime.FromUtc(leapSecond.Utc);
					}
					GnssTime gnssTime2 = gnssTime - GnssTimeSpan.FromMinutes(1);
					int value8 = gnssTime2.GalileoNavicDayOfWeek + 1;
					int value9 = gnssTime2.GalileoNavicWeek;
					int value10 = leapSecond2.Seconds;
					satelliteIndex = 8;
					byte[] second19 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
					satelliteIndex = 10;
					byte[] second20 = NavigationData.Dec2Bin(in value9, in satelliteIndex);
					satelliteIndex = 4;
					byte[] second21 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
					satelliteIndex = 8;
					byte[] second22 = NavigationData.Dec2Bin(in value10, in satelliteIndex);
					GnssTime start2 = satellite.TransmissionInterval.Start;
					double value4 = (double)start2.GalileoNavicSecondOfWeek * 0.0625;
					satelliteIndex = 16;
					byte[] second23 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					satelliteIndex = start2.GalileoNavicWeek;
					bitCount = 10;
					byte[] second24 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					IEnumerable<byte> second25 = NavigationData.ZeroBits.Take(16);
					IEnumerable<byte> second26 = NavigationData.ZeroBits.Take(13);
					IEnumerable<byte> second27 = NavigationData.ZeroBits.Take(7);
					GnssTime start3 = satellite.TransmissionInterval.Start;
					value4 = (double)start3.GalileoNavicSecondOfWeek * 0.0625;
					satelliteIndex = 16;
					byte[] second28 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					satelliteIndex = start3.GalileoNavicWeek;
					bitCount = 10;
					byte[] second29 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					satelliteIndex = (int)((value3 != 9) ? timescaleSequence[num % timescaleSequence.Length] : TimeSystem.Gps);
					bitCount = 3;
					byte[] second30 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					IEnumerable<byte> second31 = NavigationData.ZeroBits.Take(63);
					second10 = first.Concat(second16).Concat(second17).Concat(second18)
						.Concat(second19)
						.Concat(second23)
						.Concat(second24)
						.Concat(second20)
						.Concat(second21)
						.Concat(second22)
						.Concat(second25)
						.Concat(second26)
						.Concat(second27)
						.Concat(second28)
						.Concat(second29)
						.Concat(second30)
						.Concat(second31)
						.Concat(second4);
					break;
				}
				case 11:
				{
					GnssTime start = satellite.TransmissionInterval.Start;
					double value4 = (double)start.GalileoNavicSecondOfWeek * 0.0625;
					satelliteIndex = 16;
					byte[] second5 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					IEnumerable<byte> second6 = NavigationData.ZeroBits.Take(72);
					DateTime utcTime = start.UtcTime;
					DateTime utc = LeapSecond.LeapSecondsForDate(utcTime).Utc;
					TimeSpan timeSpan = LeapSecond.NextLeapSecondsAfterDate(utcTime).Utc - utc;
					if (timeSpan == TimeSpan.Zero)
					{
						timeSpan = TimeSpan.FromDays(2556.6800000000003);
					}
					double num5 = (utcTime - utc) / timeSpan;
					value4 = (0.5 - num5) * 16777216.0;
					satelliteIndex = 31;
					IEnumerable<byte> second7 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					IEnumerable<byte> second8 = NavigationData.ZeroBits.Take(19);
					IEnumerable<byte> second9 = NavigationData.ZeroBits.Take(18);
					second10 = first.Concat(second5).Concat(second6).Concat(second7)
						.Concat(second8)
						.Concat(alpha0Bits)
						.Concat(alpha1Bits)
						.Concat(alpha2Bits)
						.Concat(alpha3Bits)
						.Concat(beta0Bits)
						.Concat(beta1Bits)
						.Concat(beta2Bits)
						.Concat(beta3Bits)
						.Concat(second9)
						.Concat(second4);
					break;
				}
				default:
					throw new Exception($"Invalid NAVIC SPS message type: {value3}");
				}
				break;
			}
			default:
				throw new Exception($"Invalid NAVIC SPS subframe ID: {value + 1}");
			}
			byte[] array2 = tlmWord.Concat(second2).Append<byte>(0).Append<byte>(0)
				.Concat(second3)
				.Append<byte>(0)
				.Concat(second10)
				.ToArray(262);
			IEnumerable<byte> second71 = CRC24Q.ComputeBytes(array2);
			IEnumerable<byte> inputSequence = array2.Concat(second71).Concat(tailBits);
			satelliteIndex = 0;
			bitCount = 0;
			byte[] data = new ConvolutionalEncoder(inputSequence, in satelliteIndex, in bitCount).ToArray(584);
			IEnumerable<byte> second72 = blockInterleaver.Interleave(data);
			list.AddRange(syncBits.Concat(second72));
			if (num2 > 1)
			{
				transmissionTime += GnssTimeSpan.FromSeconds(12);
				num = transmissionTime.GalileoNavicSecondOfWeek / 12;
				value = num % 4;
			}
		}
		int num10 = (int)Math.Round(base.Interval.Width.Seconds * (double)BitRate);
		byte[] array3 = new byte[num10];
		list.CopyTo(index, array3, 0, num10);
		return array3;
	}

	static NavigationDataSPS()
	{
		int columns = 73;
		int rows = 8;
		blockInterleaver = new BlockInterleaver<byte>(in columns, in rows);
		tlmWord = new byte[8] { 1, 0, 0, 0, 1, 0, 1, 1 };
		evenMessageTypeSequence = new int[11]
		{
			11, 7, 7, 5, 5, 5, 26, 26, 5, 5,
			5
		};
		oddMessageTypeSequence = new int[11]
		{
			5, 7, 7, 5, 5, 5, 11, 26, 26, 5,
			5
		};
		messageTypeSequences = new int[2][] { evenMessageTypeSequence, oddMessageTypeSequence };
		timescaleSequence = new TimeSystem[8]
		{
			TimeSystem.UtcNpli,
			TimeSystem.Gps,
			TimeSystem.UtcNpli,
			TimeSystem.Galileo,
			TimeSystem.UtcNpli,
			TimeSystem.Gps,
			TimeSystem.UtcNpli,
			TimeSystem.Glonass
		};
		alphaParameters = Klobuchar.Alpha;
		double value = alphaParameters[0] * 1073741824.0;
		columns = 8;
		alpha0Bits = NavigationData.Dec2Bin(in value, in columns);
		value = alphaParameters[1] * 134217728.0;
		columns = 8;
		alpha1Bits = NavigationData.Dec2Bin(in value, in columns);
		value = alphaParameters[2] * 16777216.0;
		columns = 8;
		alpha2Bits = NavigationData.Dec2Bin(in value, in columns);
		value = alphaParameters[3] * 16777216.0;
		columns = 8;
		alpha3Bits = NavigationData.Dec2Bin(in value, in columns);
		betaParameters = Klobuchar.Beta;
		value = betaParameters[0] * 0.00048828125;
		columns = 8;
		beta0Bits = NavigationData.Dec2Bin(in value, in columns);
		value = betaParameters[1] * 6.103515625E-05;
		columns = 8;
		beta1Bits = NavigationData.Dec2Bin(in value, in columns);
		value = betaParameters[2] * 1.52587890625E-05;
		columns = 8;
		beta2Bits = NavigationData.Dec2Bin(in value, in columns);
		value = betaParameters[3] * 1.52587890625E-05;
		columns = 8;
		beta3Bits = NavigationData.Dec2Bin(in value, in columns);
	}
}
