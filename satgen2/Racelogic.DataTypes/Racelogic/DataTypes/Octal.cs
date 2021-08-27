using System;

namespace Racelogic.DataTypes
{
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
			return new Octal(Convert.ToUInt64(octalAsLong.ToString(), 8));
		}

		public override bool Equals(object obj)
		{
			if (obj is Octal)
			{
				Octal octal = (Octal)obj;
				return Equals(octal);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 5993773 + decimalValue.GetHashCode();
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
}
