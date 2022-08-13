using System;

namespace Racelogic.DataSource;

public struct CanData : IRacelogicData, IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	private const string DefaultCanDataFormat = "0.000000E+00";

	private readonly double _data;

	public CanData(double value)
	{
		_data = value;
	}

	public CanData(string value)
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
		return ToString(provider, FormatOptions.Instance.Can.Options, UnitsCache.NoUnitDataFormat);
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
		return ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Can.Options, UnitsCache.NoUnitDataFormat);
	}

	public string ToString(string format)
	{
		return ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Can.Options, format);
	}

	public string ToString(bool vboOutput)
	{
		if (!vboOutput)
		{
			return ToString();
		}
		return ToString(UnitsGlobal.CurrentCulture, ToStringOptions.AlwaysSigned, FormatOptions.Instance.Can.VboFormat);
	}

	public string ToString(uint decimalPlaces)
	{
		string empty = string.Empty;
		if (decimalPlaces == 0)
		{
			empty = "0";
		}
		else
		{
			string text = "0.0";
			while (decimalPlaces-- > 1)
			{
				text += "0";
			}
			empty = text;
		}
		return ToString(UnitsGlobal.CurrentCulture, ToStringOptions.AlwaysSigned, empty);
	}

	private string ToString(IFormatProvider provider, ToStringOptions options, string format)
	{
		double num = _data;
		if (num < 0.0 && (options & ToStringOptions.ReturnAbsolute) == ToStringOptions.ReturnAbsolute)
		{
			num *= -1.0;
		}
		string arg = string.Empty;
		if ((options & ToStringOptions.AlwaysSigned) == ToStringOptions.AlwaysSigned && num >= 0.0)
		{
			arg = "+";
		}
		return string.Format(provider, "{0}{1}", arg, num.ToString(format, provider));
	}

	public string ToVboString()
	{
		return ToVboString(_data);
	}

	public static string ToVboString(double data)
	{
		if (!(data >= 0.0))
		{
			return data.ToString("0.000000E+00");
		}
		return "+" + data.ToString("0.000000E+00");
	}

	public static string ToVboStringWithConvertion(double data, string currentUnit)
	{
		if (string.IsNullOrEmpty(currentUnit))
		{
			return data.ToString("0.000000E+00");
		}
		switch (currentUnit.ToLower())
		{
		case "m":
		case "metre":
		case "metres":
		case "meters":
		case "meter":
			return DistanceMetres.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Distance.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", data);
		case "km":
		{
			double num10 = data * 1000.0;
			return DistanceMetres.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Distance.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num10);
		}
		case "feet":
		{
			double num8 = data / DistanceConstants.MetresToFeet;
			return DistanceMetres.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Distance.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num8);
		}
		case "mi":
		{
			double num7 = data / DistanceConstants.MetresToMiles;
			return DistanceMetres.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Distance.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num7);
		}
		case "nmi":
		{
			double num9 = data / DistanceConstants.MetresToNauticalMiles;
			return DistanceMetres.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Distance.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num9);
		}
		case "s":
			return TimeSeconds.ToVboString(data, FormatOptions.Instance.Time.Units);
		case "hhmmss":
		{
			string text = data.ToString().Trim();
			string text2 = ((!text.Contains(".")) ? text : text.Substring(0, text.IndexOf(".")));
			string text3 = text2.Substring(text2.Length - 2);
			string text4 = text2.Substring(text2.Length - 4, 2);
			string text5 = text2.Substring(0, text2.Length - 4);
			string text6 = text5 + ":" + text4 + ":" + text3;
			if (text.Contains("."))
			{
				text6 += text.Substring(text.IndexOf("."));
			}
			DateTime dateTime = Convert.ToDateTime(text6);
			return TimeSeconds.ToVboString((dateTime - dateTime.Date).TotalSeconds, FormatOptions.Instance.Time.Units);
		}
		case "g":
			return AccelerationG.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Acceleration.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", data);
		case "feetpersecondsquared":
			return AccelerationG.ToVboString(data / AccelerationConstants.GToFeetPerSecondSquared, FormatOptions.Instance.Acceleration.Units);
		case "meterpersecondsquared":
		{
			double num6 = data / AccelerationConstants.GToMetresPerSecondSquared;
			return AccelerationG.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Acceleration.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num6);
		}
		case "kts":
		case "knots":
		{
			double num5 = data / SpeedConstants.KilometresPerHourToKnots;
			return SpeedKilometresPerHour.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Speed.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num5);
		}
		case "km/h":
			return SpeedKilometresPerHour.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Speed.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", data);
		case "mph":
		{
			double num4 = data / SpeedConstants.KilometresPerHourToMilesPerHour;
			return SpeedKilometresPerHour.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Speed.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num4);
		}
		case "f/s":
		{
			double num3 = data / SpeedConstants.KilometresPerHourToFeetPerSecond;
			return SpeedKilometresPerHour.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Speed.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num3);
		}
		case "m/s":
		{
			double num2 = data / SpeedConstants.KilometresPerHourToMetresPerSecond;
			return SpeedKilometresPerHour.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Speed.Units, ToStringOptions.AlwaysSigned, "0.000000E+00", num2);
		}
		case "degree":
		case "degrees":
		case "°":
			return AngleDegrees.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Angle.Units, "0.000000E+00", data);
		case "radian":
		{
			double num = data / AngleConstants.DegreeToRadians;
			return AngleDegrees.ToString(UnitsGlobal.CurrentCulture, FormatOptions.Instance.Angle.Units, "0.000000E+00", num);
		}
		case "m.m'":
			return LatitudeMinutes.ToVboString(data, FormatOptions.Instance.LatLong.Units);
		default:
			return data.ToString("0.000000E+00");
		}
	}

	public static implicit operator CanData(double value)
	{
		return new CanData(value);
	}

	public static implicit operator double(CanData value)
	{
		return value._data;
	}
}
