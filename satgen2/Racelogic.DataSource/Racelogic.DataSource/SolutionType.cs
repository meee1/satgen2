using System;

namespace Racelogic.DataSource;

public struct SolutionType : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private readonly sbyte _data;

	public SolutionType(int value)
	{
		_data = (sbyte)value;
	}

	public SolutionType(sbyte value)
	{
		_data = value;
	}

	public SolutionType(string value)
	{
		_data = (sbyte)(sbyte.TryParse(value, out var result) ? result : 0);
	}

	public bool Equals(IRacelogicData other)
	{
		return _data == other.ToSByte(UnitsGlobal.CurrentCulture);
	}

	public int CompareTo(object obj)
	{
		if (!(obj is IRacelogicData))
		{
			throw new ArgumentException("The argument does not implement IRacelogicData interface");
		}
		sbyte data = _data;
		return data.CompareTo(((IRacelogicData)obj).ToSByte(UnitsGlobal.CurrentCulture));
	}

	public int CompareTo(IRacelogicData other)
	{
		sbyte data = _data;
		return data.CompareTo(other.ToSByte(UnitsGlobal.CurrentCulture));
	}

	public TypeCode GetTypeCode()
	{
		return TypeCode.SByte;
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
		return _data;
	}

	public float ToSingle(IFormatProvider provider)
	{
		return _data;
	}

	public string ToString(IFormatProvider provider)
	{
		return ToString();
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
		return _data switch
		{
			-1 => Resources.NoData, 
			0 => Resources.NoSolution, 
			1 => Resources.StandAlone, 
			2 => Resources.CodeDiff, 
			3 => Resources.RTKFloat, 
			4 => Resources.RTKFixed, 
			5 => Resources.FixedPos, 
			6 => Resources.ImuCoasting, 
			_ => Resources.Unknown, 
		};
	}

	public string ToString(string format)
	{
		sbyte data = _data;
		return data.ToString(format);
	}

	public static string ToVboString(double data)
	{
		return data.ToString("000");
	}

	public static string ToVboString(int data)
	{
		return data.ToString("000");
	}

	public static string ToVboString(sbyte data)
	{
		return data.ToString("000");
	}

	public static implicit operator SolutionType(sbyte value)
	{
		return new SolutionType(value);
	}

	public static implicit operator SolutionType(int value)
	{
		return new SolutionType(value);
	}

	public static implicit operator sbyte(SolutionType value)
	{
		return value.ToSByte(UnitsGlobal.CurrentCulture);
	}

	public static implicit operator int(SolutionType value)
	{
		return value.ToInt32(UnitsGlobal.CurrentCulture);
	}
}
