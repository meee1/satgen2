using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen.Galileo;

internal abstract class NavigationDataINavFNav : NavigationData
{
	protected const int GalileoAlmanacSatCount = 36;

	protected static readonly double Rad2Semi = 1.0 / Constellation.Datum.PI;

	protected static readonly IEnumerable<byte> tailBits = NavigationData.ZeroBits.Take(6);

	protected static readonly byte[] ai0Bits;

	protected static readonly byte[] ai1Bits;

	protected static readonly byte[] ai2Bits;

	protected static readonly byte[] sisaBits;

	protected const int iodNavPeriod = 600;

	protected const double scaleT0e = 1.0 / 60.0;

	protected const double scaleM0 = 2147483648.0;

	protected const double scaleEccentricity = 8589934592.0;

	protected const double scaleSqrtA = 524288.0;

	protected const double scaleDeltaSqrtA = 524288.0;

	protected const double scaleOmega0 = 2147483648.0;

	protected const double scaleI0 = 2147483648.0;

	protected const double scaleOmega = 2147483648.0;

	protected const double scaleI0Dot = 8796093022208.0;

	protected const double scaleOmega0Dot = 8796093022208.0;

	protected const double scaleDeltaN = 8796093022208.0;

	protected const double scaleCuc = 536870912.0;

	protected const double scaleCus = 536870912.0;

	protected const double scaleCrc = 32.0;

	protected const double scaleCrs = 32.0;

	protected const double scaleCic = 536870912.0;

	protected const double scaleCis = 536870912.0;

	protected const double scaleT0c = 1.0 / 60.0;

	protected const double scaleAf0 = 17179869184.0;

	protected const double scaleAf1 = 70368744177664.0;

	protected const double scaleAf2 = 5.7646075230342349E+17;

	protected const double scaleAi0 = 4.0;

	protected const double scaleAi1 = 256.0;

	protected const double scaleAi2 = 32768.0;

	protected const double scaleBGDE1E5a = 4294967296.0;

	protected const double scaleBGDE1E5b = 4294967296.0;

	protected const double scaleA0 = 1073741824.0;

	protected const double scaleA1 = 1125899906842624.0;

	protected const double scaleT0t = 0.00027777777777777778;

	protected const double scaleA0G = 34359738368.0;

	protected const double scaleA1G = 2251799813685248.0;

	protected const double scaleT0G = 0.00027777777777777778;

	protected const int iodaPeriod = 600;

	protected const double scaleT0a = 1.0 / 600.0;

	protected const double scaleAlmanacDeltaSqrtA = 512.0;

	protected const double scaleAlmanacEccentricity = 65536.0;

	protected const double scaleAlmanacOmega = 32768.0;

	protected const double scaleAlmanacDeltaI = 16384.0;

	protected const double scaleAlmanacOmega0 = 32768.0;

	protected const double scaleAlmanacOmega0Dot = 8589934592.0;

	protected const double scaleAlmanacM0 = 32768.0;

	protected const double scaleAlmanacAf0 = 524288.0;

	protected const double scaleAlmanacAf1 = 274877906944.0;

	protected abstract SignalType SignalType { get; }

	protected NavigationDataINavFNav(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
		: base(in satIndex, almanac, in interval, simulatedSignals)
	{
	}

	protected static byte[] GetIodNavBits(in int t0e)
	{
		int value = (t0e / 600) & 0x7F;
		int bitCount = 10;
		return NavigationData.Dec2Bin(in value, in bitCount);
	}

	private static byte[] GetSisaBits(in double accuracy)
	{
		int num = (int)(100.0 * accuracy);
		int value = ((num < 50) ? num : ((num < 100) ? (50 + (num - 50 >> 1)) : ((num < 200) ? (75 + (num - 100 >> 2)) : ((num >= 600) ? 255 : (100 + (num - 200 >> 4))))));
		int bitCount = 8;
		return NavigationData.Dec2Bin(in value, in bitCount);
	}

	protected abstract int GetFirstAlmanacIndex(in int subframeIndex);

	static NavigationDataINavFNav()
	{
		double value = NeQuickG.Alpha0 * 4.0;
		int bitCount = 11;
		ai0Bits = NavigationData.Dec2Bin(in value, in bitCount);
		value = NeQuickG.Alpha1 * 256.0;
		bitCount = 11;
		ai1Bits = NavigationData.Dec2Bin(in value, in bitCount);
		value = NeQuickG.Alpha2 * 32768.0;
		bitCount = 14;
		ai2Bits = NavigationData.Dec2Bin(in value, in bitCount);
		value = 2.0;
		sisaBits = GetSisaBits(in value);
	}
}
