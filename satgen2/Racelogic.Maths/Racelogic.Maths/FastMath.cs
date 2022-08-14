using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Racelogic.Maths;

public static class FastMath
{
	private static double doubleSafetyMargin;

	private static decimal decimalSafetyMargin;

	private const double referenceAccuracy = 0.00012;

	private const int referenceLookupCount = 100;

	private static double cosAccuracy;

	private static int cosLookupCount;

	private static int quarterCycleLookupCount;

	private const double cosLookupRange = 0.5;

	private static double cosLookupStepsPerCycle;

	private static readonly (double CosValue, double NextDiff)[] cosLookupTable;

	private const double MinusQuarterCycle = -0.25;

	private const double ThreeQuartersCycle = 0.75;

	private const double doubleAccuracy = 1.7763568394002505E-15;

	private const decimal decimalAccuracy = 0.000000000000000000000000001m;

	public const double MachineEpsilonDouble = 2.2204460492503131E-16;

	public const decimal MachineEpsilonDecimal = 0.0000000000000000000000000001m;

	public const double PI = Math.PI / 2.0;

	public const double HalfPI = Math.PI / 2.0;

	public const double TwoPI = Math.PI * 2.0;

	public const double PIInv = 1.0 / Math.PI;

	public const double TwoPIInv = 1.0 / (2.0 * Math.PI);

	public const decimal PIDecimal = 3.1415926535897932384626433833m;

	public const decimal HalfPIDecimal = 1.5707963267948966192313216916m;

	public const decimal TwoPIDecimal = 6.2831853071795864769252867666m;

	public const decimal PIInvDecimal = 0.3183098861837906715377675267m;

	public const decimal TwoPIInvDecimal = 0.1591549430918953357688837634m;

	public static double DoubleSafetyMargin
	{
		[DebuggerStepThrough]
		get
		{
			return doubleSafetyMargin;
		}
		[DebuggerStepThrough]
		set
		{
			doubleSafetyMargin = value;
		}
	}

	public static decimal DecimalSafetyMargin
	{
		[DebuggerStepThrough]
		get
		{
			return decimalSafetyMargin;
		}
		[DebuggerStepThrough]
		set
		{
			decimalSafetyMargin = value;
		}
	}

	public static double SinCosAccuracy
	{
		[DebuggerStepThrough]
		get
		{
			return cosAccuracy;
		}
		set
		{
			if (value != cosAccuracy)
			{
				cosAccuracy = value;
				cosLookupCount = (int)(Math.Sqrt(0.00012 / cosAccuracy) * 100.0).SafeCeiling();
				quarterCycleLookupCount = cosLookupCount >> 1;
				cosLookupStepsPerCycle = (double)cosLookupCount / 0.5;
				InitializeCosLookupTable(cosLookupCount);
			}
		}
	}

	public static int SinCosLookupTableSize
	{
		[DebuggerStepThrough]
		get
		{
			return cosLookupCount;
		}
		set
		{
			if (value != cosLookupCount)
			{
				cosLookupCount = value;
				quarterCycleLookupCount = cosLookupCount >> 1;
				double num = 100.0 / (double)cosLookupCount;
				cosAccuracy = 0.00012 * num * num;
				cosLookupStepsPerCycle = (double)cosLookupCount / 0.5;
				InitializeCosLookupTable(cosLookupCount);
			}
		}
	}

