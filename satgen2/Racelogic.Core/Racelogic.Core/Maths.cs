using System;
using System.Collections.Generic;

namespace Racelogic.Core;

public static class Maths
{
	public static readonly double DegreesToRadians = Math.PI / 180.0;

	public static readonly double RadiansToDegrees = 180.0 / Math.PI;

	public static readonly double RadianToMinute = RadiansToDegrees * 60.0;

	public static readonly double MinuteToRadian = DegreesToRadians / 60.0;

	public static ValueIs Compare(float value, float otherValue, double epsilon = 1E-05)
	{
		if ((double)Math.Abs(value - otherValue) <= epsilon)
		{
			return ValueIs.Equal;
		}
		if (value < otherValue)
		{
			return ValueIs.LessThan;
		}
		return ValueIs.GreaterThan;
	}

	public static ValueIs Compare(double value, double otherValue, double epsilon = 1E-11)
	{
		if (Math.Abs(value - otherValue) <= epsilon)
		{
			return ValueIs.Equal;
		}
		if (value < otherValue)
		{
			return ValueIs.LessThan;
		}
		return ValueIs.GreaterThan;
	}

	public static double DegreeToRadian(double angle)
	{
		return Math.PI * angle * (1.0 / 180.0);
	}

	public static double RadianToDegree(double angle)
	{
		return angle * (180.0 / Math.PI);
	}

	public static double MinutesToRadians(double angle)
	{
		return DegreeToRadian(angle * (1.0 / 60.0));
	}

	public static double RadiansToMinutes(double angle)
	{
		return RadianToDegree(angle) * 60.0;
	}

	public static double Round(double a, double b)
	{
		return Math.Round(a / b) * b;
	}

	public static double Round(double value, uint decimalPlaces)
	{
		return Math.Round(value, (int)decimalPlaces, MidpointRounding.AwayFromZero);
	}

	public static double ByteSwap(double value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToDouble(bytes, 0);
	}

	public static float ByteSwap(float value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToSingle(bytes, 0);
	}

	public static long ByteSwap(long value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToInt64(bytes, 0);
	}

	public static ulong ByteSwap(ulong value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToUInt64(bytes, 0);
	}

	public static int ByteSwap(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToInt32(bytes, 0);
	}

	public static short ByteSwap(short value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToInt16(bytes, 0);
	}

	public static uint ByteSwap(uint value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToUInt32(bytes, 0);
	}

	public static ushort ByteSwap(ushort value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Reverse((Array)bytes);
		return BitConverter.ToUInt16(bytes, 0);
	}

	public static double Interpolate(double now, double old, double target)
	{
		double num = now - old;
		if (num == 0.0)
		{
			return 0.0;
		}
		return (now - target) / num;
	}

	public static double FindExactValue(double now, double old, double actual, double rangeNow, double rangeOld)
	{
		double num = Interpolate(now, old, actual);
		return rangeNow - num * (rangeNow - rangeOld);
	}

	public static double LinearInterpolation(double now, double old, double actual, double rangeNow, double rangeOld)
	{
		if (now - old == 0.0)
		{
			return (rangeOld + rangeNow) / 2.0;
		}
		return rangeOld + (actual - old) * (rangeNow - rangeOld) / (now - old);
	}

	public static List<double> WindowSmooth(List<double> data, int smoothBy)
	{
		List<double> list = new List<double>();
		double num = 1.0 / (double)(smoothBy + smoothBy + 1);
		for (int i = 0; i < data.Count; i++)
		{
			double num2 = 0.0;
			for (int j = -smoothBy; j <= smoothBy; j++)
			{
				int num3 = i + j;
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num3 >= data.Count)
				{
					num3 = data.Count - 1;
				}
				num2 += data[num3];
			}
			list.Add(num2 * num);
		}
		return list;
	}

	public static double CalculateAngle(double x1, double y1, double x2, double y2)
	{
		double num = 0.0;
		double num2 = y2 - y1;
		double num3 = x2 - x1;
		num = ((Math.Abs(num2) > 0.0) ? RadianToDegree(Math.Atan(num3 / num2)) : ((!(num3 > 0.0)) ? 270.0 : 90.0));
		if (num3 >= 0.0)
		{
			if (num2 < 0.0)
			{
				num = 180.0 + num;
			}
		}
		else if (num2 > 0.0)
		{
			num = 360.0 + num;
		}
		else if (num2 < 0.0)
		{
			num = 180.0 + num;
		}
		return num;
	}
}
