using System;
using System.Linq;

namespace Racelogic.DataSource.Can;

public class DataMessageConstrainer
{
	public static byte[] GetSupportedDataLengthCodes(bool isFlexibleDataRateCan, bool allowZero)
	{
		return ((byte[])Enum.GetValues(typeof(DataLengthCodes))).Skip((!allowZero) ? 1 : 0).TakeWhile((byte x) => isFlexibleDataRateCan || x <= 8).ToArray();
	}
}
