using System;

namespace Racelogic.DataSource;

public struct Status : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly byte _data;

	public Status(int value)
	{
		_data = (byte)value;
	}

	public Status(byte value)
	{
		_data = value;
	}

	public Status(string value)
	{
		_data = (byte)(byte.TryParse(value, out var result) ? result : 0);
	}

	public Status(bool value)
		: this((byte)(value ? 1 : 0))
	{
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

	public string ToString(string format)
	{
		byte data = _data;
		return data.ToString(format);
	}

	public static string ToVboString(double data)
	{
		if (data != 0.0)
		{
			return "1";
		}
		return "0";
	}

	public static string ToVboString(int data)
	{
		if (data != 0)
		{
			return "1";
		}
		return "0";
	}

	public static string ToVboString(byte data)
	{
		if (data != 0)
		{
			return "1";
		}
		return "0";
	}

	public static Status Parse(string s)
	{
		return byte.Parse(s);
	}

	public static bool TryParse(string s, out Status result)
	{
		byte result2;
		bool flag = byte.TryParse(s, out result2);
		result = (byte)(flag ? result2 : 0);
		return flag;
	}

	public static implicit operator Status(int value)
	{
		return new Status(value);
	}

	public static implicit operator Status(byte value)
	{
		return new Status(value);
	}

	public static implicit operator Status(bool value)
	{
		return new Status(value);
	}

	public static implicit operator int(Status value)
	{
		return value.ToInt32(UnitsGlobal.CurrentCulture);
	}

	public static implicit operator byte(Status value)
	{
		return value.ToByte(UnitsGlobal.CurrentCulture);
	}

	public static implicit operator bool(Status value)
	{
		return value.ToBoolean(UnitsGlobal.CurrentCulture);
	}
}
