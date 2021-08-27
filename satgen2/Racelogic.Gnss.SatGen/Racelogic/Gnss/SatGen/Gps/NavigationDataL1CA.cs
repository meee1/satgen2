using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Gps
{
	internal sealed class NavigationDataL1CA : NavigationData
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GpsL1CA);

		private byte[] lastWord = new byte[30];

		private static readonly double rad2Semi = 1.0 / Constellation.Datum.PI;

		private const int frameLength = 30;

		private const int subframeLength = 6;

		private const int subFramesPerFrame = 5;

		private const int subframesPerWeek = 100800;

		private const int pagesPerFrame = 25;

		private const int zCountWeekLimit = 100800;

		private static readonly byte[] dataIdBits = new byte[2] { 0, 1 };

		private static readonly byte[] erdBits = new byte[6] { 1, 0, 0, 0, 0, 0 };

		private static readonly byte[] satHealthOKBits = new byte[6];

		private static readonly byte[] satHealthBadBits = new byte[6] { 1, 1, 1, 1, 1, 1 };

		private const byte integrityFlag = 1;

		private const byte tlmReservedBit = 0;

		private static readonly byte[] preambleBits = new byte[8] { 1, 0, 0, 0, 1, 0, 1, 1 };

		private static readonly byte[] tlmMessageBits = new byte[14]
		{
			1, 0, 1, 0, 1, 0, 1, 0, 1, 0,
			1, 0, 1, 0
		};

		private static readonly byte[] tlmWordWithoutParityBits = preambleBits.Concat(tlmMessageBits).Append<byte>(1).Append<byte>(0)
			.ToArray(24);

		private const byte howAlertFlag = 0;

		private static readonly byte[] codesOnL2Bits = new byte[2] { 0, 1 };

		private static readonly byte[] uraiBits = new byte[4];

		private static readonly byte[] nmctBadBits = new byte[5] { 1, 1, 1, 1, 1 };

		private static readonly IReadOnlyList<double> alphaParameters = Klobuchar.Alpha;

		private static readonly byte[] alpha0Bits;

		private static readonly byte[] alpha1Bits;

		private static readonly byte[] alpha2Bits;

		private static readonly byte[] alpha3Bits;

		private static readonly IReadOnlyList<double> betaParameters;

		private static readonly byte[] beta0Bits;

		private static readonly byte[] beta1Bits;

		private static readonly byte[] beta2Bits;

		private static readonly byte[] beta3Bits;

		private static readonly byte[][] parityBitOptions;

		private static readonly int[] subframe4SatIds;

		private static readonly byte[] svIdFrame5Subframe25Bits;

		private const double scaleT0c = 0.0625;

		private const double scaleDeltaN = 8796093022208.0;

		private const double scaleM0 = 2147483648.0;

		private const double scaleE = 8589934592.0;

		private const double scaleSqrtA = 524288.0;

		private const double scaleT0e = 0.0625;

		private const double scaleOmega0 = 2147483648.0;

		private const double scaleI0 = 2147483648.0;

		private const double scalePerigee = 2147483648.0;

		private const double scaleOmegaDot = 8796093022208.0;

		private const double scaleT0a = 0.000244140625;

		private const double scaleT0t = 0.000244140625;

		private const double scaleAlpha0 = 1073741824.0;

		private const double scaleAlpha1 = 134217728.0;

		private const double scaleAlpha2 = 16777216.0;

		private const double scaleAlpha3 = 16777216.0;

		private const double scaleBeta0 = 0.00048828125;

		private const double scaleBeta1 = 6.103515625E-05;

		private const double scaleBeta2 = 1.52587890625E-05;

		private const double scaleBeta3 = 1.52587890625E-05;

		private const double scaleAlmanacE = 2097152.0;

		private const double scaleAlmanacT0a = 0.000244140625;

		private const double scaleAlmanacDeltaI = 524288.0;

		private const double scaleAlmanacOmegaDot = 274877906944.0;

		private const double scaleAlmanacSqrtA = 2048.0;

		private const double scaleAlmanacOmega0 = 8388608.0;

		private const double scaleAlmanacOmega = 8388608.0;

		private const double scaleAlmanacM0 = 8388608.0;

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

		public NavigationDataL1CA(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> allSignals)
			: base(in satIndex, almanac, in interval, allSignals)
		{
		}

		public sealed override byte[] Generate()
		{
			int num6 = base.Interval.Start.GpsSecondOfWeek / 6;
			int num2 = num6 * 6;
			int gpsWeek = base.Interval.Start.GpsWeek;
			GnssTime gnssTime = GnssTime.FromGps(gpsWeek, num2);
			double seconds = (base.Interval.Start - gnssTime).Seconds;
			int num3 = (int)((base.Interval.End - gnssTime).Seconds / 6.0).SafeCeiling();
			int num4 = num2 % 604800 / 6;
			int value = num6 % 5 + 1;
			int pageNumber = num2 / 30 % 25 + 1;
			byte b = (byte)((!base.SimulatedSignalTypes.Any((SignalType s) => s == SignalType.GpsL1P || s == SignalType.GpsL2P)) ? 1 : 0);
			List<byte> list = new List<byte>(num3 * 6 * BitRate);
			for (int i = 1; i <= num3; i++)
			{
				int value2 = (i + num4) % 100800;
				GnssTime transmissionTime = GnssTime.FromGps(gpsWeek, (i + num4 - 1) * 6);
				AlmanacBase almanacBase = base.Almanac;
				int satelliteIndex = base.SatelliteIndex;
				Satellite satellite = (almanacBase.GetEphemeris(in satelliteIndex, SignalType.GpsL1CA, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Gps", base.SatelliteIndex + 1));
				AddFirstWord(list, tlmWordWithoutParityBits);
				satelliteIndex = 17;
				byte[] source10 = NavigationData.Dec2Bin(in value2, in satelliteIndex);
				satelliteIndex = 3;
				IEnumerable<byte> rawWord = Enumerable.Concat(second: NavigationData.Dec2Bin(in value, in satelliteIndex), first: ((IEnumerable<byte>)source10).Append((byte)0).Append(b));
				AddWordWithParityMatchingBits(list, rawWord);
				switch (value)
				{
				case 1:
				{
					int value8 = satellite.Week;
					if (satellite.TimeOfApplicability == 0)
					{
						value8--;
					}
					satelliteIndex = 10;
					byte[] first4 = NavigationData.Dec2Bin(in value8, in satelliteIndex);
					byte[] second15 = satHealthOKBits;
					satelliteIndex = satellite.IssueOfDataClock;
					int bitCount = 10;
					byte[] source9 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
					AddWord(list, first4.Concat(codesOnL2Bits).Concat(uraiBits).Concat(second15)
						.Concat(source9.Take(2)));
					byte[] first5 = new byte[1] { (byte)((!base.SimulatedSignalTypes.Contains(SignalType.GpsL2P)) ? 1 : 0) };
					AddWord(list, first5.Concat(NavigationData.OneAndZeroBits.Take(23)));
					AddWord(list, NavigationData.OneAndZeroBits.Take(24));
					AddWord(list, NavigationData.OneAndZeroBits.Take(24));
					IEnumerable<byte> second16 = NavigationData.ZeroBits.Take(8);
					AddWord(list, NavigationData.OneAndZeroBits.Take(16).Concat(second16));
					double value3 = (double)satellite.TimeOfApplicability * 0.0625;
					satelliteIndex = 16;
					byte[] second17 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, source9.Skip(2).Take(8).Concat(second17));
					IEnumerable<byte> first6 = NavigationData.ZeroBits.Take(8);
					IEnumerable<byte> second18 = NavigationData.ZeroBits.Take(16);
					AddWord(list, first6.Concat(second18));
					IEnumerable<byte> rawWord5 = NavigationData.ZeroBits.Take(22);
					AddWordWithParityMatchingBits(list, rawWord5);
					break;
				}
				case 2:
				{
					IEnumerable<byte> second12 = NavigationData.ZeroBits.Take(16);
					satelliteIndex = satellite.IssueOfDataClock;
					int bitCount = 10;
					IEnumerable<byte> first12 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount).Skip(2).Take(8);
					AddWord(list, first12.Concat(second12));
					double value3 = satellite.MeanMotionCorrection * rad2Semi * 8796093022208.0;
					satelliteIndex = 16;
					byte[] first13 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					value3 = satellite.MeanAnomaly * rad2Semi * 2147483648.0;
					satelliteIndex = 32;
					byte[] source6 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first13.Concat(source6.Take(8)));
					AddWord(list, source6.Skip(8).Take(24));
					IEnumerable<byte> first14 = NavigationData.ZeroBits.Take(16);
					value3 = satellite.Eccentricity * 8589934592.0;
					satelliteIndex = 32;
					byte[] source7 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first14.Concat(source7.Take(8)));
					AddWord(list, source7.Skip(8).Take(24));
					IEnumerable<byte> first2 = NavigationData.ZeroBits.Take(16);
					value3 = satellite.SqrtA * 524288.0;
					satelliteIndex = 32;
					byte[] source8 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first2.Concat(source8.Take(8)));
					AddWord(list, source8.Skip(8).Take(24));
					value3 = (double)satellite.TimeOfApplicability * 0.0625;
					satelliteIndex = 16;
					byte[] first3 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					IEnumerable<byte> second13 = NavigationData.ZeroBits.Take(1);
					IEnumerable<byte> second14 = nmctBadBits;
					AddWordWithParityMatchingBits(list, first3.Concat(second13).Concat(second14));
					break;
				}
				case 3:
				{
					IEnumerable<byte> first8 = NavigationData.ZeroBits.Take(16);
					double value3 = satellite.LongitudeOfAscendingNode * rad2Semi * 2147483648.0;
					satelliteIndex = 32;
					byte[] source3 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first8.Concat(source3.Take(8)));
					AddWord(list, source3.Skip(8).Take(24));
					IEnumerable<byte> first9 = NavigationData.ZeroBits.Take(16);
					value3 = satellite.Inclination * rad2Semi * 2147483648.0;
					satelliteIndex = 32;
					byte[] source4 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first9.Concat(source4.Take(8)));
					AddWord(list, source4.Skip(8).Take(24));
					IEnumerable<byte> first10 = NavigationData.ZeroBits.Take(16);
					value3 = satellite.ArgumentOfPerigee * rad2Semi * 2147483648.0;
					satelliteIndex = 32;
					byte[] source5 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, first10.Concat(source5.Take(8)));
					AddWord(list, source5.Skip(8).Take(24));
					value3 = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 8796093022208.0;
					satelliteIndex = 24;
					byte[] rawWord4 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
					AddWord(list, rawWord4);
					IEnumerable<byte> second11 = NavigationData.ZeroBits.Take(14);
					satelliteIndex = satellite.IssueOfDataClock;
					int bitCount = 10;
					IEnumerable<byte> first11 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount).Skip(2).Take(8);
					AddWordWithParityMatchingBits(list, first11.Concat(second11));
					break;
				}
				case 4:
					switch (pageNumber)
					{
					case 1:
					case 6:
					case 11:
					case 12:
					case 14:
					case 15:
					case 16:
					case 17:
					case 19:
					case 20:
					case 21:
					case 22:
					case 23:
					case 24:
					{
						byte[] second10 = Subframe4SvIdFromPageId(in pageNumber);
						AddWord(list, dataIdBits.Concat(second10).Concat(NavigationData.OneAndZeroBits.Take(16)));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWord(list, NavigationData.OneAndZeroBits.Take(24));
						AddWordWithParityMatchingBits(list, NavigationData.OneAndZeroBits.Take(22));
						break;
					}
					case 18:
					{
						byte[] second22 = Subframe4SvIdFromPageId(in pageNumber);
						AddWord(list, dataIdBits.Concat(second22).Concat(alpha0Bits).Concat(alpha1Bits));
						AddWord(list, alpha2Bits.Concat(alpha3Bits).Concat(beta0Bits));
						AddWord(list, beta1Bits.Concat(beta2Bits).Concat(beta3Bits));
						IEnumerable<byte> rawWord3 = NavigationData.ZeroBits.Take(24);
						AddWord(list, rawWord3);
						IEnumerable<byte> source2 = NavigationData.ZeroBits.Take(32);
						AddWord(list, source2.Take(24));
						GnssTime start = satellite.TransmissionInterval.Start;
						GnssTime gnssTime2 = start + GnssTimeSpan.FromHours(70);
						double value3 = (double)gnssTime2.GpsSecondOfWeek * 0.000244140625;
						satelliteIndex = 8;
						byte[] second23 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
						satelliteIndex = gnssTime2.GpsWeek;
						int bitCount = 8;
						byte[] second24 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
						AddWord(list, source2.Skip(24).Take(8).Concat(second23)
							.Concat(second24));
						DateTime utcTime = start.UtcTime;
						LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime);
						int value4 = leapSecond.Seconds;
						LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime);
						GnssTime gnssTime3 = GnssTime.FromUtc(leapSecond2.Utc);
						if ((int)(gnssTime3 - start).Seconds > 15552000)
						{
							leapSecond2 = leapSecond;
							gnssTime3 = GnssTime.FromUtc(leapSecond.Utc);
						}
						GnssTime gnssTime4 = gnssTime3 - GnssTimeSpan.FromMinutes(1);
						int value5 = gnssTime4.GpsDayOfWeek + 1;
						int value6 = gnssTime4.GpsWeek;
						int value7 = leapSecond2.Seconds;
						satelliteIndex = 8;
						byte[] first = NavigationData.Dec2Bin(in value4, in satelliteIndex);
						satelliteIndex = 8;
						byte[] second25 = NavigationData.Dec2Bin(in value6, in satelliteIndex);
						satelliteIndex = 8;
						byte[] second26 = NavigationData.Dec2Bin(in value5, in satelliteIndex);
						AddWord(list, first.Concat(second25).Concat(second26));
						satelliteIndex = 8;
						byte[] first7 = NavigationData.Dec2Bin(in value7, in satelliteIndex);
						AddWordWithParityMatchingBits(list, first7.Concat(NavigationData.OneAndZeroBits.Take(14)));
						break;
					}
					case 25:
					{
						byte[] array;
						if (base.SimulatedSignalTypes.Contains(SignalType.GpsL1C))
						{
							array = new byte[4] { b, 1, 0, 0 };
						}
						array = ((base.SimulatedSignalTypes.Contains(SignalType.GpsL5I) || base.SimulatedSignalTypes.Contains(SignalType.GpsL5Q)) ? new byte[4] { b, 0, 1, 1 } : ((!base.SimulatedSignalTypes.Contains(SignalType.GpsL2C)) ? new byte[4] { b, 0, 0, 1 } : new byte[4] { b, 0, 1, 0 }));
						AddWord(list, dataIdBits.Concat(Subframe4SvIdFromPageId(in pageNumber)).Concat(array).Concat(array)
							.Concat(array)
							.Concat(array));
						for (int l = 4; l <= 7; l++)
						{
							AddWord(list, array.Concat(array).Concat(array).Concat(array)
								.Concat(array)
								.Concat(array));
						}
						AddWord(list, array.Concat(array).Concat(array).Concat(array)
							.Concat(NavigationData.ZeroBits.Take(2))
							.Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[24])));
						AddWord(list, Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[25]).Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[26])).Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[27])).Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[28])));
						AddWordWithParityMatchingBits(list, Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[29]).Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[30])).Concat(Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[31])).Concat(NavigationData.ZeroBits.Take(4)));
						break;
					}
					case 13:
					{
						byte[] second21 = new byte[2] { 1, 0 };
						AddWord(list, dataIdBits.Concat(Subframe4SvIdFromPageId(in pageNumber)).Concat(second21).Concat(erdBits)
							.Concat(erdBits)
							.Concat(erdBits.Take(2)));
						byte[] rawWord2 = erdBits.Skip(2).Take(4).Concat(erdBits)
							.Concat(erdBits)
							.Concat(erdBits)
							.Concat(erdBits.Take(2))
							.ToArray(24);
						AddWord(list, rawWord2);
						AddWord(list, rawWord2);
						AddWord(list, rawWord2);
						AddWord(list, rawWord2);
						AddWord(list, rawWord2);
						AddWord(list, rawWord2);
						AddWordWithParityMatchingBits(list, erdBits.Skip(2).Take(4).Concat(erdBits)
							.Concat(erdBits)
							.Concat(erdBits));
						break;
					}
					case 2:
						satelliteIndex = 25;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 3:
						satelliteIndex = 26;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 4:
						satelliteIndex = 27;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 5:
						satelliteIndex = 28;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 7:
						satelliteIndex = 29;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 8:
						satelliteIndex = 30;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 9:
						satelliteIndex = 31;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					case 10:
						satelliteIndex = 32;
						AddAlmanacData(list, in satelliteIndex, in transmissionTime);
						break;
					}
					break;
				case 5:
					if (pageNumber == 25)
					{
						AlmanacBase almanacBase2 = base.Almanac;
						satelliteIndex = base.SatelliteIndex;
						Satellite obj = (almanacBase2.GetAlmanac(in satelliteIndex, in transmissionTime) as Satellite) ?? base.Almanac.BaselineSatellites.Select((SatelliteBase s) => s as Satellite).FirstOrDefault((Satellite s) => s?.IsHealthy ?? false);
						double value3 = (((double?)obj?.TimeOfApplicability) ?? 0.0) * 0.000244140625;
						satelliteIndex = 8;
						byte[] second19 = NavigationData.Dec2Bin(in value3, in satelliteIndex);
						satelliteIndex = obj?.Week ?? 0;
						int bitCount = 8;
						byte[] second20 = NavigationData.Dec2Bin(in satelliteIndex, in bitCount);
						AddWord(list, dataIdBits.Concat(svIdFrame5Subframe25Bits).Concat(second19).Concat(second20));
						for (int j = 0; j < 6; j++)
						{
							List<byte> list2 = new List<byte>();
							for (int k = 0; k < 4; k++)
							{
								byte[] collection = Get6BitSatelliteHealth(base.Almanac.BaselineSatellites[j * 4 + k]);
								list2.AddRange(collection);
							}
							AddWord(list, list2);
						}
						AddWordWithParityMatchingBits(list, NavigationData.OneAndZeroBits.Take(22));
					}
					else
					{
						AddAlmanacData(list, in pageNumber, in transmissionTime);
					}
					pageNumber++;
					if (pageNumber > 25)
					{
						pageNumber = 1;
					}
					break;
				default:
					RLLogger.GetLogger().LogMessage("Subframe counter out of whack.  Need help.");
					break;
				}
				value++;
				if (value > 5)
				{
					value = 1;
				}
				if (value2 == 0)
				{
					pageNumber = 1;
					value = 1;
				}
			}
			int index = (int)Math.Round(seconds * (double)Info.BitRate);
			int num5 = (int)Math.Round(base.Interval.Width.Seconds * (double)Info.BitRate);
			byte[] array2 = new byte[num5];
			list.CopyTo(index, array2, 0, num5);
			return array2;
		}

		private void AddAlmanacData(List<byte> data, in int satId, in GnssTime subframeTime)
		{
			AlmanacBase almanacBase = base.Almanac;
			int num = satId - 1;
			Satellite satellite = almanacBase.GetAlmanac(in num, in subframeTime) as Satellite;
			if (satellite == null)
			{
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWord(data, NavigationData.OneAndZeroBits.Take(24));
				AddWordWithParityMatchingBits(data, NavigationData.OneAndZeroBits.Take(22));
				return;
			}
			num = satellite.Id;
			int bitCount = 6;
			byte[] second = NavigationData.Dec2Bin(in num, in bitCount);
			double value = satellite.Eccentricity * 2097152.0;
			num = 16;
			byte[] second2 = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, dataIdBits.Concat(second).Concat(second2));
			value = (double)satellite.TimeOfApplicability * 0.000244140625;
			num = 8;
			byte[] first = NavigationData.Dec2Bin(in value, in num);
			value = (satellite.Inclination * rad2Semi - 0.3) * 524288.0;
			num = 16;
			byte[] second3 = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, first.Concat(second3));
			value = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 274877906944.0;
			num = 16;
			byte[] first2 = NavigationData.Dec2Bin(in value, in num);
			uint value2 = (uint)satellite.Health;
			num = 3;
			byte[] second4 = NavigationData.Dec2Bin(in value2, in num).Concat(NavigationData.ZeroBits.Take(5)).ToArray(8);
			AddWord(data, first2.Concat(second4));
			value = satellite.SqrtA * 2048.0;
			num = 24;
			byte[] rawWord = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, rawWord);
			value = satellite.LongitudeOfAscendingNode * rad2Semi * 8388608.0;
			num = 24;
			byte[] rawWord2 = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, rawWord2);
			value = satellite.ArgumentOfPerigee * rad2Semi * 8388608.0;
			num = 24;
			byte[] rawWord3 = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, rawWord3);
			value = satellite.MeanAnomaly * rad2Semi * 8388608.0;
			num = 24;
			byte[] rawWord4 = NavigationData.Dec2Bin(in value, in num);
			AddWord(data, rawWord4);
			IEnumerable<byte> source = NavigationData.ZeroBits.Take(11);
			IEnumerable<byte> second5 = NavigationData.ZeroBits.Take(11);
			AddWordWithParityMatchingBits(data, source.Take(8).Concat(second5).Concat(source.Skip(8).Take(3)));
		}

		private static byte[] Subframe4SvIdFromPageId(in int pageNumber)
		{
			int value = subframe4SatIds[pageNumber - 1];
			int bitCount = 6;
			return NavigationData.Dec2Bin(in value, in bitCount);
		}

		private static byte[] Get6BitSatelliteHealth(SatelliteBase? sat)
		{
			Satellite satellite = sat as Satellite;
			if (satellite == null || !satellite.IsHealthy)
			{
				return satHealthBadBits;
			}
			return satHealthOKBits;
		}

		private void AddWordWithParityMatchingBits(List<byte> data, IEnumerable<byte> rawWord)
		{
			AddWord(data, SolveFor2BitsParity00(lastWord, rawWord.ToArray(22)));
		}

		protected sealed override IEnumerable<byte> EncodeWord(IEnumerable<byte> rawWord)
		{
			return lastWord = ParityGeneration(lastWord, rawWord.ToArray(24));
		}

		private static byte[] ParityGeneration(byte[] lastWord, byte[] nextWord)
		{
			byte[] array = new byte[30];
			int num = lastWord[28];
			int num2 = lastWord[29];
			for (int i = 0; i < 24; i++)
			{
				array[i] = (byte)(nextWord[i] ^ num2);
			}
			array[24] = (byte)((uint)(num + nextWord[0] + nextWord[1] + nextWord[2] + nextWord[4] + nextWord[5] + nextWord[9] + nextWord[10] + nextWord[11] + nextWord[12] + nextWord[13] + nextWord[16] + nextWord[17] + nextWord[19] + nextWord[22]) & 1u);
			array[25] = (byte)((uint)(num2 + nextWord[1] + nextWord[2] + nextWord[3] + nextWord[5] + nextWord[6] + nextWord[10] + nextWord[11] + nextWord[12] + nextWord[13] + nextWord[14] + nextWord[17] + nextWord[18] + nextWord[20] + nextWord[23]) & 1u);
			array[26] = (byte)((uint)(num + nextWord[0] + nextWord[2] + nextWord[3] + nextWord[4] + nextWord[6] + nextWord[7] + nextWord[11] + nextWord[12] + nextWord[13] + nextWord[14] + nextWord[15] + nextWord[18] + nextWord[19] + nextWord[21]) & 1u);
			array[27] = (byte)((uint)(num2 + nextWord[1] + nextWord[3] + nextWord[4] + nextWord[5] + nextWord[7] + nextWord[8] + nextWord[12] + nextWord[13] + nextWord[14] + nextWord[15] + nextWord[16] + nextWord[19] + nextWord[20] + nextWord[22]) & 1u);
			array[28] = (byte)((uint)(num2 + nextWord[0] + nextWord[2] + nextWord[4] + nextWord[5] + nextWord[6] + nextWord[8] + nextWord[9] + nextWord[13] + nextWord[14] + nextWord[15] + nextWord[16] + nextWord[17] + nextWord[20] + nextWord[21] + nextWord[23]) & 1u);
			array[29] = (byte)((uint)(num + nextWord[2] + nextWord[4] + nextWord[5] + nextWord[7] + nextWord[8] + nextWord[9] + nextWord[10] + nextWord[12] + nextWord[14] + nextWord[18] + nextWord[21] + nextWord[22] + nextWord[23]) & 1u);
			return array;
		}

		private static byte[] SolveFor2BitsParity00(byte[] lastWord, byte[] shortWord)
		{
			byte[] array = new byte[24];
			for (int i = 0; i < 22; i++)
			{
				array[i] = shortWord[i];
			}
			int num = 0;
			while (num < 3)
			{
				if (((lastWord[29] + array[0] + array[2] + array[4] + array[5] + array[6] + array[8] + array[9] + array[13] + array[14] + array[15] + array[16] + array[17] + array[20] + array[21] + array[23]) & 1) == 0 && ((lastWord[28] + array[2] + array[4] + array[5] + array[7] + array[8] + array[9] + array[10] + array[12] + array[14] + array[18] + array[21] + array[22] + array[23]) & 1) == 0)
				{
					return array;
				}
				num++;
				byte[] array2 = parityBitOptions[num];
				array[22] = array2[0];
				array[23] = array2[1];
			}
			return array;
		}

		static NavigationDataL1CA()
		{
			double value = alphaParameters[0] * 1073741824.0;
			int bitCount = 8;
			alpha0Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = alphaParameters[1] * 134217728.0;
			bitCount = 8;
			alpha1Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = alphaParameters[2] * 16777216.0;
			bitCount = 8;
			alpha2Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = alphaParameters[3] * 16777216.0;
			bitCount = 8;
			alpha3Bits = NavigationData.Dec2Bin(in value, in bitCount);
			betaParameters = Klobuchar.Beta;
			value = betaParameters[0] * 0.00048828125;
			bitCount = 8;
			beta0Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = betaParameters[1] * 6.103515625E-05;
			bitCount = 8;
			beta1Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = betaParameters[2] * 1.52587890625E-05;
			bitCount = 8;
			beta2Bits = NavigationData.Dec2Bin(in value, in bitCount);
			value = betaParameters[3] * 1.52587890625E-05;
			bitCount = 8;
			beta3Bits = NavigationData.Dec2Bin(in value, in bitCount);
			parityBitOptions = new byte[4][]
			{
				new byte[2],
				new byte[2] { 0, 1 },
				new byte[2] { 1, 0 },
				new byte[2] { 1, 1 }
			};
			subframe4SatIds = new int[25]
			{
				57, 25, 26, 27, 28, 57, 29, 30, 31, 32,
				57, 62, 52, 53, 54, 57, 55, 56, 58, 59,
				57, 60, 61, 62, 63
			};
			bitCount = 51;
			int bitCount2 = 6;
			svIdFrame5Subframe25Bits = NavigationData.Dec2Bin(in bitCount, in bitCount2);
		}
	}
}
