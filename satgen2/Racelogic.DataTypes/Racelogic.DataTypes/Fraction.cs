using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.DataTypes;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("{Value} ({Numerator}/{Denominator})")]
public readonly struct Fraction : IComparable<Fraction>
{
	private readonly int numerator;

	private readonly int denominator;

	private readonly double realValue;

	private static bool throwArgumentException;

	[JsonProperty(PropertyName = "Numerator")]
	public int Numerator
	{
		[DebuggerStepThrough]
		get
		{
			return numerator;
		}
	}

	[JsonProperty(PropertyName = "Denominator")]
	public int Denominator
	{
		[DebuggerStepThrough]
		get
		{
			return denominator;
		}
	}

	public double Value
	{
		[DebuggerStepThrough]
		get
		{
			return realValue;
		}
	}

	public static bool ThrowArgumentException
	{
		[DebuggerStepThrough]
		get
		{
			return throwArgumentException;
		}
		[DebuggerStepThrough]
		set
		{
			throwArgumentException = value;
		}
	}

	[JsonConstructor]
	public Fraction(int numerator, int denominator)
	{
		this.numerator = numerator;
		this.denominator = ((denominator == 0) ? 1 : denominator);
		if (denominator == 0 && throwArgumentException)
		{
			throw new ArgumentException("denominator cannot be zero", "denominator");
		}
		int num = GreatestCommonDivisor(numerator, denominator);
		if (num != 0)
		{
			numerator /= num;
			denominator /= num;
		}
		realValue = (double)numerator / (double)denominator;
	}

	public Fraction(uint numerator, uint denominator)
		: this((int)numerator, (int)denominator)
	{
	}

	public Fraction(int integerValue)
		: this(integerValue, 1)
	{
	}

	public static Fraction operator +(in Fraction first, in Fraction second)
	{
		int num = first.Numerator * second.Denominator + second.Numerator * first.Denominator;
		int num2 = first.Denominator * second.Denominator;
		return new Fraction(num, num2);
	}

	public static Fraction operator -(in Fraction first, in Fraction second)
	{
		int num = first.Numerator * second.Denominator - second.Numerator * first.Denominator;
		int num2 = first.Denominator * second.Denominator;
		return new Fraction(num, num2);
	}

	public static Fraction operator *(in Fraction first, in Fraction second)
	{
		int num = first.Numerator * second.Numerator;
		int num2 = first.Denominator * second.Denominator;
		return new Fraction(num, num2);
	}

	public static Fraction operator /(in Fraction first, in Fraction second)
	{
		int num = first.Numerator * second.Denominator;
		int num2 = first.Denominator * second.Numerator;
		return new Fraction(num, num2);
	}

	public static implicit operator double(in Fraction fraction)
	{
		return fraction.Value;
	}

	public static explicit operator Fraction(int integer)
	{
		return new Fraction(integer);
	}

	public static bool operator <(in Fraction left, in Fraction right)
	{
		return left.realValue < right.realValue;
	}

	public static bool operator <=(in Fraction left, in Fraction right)
	{
		return left.realValue <= right.realValue;
	}

	public static bool operator >(in Fraction left, in Fraction right)
	{
		return left.realValue > right.realValue;
	}

	public static bool operator >=(in Fraction left, in Fraction right)
	{
		return left.realValue >= right.realValue;
	}

	public int CompareTo(Fraction other)
	{
		double num = realValue;
		return num.CompareTo(other.realValue);
	}

	public static bool operator ==(in Fraction fraction1, in Fraction fraction2)
	{
		if ((object)fraction1 == (object)fraction2)
		{
			return true;
		}
		return fraction1.CompareTo(fraction2) == 0;
	}

	public static bool operator !=(in Fraction fraction1, in Fraction fraction2)
	{
		return !(fraction1 == fraction2);
	}

	public override bool Equals(object obj)
	{
		if (obj is Fraction)
		{
			return CompareTo((Fraction)obj) == 0;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = numerator;
		int num2 = (5993773 + num.GetHashCode()) * 9973;
		num = denominator;
		return num2 + num.GetHashCode();
	}

	public Fraction Inverse()
	{
		return new Fraction(denominator, numerator);
	}

	public override string ToString()
	{
		int num = numerator;
		string text = num.ToString();
		num = denominator;
		return text + "/" + num;
	}

	private static int GreatestCommonDivisor(int a, int b)
	{
		if (b != 0)
		{
			return GreatestCommonDivisor(b, a % b);
		}
		return a;
	}
}
