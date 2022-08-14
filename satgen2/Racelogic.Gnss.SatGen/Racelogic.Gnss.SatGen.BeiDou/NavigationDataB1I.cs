using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.BeiDou;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.BeiDou;

internal abstract class NavigationDataB1I : NavigationData
{
	protected readonly bool[] satelliteHealth = new bool[50];

	protected readonly double rad2Semi = 1.0 / Constellation.Datum.PI;

	protected static readonly byte[] preambleAndRevBits = new byte[15]
	{
		1, 1, 1, 0, 0, 0, 1, 0, 0, 1,
		0, 0, 0, 0, 0
	};

	protected static readonly byte[] pattern10000000Bits = (from n in Enumerable.Range(0, 176)
		select (byte)((uint)n & 7u) into n
		select (byte)((n == 0) ? 1 : 0)).ToArray(176);

	protected const uint uraiGood = 0u;

	protected static readonly IReadOnlyList<double> alphaParameters = Klobuchar.Alpha;

	protected static readonly IReadOnlyList<double> betaParameters = Klobuchar.Beta;

	protected const byte satH1OKBit = 0;

	protected const byte satH1BadBit = 1;

	protected static readonly byte[] satHealthOKBits = new byte[9];

	protected static readonly byte[] satHealthUnavailableBits = new byte[9] { 1, 0, 0, 0, 0, 0, 0, 0, 0 };

