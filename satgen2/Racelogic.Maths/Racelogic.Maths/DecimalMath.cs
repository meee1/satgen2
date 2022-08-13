using System;

namespace Racelogic.Maths;

public static class DecimalMath
{
	private const decimal TwoPI = 6.2831853071795864769252867666m;

	private const decimal HalfPI = 1.5707963267948966192313216916m;

	private const decimal QuarterPI = 0.7853981633974483096156608458m;

	private const decimal Einv = 0.3678794411714423215955237702m;

	private const decimal Log10Inv = 0.4342944819032518276511289189m;

	private const int MaxIteration = 100;

	public const decimal PI = 3.1415926535897932384626433833m;

	public const decimal E = 2.7182818284590452353602874714m;

	public static decimal Exp(decimal x)
	{
		int num = 0;
		while (x > 1.0m)
		{
			--x;
			num++;
		}
		while (x < 0.0m)
		{
			++x;
			num--;
		}
		int num2 = 1;
		decimal num3 = 1.0m;
		decimal num4 = 1.0m;
		decimal num5;
		do
		{
			num5 = num3;
			num4 *= x / (decimal)num2++;
			num3 += num4;
		}
		while (num5 != num3);
		if (num != 0)
		{
			num3 *= IntPow(2.7182818284590452353602874714m, num);
		}
		return num3;
	}

	public static decimal Pow(decimal x, decimal pow)
	{
		if (pow == 0.0m)
		{
			return 1.0m;
		}
		if (pow == 1.0m)
		{
			return x;
		}
		if (x == 1.0m)
		{
			return 1.0m;
		}
		if (x == 0.0m && pow == 0.0m)
		{
			return 1.0m;
		}
		if (x == 0.0m)
		{
			if (pow > 0.0m)
			{
				return 0.0m;
			}
			throw new InvalidOperationException("Zero base and negative power");
		}
		if (pow == -1.0m)
		{
			return 1.0m / x;
		}
		bool flag = IsInteger(pow);
		if (x < 0.0m && !flag)
		{
			throw new InvalidOperationException("Negative base and non-integer power");
		}
		if (flag && x > 0.0m)
		{
			int power = (int)pow;
			return IntPow(x, power);
		}
		if (flag && x < 0.0m)
		{
			if ((int)pow % 2 == 0)
			{
				return Exp(pow * Log(-x));
			}
			return -Exp(pow * Log(-x));
		}
		return Exp(pow * Log(x));
	}

	public static decimal IntPow(decimal x, int power)
	{
		if ((decimal)power == 0.0m)
		{
			return 1.0m;
		}
		if ((decimal)power < 0.0m)
		{
			return IntPow(1.0m / x, -power);
		}
		int num = power;
		decimal num2 = 1.0m;
		decimal num3 = x;
		while (num > 0)
		{
			if (num % 2 == 1)
			{
				num2 = num3 * num2;
				num--;
			}
			num3 *= num3;
			num /= 2;
		}
		return num2;
	}

	public static decimal Log10(decimal x)
	{
		return Log(x) * 0.4342944819032518276511289189m;
	}

	public static decimal Log(decimal x)
	{
		if (x <= 0.0m)
		{
			throw new ArgumentException("x must be greater than zero", "x");
		}
		int num = 0;
		while (x >= 1.0m)
		{
			x *= 0.3678794411714423215955237702m;
			num++;
		}
		while (x <= 0.3678794411714423215955237702m)
		{
			x *= 2.7182818284590452353602874714m;
			num--;
		}
		--x;
		if (x == 0m)
		{
			return num;
		}
		decimal num2 = 0.0m;
		int num3 = 0;
		decimal num4 = 1.0m;
		for (decimal num5 = num2 - 1.0m; num5 != num2; num2 += num4 / (decimal)num3)
		{
			if (num3 >= 100)
			{
				break;
			}
			num3++;
			num5 = num2;
			num4 *= -x;
		}
		return (decimal)num - num2;
	}

	public static decimal Cos(decimal x)
	{
		x = FastMath.NormalizeRadiansPi(x);
		if (x == -3.1415926535897932384626433833m)
		{
			return -1.0m;
		}
		if (x == 0.0m)
		{
			return 1.0m;
		}
		x *= x;
		decimal num = -x * 0.5m;
		decimal num2 = 1.0m + num;
		decimal num3 = num2 - 1.0m;
		int num4 = 1;
		while (num3 != num2 && num4 < 100)
		{
			num3 = num2;
			decimal num5 = num4 * (num4 + num4 + 3) + 1;
			num5 = -0.5m / num5;
			num *= x * num5;
			num2 += num;
			num4++;
		}
		return num2;
	}

