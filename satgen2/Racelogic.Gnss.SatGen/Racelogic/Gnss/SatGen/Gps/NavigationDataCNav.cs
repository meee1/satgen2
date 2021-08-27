using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Gps
{
	internal abstract class NavigationDataCNav : NavigationData
	{
		private static readonly double rad2Semi = 1.0 / Constellation.Datum.PI;

		protected const int packetLengthBits = 300;

		private readonly int packetLength;

		private readonly int packetsPerWeek;

		private static readonly CNavMessageType[] packetSequence = new CNavMessageType[4]
		{
			CNavMessageType.Ephemeris1,
			CNavMessageType.Ephemeris2,
			CNavMessageType.ClockIonoAndGroupDelay,
			CNavMessageType.ClockAndUTC
		};

		private static readonly byte[] preambleBits = new byte[8] { 1, 0, 0, 0, 1, 0, 1, 1 };

		private const byte alertFlag = 0;

		private static readonly byte[] tgdUnavailableBits = new byte[13]
		{
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0
		};

		private const double aReference = 26559710.0;

		private const double omegaDotReference = -2.6E-09;

		private const byte healthOK = 0;

		private const byte healthBad = 1;

		private static readonly byte[] uraEDBits;

		private static readonly byte[] uraNED0Bits;

		private const byte integrityStatusFlag = 1;

		private const byte l2CPhasingBit = 0;

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

		private const double scaleT0p = 0.0033333333333333335;

		private const double scaleDeltaA = 512.0;

		private const double scaleDeltaN0 = 17592186044416.0;

		private const double scaleM0n = 4294967296.0;

		private const double scaleEn = 17179869184.0;

		private const double scaleOmegaN = 4294967296.0;

		private const double scaleT0e = 0.0033333333333333335;

		private const double scaleOmega0n = 4294967296.0;

		private const double scaleDeltaOmegaDot = 17592186044416.0;

		private const double scaleI0n = 4294967296.0;

		private const double scaleT0c = 0.0033333333333333335;

		private const double scaleAlpha0 = 1073741824.0;

		private const double scaleAlpha1 = 134217728.0;

		private const double scaleAlpha2 = 16777216.0;

		private const double scaleAlpha3 = 16777216.0;

		private const double scaleBeta0 = 0.00048828125;

		private const double scaleBeta1 = 6.103515625E-05;

		private const double scaleBeta2 = 1.52587890625E-05;

		private const double scaleBeta3 = 1.52587890625E-05;

		private const double scaleT0t = 0.0625;

		protected abstract SignalType SignalType
		{
			[DebuggerStepThrough]
			get;
		}

		protected abstract int FirstPacketFecState
		{
			[DebuggerStepThrough]
			get;
		}

		protected abstract FixedSizeDictionary<int, int?[]> FecLibrary
		{
			[DebuggerStepThrough]
			get;
		}

		protected abstract SyncLock FecLock
		{
			[DebuggerStepThrough]
			get;
		}

		protected NavigationDataCNav(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> signals, in int packetLengthSeconds)
			: base(in satIndex, almanac, in interval, signals)
		{
			packetLength = packetLengthSeconds;
			packetsPerWeek = 604800 / packetLengthSeconds;
		}

		public sealed override byte[] Generate()
		{
			int num = base.Interval.Start.GpsSecondOfWeek / packetLength;
			int num13 = base.Interval.End.GpsSecondOfWeek / packetLength;
			int num6 = base.Interval.End.GpsWeek - base.Interval.Start.GpsWeek;
			int num7 = num13 - num + 1 + num6 * packetsPerWeek;
			int num8 = num * packetLength;
			double num9 = base.Interval.Start.GpsSecondOfWeek - num8;
			List<byte> list = new List<byte>(num7 * 300);
			int num10 = num + num7;
			int satelliteIndex;
			for (int i = num; i < num10; i++)
			{
				GnssTime transmissionTime = GnssTime.FromGps(base.Interval.Start.GpsWeek, i * packetLength);
				AlmanacBase almanacBase = base.Almanac;
				satelliteIndex = base.SatelliteIndex;
				Satellite ephemeris = (almanacBase.GetEphemeris(in satelliteIndex, SignalType, in transmissionTime) as Satellite) ?? throw new InvalidOperationException(string.Format("Ephemeris is not a {0} PRN{1} satellite", "Gps", base.SatelliteIndex + 1));
				GeneratePacketData(list, in i, ephemeris);
			}
			GnssTime time = base.Interval.Start;
			int packetIndex = GetPacketIndex(in time);
			time = base.Interval.End - GnssTimeSpan.FromSeconds(ConstellationBase.SignalTravelTimeLimits.Min);
			int packetIndex2 = GetPacketIndex(in time);
			int num11 = packetIndex2 - packetIndex;
			int captureIndex = (((long)num11 > 0L) ? (num11 * 300) : 0);
			satelliteIndex = FirstPacketFecState;
			using ConvolutionalEncoder convolutionalEncoder = new ConvolutionalEncoder(list, in satelliteIndex, in captureIndex);
			int num12 = (int)Math.Round(num9 * (double)BitRate);
			int num2 = (int)Math.Round(base.Interval.Width.Seconds * (double)BitRate);
			byte[] array = new byte[num2];
			int num3 = num12 + num2;
			int num4 = captureIndex << 1;
			int num5 = ((num3 > num4) ? num3 : (num4 + 1));
			using (IEnumerator<byte> enumerator = convolutionalEncoder.GetEnumerator())
			{
				for (int j = 0; j < num5; j++)
				{
					enumerator.MoveNext();
					if (j >= num12 && j < num3)
					{
						array[j - num12] = enumerator.Current;
					}
				}
			}
			if (convolutionalEncoder.CapturedState.HasValue)
			{
				satelliteIndex = base.SatelliteIndex;
				int fecState = convolutionalEncoder.CapturedState.Value;
				StoreFEC(in packetIndex2, in satelliteIndex, in fecState);
				return array;
			}
			return array;
		}

		private void GeneratePacketData(List<byte> data, in int packetIndexInWeek, Satellite ephemeris)
		{
			CNavMessageType cNavMessageType = packetSequence[packetIndexInWeek % packetSequence.Length];
			int value = base.SatelliteId;
			int bitCount = 6;
			byte[] second = NavigationData.Dec2Bin(in value, in bitCount);
			uint value4 = (uint)cNavMessageType;
			value = 6;
			byte[] second12 = NavigationData.Dec2Bin(in value4, in value);
			int value5 = (packetIndexInWeek + 1) * packetLength % 604800 / 6;
			value = 17;
			byte[] second23 = NavigationData.Dec2Bin(in value5, in value);
			IEnumerable<byte> first = preambleBits.Concat(second).Concat(second12).Concat(second23)
				.Append<byte>(0);
			switch (cNavMessageType)
			{
			default:
				AddWord(data, first.Concat(NavigationData.OneAndZeroBits.Take(238)));
				break;
			case CNavMessageType.Ephemeris1:
			{
				int value3 = ephemeris.Week;
				if (ephemeris.TimeOfApplicability == 0)
				{
					value3--;
				}
				value = 13;
				byte[] second42 = NavigationData.Dec2Bin(in value3, in value);
				byte b = (byte)((!base.SimulatedSignalTypes.Contains(SignalType.GpsL1CA)) ? 1 : 0);
				byte b2 = (byte)((!base.SimulatedSignalTypes.Contains(SignalType.GpsL2C) && !base.SimulatedSignalTypes.Contains(SignalType.GpsL2P)) ? 1 : 0);
				byte b3 = (byte)((!base.SimulatedSignalTypes.Contains(SignalType.GpsL5I) && !base.SimulatedSignalTypes.Contains(SignalType.GpsL5Q)) ? 1 : 0);
				byte[] second43 = new byte[3] { b, b2, b3 };
				int timeOfApplicability = ephemeris.TimeOfApplicability;
				int num3 = timeOfApplicability - 86400;
				if (num3 < 0)
				{
					num3 += 604800;
				}
				double value6 = (double)num3 * 0.0033333333333333335;
				value = 11;
				byte[] second44 = NavigationData.Dec2Bin(in value6, in value);
				value6 = (double)timeOfApplicability * 0.0033333333333333335;
				value = 11;
				byte[] second46 = NavigationData.Dec2Bin(in value6, in value);
				value6 = (ephemeris.SqrtA * ephemeris.SqrtA - 26559710.0) * 512.0;
				value = 26;
				byte[] second47 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second48 = NavigationData.ZeroBits.Take(25);
				value6 = ephemeris.MeanMotionCorrection * rad2Semi * 17592186044416.0;
				value = 17;
				byte[] second49 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second50 = NavigationData.ZeroBits.Take(23);
				value6 = ephemeris.MeanAnomaly * rad2Semi * 4294967296.0;
				value = 33;
				byte[] second51 = NavigationData.Dec2Bin(in value6, in value);
				value6 = ephemeris.Eccentricity * 17179869184.0;
				value = 33;
				byte[] second52 = NavigationData.Dec2Bin(in value6, in value);
				value6 = ephemeris.ArgumentOfPerigee * rad2Semi * 4294967296.0;
				value = 33;
				byte[] second53 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second54 = NavigationData.ZeroBits.Take(3);
				AddWord(data, first.Concat(second42).Concat(second43).Concat(second44)
					.Concat(uraEDBits)
					.Concat(second46)
					.Concat(second47)
					.Concat(second48)
					.Concat(second49)
					.Concat(second50)
					.Concat(second51)
					.Concat(second52)
					.Concat(second53)
					.Append<byte>(1)
					.Append<byte>(0)
					.Concat(second54));
				break;
			}
			case CNavMessageType.Ephemeris2:
			{
				double value6 = (double)ephemeris.TimeOfApplicability * 0.0033333333333333335;
				value = 11;
				byte[] second14 = NavigationData.Dec2Bin(in value6, in value);
				value6 = ephemeris.LongitudeOfAscendingNode * rad2Semi * 4294967296.0;
				value = 33;
				byte[] second15 = NavigationData.Dec2Bin(in value6, in value);
				value6 = (ephemeris.RateOfLongitudeOfAscendingNode - -2.6E-09) * rad2Semi * 17592186044416.0;
				value = 17;
				byte[] second16 = NavigationData.Dec2Bin(in value6, in value);
				value6 = ephemeris.Inclination * rad2Semi * 4294967296.0;
				value = 33;
				byte[] second17 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second18 = NavigationData.ZeroBits.Take(15);
				IEnumerable<byte> second19 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second20 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second21 = NavigationData.ZeroBits.Take(24);
				IEnumerable<byte> second22 = NavigationData.ZeroBits.Take(24);
				IEnumerable<byte> second24 = NavigationData.ZeroBits.Take(21);
				IEnumerable<byte> second25 = NavigationData.ZeroBits.Take(21);
				IEnumerable<byte> second26 = NavigationData.ZeroBits.Take(7);
				AddWord(data, first.Concat(second14).Concat(second15).Concat(second17)
					.Concat(second16)
					.Concat(second18)
					.Concat(second19)
					.Concat(second20)
					.Concat(second21)
					.Concat(second22)
					.Concat(second24)
					.Concat(second25)
					.Concat(second26));
				break;
			}
			case CNavMessageType.ClockIonoAndGroupDelay:
			{
				int num2 = ephemeris.TimeOfApplicability - 86400;
				int value2 = ephemeris.Week;
				if (num2 < 0)
				{
					num2 += 604800;
					value2--;
				}
				else if (num2 == 0)
				{
					value2--;
				}
				double value6 = (double)num2 * 0.0033333333333333335;
				value = 11;
				byte[] second27 = NavigationData.Dec2Bin(in value6, in value);
				value = 8;
				byte[] second28 = NavigationData.Dec2Bin(in value2, in value);
				IEnumerable<byte> second29 = NavigationData.ZeroBits.Take(3);
				IEnumerable<byte> second30 = NavigationData.ZeroBits.Take(3);
				value6 = (double)ephemeris.TimeOfApplicability * 0.0033333333333333335;
				value = 11;
				byte[] second31 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second32 = NavigationData.ZeroBits.Take(26);
				IEnumerable<byte> second33 = NavigationData.ZeroBits.Take(20);
				IEnumerable<byte> second35 = NavigationData.ZeroBits.Take(10);
				byte[] second36 = tgdUnavailableBits;
				byte[] second37 = tgdUnavailableBits;
				byte[] second38 = tgdUnavailableBits;
				byte[] second39 = tgdUnavailableBits;
				byte[] second40 = tgdUnavailableBits;
				IEnumerable<byte> second41 = NavigationData.ZeroBits.Take(12);
				AddWord(data, first.Concat(second27).Concat(uraNED0Bits).Concat(second29)
					.Concat(second30)
					.Concat(second31)
					.Concat(second32)
					.Concat(second33)
					.Concat(second35)
					.Concat(second36)
					.Concat(second37)
					.Concat(second38)
					.Concat(second39)
					.Concat(second40)
					.Concat(alpha0Bits)
					.Concat(alpha1Bits)
					.Concat(alpha2Bits)
					.Concat(alpha3Bits)
					.Concat(beta0Bits)
					.Concat(beta1Bits)
					.Concat(beta2Bits)
					.Concat(beta3Bits)
					.Concat(second28)
					.Concat(second41));
				break;
			}
			case CNavMessageType.ClockAndUTC:
			{
				int num = ephemeris.TimeOfApplicability - 86400;
				if (num < 0)
				{
					num += 604800;
				}
				double value6 = (double)num * 0.0033333333333333335;
				value = 11;
				byte[] second34 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second45 = NavigationData.ZeroBits.Take(3);
				IEnumerable<byte> second55 = NavigationData.ZeroBits.Take(3);
				value6 = (double)ephemeris.TimeOfApplicability * 0.0033333333333333335;
				value = 11;
				byte[] second56 = NavigationData.Dec2Bin(in value6, in value);
				IEnumerable<byte> second57 = NavigationData.ZeroBits.Take(26);
				IEnumerable<byte> second58 = NavigationData.ZeroBits.Take(20);
				IEnumerable<byte> second2 = NavigationData.ZeroBits.Take(10);
				IEnumerable<byte> second3 = NavigationData.ZeroBits.Take(16);
				IEnumerable<byte> second4 = NavigationData.ZeroBits.Take(13);
				IEnumerable<byte> second5 = NavigationData.ZeroBits.Take(7);
				GnssTime gnssTime = ephemeris.TransmissionInterval.Start + GnssTimeSpan.FromHours(70);
				value6 = (double)(gnssTime.GpsSecondOfWeek >> 12 << 12) * 0.0625;
				value = 16;
				byte[] second6 = NavigationData.Dec2Bin(in value6, in value);
				int value7 = gnssTime.GpsWeek;
				value = 13;
				byte[] second7 = NavigationData.Dec2Bin(in value7, in value);
				DateTime utcTime = GnssTime.FromGps(base.Interval.Start.GpsWeek, packetIndexInWeek * packetLength).UtcTime;
				LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(utcTime);
				LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(utcTime);
				GnssTime gnssTime2 = GnssTime.FromUtc(leapSecond2.Utc);
				if ((int)(gnssTime2 - GnssTime.FromUtc(utcTime)).Seconds > 15552000)
				{
					leapSecond2 = leapSecond;
					gnssTime2 = GnssTime.FromUtc(leapSecond.Utc);
				}
				GnssTime gnssTime3 = gnssTime2 - GnssTimeSpan.FromMinutes(1);
				int value8 = leapSecond.Seconds;
				value = 8;
				byte[] second8 = NavigationData.Dec2Bin(in value8, in value);
				int value9 = gnssTime3.GpsWeek;
				value = 13;
				byte[] second9 = NavigationData.Dec2Bin(in value9, in value);
				int value10 = gnssTime3.GpsDayOfWeek + 1;
				value = 4;
				byte[] second10 = NavigationData.Dec2Bin(in value10, in value);
				int value11 = leapSecond2.Seconds;
				value = 8;
				byte[] second11 = NavigationData.Dec2Bin(in value11, in value);
				IEnumerable<byte> second13 = NavigationData.ZeroBits.Take(51);
				AddWord(data, first.Concat(second34).Concat(uraNED0Bits).Concat(second45)
					.Concat(second55)
					.Concat(second56)
					.Concat(second57)
					.Concat(second58)
					.Concat(second2)
					.Concat(second3)
					.Concat(second4)
					.Concat(second5)
					.Concat(second8)
					.Concat(second6)
					.Concat(second7)
					.Concat(second9)
					.Concat(second10)
					.Concat(second11)
					.Concat(second13));
				break;
			}
			}
		}

		private int GetPacketIndex(in GnssTime time)
		{
			return time.GpsSecondOfWeek / packetLength;
		}

		private void StoreFEC(in int packetIndex, in int satIndex, in int fecState)
		{
			using (FecLock.Lock())
			{
				if (!FecLibrary.TryGetValue(packetIndex, out var value))
				{
					value = new int?[50];
					FecLibrary[packetIndex] = value;
				}
				value[satIndex] = fecState;
			}
		}

		protected int? ReadFEC(in int firstPacketIndex)
		{
			using (FecLock.Lock())
			{
				if (FecLibrary.TryGetValue(firstPacketIndex, out var value))
				{
					return value[base.SatelliteIndex];
				}
				if (!FecLibrary.Any())
				{
					value = new int?[50];
					int? result = 0;
					for (int i = 0; i < value.Length; i++)
					{
						value[i] = 0;
					}
					FecLibrary.Add(firstPacketIndex, value);
					return result;
				}
				return null;
			}
		}

		protected sealed override IEnumerable<byte> EncodeWord(IEnumerable<byte> rawWord)
		{
			byte[] array = rawWord.ToArray(276);
			IEnumerable<byte> second = CRC24Q.ComputeBytes(array);
			return array.Concat(second);
		}

		static NavigationDataCNav()
		{
			int value = -3;
			int bitCount = 5;
			uraEDBits = NavigationData.Dec2Bin(in value, in bitCount);
			value = -5;
			bitCount = 5;
			uraNED0Bits = NavigationData.Dec2Bin(in value, in bitCount);
			alphaParameters = Klobuchar.Alpha;
			double value2 = alphaParameters[0] * 1073741824.0;
			value = 8;
			alpha0Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = alphaParameters[1] * 134217728.0;
			value = 8;
			alpha1Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = alphaParameters[2] * 16777216.0;
			value = 8;
			alpha2Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = alphaParameters[3] * 16777216.0;
			value = 8;
			alpha3Bits = NavigationData.Dec2Bin(in value2, in value);
			betaParameters = Klobuchar.Beta;
			value2 = betaParameters[0] * 0.00048828125;
			value = 8;
			beta0Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = betaParameters[1] * 6.103515625E-05;
			value = 8;
			beta1Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = betaParameters[2] * 1.52587890625E-05;
			value = 8;
			beta2Bits = NavigationData.Dec2Bin(in value2, in value);
			value2 = betaParameters[3] * 1.52587890625E-05;
			value = 8;
			beta3Bits = NavigationData.Dec2Bin(in value2, in value);
		}
	}
}
