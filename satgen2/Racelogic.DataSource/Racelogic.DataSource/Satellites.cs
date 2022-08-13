using System;

namespace Racelogic.DataSource;

public struct Satellites : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly byte _data;

	public Satellites(int value)
	{
		_data = (byte)value;
	}

	public Satellites(byte value)
	{
		_data = value;
	}

	public Satellites(string value)
	{
		_data = (byte)(byte.TryParse(value, out var result) ? result : 0);
	}

	public bool Equals(IRacelogicData other)
	{
		return _data == other.ToByte(UnitsGlobal.CurrentCulture);
	}

	public int CompareTo(object obj)
	{
		if (!(obj is IRacelogicData))
		{
			throw new ArgumentException("The argument does not implement IRacelogicData interface");
		}
		byte data = _data;
		return data.CompareTo(((IRacelogicData)obj).ToByte(UnitsGlobal.CurrentCulture));
	}

	public int CompareTo(IRacelogicData other)
	{
		byte data = _data;
		return data.CompareTo(other.ToByte(UnitsGlobal.CurrentCulture));
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Byte;
	}

	public bool ToBoolean(IFormatProvider provider)
	{
		return _data != 0;
	}

	public byte ToByte(IFormatProvider provider)
	{
		return _data;
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
		return _data;
	}

	public double ToDouble(IFormatProvider provider)
	{
		return (int)_data;
	}

	public short ToInt16(IFormatProvider provider)
	{
		return _data;
	}

	public int ToInt32(IFormatProvider provider)
	{
		return _data;
	}

	public long ToInt64(IFormatProvider provider)
	{
		return _data;
	}

	public sbyte ToSByte(IFormatProvider provider)
	{
		return (sbyte)_data;
	}

	public float ToSingle(IFormatProvider provider)
	{
		return (int)_data;
	}

	public string ToString(IFormatProvider provider)
	{
		byte data = _data;
		return data.ToString(provider);
	}

	public string ToString(string format)
	{
		byte data = _data;
		return data.ToString(format);
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
		return _data;
	}

	public uint ToUInt32(IFormatProvider provider)
	{
		return _data;
	}

	public ulong ToUInt64(IFormatProvider provider)
	{
		return _data;
	}

	public string ToVboString()
	{
		return ToVboString(_data);
	}

	public static string ToVboString(double data)
	{
		return data.ToString("000");
	}

	public static string ToVboString(int data)
	{
		return data.ToString("000");
	}

	public static string ToVboString(byte data)
	{
		return data.ToString("000");
	}

	public static Satellites Parse(string s)
	{
		return byte.Parse(s);
	}

	public static bool TryParse(string s, out Satellites result)
	{
		byte result2;
		bool flag = byte.TryParse(s, out result2);
		result = (byte)(flag ? result2 : 0);
		return flag;
	}

	public static implicit operator Satellites(int value)
	{
		return new Satellites((byte)value);
	}

	public static implicit operator Satellites(byte value)
	{
		return new Satellites(value);
	}

	public static implicit operator int(Satellites value)
	{
		return value.ToInt32(UnitsGlobal.CurrentCulture);
	}

	public static implicit operator byte(Satellites value)
	{
		return value.ToByte(UnitsGlobal.CurrentCulture);
	}
}
