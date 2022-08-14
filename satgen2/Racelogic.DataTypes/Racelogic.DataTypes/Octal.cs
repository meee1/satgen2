using System;

namespace Racelogic.DataTypes;

public readonly struct Octal
{
	private readonly ulong decimalValue;

	public Octal(ulong decimalValue)
	{
		this.decimalValue = decimalValue;
	}

	public static implicit operator long(in Octal octal)
	{
		return (long)octal.decimalValue;
	}

	public static implicit operator ulong(in Octal octal)
	{
		return octal.decimalValue;
	}

	public static implicit operator int(in Octal octal)
	{
		return (int)octal.decimalValue;
	}

	public static implicit operator uint(in Octal octal)
	{
		return (uint)octal.decimalValue;
	}

	public static implicit operator sbyte(in Octal octal)
	{
		return (sbyte)octal.decimalValue;
	}

	public static implicit operator byte(in Octal octal)
	{
		return (byte)octal.decimalValue;
	}

	public static explicit operator Octal(in ulong octalAsLong)
	{
		ulong num = octalAsLong;
		return new Octal(Convert.ToUInt64(num.ToString(), 8));
	}

	public override bool Equals(object obj)
	{
		if (obj is Octal octal)
		{
			return Equals(octal);
		}
		return false;
	}

	public override int GetHashCode()
	{
		ulong num = decimalValue;
		return 5993773 + num.GetHashCode();
	}

	public static bool operator ==(in Octal left, in Octal right)
	{
		return left.decimalValue == right.decimalValue;
	}

	public static bool operator !=(in Octal left, in Octal right)
	{
		return left.decimalValue != right.decimalValue;
	}
}
