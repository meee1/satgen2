using System;
using System.Globalization;
using Racelogic.Utilities;

namespace Racelogic.Libraries.Nmea.Sentences;

public abstract class NmeaSentence
{
	protected static DateTime ParseDate(string date)
	{
		if (string.IsNullOrEmpty(date))
		{
			return DateTime.MinValue;
		}
		int result;
		if (date.Length >= 2)
		{
			int.TryParse(date.Substring(0, 2), out result);
		}
		else
		{
			result = 0;
		}
		int result2;
		if (date.Length >= 4)
		{
			int.TryParse(date.Substring(2, 2), out result2);
		}
		else
		{
			result2 = 0;
		}
		int result3;
		if (date.Length > 4)
		{
			int.TryParse(date.Substring(4), out result3);
		}
		else
		{
			result3 = 0;
		}
		result3 = ((result3 < 80) ? (result3 + 2000) : (result3 + 1900));
		try
		{
			return new DateTime(result3, result2, result);
		}
		catch (ArgumentOutOfRangeException)
		{
			RLLogger.GetLogger().LogMessage("Argument out of range exception trying to parse " + date);
			return DateTime.MinValue;
		}
		catch (ArgumentException)
		{
			RLLogger.GetLogger().LogMessage("Argument exception trying to parse " + date);
			return DateTime.MinValue;
		}
	}

	protected static TimeSpan ParseTime(string time)
	{
		if (string.IsNullOrEmpty(time))
		{
			return TimeSpan.MinValue;
		}
		int result;
		if (time.Length >= 2)
		{
			int.TryParse(time.Substring(0, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
		}
		else
		{
			result = 0;
		}
		int result2;
		if (time.Length >= 4)
		{
			int.TryParse(time.Substring(2, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out result2);
		}
		else
		{
			result2 = 0;
		}
		decimal result3;
		if (time.Length > 4)
		{
			decimal.TryParse(time.Substring(4), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result3);
		}
		else
		{
			result3 = default(decimal);
		}
		return new TimeSpan(0, result, result2, (int)result3, (int)((result3 - (decimal)(int)result3) * 1000m));
	}
}
