using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.BeiDou
{
	internal sealed class NavigationDataB1ID2 : NavigationDataB1I
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.BeiDouD2);

		private const int subframe1PagesPerSuperframe = 10;

		private const int subframe234PagesPerSuperframe = 6;

		private const int subframe5PagesPerSuperframe = 120;

		private const int frameLength = 3;

		private const int subframesPerFrame = 5;

		private const double subframeLength = 0.6;

		private const int framesPerWeek = 201600;

		private const uint aodc = 0u;

		private const uint aode = 1u;

		private static readonly byte[] satH2Bits = new byte[2] { 0, 1 };

		private const uint ruraiGood = 4u;

		private const uint ruraiBad = 15u;

		private const uint udrei = 15u;

		private const double deltaTOK = 0.0;

		private const double deltaTUnused = 0.0;

		private const double dTau = 63.875;

		private const uint givei = 15u;

		private const double scaleDeltaT = 10.0;

		private const double scaleDTau = 8.0;

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

		public NavigationDataB1ID2(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> allSignals)
			: base(in satIndex, almanac, in interval, allSignals)
		{
		}

		public sealed override byte[] Generate()
		{
			int num18 = base.Interval.Start.BeiDouSecondOfWeek / 3;
			int second = num18 * 3;
			int beiDouWeek = base.Interval.Start.BeiDouWeek;
			GnssTime gnssTime = GnssTime.FromBeiDou(beiDouWeek, second);
			double seconds = (base.Interval.Start - gnssTime).Seconds;
			int num12 = (int)((base.Interval.End - gnssTime).Seconds / 0.6).SafeCeiling();
			int num13 = num18 * 5;
			int value = 1;
			int value3 = num18 % 10 + 1;
			int value4 = num18 % 6 + 1;
			int value5 = num18 % 120 + 1;
			int num14 = num13 / 5;
			List<byte> list = new List<byte>((int)Math.Round((double)num12 * 0.6 * (double)BitRate));
			for (int i = 0; i < num12; i++)
			{
				GnssTime transmissionTime = GnssTime.FromBeiDou(beiDouWeek, num14 * 3);
				AlmanacBase almanacBase = base.Almanac;
				int satelliteIndex = base.SatelliteIndex;
				Satellite satellite = (almanacBase.GetEphemeris(in satelliteIndex, SignalType.BeiDouB1I, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "BeiDou", base.SatelliteIndex + 1));
				int value6 = num14 % 201600 * 3;
				satelliteIndex = 20;
				byte[] source47 = NavigationData.Dec2Bin(in value6, in satelliteIndex);
				IEnumerable<byte> second12 = source47.Take(8);
				IEnumerable<byte> enumerable = source47.Skip(8).Take(12);
				satelliteIndex = 3;
				byte[] second21 = NavigationData.Dec2Bin(in value, in satelliteIndex);
				AddFirstWord(list, NavigationDataB1I.preambleAndRevBits.Concat(second21).Concat(second12));
				switch (value)
				{
				case 1:
				{
					satelliteIndex = 4;
					byte[] second2 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					IEnumerable<byte> rawWord;
					switch (value3)
					{
					case 1:
					{
						byte element = (byte)((!satellite.IsHealthy) ? 1 : 0);
						uint value8 = 0u;
						satelliteIndex = 5;
						byte[] second13 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Append(element).Concat(second13));
						value8 = 0u;
						satelliteIndex = 4;
						byte[] first4 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
						satelliteIndex = satellite.Week;
						int bitCount = 13;
						byte[] second14 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
						double value7 = (double)satellite.TimeOfApplicability * 0.125;
						satelliteIndex = 17;
						byte[] source37 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, first4.Concat(second14).Concat(source37.Take(5)));
						IEnumerable<byte> second15 = NavigationData.ZeroBits.Take(10);
						AddWord(list, source37.Skip(5).Take(12).Concat(second15));
						IEnumerable<byte> first5 = NavigationData.ZeroBits.Take(10);
						AddWord(list, first5.Concat(NavigationData.ZeroBits.Take(6)).Concat(NavigationData.ZeroAndOneBits.Take(6)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 2:
					{
						double value7 = NavigationDataB1I.alphaParameters[0] * 1073741824.0;
						satelliteIndex = 8;
						byte[] source34 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source34.Take(6)));
						value7 = NavigationDataB1I.alphaParameters[1] * 134217728.0;
						satelliteIndex = 8;
						byte[] second7 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = NavigationDataB1I.alphaParameters[2] * 16777216.0;
						satelliteIndex = 8;
						byte[] second8 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = NavigationDataB1I.alphaParameters[3] * 16777216.0;
						satelliteIndex = 8;
						byte[] source35 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source34.Skip(6).Take(2).Concat(second7)
							.Concat(second8)
							.Concat(source35.Take(4)));
						value7 = NavigationDataB1I.betaParameters[0] * 0.00048828125;
						satelliteIndex = 8;
						byte[] second9 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = NavigationDataB1I.betaParameters[1] * 6.103515625E-05;
						satelliteIndex = 8;
						byte[] second10 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = NavigationDataB1I.betaParameters[2] * 1.52587890625E-05;
						satelliteIndex = 8;
						byte[] source36 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source35.Skip(4).Take(4).Concat(second9)
							.Concat(second10)
							.Concat(source36.Take(2)));
						value7 = NavigationDataB1I.betaParameters[3] * 1.52587890625E-05;
						satelliteIndex = 8;
						byte[] second11 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source36.Skip(2).Take(6).Concat(second11)
							.Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 3:
					{
						AddWord(list, enumerable.Concat(second2).Concat(NavigationData.ZeroBits.Take(6)));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						double value7 = satellite.A0 * 8589934592.0;
						satelliteIndex = 24;
						byte[] source32 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, NavigationData.ZeroBits.Take(10).Concat(source32.Take(12)));
						value7 = satellite.A1 * 1125899906842624.0;
						satelliteIndex = 22;
						byte[] source33 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source32.Skip(12).Take(12).Concat(source33.Take(4))
							.Concat(NavigationData.ZeroAndOneBits.Take(6)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 4:
					{
						double value7 = satellite.A1 * 1125899906842624.0;
						satelliteIndex = 22;
						byte[] source28 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source28.Skip(4).Take(6)));
						value7 = satellite.A2 * 4.0;
						satelliteIndex = 11;
						byte[] source29 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source28.Skip(10).Take(12).Concat(source29.Take(10)));
						uint value8 = 1u;
						satelliteIndex = 5;
						byte[] second5 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
						value7 = satellite.MeanMotionCorrection * rad2Semi * 8796093022208.0;
						satelliteIndex = 16;
						byte[] second6 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source29.Skip(10).Take(1).Concat(second5)
							.Concat(second6));
						value7 = satellite.Cuc * 2147483648.0;
						satelliteIndex = 18;
						byte[] source30 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source30.Take(14).Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 5:
					{
						double value7 = satellite.Cuc * 2147483648.0;
						satelliteIndex = 18;
						byte[] source24 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.MeanAnomaly * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source25 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source24.Skip(14).Take(4)).Concat(source25.Take(2)));
						AddWord(list, source25.Skip(2).Take(22));
						value7 = satellite.Cus * 2147483648.0;
						satelliteIndex = 18;
						byte[] source26 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source25.Skip(24).Take(8).Concat(source26.Take(14)));
						value7 = satellite.Eccentricity * 8589934592.0;
						satelliteIndex = 32;
						byte[] source27 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source26.Skip(14).Take(4).Concat(source27.Take(10))
							.Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 6:
					{
						double value7 = satellite.Eccentricity * 8589934592.0;
						satelliteIndex = 32;
						byte[] source21 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source21.Skip(10).Take(6)));
						value7 = satellite.SqrtA * 524288.0;
						satelliteIndex = 32;
						byte[] source22 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source21.Skip(16).Take(16).Concat(source22.Take(6)));
						AddWord(list, source22.Skip(6).Take(22));
						value7 = satellite.Cis * 2147483648.0;
						satelliteIndex = 18;
						byte[] source23 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source22.Skip(28).Take(4).Concat(source23.Take(10))
							.Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 7:
					{
						double value7 = satellite.Cis * 2147483648.0;
						satelliteIndex = 18;
						byte[] source17 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source17.Skip(10).Take(6)));
						value7 = satellite.Cis * 2147483648.0;
						satelliteIndex = 18;
						byte[] second4 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = (double)satellite.TimeOfApplicability * 0.125;
						satelliteIndex = 17;
						byte[] source18 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source17.Skip(16).Take(2).Concat(second4)
							.Concat(source18.Take(2)));
						value7 = satellite.Inclination * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source19 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source18.Skip(2).Take(15).Concat(source19.Take(7)));
						AddWord(list, source19.Skip(7).Take(14).Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 8:
					{
						double value7 = satellite.Inclination * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source14 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source14.Skip(21).Take(6)));
						value7 = satellite.Crs * 64.0;
						satelliteIndex = 18;
						byte[] source15 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source14.Skip(27).Take(5).Concat(source15.Take(17)));
						value7 = satellite.Crs * 64.0;
						satelliteIndex = 18;
						byte[] second3 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 8796093022208.0;
						satelliteIndex = 24;
						byte[] source16 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source15.Skip(17).Take(1).Concat(second3)
							.Concat(source16.Take(3)));
						AddWord(list, source16.Skip(3).Take(16).Concat(NavigationData.ZeroAndOneBits.Take(6)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					case 9:
					{
						double value7 = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 8796093022208.0;
						satelliteIndex = 24;
						byte[] source11 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.LongitudeOfAscendingNode * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source12 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source11.Skip(19).Take(5)).Concat(source12.Take(1)));
						AddWord(list, source12.Skip(1).Take(22));
						value7 = satellite.ArgumentOfPerigee * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source13 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source12.Skip(23).Take(9).Concat(source13.Take(13)));
						AddWord(list, source13.Skip(13).Take(14).Concat(NavigationData.ZeroAndOneBits.Take(8)));
						rawWord = NavigationData.ZeroAndOneBits.Take(22);
						break;
					}
					default:
					{
						double value7 = satellite.ArgumentOfPerigee * rad2Semi * 2147483648.0;
						satelliteIndex = 32;
						byte[] source46 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.RateOfInclination * rad2Semi * 8796093022208.0;
						satelliteIndex = 14;
						byte[] source10 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, enumerable.Concat(second2).Concat(source46.Skip(27).Take(5)).Concat(source10.Take(1)));
						AddWord(list, source10.Skip(1).Take(13).Concat(NavigationData.ZeroAndOneBits.Take(9)));
						rawWord = NavigationData.OneAndZeroBits.Take(22);
						AddWord(list, rawWord);
						AddWord(list, rawWord);
						break;
					}
					}
					AddWord(list, rawWord);
					AddWord(list, rawWord);
					AddWord(list, rawWord);
					AddWord(list, rawWord);
					AddWord(list, rawWord);
					break;
				}
				case 2:
				{
					satelliteIndex = 4;
					byte[] second16 = NavigationData.Dec2Bin(in value4, in satelliteIndex);
					byte[] source38 = (from h in satelliteHealth.Take(30)
						select (byte)(h ? 1 : 0)).ToArray(30);
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(second16).Concat(satH2Bits)
						.Concat(source38.Take(3)));
					AddWord(list, source38.Skip(3).Take(22));
					AddWord(list, source38.Skip(25).Take(5).Concat(NavigationData.ZeroBits.Take(17)));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					uint value8 = 15u;
					satelliteIndex = 4;
					byte[] array4 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
					AddWord(list, NavigationData.ZeroBits.Take(21).Concat(array4.Take(1)));
					AddWord(list, array4.Skip(1).Take(3).Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4.Take(3)));
					AddWord(list, array4.Skip(3).Take(1).Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4.Take(1)));
					AddWord(list, array4.Skip(1).Take(3).Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4)
						.Concat(array4.Take(3)));
					int num16 = 3 * (value4 - 1);
					bool num19 = satelliteHealth[num16];
					uint value9 = (num19 ? 4u : 15u);
					satelliteIndex = 4;
					byte[] second17 = NavigationData.Dec2Bin(in value9, in satelliteIndex);
					double value7 = (num19 ? 0.0 : 0.0) * 10.0;
					satelliteIndex = 13;
					byte[] second18 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
					AddWord(list, array4.Skip(3).Take(1).Concat(array4)
						.Concat(second17)
						.Concat(second18));
					break;
				}
				case 3:
				{
					int num17 = 3 * (value4 - 1) + 1;
					bool num20 = satelliteHealth[num17];
					uint value10 = (num20 ? 4u : 15u);
					satelliteIndex = 4;
					byte[] second19 = NavigationData.Dec2Bin(in value10, in satelliteIndex);
					double value7 = (num20 ? 0.0 : 0.0) * 10.0;
					satelliteIndex = 13;
					byte[] source39 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(second19).Concat(source39.Take(5)));
					int num10 = 3 * (value4 - 1) + 2;
					bool num21 = satelliteHealth[num10];
					uint value2 = (num21 ? 4u : 15u);
					satelliteIndex = 4;
					byte[] second20 = NavigationData.Dec2Bin(in value2, in satelliteIndex);
					value7 = (num21 ? 0.0 : 0.0) * 10.0;
					satelliteIndex = 13;
					byte[] source40 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
					AddWord(list, source39.Skip(5).Take(8).Concat(second20)
						.Concat(source40.Take(10)));
					AddWord(list, source40.Skip(10).Take(3).Concat(NavigationData.ZeroBits.Take(19)));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					break;
				}
				case 4:
					AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(NavigationData.ZeroBits.Take(9)));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					break;
				case 5:
				{
					satelliteIndex = 7;
					byte[] array = NavigationData.Dec2Bin(in value5, in satelliteIndex);
					if ((long)value5 <= 12L || ((long)value5 >= 61L && (long)value5 <= 72L))
					{
						double value7 = 511.0;
						satelliteIndex = 9;
						byte[] first6 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						uint value8 = 15u;
						int bitCount = 4;
						byte[] array2 = first6.Concat(NavigationData.Dec2Bin(in value8, in bitCount)).ToArray(13);
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(array2.Take(2)));
						AddWord(list, array2.Skip(2).Take(11).Concat(array2.Take(11)));
						AddWord(list, array2.Skip(11).Take(2).Concat(array2)
							.Concat(array2.Take(7)));
						AddWord(list, array2.Skip(7).Take(6).Concat(array2)
							.Concat(array2.Take(3)));
						AddWord(list, array2.Skip(3).Take(10).Concat(array2.Take(12)));
						AddWord(list, array2.Skip(12).Take(1).Concat(array2)
							.Concat(array2.Take(8)));
						AddWord(list, array2.Skip(8).Take(5).Concat(array2)
							.Concat(array2.Take(4)));
						AddWord(list, array2.Skip(4).Take(9).Concat(array2));
						AddWord(list, array2.Concat(NavigationData.ZeroBits.Take(9)));
					}
					else if ((long)value5 == 13 || (long)value5 == 73)
					{
						double value7 = 511.0;
						satelliteIndex = 9;
						byte[] first7 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						uint value8 = 15u;
						int bitCount = 4;
						byte[] array3 = first7.Concat(NavigationData.Dec2Bin(in value8, in bitCount)).ToArray(13);
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(array3.Take(2)));
						AddWord(list, array3.Skip(2).Take(11).Concat(array3.Take(11)));
						AddWord(list, array3.Skip(11).Take(2).Concat(array3)
							.Concat(array3.Take(7)));
						AddWord(list, array3.Skip(7).Take(6).Concat(NavigationData.ZeroBits.Take(16)));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						AddWord(list, NavigationData.ZeroBits.Take(22));
					}
					else if ((long)value5 == 35)
					{
						satelliteIndex = 1;
						byte[] satelliteHealthBits = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(satelliteHealthBits.Take(2)));
						satelliteIndex = 2;
						byte[] satelliteHealthBits12 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 3;
						byte[] satelliteHealthBits23 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits.Skip(2).Take(7).Concat(satelliteHealthBits12)
							.Concat(satelliteHealthBits23.Take(6)));
						satelliteIndex = 4;
						byte[] satelliteHealthBits25 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 5;
						byte[] satelliteHealthBits26 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 6;
						byte[] satelliteHealthBits27 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits23.Skip(6).Take(3).Concat(satelliteHealthBits25)
							.Concat(satelliteHealthBits26)
							.Concat(satelliteHealthBits27.Take(1)));
						satelliteIndex = 7;
						byte[] satelliteHealthBits28 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 8;
						byte[] satelliteHealthBits29 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits27.Skip(1).Take(8).Concat(satelliteHealthBits28)
							.Concat(satelliteHealthBits29.Take(5)));
						satelliteIndex = 9;
						byte[] satelliteHealthBits30 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 10;
						byte[] satelliteHealthBits2 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits29.Skip(5).Take(4).Concat(satelliteHealthBits30)
							.Concat(satelliteHealthBits2));
						satelliteIndex = 11;
						byte[] satelliteHealthBits3 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 12;
						byte[] satelliteHealthBits4 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 13;
						byte[] satelliteHealthBits5 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits3.Concat(satelliteHealthBits4).Concat(satelliteHealthBits5.Take(4)));
						satelliteIndex = 14;
						byte[] satelliteHealthBits6 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 15;
						byte[] satelliteHealthBits7 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits5.Skip(4).Take(5).Concat(satelliteHealthBits6)
							.Concat(satelliteHealthBits7.Take(8)));
						satelliteIndex = 16;
						byte[] satelliteHealthBits8 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 17;
						byte[] satelliteHealthBits9 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 18;
						byte[] satelliteHealthBits10 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits7.Skip(8).Take(1).Concat(satelliteHealthBits8)
							.Concat(satelliteHealthBits9)
							.Concat(satelliteHealthBits10.Take(3)));
						satelliteIndex = 19;
						byte[] satelliteHealthBits11 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits10.Skip(3).Take(6).Concat(satelliteHealthBits11)
							.Concat(NavigationData.ZeroBits.Take(7)));
					}
					else if ((long)value5 == 36)
					{
						satelliteIndex = 20;
						byte[] satelliteHealthBits13 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(satelliteHealthBits13.Take(2)));
						satelliteIndex = 21;
						byte[] satelliteHealthBits14 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 22;
						byte[] satelliteHealthBits15 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits13.Skip(2).Take(7).Concat(satelliteHealthBits14)
							.Concat(satelliteHealthBits15.Take(6)));
						satelliteIndex = 23;
						byte[] satelliteHealthBits16 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 24;
						byte[] satelliteHealthBits17 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 25;
						byte[] satelliteHealthBits18 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits15.Skip(6).Take(3).Concat(satelliteHealthBits16)
							.Concat(satelliteHealthBits17)
							.Concat(satelliteHealthBits18.Take(1)));
						satelliteIndex = 26;
						byte[] satelliteHealthBits19 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 27;
						byte[] satelliteHealthBits20 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits18.Skip(1).Take(8).Concat(satelliteHealthBits19)
							.Concat(satelliteHealthBits20.Take(5)));
						satelliteIndex = 28;
						byte[] satelliteHealthBits21 = GetSatelliteHealthBits(in satelliteIndex);
						satelliteIndex = 29;
						byte[] satelliteHealthBits22 = GetSatelliteHealthBits(in satelliteIndex);
						AddWord(list, satelliteHealthBits20.Skip(5).Take(4).Concat(satelliteHealthBits21)
							.Concat(satelliteHealthBits22));
						satelliteIndex = 30;
						byte[] satelliteHealthBits24 = GetSatelliteHealthBits(in satelliteIndex);
						AlmanacBase almanacBase2 = base.Almanac;
						satelliteIndex = base.SatelliteIndex;
						Satellite obj = (almanacBase2.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? base.Almanac.BaselineSatellites.Select((SatelliteBase s) => s as Satellite).FirstOrDefault((Satellite s) => s?.IsHealthy ?? false);
						satelliteIndex = obj?.Week ?? 0;
						int bitCount = 8;
						byte[] second22 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
						double value7 = (double)(obj?.TimeOfApplicability ?? 0) * 0.000244140625;
						satelliteIndex = 8;
						byte[] source20 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, satelliteHealthBits24.Concat(second22).Concat(source20.Take(5)));
						AddWord(list, source20.Skip(5).Take(3).Concat(NavigationData.ZeroAndOneBits.Take(19)));
						AddWord(list, NavigationData.OneAndZeroBits.Take(22));
						AddWord(list, NavigationData.OneAndZeroBits.Take(22));
					}
					else if (((long)value5 >= 37L && (long)value5 <= 60L) || ((long)value5 >= 95L && (long)value5 <= 100L))
					{
						int num15 = (int)(((long)value5 <= 60L) ? ((long)value5 - 36L) : ((long)value5 - 70L));
						AlmanacBase almanacBase3 = base.Almanac;
						satelliteIndex = num15 - 1;
						Satellite satellite2 = (almanacBase3.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? new Satellite
						{
							Id = num15,
							IsHealthy = false
						};
						AddAlmanacData(list, enumerable, array, satellite2);
					}
					else if ((long)value5 == 101)
					{
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(NavigationData.ZeroBits.Take(2)));
						AddWord(list, NavigationData.ZeroBits.Take(22));
						double value7 = satellite.A0GPS * 10.0;
						satelliteIndex = 14;
						byte[] second23 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.A1GPS * 10.0;
						satelliteIndex = 16;
						byte[] source31 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, NavigationData.ZeroBits.Take(6).Concat(second23).Concat(source31.Take(2)));
						value7 = satellite.A0Gal * 10.0;
						satelliteIndex = 14;
						byte[] source41 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source31.Skip(2).Take(14).Concat(source41.Take(8)));
						value7 = satellite.A1Gal * 10.0;
						satelliteIndex = 16;
						byte[] second24 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source41.Skip(8).Take(6).Concat(second24));
						value7 = satellite.A0GLO * 10.0;
						satelliteIndex = 14;
						byte[] first3 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						value7 = satellite.A1GLO * 10.0;
						satelliteIndex = 16;
						byte[] source42 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, first3.Concat(source42.Take(8)));
						AddWord(list, source42.Skip(8).Take(8).Concat(NavigationData.ZeroAndOneBits.Take(14)));
						AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
						AddWord(list, NavigationData.ZeroAndOneBits.Take(22));
					}
					else if ((long)value5 == 102)
					{
						NavigationDataB1I.GetLeapSecond(in transmissionTime, out var currentLeapSecond, out var nextLeapSecond, out var nextLeapSecondWeek, out var nextLeapSecondDay);
						satelliteIndex = 8;
						byte[] source43 = NavigationData.Dec2Bin(in currentLeapSecond, in satelliteIndex);
						AddWord(list, enumerable.Concat(NavigationData.ZeroBits.Take(1)).Concat(array).Concat(source43.Take(2)));
						satelliteIndex = 8;
						byte[] second25 = NavigationData.Dec2Bin(in nextLeapSecond, in satelliteIndex);
						satelliteIndex = 8;
						byte[] second26 = NavigationData.Dec2Bin(in nextLeapSecondWeek, in satelliteIndex);
						AddWord(list, source43.Skip(2).Take(6).Concat(second25)
							.Concat(second26));
						double value7 = satellite.A0UTC * 1073741824.0;
						satelliteIndex = 32;
						byte[] source44 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source44.Take(22));
						value7 = satellite.A1UTC * 1125899906842624.0;
						satelliteIndex = 24;
						byte[] source45 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWord(list, source44.Skip(22).Take(10).Concat(source45.Take(12)));
						satelliteIndex = 8;
						byte[] second27 = NavigationData.Dec2Bin(in nextLeapSecondDay, in satelliteIndex);
						AddWord(list, source45.Skip(12).Take(12).Concat(second27)
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
				value = ++num13 % 5 + 1;
				if ((long)value == 1)
				{
					num14++;
					value3 = value3 % 10 + 1;
					value4 = value4 % 6 + 1;
					value5 = value5 % 120 + 1;
				}
			}
			int index = (int)Math.Round(seconds * (double)Info.BitRate);
			int num11 = (int)Math.Round(base.Interval.Width.Seconds * (double)Info.BitRate);
			byte[] array5 = new byte[num11];
			list.CopyTo(index, array5, 0, num11);
			return array5;
		}

		protected sealed override void AddEmptyAlmanacData(List<byte> data, IEnumerable<byte> pageNumberBits, IEnumerable<byte> sowLsb, Satellite satellite)
		{
			AddWord(data, sowLsb.Concat(NavigationData.ZeroBits.Take(1)).Concat(pageNumberBits).Concat(NavigationData.ZeroBits.Take(2)));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
			AddWord(data, NavigationData.ZeroBits.Take(22));
		}
	}
}
