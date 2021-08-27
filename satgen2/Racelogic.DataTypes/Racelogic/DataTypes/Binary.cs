using System;

namespace Racelogic.DataTypes
{
	public readonly struct Binary
	{
		private readonly ulong decimalValue;

		public Binary(ulong decimalValue)
		{
			this.decimalValue = decimalValue;
		}

		public static implicit operator long(in Binary binary)
		{
			return (long)binary.decimalValue;
		}

		public static implicit operator ulong(in Binary binary)
		{
			return binary.decimalValue;
		}

		public static implicit operator int(in Binary binary)
		{
			return (int)binary.decimalValue;
		}

		public static implicit operator uint(in Binary binary)
		{
			return (uint)binary.decimalValue;
		}

		public static implicit operator sbyte(in Binary binary)
		{
			return (sbyte)binary.decimalValue;
		}

		public static implicit operator byte(in Binary binary)
		{
			return (byte)binary.decimalValue;
		}

		public static explicit operator Binary(ulong binaryAsULong)
		{
			return new Binary(Convert.ToUInt64(binaryAsULong.ToString(), 2));
		}

		public override bool Equals(object obj)
		{
			if (obj is Binary)
			{
				Binary binary = (Binary)obj;
				return Equals(binary);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 5993773 + decimalValue.GetHashCode();
		}

		public static bool operator ==(in Binary left, in Binary right)
		{
			return left.decimalValue == right.decimalValue;
		}

		public static bool operator !=(in Binary left, in Binary right)
		{
			return left.decimalValue != right.decimalValue;
		}
	}
}
