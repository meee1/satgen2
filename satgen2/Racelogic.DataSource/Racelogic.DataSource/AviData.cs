using System;

namespace Racelogic.DataSource;

public struct AviData : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly int _data;

	public AviData(int value)
	{
		_data = value;
	}

	public AviData(string value)
	{
		_data = (int.TryParse(value, out var result) ? result : 0);
	}

	public bool Equals(IRacelogicData other)
	{
		return _data == other.ToInt32(UnitsGlobal.CurrentCulture);
	}

	public int CompareTo(object obj)
	{
		if (!(obj is IRacelogicData))
		{
			throw new ArgumentException("The argument does not implement IRacelogicData interface");
		}
		int data = _data;
		return data.CompareTo(((IRacelogicData)obj).ToInt32(UnitsGlobal.CurrentCulture));
	}

	public int CompareTo(IRacelogicData other)
	{
		int data = _data;
		return data.CompareTo(other.ToInt32(UnitsGlobal.CurrentCulture));
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.Int32;
	}

	public bool ToBoolean(IFormatProvider provider)
	{
		return _data != 0;
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
		return _data;
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
		return _data;
	}

	public string ToString(IFormatProvider provider)
	{
		int data = _data;
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

	public string ToString(string format)
	{
		int data = _data;
		return data.ToString(format);
	}

	public static string ToVboString(double data, VBoxChannel channelType)
	{
		return data.ToString((channelType == VBoxChannel.AviSyncTime) ? "000000000" : "0000");
	}

	public static string ToVboString(int data, VBoxChannel channelType)
	{
		return data.ToString((channelType == VBoxChannel.AviSyncTime) ? "000000000" : "0000");
	}

	public static AviData Parse(string s)
	{
		return int.Parse(s);
	}

	public static bool TryParse(string s, out AviData result)
	{
		int result2;
		bool flag = int.TryParse(s, out result2);
		result = (flag ? result2 : 0);
		return flag;
	}

	public static implicit operator AviData(int value)
	{
		return new AviData(value);
	}

	public static implicit operator int(AviData value)
	{
		return value.ToInt32(UnitsGlobal.CurrentCulture);
	}
}
