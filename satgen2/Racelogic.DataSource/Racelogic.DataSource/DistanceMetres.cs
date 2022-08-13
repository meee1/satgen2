using System;

namespace Racelogic.DataSource;

public struct DistanceMetres : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly double _data;

	public DistanceMetres(double value)
	{
		_data = value;
	}

	public DistanceMetres(string value)
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
		return ToString(provider, UnitsCache.DistanceUnits, FormatOptions.Instance.Distance.Options, UnitsCache.DistanceFormat);
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
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.DistanceUnits, FormatOptions.Instance.Distance.Options, UnitsCache.DistanceFormat);
	}

	public string ToString(string format)
	{
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.DistanceUnits, FormatOptions.Instance.Distance.Options, format);
	}

	public string ToString(DistanceUnit units)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, FormatOptions.Instance.Distance.Options, UnitsCache.DistanceFormat);
	}

	public string ToString(DistanceUnit units, ToStringOptions options)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, options, UnitsCache.DistanceFormat);
	}

	public string ToString(IFormatProvider provider, DistanceUnit units, ToStringOptions options, string format)
	{
		return ToString(provider, units, options, format, _data);
	}

	internal static string ToString(IFormatProvider provider, DistanceUnit units, ToStringOptions options, string format, double data)
	{
		double num = data;
		if (num < 0.0 && (options & ToStringOptions.ReturnAbsolute) == ToStringOptions.ReturnAbsolute)
		{
			num *= -1.0;
		}
		switch (units)
		{
		case DistanceUnit.Feet:
			num *= DistanceConstants.MetresToFeet;
			break;
		case DistanceUnit.Kilometres:
			num /= 1000.0;
			break;
		case DistanceUnit.Miles:
			num *= DistanceConstants.MetresToMiles;
			break;
		case DistanceUnit.NauticalMiles:
			num *= DistanceConstants.MetresToNauticalMiles;
			break;
		default:
			throw new ArgumentOutOfRangeException("units", string.Format(provider, "{0}{1}{2}", "Distance.ToString()", Environment.NewLine, "value passed in for distance units is not recognised - " + units));
		case DistanceUnit.Metres:
			break;
		}
		string arg = string.Empty;
		if ((options & ToStringOptions.AlwaysSigned) == ToStringOptions.AlwaysSigned && num >= 0.0)
		{
			arg = "+";
		}
		return string.Format(provider, "{0}{1}", arg, num.ToString(format, provider));
	}

	public string ToHeightVboString()
	{
		return ToVboString(_data, ToStringOptions.AlwaysSigned);
	}

	public static string ToVboString(double data, ToStringOptions options)
	{
		return ToVboString(data, options, UnitsCache.DistanceUnits);
	}

	public static string ToVboString(double data, ToStringOptions options, DistanceUnit unit)
	{
		return ToString(UnitsGlobal.CurrentCulture, unit, options, "00000.000", data);
	}

	public static DistanceMetres Parse(string s)
	{
		return double.Parse(s);
	}

	public static bool TryParse(string s, out DistanceMetres result)
	{
		double result2;
		bool flag = double.TryParse(s, out result2);
		result = (flag ? result2 : 0.0);
		return flag;
	}

	public static double Convert(double value, DistanceUnit oldUnits, DistanceUnit newUnits)
	{
		double num = 1.0;
		value = Convert(value, oldUnits).ToDouble(UnitsGlobal.CurrentCulture);
		return value * newUnits switch
		{
			DistanceUnit.Feet => DistanceConstants.MetresToFeet, 
			DistanceUnit.Kilometres => 0.001, 
			DistanceUnit.Miles => DistanceConstants.MetresToMiles, 
			DistanceUnit.NauticalMiles => DistanceConstants.MetresToNauticalMiles, 
			_ => 1.0, 
		};
	}

	public static DistanceMetres Convert(double value, DistanceUnit units)
	{
		double num = 1.0;
		return new DistanceMetres(value * units switch
		{
			DistanceUnit.Feet => 1.0 / DistanceConstants.MetresToFeet, 
			DistanceUnit.Kilometres => 1000.0, 
			DistanceUnit.Miles => 1.0 / DistanceConstants.MetresToMiles, 
			DistanceUnit.NauticalMiles => 1.0 / DistanceConstants.MetresToNauticalMiles, 
			_ => 1.0, 
		});
	}

	public static implicit operator DistanceMetres(double value)
	{
		return new DistanceMetres(value);
	}

	public static implicit operator double(DistanceMetres value)
	{
		return value._data;
	}
}
