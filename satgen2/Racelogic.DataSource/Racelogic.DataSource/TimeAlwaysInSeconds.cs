using System;

namespace Racelogic.DataSource;

public struct TimeAlwaysInSeconds : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	public const int DefaultDecimalPlaces = 2;

	private readonly double _data;

	public TimeAlwaysInSeconds(double value)
	{
		_data = value;
	}

	public TimeAlwaysInSeconds(string value)
	{
		_data = (double.TryParse(value, out var result) ? result : 0.0);
	}

	public bool Equals(IRacelogicData other)
	{
		return _data == other.ToDouble(UnitsGlobal.CurrentCulture);
	}

	public int CompareTo(object obj)
	{
		if (!(obj is IRacelogicData))
		{
			throw new ArgumentException("The argument does not implement IRacelogicData interface");
		}
		double data = _data;
		return data.CompareTo(((IRacelogicData)obj).ToDouble(UnitsGlobal.CurrentCulture));
	}

	public int CompareTo(IRacelogicData other)
	{
		double data = _data;
		return data.CompareTo(other.ToDouble(UnitsGlobal.CurrentCulture));
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Double;
	}

	public bool ToBoolean(IFormatProvider provider)
	{
		return _data != 0.0;
	}

	public byte ToByte(IFormatProvider provider)
	{
		return (byte)_data;
	}

	public char ToChar(IFormatProvider provider)
	{
		throw new NotImplementedException();
	}

	public DateTime ToDateTime(IFormatProvider provider)
	{
		throw new NotImplementedException();
	}

	public decimal ToDecimal(IFormatProvider provider)
	{
		return (decimal)_data;
	}

	public double ToDouble(IFormatProvider provider)
	{
		return _data;
	}

	public short ToInt16(IFormatProvider provider)
	{
		return (short)_data;
	}

	public int ToInt32(IFormatProvider provider)
	{
		return (int)_data;
	}

	public long ToInt64(IFormatProvider provider)
	{
		return (long)_data;
	}

	public sbyte ToSByte(IFormatProvider provider)
	{
		return (sbyte)_data;
	}

	public float ToSingle(IFormatProvider provider)
	{
		return (float)_data;
	}

	public string ToString(IFormatProvider provider)
	{
		return ToString(provider, FormatOptions.Instance.Time.Units, ':', FormatOptions.Instance.Time.Options, FormatOptions.Instance.Time.Format);
	}

	public object ToType(Type conversionType, IFormatProvider provider)
	{
		if (conversionType == typeof(bool))
		{
			return ToBoolean(provider);
		}
		if (conversionType == typeof(byte))
		{
			return ToByte(provider);
		}
		if (conversionType == typeof(char))
		{
			return ToChar(provider);
		}
		if (conversionType == typeof(DateTime))
		{
			return ToDateTime(provider);
		}
		if (conversionType == typeof(decimal))
		{
			return ToDecimal(provider);
		}
		if (conversionType == typeof(double))
		{
			return ToDouble(provider);
		}
		if (conversionType == typeof(short))
		{
			return ToInt16(provider);
		}
		if (conversionType == typeof(int))
		{
			return ToInt32(provider);
		}
		if (conversionType == typeof(long))
		{
			return ToInt64(provider);
		}
		if (conversionType == typeof(sbyte))
		{
			return ToSByte(provider);
		}
		if (conversionType == typeof(float))
		{
			return ToSingle(provider);
		}
		if (conversionType == typeof(string))
		{
			return ToString(provider);
		}
		if (conversionType == typeof(ushort))
		{
			return ToUInt16(provider);
		}
		if (conversionType == typeof(uint))
		{
			return ToUInt32(provider);
		}
		if (conversionType == typeof(ulong))
		{
			return ToUInt64(provider);
		}
		throw new NotImplementedException();
	}

	public ushort ToUInt16(IFormatProvider provider)
	{
		return (ushort)_data;
	}

	public uint ToUInt32(IFormatProvider provider)
	{
		return (uint)_data;
	}

	public ulong ToUInt64(IFormatProvider provider)
	{
		return (ulong)_data;
	}

	public override string ToString()
	{
		return ToString(UnitsGlobal.CurrentCulture, TimeUnit.SecondsSinceMidnight, ':', ToStringOptions.None, FormatOptions.Instance.TimeAlwaysInSeconds.Format);
	}

	public string ToString(string format)
	{
		return ToString(UnitsGlobal.CurrentCulture, TimeUnit.SecondsSinceMidnight, ':', ToStringOptions.None, format);
	}

	public string ToString(IFormatProvider provider, TimeUnit units, char separator, ToStringOptions options, string format)
	{
		return ToString(provider, units, separator, options, format, showHoursMinutesSecondsText: false);
	}

	public string ToString(IFormatProvider provider, TimeUnit units, char separator, ToStringOptions options, string format, bool showHoursMinutesSecondsText)
	{
		return ToString(provider, units, separator, options, format, showHoursMinutesSecondsText, _data);
	}

	private static string ToString(IFormatProvider provider, TimeUnit units, char separator, ToStringOptions options, string format, bool showHoursMinutesSecondsText, double data)
	{
		double num = data;
		if (num < 0.0 && (options & ToStringOptions.ReturnAbsolute) == ToStringOptions.ReturnAbsolute)
		{
			num *= -1.0;
		}
		string text = string.Empty;
		switch (units)
		{
		case TimeUnit.SecondsSinceMidnight:
			if ((options & ToStringOptions.AlwaysSigned) == ToStringOptions.AlwaysSigned && num >= 0.0)
			{
				text = "+";
			}
			text = string.Format(provider, "{0}{1}", text, num.ToString(format, provider));
			break;
		case TimeUnit.Milliseconds:
			if ((options & ToStringOptions.AlwaysSigned) == ToStringOptions.AlwaysSigned && num >= 0.0)
			{
				text = "+";
			}
			text = string.Format(provider, "{0}{1}", text, (num * 1000.0).ToString(format, provider));
			break;
		case TimeUnit.DaysHoursMinutesWholeSeconds:
		case TimeUnit.HoursMinutesSeconds:
		case TimeUnit.HoursMinutesWholeSeconds:
		case TimeUnit.HoursMinutesSecondsNoSeparator:
		case TimeUnit.HoursMinutesWholeSecondsNoSeparator:
		case TimeUnit.HoursMinutes:
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(num);
			if (!showHoursMinutesSecondsText)
			{
				switch (units)
				{
				case TimeUnit.DaysHoursMinutesWholeSeconds:
					text = string.Format(provider, "{0}{4}{1}{4}{2}{4}{3}", timeSpan.Hours.ToString("00", provider), timeSpan.Days.ToString("00", provider), timeSpan.Minutes.ToString("00", provider), timeSpan.Seconds.ToString("00", provider), separator);
					break;
				case TimeUnit.HoursMinutes:
					text = string.Format(provider, "{0}{2}{1}", timeSpan.Hours.ToString("00", provider), timeSpan.Minutes.ToString("00", provider), separator);
					break;
				case TimeUnit.HoursMinutesWholeSeconds:
					text = string.Format(provider, "{0}{3}{1}{3}{2}", timeSpan.Hours.ToString("00", provider), timeSpan.Minutes.ToString("00", provider), timeSpan.Seconds.ToString("00", provider), separator);
					break;
				case TimeUnit.HoursMinutesSeconds:
					num = (double)timeSpan.Seconds + (double)timeSpan.Milliseconds / 1000.0;
					text = string.Format(provider, "{0}{3}{1}{3}{2}", timeSpan.Hours.ToString("00", provider), timeSpan.Minutes.ToString("00", provider), num.ToString("0" + format, provider), separator);
					break;
				case TimeUnit.HoursMinutesSecondsNoSeparator:
					text = string.Format(arg2: ((double)timeSpan.Seconds + (double)timeSpan.Milliseconds / 1000.0).ToString("0" + format, provider), provider: provider, format: "{0}{1}{2}", arg0: timeSpan.Hours.ToString("00", provider), arg1: timeSpan.Minutes.ToString("00", provider));
					break;
				case TimeUnit.HoursMinutesWholeSecondsNoSeparator:
					text = string.Format(provider, "{0}{1}{2}", timeSpan.Hours.ToString("00", provider), timeSpan.Minutes.ToString("00", provider), timeSpan.Seconds.ToString("00", provider));
					break;
				}
			}
			else if (timeSpan.TotalSeconds < 1.0)
			{
				text = $"0{Resources.SecondsText}";
			}
			else
			{
				string text2 = string.Format("{0}{1}{2}", (timeSpan.Minutes > 0 || timeSpan.Hours > 0 || timeSpan.Days > 0) ? " " : string.Empty, (timeSpan.Seconds > 9 || timeSpan.Minutes > 0 || timeSpan.Hours > 0 || timeSpan.Days > 0) ? timeSpan.Seconds.ToString("00", provider) : timeSpan.Seconds.ToString("0", provider), Resources.SecondsText);
				string text3 = ((timeSpan.Minutes < 1 && timeSpan.Hours < 1 && timeSpan.Days < 1) ? string.Empty : string.Format("{0}{1}{2}", (timeSpan.Hours > 0 || timeSpan.Days > 0) ? " " : string.Empty, (timeSpan.Minutes > 9 || timeSpan.Hours > 0 || timeSpan.Days > 0) ? timeSpan.Minutes.ToString("00", provider) : timeSpan.Minutes.ToString("0", provider), Resources.MinutesText));
				string text4 = ((timeSpan.Hours < 1 && timeSpan.Days < 1) ? string.Empty : string.Format("{0}{1}{2}", (timeSpan.Days > 0) ? " " : string.Empty, (timeSpan.Hours > 9 || timeSpan.Days > 0) ? timeSpan.Hours.ToString("00", provider) : timeSpan.Hours.ToString("0", provider), Resources.HoursText));
				string text5 = ((timeSpan.Days < 1) ? string.Empty : $"{timeSpan.Days.ToString()}{Resources.DaysText}");
				text = $"{text5}{text4}{text3}{text2}";
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("units", string.Format(provider, "{0}{1}{2}", "Time.ToString()", Environment.NewLine, "value passed in for time units is not recognised - " + units));
		}
		return text;
	}

	public static TimeAlwaysInSeconds Parse(string s)
	{
		return double.Parse(s);
	}

	public static bool TryParse(string s, out TimeAlwaysInSeconds result)
	{
		double result2;
		bool flag = double.TryParse(s, out result2);
		result = (flag ? result2 : 0.0);
		return flag;
	}

	public static implicit operator TimeAlwaysInSeconds(double value)
	{
		return new TimeAlwaysInSeconds(value);
	}

	public static implicit operator TimeAlwaysInSeconds(TimeSeconds value)
	{
		return new TimeAlwaysInSeconds(value.ToDouble(UnitsGlobal.CurrentCulture));
	}

	public static implicit operator double(TimeAlwaysInSeconds value)
	{
		return value._data;
	}

	public bool Equals(TimeAlwaysInSeconds other)
	{
		return Equals((IRacelogicData)other);
	}
}
