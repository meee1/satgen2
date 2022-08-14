using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.BeiDou;

internal sealed class NavigationDataB1ID1 : NavigationDataB1I
{
	private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.BeiDouD1);

	private const int framesPerSuperframe = 24;

	private const int frameLength = 30;

	private const int subframesPerFrame = 5;

	private const int subframeLength = 6;

	private const int subframesPerWeek = 100800;

	private const uint aodc = 0u;

	private const uint aode = 1u;

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

	public NavigationDataB1ID1(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> allSignals)
		: base(in satIndex, almanac, in interval, allSignals)
	{
	}

	public sealed override byte[] Generate()
	{
		int num = base.Interval.Start.BeiDouSecondOfWeek / 6;
		int num2 = num * 6;
		int beiDouWeek = base.Interval.Start.BeiDouWeek;
		GnssTime gnssTime = GnssTime.FromBeiDou(beiDouWeek, num2);
		double seconds = (base.Interval.Start - gnssTime).Seconds;
		int num3 = (int)((base.Interval.End - gnssTime).Seconds / 6.0).SafeCeiling();
		int num4 = num;
		int value = num4 % 5 + 1;
		int value2 = num2 / 30 % 24 + 1;
		List<byte> list = new List<byte>(num3 * 6 * BitRate);
		for (int i = 0; i < num3; i++)
		{
			GnssTime transmissionTime = GnssTime.FromBeiDou(beiDouWeek, num4 * 6);
			AlmanacBase almanacBase = base.Almanac;
			int satelliteIndex = base.SatelliteIndex;
			Satellite satellite = (almanacBase.GetEphemeris(in satelliteIndex, SignalType.BeiDouB1I, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "BeiDou", base.SatelliteIndex + 1));
			int value3 = num4 % 100800 * 6;
			satelliteIndex = 20;
			byte[] source = NavigationData.Dec2Bin(in value3, in satelliteIndex);
			IEnumerable<byte> enumerable = source.Skip(8).Take(12);
			satelliteIndex = 3;
			byte[] second = NavigationData.Dec2Bin(in value, in satelliteIndex);
			AddFirstWord(list, NavigationDataB1I.preambleAndRevBits.Concat(second).Concat(source.Take(8)));
			switch (value)
			{
			case 1:
			{
				byte element = (byte)((!satellite.IsHealthy) ? 1 : 0);
				uint value5 = 0u;
				satelliteIndex = 5;
				byte[] second8 = NavigationData.Dec2Bin(in value5, in satelliteIndex);
				value5 = 0u;
				satelliteIndex = 4;
				byte[] second9 = NavigationData.Dec2Bin(in value5, in satelliteIndex);
				AddWord(list, enumerable.Append(element).Concat(second8).Concat(second9));
				satelliteIndex = satellite.Week;
				int bitCount = 13;
				byte[] first2 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
				double value4 = (double)satellite.TimeOfApplicability * 0.125;
				satelliteIndex = 17;
				byte[] source9 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, first2.Concat(source9.Take(9)));
				IEnumerable<byte> second10 = NavigationData.ZeroBits.Take(10);
				IEnumerable<byte> source10 = NavigationData.ZeroBits.Take(10);
				AddWord(list, source9.Skip(9).Take(8).Concat(second10)
					.Concat(source10.Take(4)));
				value4 = NavigationDataB1I.alphaParameters[0] * 1073741824.0;
				satelliteIndex = 8;
				byte[] second11 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = NavigationDataB1I.alphaParameters[1] * 134217728.0;
				satelliteIndex = 8;
				byte[] second12 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source10.Skip(4).Take(6).Concat(second11)
					.Concat(second12));
				value4 = NavigationDataB1I.alphaParameters[2] * 16777216.0;
				satelliteIndex = 8;
				byte[] first3 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = NavigationDataB1I.alphaParameters[3] * 16777216.0;
				satelliteIndex = 8;
				byte[] second13 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = NavigationDataB1I.betaParameters[0] * 0.00048828125;
				satelliteIndex = 8;
				byte[] source11 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, first3.Concat(second13).Concat(source11.Take(6)));
				value4 = NavigationDataB1I.betaParameters[1] * 6.103515625E-05;
				satelliteIndex = 8;
				byte[] second14 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = NavigationDataB1I.betaParameters[2] * 1.52587890625E-05;
				satelliteIndex = 8;
				byte[] second15 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = NavigationDataB1I.betaParameters[3] * 1.52587890625E-05;
				satelliteIndex = 8;
				byte[] source12 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source11.Skip(6).Take(2).Concat(second14)
					.Concat(second15)
					.Concat(source12.Take(4)));
				value4 = satellite.A2 * 4.0;
				satelliteIndex = 11;
				byte[] second16 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.A0 * 8589934592.0;
				satelliteIndex = 24;
				byte[] source13 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source12.Skip(4).Take(4).Concat(second16)
					.Concat(source13.Take(7)));
				value4 = satellite.A1 * 1125899906842624.0;
				satelliteIndex = 22;
				byte[] source14 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source13.Skip(7).Take(17).Concat(source14.Take(5)));
				value5 = 1u;
				satelliteIndex = 5;
				byte[] second17 = NavigationData.Dec2Bin(in value5, in satelliteIndex);
				AddWord(list, source14.Skip(5).Take(17).Concat(second17));
				break;
			}
			case 2:
			{
				double value4 = satellite.MeanMotionCorrection * rad2Semi * 8796093022208.0;
				satelliteIndex = 16;
				byte[] source15 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, enumerable.Concat(source15.Take(10)));
				value4 = satellite.Cuc * 2147483648.0;
				satelliteIndex = 18;
				byte[] source16 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source15.Skip(10).Take(6).Concat(source16.Take(16)));
				value4 = satellite.MeanAnomaly * rad2Semi * 2147483648.0;
				satelliteIndex = 32;
				byte[] source17 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source16.Skip(16).Take(2).Concat(source17.Take(20)));
				value4 = satellite.Eccentricity * 8589934592.0;
				satelliteIndex = 32;
				byte[] source18 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source17.Skip(20).Take(12).Concat(source18.Take(10)));
				AddWord(list, source18.Skip(10).Take(22));
				value4 = satellite.Cus * 2147483648.0;
				satelliteIndex = 18;
				byte[] first4 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				value4 = satellite.Crs * 64.0;
				satelliteIndex = 18;
				byte[] source19 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, first4.Concat(source19.Take(4)));
				value4 = satellite.Crs * 64.0;
				satelliteIndex = 18;
				byte[] source20 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source19.Skip(4).Take(14).Concat(source20.Take(8)));
				value4 = satellite.SqrtA * 524288.0;
				satelliteIndex = 32;
				byte[] source21 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source20.Skip(8).Take(10).Concat(source21.Take(12)));
				value4 = (double)satellite.TimeOfApplicability * 0.125;
				satelliteIndex = 17;
				byte[] source22 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source21.Skip(12).Take(20).Concat(source22.Take(2)));
				break;
			}
			case 3:
			{
				double value4 = (double)satellite.TimeOfApplicability * 0.125;
				satelliteIndex = 17;
				byte[] source23 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, enumerable.Concat(source23.Skip(2).Take(10)));
				value4 = satellite.Inclination * rad2Semi * 2147483648.0;
				satelliteIndex = 32;
				byte[] source24 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source23.Skip(12).Take(5).Concat(source24.Take(17)));
				value4 = satellite.Cis * 2147483648.0;
				satelliteIndex = 18;
				byte[] source25 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source24.Skip(17).Take(15).Concat(source25.Take(7)));
				value4 = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 8796093022208.0;
				satelliteIndex = 24;
				byte[] source26 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source25.Skip(7).Take(11).Concat(source26.Take(11)));
				value4 = satellite.Cis * 2147483648.0;
				satelliteIndex = 18;
				byte[] source27 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source26.Skip(11).Take(13).Concat(source27.Take(9)));
				value4 = satellite.RateOfInclination * rad2Semi * 8796093022208.0;
				satelliteIndex = 14;
				byte[] source28 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source27.Skip(9).Take(9).Concat(source28.Take(13)));
				value4 = satellite.LongitudeOfAscendingNode * rad2Semi * 2147483648.0;
				satelliteIndex = 32;
				byte[] source29 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source28.Skip(13).Take(1).Concat(source29.Take(21)));
				value4 = satellite.ArgumentOfPerigee * rad2Semi * 2147483648.0;
				satelliteIndex = 32;
				byte[] source30 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
				AddWord(list, source29.Skip(21).Take(11).Concat(source30.Take(11)));
				AddWord(list, source30.Skip(11).Take(21).Concat(NavigationData.ZeroBits.Take(1)));
				break;
			}
			case 4:
			{
				satelliteIndex = 7;
				byte[] pageNumberBits = NavigationData.Dec2Bin(in value2, in satelliteIndex);
				int num6 = value2;
				AlmanacBase almanacBase4 = base.Almanac;
				satelliteIndex = num6 - 1;
				Satellite satellite3 = (almanacBase4.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
				{
					Id = num6,
					IsHealthy = false
				};
				AddAlmanacData(list, enumerable, pageNumberBits, satellite3);
				break;
			}
			case 5:
			{
				satelliteIndex = 7;
				byte[] array = NavigationData.Dec2Bin(in value2, in satelliteIndex);
				if ((long)value2 <= 6L)
				{
					int num5 = value2 + 24;
					AlmanacBase almanacBase2 = base.Almanac;
					satelliteIndex = num5 - 1;
					Satellite satellite2 = (almanacBase2.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
					{
						Id = num5,
						IsHealthy = false
					};
					AddAlmanacData(list, enumerable, array, satellite2);
				}
				else if ((long)value2 == 7)
				{
					satelliteIndex = 1;
					byte[] satelliteHealthBits = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(satelliteHealthBits.Take(2)));
					satelliteIndex = 2;
					byte[] satelliteHealthBits2 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 3;
					byte[] satelliteHealthBits3 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits.Skip(2).Take(7).Concat(satelliteHealthBits2)
						.Concat(satelliteHealthBits3.Take(6)));
					satelliteIndex = 4;
					byte[] satelliteHealthBits4 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 5;
					byte[] satelliteHealthBits5 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 6;
					byte[] satelliteHealthBits6 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits3.Skip(6).Take(3).Concat(satelliteHealthBits4)
						.Concat(satelliteHealthBits5)
						.Concat(satelliteHealthBits6.Take(1)));
					satelliteIndex = 7;
					byte[] satelliteHealthBits7 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 8;
					byte[] satelliteHealthBits8 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits6.Skip(1).Take(8).Concat(satelliteHealthBits7)
						.Concat(satelliteHealthBits8.Take(5)));
					satelliteIndex = 9;
					byte[] satelliteHealthBits9 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 10;
					byte[] satelliteHealthBits10 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits8.Skip(5).Take(4).Concat(satelliteHealthBits9)
						.Concat(satelliteHealthBits10));
					satelliteIndex = 11;
					byte[] satelliteHealthBits11 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 12;
					byte[] satelliteHealthBits12 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 13;
					byte[] satelliteHealthBits13 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits11.Concat(satelliteHealthBits12).Concat(satelliteHealthBits13.Take(4)));
					satelliteIndex = 14;
					byte[] satelliteHealthBits14 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 15;
					byte[] satelliteHealthBits15 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits13.Skip(4).Take(5).Concat(satelliteHealthBits14)
						.Concat(satelliteHealthBits15.Take(8)));
					satelliteIndex = 16;
					byte[] satelliteHealthBits16 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 17;
					byte[] satelliteHealthBits17 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 18;
					byte[] satelliteHealthBits18 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits15.Skip(8).Take(1).Concat(satelliteHealthBits16)
						.Concat(satelliteHealthBits17)
						.Concat(satelliteHealthBits18.Take(3)));
					satelliteIndex = 19;
					byte[] satelliteHealthBits19 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits18.Skip(3).Take(6).Concat(satelliteHealthBits19)
						.Concat(NavigationData.ZeroAndOneBits.Take(7)));
				}
				else if ((long)value2 == 8)
				{
					satelliteIndex = 20;
					byte[] satelliteHealthBits20 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(satelliteHealthBits20.Take(2)));
					satelliteIndex = 21;
					byte[] satelliteHealthBits21 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 22;
					byte[] satelliteHealthBits22 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits20.Skip(2).Take(7).Concat(satelliteHealthBits21)
						.Concat(satelliteHealthBits22.Take(6)));
					satelliteIndex = 23;
					byte[] satelliteHealthBits23 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 24;
					byte[] satelliteHealthBits24 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 25;
					byte[] satelliteHealthBits25 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits22.Skip(6).Take(3).Concat(satelliteHealthBits23)
						.Concat(satelliteHealthBits24)
						.Concat(satelliteHealthBits25.Take(1)));
					satelliteIndex = 26;
					byte[] satelliteHealthBits26 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 27;
					byte[] satelliteHealthBits27 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits25.Skip(1).Take(8).Concat(satelliteHealthBits26)
						.Concat(satelliteHealthBits27.Take(5)));
					satelliteIndex = 28;
					byte[] satelliteHealthBits28 = GetSatelliteHealthBits(in satelliteIndex);
					satelliteIndex = 29;
					byte[] satelliteHealthBits29 = GetSatelliteHealthBits(in satelliteIndex);
					AddWord(list, satelliteHealthBits27.Skip(5).Take(4).Concat(satelliteHealthBits28)
						.Concat(satelliteHealthBits29));
					satelliteIndex = 30;
					byte[] satelliteHealthBits30 = GetSatelliteHealthBits(in satelliteIndex);
					AlmanacBase almanacBase3 = base.Almanac;
					satelliteIndex = base.SatelliteIndex;
					Satellite obj = (almanacBase3.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? base.Almanac.BaselineSatellites.Select((SatelliteBase s) => s as Satellite).FirstOrDefault((Satellite s) => s?.IsHealthy ?? false);
					satelliteIndex = obj?.Week ?? 0;
					int bitCount = 8;
					byte[] second2 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					double value4 = (((double?)obj?.TimeOfApplicability) ?? 0.0) * 0.000244140625;
					satelliteIndex = 8;
					byte[] source2 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, satelliteHealthBits30.Concat(second2).Concat(source2.Take(5)));
					AddWord(list, source2.Skip(5).Take(3).Concat(NavigationData.ZeroAndOneBits.Take(19)));
					AddWord(list, NavigationData.OneAndZeroBits.Take(22));
					AddWord(list, NavigationData.OneAndZeroBits.Take(22));
				}
				else if ((long)value2 == 9)
				{
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(NavigationData.ZeroBits.Take(2)));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					double value4 = satellite.A0GPS * 10.0;
					satelliteIndex = 14;
					byte[] second3 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = satellite.A1GPS * 10.0;
					satelliteIndex = 16;
					byte[] source3 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, NavigationData.ZeroBits.Take(6).Concat(second3).Concat(source3.Take(2)));
					value4 = satellite.A0Gal * 10.0;
					satelliteIndex = 14;
					byte[] source4 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, source3.Skip(2).Take(14).Concat(source4.Take(8)));
					value4 = satellite.A1Gal * 10.0;
					satelliteIndex = 16;
					byte[] second4 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, source4.Skip(8).Take(6).Concat(second4));
					value4 = satellite.A0GLO * 10.0;
					satelliteIndex = 14;
					byte[] first = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					value4 = satellite.A1GLO * 10.0;
					satelliteIndex = 16;
					byte[] source5 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, first.Concat(source5.Take(8)));
					AddWord(list, source5.Skip(8).Take(8).Concat(NavigationData.ZeroAndOneBits.Take(14)));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
				}
				else if ((long)value2 == 10)
				{
					NavigationDataB1I.GetLeapSecond(in transmissionTime, out var currentLeapSecond, out var nextLeapSecond, out var nextLeapSecondWeek, out var nextLeapSecondDay);
					satelliteIndex = 8;
					byte[] source6 = NavigationData.Dec2Bin(in currentLeapSecond, in satelliteIndex);
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(source6.Take(2)));
					satelliteIndex = 8;
					byte[] second5 = NavigationData.Dec2Bin(in nextLeapSecond, in satelliteIndex);
					satelliteIndex = 8;
					byte[] second6 = NavigationData.Dec2Bin(in nextLeapSecondWeek, in satelliteIndex);
					AddWord(list, source6.Skip(2).Take(6).Concat(second5)
						.Concat(second6));
					double value4 = satellite.A0UTC * 1073741824.0;
					satelliteIndex = 32;
					byte[] source7 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, source7.Take(22));
					value4 = satellite.A1UTC * 1125899906842624.0;
					satelliteIndex = 24;
					byte[] source8 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					AddWord(list, source7.Skip(22).Take(10).Concat(source8.Take(12)));
					satelliteIndex = 8;
					byte[] second7 = NavigationData.Dec2Bin(in nextLeapSecondDay, in satelliteIndex);
					AddWord(list, source8.Skip(12).Take(12).Concat(second7)
						.Concat(NavigationData.ZeroAndOneBits.Take(2)));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
				}
				else
				{
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(NavigationData.ZeroBits.Take(2)));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
				}
				break;
			}
			default:
				RLLogger.GetLogger().LogMessage("Subframe counter out of whack.  Need help.");
				break;
			}
			value = ++num4 % 5 + 1;
			if ((long)value == 1)
			{
				value2 = value2 % 24 + 1;
			}
		}
		int index = (int)Math.Round(seconds * (double)Info.BitRate);
		int num7 = (int)Math.Round(base.Interval.Width.Seconds * (double)Info.BitRate);
		byte[] array2 = new byte[num7];
		list.CopyTo(index, array2, 0, num7);
		return array2;
	}

	protected sealed override void AddEmptyAlmanacData(List<byte> data, IEnumerable<byte> pageNumberBits, IEnumerable<byte> sowLsb, Satellite satellite)
	{
		AddWord(data, sowLsb.Concat(NavigationData.ZeroBits.Take(1)).Concat(pageNumberBits).Concat(NavigationData.ZeroBits.Take(2)));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(22).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(44).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(66).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(88).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(110).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(132).Take(22));
		AddWord(data, NavigationDataB1I.pattern10000000Bits.Skip(154).Take(22));
	}
}
