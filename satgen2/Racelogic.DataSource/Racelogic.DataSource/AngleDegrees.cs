using System;

namespace Racelogic.DataSource;

public struct AngleDegrees : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly double _data;

	public AngleDegrees(double value)
	{
		_data = value;
	}

	public AngleDegrees(string value)
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
		return ToString(provider, UnitsCache.AngleUnits, UnitsCache.AngleFormat);
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
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.AngleUnits, UnitsCache.AngleFormat);
	}

	public string ToString(string format)
	{
		return ToString(UnitsGlobal.CurrentCulture, UnitsCache.AngleUnits, format);
	}

	public string ToString(AngleUnit units)
	{
		return ToString(UnitsGlobal.CurrentCulture, units, UnitsCache.AngleFormat);
	}

	public string ToString(IFormatProvider provider, AngleUnit units, string format)
	{
		return ToString(provider, units, format, _data);
	}

	internal static string ToString(IFormatProvider provider, AngleUnit units, string format, double data)
	{
		double num = data;
		switch (units)
		{
		case AngleUnit.Radians:
			num *= AngleConstants.DegreeToRadians;
			break;
		default:
			throw new ArgumentOutOfRangeException("units", string.Format(provider, "{0}{1}{2}", "AngleDegrees.ToString()", Environment.NewLine, "value passed in for angle units is not recognised - " + units));
		case AngleUnit.Degree:
			break;
		}
		return string.Format(provider, "{0}", num.ToString(format, provider));
	}

	public static string ToVboString(double data)
	{
		return ToVboString(data, UnitsCache.AngleUnits);
	}

	public static string ToVboString(double data, AngleUnit unit)
	{
		return ToString(UnitsGlobal.CurrentCulture, unit, "00000.00", data);
	}

	public static double DifferenceBetween(AngleDegrees angle1, AngleDegrees angle2, bool returnAbsoluteValue = true)
	{
		return DifferenceBetween(angle1._data, angle2._data);
	}

	public static double DifferenceBetween(double angle1, double angle2, bool returnAbsoluteValue = true)
	{
		angle1 -= angle2;
		if (angle1 > 180.0)
		{
			angle1 -= 360.0;
		}
		else if (angle1 < -180.0)
		{
			angle1 += 360.0;
		}
		if (!returnAbsoluteValue)
		{
			return angle1;
		}
		return Math.Abs(angle1);
	}

	public static AngleDegrees ToCompass(double value)
	{
		while (value < 0.0)
		{
			value += 360.0;
		}
		while (value > 360.0)
		{
			value -= 360.0;
		}
		return value;
	}

	public static AngleDegrees Parse(string s)
	{
		return double.Parse(s);
	}

	public static bool TryParse(string s, out AngleDegrees result)
	{
		double result2;
		bool flag = double.TryParse(s, out result2);
		result = (flag ? result2 : 0.0);
		return flag;
	}

	public static double Convert(double value, AngleUnit oldUnits, AngleUnit newUnits)
	{
		double num = 1.0;
		value = Convert(value, oldUnits).ToDouble(UnitsGlobal.CurrentCulture);
		num = ((newUnits != AngleUnit.Radians) ? 1.0 : AngleConstants.DegreeToRadians);
		return value * num;
	}

	public static AngleDegrees Convert(double value, AngleUnit units)
	{
		double num = 1.0;
		num = ((units != AngleUnit.Radians) ? 1.0 : (1.0 / AngleConstants.DegreeToRadians));
		return new AngleDegrees(value * num);
	}

	public static implicit operator AngleDegrees(double value)
	{
		return new AngleDegrees(value);
	}

	public static implicit operator double(AngleDegrees value)
	{
		return value._data;
	}
}