	static FastMath()
	{
		doubleSafetyMargin = 1E-12;
		decimalSafetyMargin = 0.000000000000000000001m;
		cosAccuracy = 0.00012;
		cosLookupCount = 100;
		quarterCycleLookupCount = cosLookupCount >> 1;
		cosLookupStepsPerCycle = (double)cosLookupCount / 0.5;
		cosLookupTable = new(double, double)[cosLookupCount + 1];
		/*decimalAccuracy = 0.000000000000000000000000001m;
		MachineEpsilonDecimal = 0.0000000000000000000000000001m;
		PIDecimal = 3.1415926535897932384626433833m;
		HalfPIDecimal = 1.5707963267948966192313216916m;
		TwoPIDecimal = 6.2831853071795864769252867666m;
		PIInvDecimal = 0.3183098861837906715377675267m;
		TwoPIInvDecimal = 0.1591549430918953357688837634m;*/
		InitializeCosLookupTable(cosLookupCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Floor(this double x)
	{
		long num = (long)x;
		if (x < 0.0)
		{
			num--;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal Floor(this decimal x)
	{
		long num = (long)x;
		if (x < 0.0m)
		{
			num--;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double SafeFloor(this double x)
	{
		return (x + doubleSafetyMargin).Floor();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal SafeFloor(this decimal x)
	{
		return (x + decimalSafetyMargin).Floor();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Ceiling(this double x)
	{
		long num = (long)x;
		if (x > 0.0)
		{
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal Ceiling(this decimal x)
	{
		long num = (long)x;
		if (x > 0.0m)
		{
			num++;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double SafeCeiling(this double x)
	{
		return (x - doubleSafetyMargin).Ceiling();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal SafeCeiling(this decimal x)
	{
		return (x - decimalSafetyMargin).Ceiling();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static bool IsFinite(this double number)
	{
		return (ulong)(*(long*)(&number) & 0x7FFFFFFFFFFFFFFFL) < 9218868437227405312uL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNegativeZero(this double number)
	{
		if (number == 0.0)
		{
			return BitConverter.DoubleToInt64Bits(number) < 0;
		}
		return false;
	}

	public static uint SafeAdd(this uint unsignedValue, int signedValue)
	{
		if (signedValue < 0)
		{
			return unsignedValue - (uint)(-signedValue);
		}
		return unsignedValue + (uint)signedValue;
	}

	public static int SafeSubtract(this uint firstUnsignedValue, uint secondUnsignedValue)
	{
		if (firstUnsignedValue >= secondUnsignedValue)
		{
			return (int)(firstUnsignedValue - secondUnsignedValue);
		}
		return (int)(0 - (secondUnsignedValue - firstUnsignedValue));
	}

	public static double NormalizeRadiansPi(double radians)
	{
		if ((radians >= -Math.PI && radians < Math.PI) || !radians.IsFinite())
		{
			return radians;
		}
		decimal num = ((decimal)radians + 3.1415926535897932384626433833m) % 6.2831853071795864769252867666m;
		if (num < 0.0m)
		{
			return (double)(num + 3.1415926535897932384626433833m);
		}
		return (double)(num - 3.1415926535897932384626433833m);
	}

	public static decimal NormalizeRadiansPi(decimal radians)
	{
		if (radians >= -3.1415926535897932384626433833m && radians < 3.1415926535897932384626433833m)
		{
			return radians;
		}
		decimal num = (radians + 3.1415926535897932384626433833m) % 6.2831853071795864769252867666m;
		if (num < 0.0m)
		{
			return num + 3.1415926535897932384626433833m;
		}
		return num - 3.1415926535897932384626433833m;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double NormalizeCycles(this double cycles)
	{
		return cycles - cycles.Floor();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double NormalizeCycles5(this double cycles)
	{
		return cycles - (cycles + 0.5).Floor();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double DegreesToCycles(this double degrees)
	{
		return (degrees * (1.0 / 360.0)).NormalizeCycles();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double DegreesToCycles5(this double degrees)
	{
		return (degrees * (1.0 / 360.0)).NormalizeCycles5();
	}

	public static double Sin(this double cycles)
	{
		cycles += ((cycles < -0.25) ? 0.75 : (-0.25));
		double num = cosLookupStepsPerCycle * Math.Abs(cycles);
		int num2 = (int)num;
		double num3 = num - (double)num2;
		var (num4, num5) = cosLookupTable[num2];
		return num4 + num3 * num5;
	}

	public static double Cos(this double cycles)
	{
		double num = cosLookupStepsPerCycle * Math.Abs(cycles);
		int num2 = (int)num;
		double num3 = num - (double)num2;
		var (num4, num5) = cosLookupTable[num2];
		return num4 + num3 * num5;
	}

	private static void InitializeCosLookupTable(int count)
	{
		double num = Math.PI / (double)count;
		int num2 = count >> 1;
		double num3 = 1.0;
		for (int i = 1; i <= cosLookupTable.Length; i++)
		{
			double num4 = ((i != num2) ? Math.Cos((double)i * num) : 0.0);
			cosLookupTable[i - 1] = (num3, num4 - num3);
			num3 = num4;
		}
	}

	public static bool AlmostEquals(this double a, double b)
	{
		double num = Math.Abs(a - b);
		double num2 = Math.Abs(a);
		double num3 = Math.Abs(b);
		if (num2 < 2.2204460492503131E-16 || num3 < 2.2204460492503131E-16)
		{
			return num < 1.7763568394002505E-15;
		}
		if ((num2 == 0.0 && num3 < 1.7763568394002505E-15) || (num3 == 0.0 && num2 < 1.7763568394002505E-15))
		{
			return true;
		}
		return num < 1.7763568394002505E-15 * Math.Max(num2, num3);
	}

	public static bool AlmostEquals(this decimal a, decimal b)
	{
		decimal num = Math.Abs(a - b);
		decimal num2 = Math.Abs(a);
		decimal num3 = Math.Abs(b);
		if (num2 < 0.0000000000000000000000000001m || num3 < 0.0000000000000000000000000001m)
		{
			return num < 0.000000000000000000000000001m;
		}
		if ((num2 == 0.0m && num3 < 0.000000000000000000000000001m) || (num3 == 0.0m && num2 < 0.000000000000000000000000001m))
		{
			return true;
		}
		return num < 0.000000000000000000000000001m * Math.Max(num2, num3);
	}

	public static double IntPow(double value, int power)
	{
		double num = 1.0;
		while (power != 0)
		{
			if (((uint)power & (true ? 1u : 0u)) != 0)
			{
				num *= value;
			}
			value *= value;
			power >>= 1;
		}
		return num;
	}

	public static double IntPow(double value, uint power)
	{
		double num = 1.0;
		while (power != 0)
		{
			if ((power & (true ? 1u : 0u)) != 0)
			{
				num *= value;
			}
			value *= value;
			power >>= 1;
		}
		return num;
	}

	public static double IntPow(double value, long power)
	{
		double num = 1.0;
		while (power != 0L)
		{
			if ((power & 1) != 0L)
			{
				num *= value;
			}
			value *= value;
			power >>= 1;
		}
		return num;
	}

	public static double IntPow(double value, ulong power)
	{
		double num = 1.0;
		while (power != 0L)
		{
			if ((power & 1) != 0L)
			{
				num *= value;
			}
			value *= value;
			power >>= 1;
		}
		return num;
	}

	public static int GreatestCommonDivisor(int a, int b)
	{
		if (a <= 0)
		{
			throw new ArgumentOutOfRangeException("a must be greater than 0.");
		}
		if (b <= 0)
		{
			throw new ArgumentOutOfRangeException("b must be greater than 0.");
		}
		if (a == b)
		{
			return a;
		}
		if (a > b && a % b == 0)
		{
			return b;
		}
		if (b > a && b % a == 0)
		{
			return a;
		}
		int num = 1;
		while (b != 0)
		{
			num = b;
			b = a % b;
			a = num;
		}
		return num;
	}

	public static int LowestCommonMultiple(int a, int b)
	{
		a /= GreatestCommonDivisor(a, b);
		return a * b;
	}

	public static double LevelToGain(this double level)
	{
		return 20.0 * Math.Log10(level);
	}

	public static double GainToLevel(this double gain)
	{
		return Math.Pow(10.0, 0.05 * gain);
	}
}