	public static decimal Sin(decimal x)
	{
		decimal num = Cos(x);
		decimal num2 = Sqrt(1.0m - num * num);
		if (!IsSignOfSinePositive(x))
		{
			return -num2;
		}
		return num2;
	}

	public static decimal Tan(decimal x)
	{
		decimal num = Cos(x);
		if (num == 0.0m)
		{
			throw new ArgumentException("x is a multiple of +/- Pi/2 and the tangent is +/- infinity, which cannot be represented using decimal numbers.", "x");
		}
		return Sin(x) / num;
	}

	public static decimal Asin(decimal x)
	{
		if (x > 1.0m || x < -1.0m)
		{
			throw new ArgumentException("x must be in <-1, 1>", "x");
		}
		if (x == 0.0m)
		{
			return 0.0m;
		}
		if (x == 1.0m)
		{
			return 1.5707963267948966192313216916m;
		}
		if (x < 0.0m)
		{
			return -Asin(-x);
		}
		decimal num = 1.0m - 2m * x * x;
		if (Math.Abs(x) > Math.Abs(num))
		{
			decimal num2 = Asin(num);
			return 0.5m * (1.5707963267948966192313216916m - num2);
		}
		decimal result = 0.0m;
		decimal num3 = x;
		int num4 = 1;
		int num5 = 3;
		result += num3;
		decimal num6 = x * x;
		decimal num7;
		do
		{
			num7 = num3;
			num3 *= num6 * (1.0m - 0.5m / (decimal)num4);
			result += num3 / (decimal)num5;
			num4++;
			num5 += 2;
		}
		while (num7 != num3);
		return result;
	}

	public static decimal Acos(decimal x)
	{
		if (x > 1.0m || x < -1.0m)
		{
			throw new ArgumentException("x must be in <-1, 1>", "x");
		}
		if (x == 0.0m)
		{
			return 1.5707963267948966192313216916m;
		}
		if (x == 1.0m)
		{
			return 0.0m;
		}
		if (x < 0.0m)
		{
			return 3.1415926535897932384626433833m - Acos(-x);
		}
		return 1.5707963267948966192313216916m - Asin(x);
	}

	public static decimal Atan(decimal x)
	{
		if (x == 0.0m)
		{
			return 0.0m;
		}
		if (x == 1.0m)
		{
			return 0.7853981633974483096156608458m;
		}
		return Asin(x / Sqrt(1.0m + x * x));
	}

	public static decimal Atan2(decimal y, decimal x)
	{
		if (x > 0.0m)
		{
			return Atan(y / x);
		}
		if (x < 0.0m && y >= 0.0m)
		{
			return Atan(y / x) + 3.1415926535897932384626433833m;
		}
		if (x < 0.0m && y < 0.0m)
		{
			return Atan(y / x) - 3.1415926535897932384626433833m;
		}
		if (x == 0.0m && y > 0.0m)
		{
			return 1.5707963267948966192313216916m;
		}
		if (x == 0.0m && y < 0.0m)
		{
			return -1.5707963267948966192313216916m;
		}
		throw new ArgumentException("Invalid Atan2 arguments");
	}

	public static decimal Sqrt(decimal x)
	{
		if (x < 0.0m)
		{
			throw new ArgumentException("Cannot calculate square root from a negative number", "x");
		}
		decimal num = (decimal)Math.Sqrt((double)x);
		decimal num2;
		do
		{
			num2 = num;
			if (num2 == 0.0m)
			{
				return 0.0m;
			}
			num = (num2 + x / num2) * 0.5m;
		}
		while (Math.Abs(num2 - num) > 0.0m);
		return num;
	}

	public static decimal Sinh(decimal x)
	{
		decimal num = Exp(x);
		decimal num2 = 1.0m / num;
		return (num - num2) * 0.5m;
	}

	public static decimal Cosh(decimal x)
	{
		decimal num = Exp(x);
		decimal num2 = 1.0m / num;
		return (num + num2) * 0.5m;
	}

	public static decimal Tanh(decimal x)
	{
		decimal num = Exp(x);
		decimal num2 = 1.0m / num;
		return (num - num2) / (num + num2);
	}

	private static bool IsInteger(decimal x)
	{
		long num = (long)x;
		return x.AlmostEquals(num);
	}

	private static bool IsSignOfSinePositive(decimal x)
	{
		x = FastMath.NormalizeRadiansPi(x);
		if (!(x >= 0.0m))
		{
			return x == -3.1415926535897932384626433833m;
		}
		return true;
	}
}
