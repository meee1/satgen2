using System;
using Racelogic.Core;

namespace Racelogic.DataSource;

public struct SpeedKilometresPerHour : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly double _data;

	public SpeedKilometresPerHour(double value)
	{
		_data = value;
	}

	public SpeedKilometresPerHour(string value)
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
		return ToString(provider, UnitsCache.SpeedUnits, FormatOptions.Instance.Speed.Options, UnitsCache.SpeedFormat);
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
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.SpeedUnits, FormatOptions.Instance.Speed.Options, UnitsCache.SpeedFormat);
	}

	public string ToString(string format)
	{
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.SpeedUnits, FormatOptions.Instance.Speed.Options, format);
	}

	public string ToString(SpeedUnit units)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, FormatOptions.Instance.Speed.Options, UnitsCache.SpeedFormat);
	}

	public string ToString(SpeedUnit units, ToStringOptions options)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, options, UnitsCache.SpeedFormat);
	}

	public string ToString(IFormatProvider provider, SpeedUnit units, ToStringOptions options, string format)
	{
		return ToString(provider, units, options, format, _data);
	}

	internal static string ToString(IFormatProvider provider, SpeedUnit units, ToStringOptions options, string format, double data)
	{
		double num = data;
		if (num < 0.0 && (options & ToStringOptions.ReturnAbsolute) == ToStringOptions.ReturnAbsolute)
		{
			num *= -1.0;
		}
		switch (units)
		{
		case SpeedUnit.MilesPerHour:
			num *= SpeedConstants.KilometresPerHourToMilesPerHour;
			break;
		case SpeedUnit.Knots:
			num *= SpeedConstants.KilometresPerHourToKnots;
			break;
		case SpeedUnit.MetresPerSecond:
			num *= SpeedConstants.KilometresPerHourToMetresPerSecond;
			break;
		case SpeedUnit.FeetPerSecond:
			num *= SpeedConstants.KilometresPerHourToFeetPerSecond;
			break;
		default:
			throw new ArgumentOutOfRangeException("units", string.Format(provider, "{0}{1}{2}", "Speed.ToString()", Environment.NewLine, "value passed in for speed units is not recognised - " + units));
		case SpeedUnit.KilometresPerHour:
			break;
		}
		string arg = string.Empty;
		if ((options & ToStringOptions.AlwaysSigned) == ToStringOptions.AlwaysSigned && Maths.Compare(num, 0.0) != ValueIs.LessThan)
		{
			arg = "+";
		}
		return string.Format(provider, "{0}{1}", arg, num.ToString(format, provider));
	}

	public string ToVboString()
	{
		return ToVboString(_data, UnitsCache.SpeedUnits);
	}

	public string ToVboString(SpeedUnit units)
	{
		return ToVboString(_data, units);
	}

	public string ToVerticalVelocityVboString()
	{
		return ToVerticalVelocityVboString(_data);
	}

	public static string ToVboString(double data, SpeedUnit unit)
	{
		return ToString(UnitsGlobal.CurrentCulture, unit, ToStringOptions.ReturnAbsolute, "000.000", data);
	}

	public static string ToVboString(double data)
	{
		return ToVboString(data, UnitsCache.SpeedUnits);
	}

	public static string ToVerticalVelocityVboString(double data)
	{
		return ToVerticalVelocityVboString(data, UnitsCache.SpeedUnits);
	}

	public static string ToVerticalVelocityVboString(double data, SpeedUnit units)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, ToStringOptions.AlwaysSigned, "000.000", data);
	}

	public static SpeedKilometresPerHour Parse(string s)
	{
		return double.Parse(s);
	}

	public static bool TryParse(string s, out SpeedKilometresPerHour result)
	{
		double result2;
		bool flag = double.TryParse(s, out result2);
		result = (flag ? result2 : 0.0);
		return flag;
	}

	public static implicit operator SpeedKilometresPerHour(double value)
	{
		return new SpeedKilometresPerHour(value);
	}

	public static implicit operator double(SpeedKilometresPerHour value)
	{
		return value._data;
	}
}