	protected static readonly byte[] satHealthFailureBits = new byte[9] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };

	protected const double scaleT0c = 0.125;

	protected const double scaleA0 = 8589934592.0;

	protected const double scaleA1 = 1125899906842624.0;

	protected const double scaleA2 = 4.0;

	protected const double scaleA = 10.0;

	protected const double scaleA0Utc = 1073741824.0;

	protected const double scaleA1Utc = 1125899906842624.0;

	protected const double scaleT0e = 0.125;

	protected const double scaleSqrtA = 524288.0;

	protected const double scaleE = 8589934592.0;

	protected const double scaleOmega = 2147483648.0;

	protected const double scaleDeltaN = 8796093022208.0;

	protected const double scaleM0 = 2147483648.0;

	protected const double scaleOmega0 = 2147483648.0;

	protected const double scaleOmegaDot = 8796093022208.0;

	protected const double scaleI0 = 2147483648.0;

	protected const double scaleIDot = 8796093022208.0;

	protected const double scaleCu = 2147483648.0;

	protected const double scaleCr = 64.0;

	protected const double scaleCi = 2147483648.0;

	protected const double scaleT0a = 0.000244140625;

	private const double scaleAlmanacSqrtA = 2048.0;

	private const double scaleAlmanacE = 2097152.0;

	private const double scaleAlmanacOmega = 8388608.0;

	private const double scaleAlmanacM0 = 8388608.0;

	private const double scaleAlmanacOmega0 = 8388608.0;

	private const double scaleAlmanacOmegaDot = 274877906944.0;

	private const double scaleAlmanacDeltaI = 524288.0;

	private const double scaleAlmanacA0 = 1048576.0;

	private const double scaleAlmanacA1 = 274877906944.0;

	protected const double scaleAlpha0 = 1073741824.0;

	protected const double scaleAlpha1 = 134217728.0;

	protected const double scaleAlpha2 = 16777216.0;

	protected const double scaleAlpha3 = 16777216.0;

	protected const double scaleBeta0 = 0.00048828125;

	protected const double scaleBeta1 = 6.103515625E-05;

	protected const double scaleBeta2 = 1.52587890625E-05;

	protected const double scaleBeta3 = 1.52587890625E-05;

	protected NavigationDataB1I(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> allSignals)
		: base(in satIndex, almanac, in interval, allSignals)
	{
		foreach (Satellite item in from s in almanac.BaselineSatellites
			select s as Satellite into s
			where s?.IsHealthy ?? false
			select (s))
		{
			satelliteHealth[item.Index] = true;
		}
	}

	protected void AddAlmanacData(List<byte> data, IEnumerable<byte> sowLsb, IEnumerable<byte> pageNumberBits, Satellite satellite)
	{
		if (!satellite.IsHealthy)
		{
			AddEmptyAlmanacData(data, pageNumberBits, sowLsb, satellite);
			return;
		}
		double value = satellite.SqrtA * 2048.0;
		int bitCount = 24;
		byte[] source = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, sowLsb.Concat(NavigationData.ZeroBits.Take(1)).Concat(pageNumberBits).Concat(source.Take(2)));
		AddWord(data, source.Skip(2).Take(22));
		value = satellite.A1 * 274877906944.0;
		bitCount = 11;
		byte[] first = NavigationData.Dec2Bin(in value, in bitCount);
		value = satellite.A0 * 1048576.0;
		bitCount = 11;
		byte[] second = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, first.Concat(second));
		value = satellite.LongitudeOfAscendingNode * rad2Semi * 8388608.0;
		bitCount = 24;
		byte[] source2 = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, source2.Take(22));
		value = satellite.Eccentricity * 2097152.0;
		bitCount = 17;
		byte[] second2 = NavigationData.Dec2Bin(in value, in bitCount);
		double num = ((satellite.OrbitType == OrbitType.GEO) ? 0.0 : 0.3);
		value = (satellite.Inclination * rad2Semi - num) * 524288.0;
		bitCount = 16;
		byte[] source3 = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, source2.Skip(22).Take(2).Concat(second2)
			.Concat(source3.Take(3)));
		value = (double)satellite.TimeOfApplicability * 0.000244140625;
		bitCount = 8;
		byte[] second3 = NavigationData.Dec2Bin(in value, in bitCount);
		value = satellite.RateOfLongitudeOfAscendingNode * rad2Semi * 274877906944.0;
		bitCount = 17;
		byte[] source4 = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, source3.Skip(3).Take(13).Concat(second3)
			.Concat(source4.Take(1)));
		value = satellite.ArgumentOfPerigee * rad2Semi * 8388608.0;
		bitCount = 24;
		byte[] source5 = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, source4.Skip(1).Take(16).Concat(source5.Take(6)));
		value = satellite.MeanAnomaly * rad2Semi * 8388608.0;
		bitCount = 24;
		byte[] source6 = NavigationData.Dec2Bin(in value, in bitCount);
		AddWord(data, source5.Skip(6).Take(18).Concat(source6.Take(4)));
		AddWord(data, source6.Skip(4).Take(20).Concat(NavigationData.ZeroAndOneBits.Take(2)));
	}

	protected abstract void AddEmptyAlmanacData(List<byte> data, IEnumerable<byte> pageNumberBits, IEnumerable<byte> sowLsb, Satellite satellite);

	protected byte[] GetSatelliteHealthBits(in int satId)
	{
		if (!satelliteHealth[satId - 1])
		{
			return satHealthUnavailableBits;
		}
		return satHealthOKBits;
	}

	protected static void GetLeapSecond(in GnssTime subframeTime, out int currentLeapSecond, out int nextLeapSecond, out int nextLeapSecondWeek, out int nextLeapSecondDay)
	{
		LeapSecond leapSecond = LeapSecond.LeapSecondsForDate(subframeTime.UtcTime);
		LeapSecond leapSecond2 = LeapSecond.NextLeapSecondsAfterDate(subframeTime.UtcTime);
		GnssTime gnssTime = GnssTime.FromUtc(leapSecond2.Utc);
		if ((int)(gnssTime - GnssTime.FromUtc(subframeTime.UtcTime)).Seconds > 15552000)
		{
			leapSecond2 = leapSecond;
			gnssTime = GnssTime.FromUtc(leapSecond.Utc);
		}
		GnssTime gnssTime2 = gnssTime - GnssTimeSpan.FromMinutes(1);
		nextLeapSecondDay = gnssTime2.BeiDouDayOfWeek;
		nextLeapSecondWeek = gnssTime2.BeiDouWeek;
		currentLeapSecond = leapSecond.Seconds - LeapSecond.GpsLeapSecondsForBeiDouEpoch;
		nextLeapSecond = leapSecond2.Seconds - LeapSecond.GpsLeapSecondsForBeiDouEpoch;
	}

	protected override IEnumerable<byte> EncodeFirstWord(IEnumerable<byte> rawWord)
	{
		return rawWord.Take(15).Concat(BchCoding.Encode(rawWord.Skip(15)));
	}

	protected override IEnumerable<byte> EncodeWord(IEnumerable<byte> rawWord)
	{
		return BchCoding.EncodeInterleaved(rawWord);
	}
}
