using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Racelogic.Gnss
{
	public static class Code
	{
		private static readonly byte[] nh10 = new byte[10] { 0, 0, 0, 0, 1, 1, 0, 1, 0, 1 };

		private static readonly byte[] negatedNh10 = new byte[10] { 1, 1, 1, 1, 0, 0, 1, 0, 1, 0 };

		private static readonly byte[] nh20 = new byte[20]
		{
			0, 0, 0, 0, 0, 1, 0, 0, 1, 1,
			0, 1, 0, 1, 0, 0, 1, 1, 1, 0
		};

		private static readonly byte[] negatedNh20 = new byte[20]
		{
			1, 1, 1, 1, 1, 0, 1, 1, 0, 0,
			1, 0, 1, 0, 1, 1, 0, 0, 0, 1
		};

		public static byte[] NH10
		{
			[DebuggerStepThrough]
			get
			{
				return nh10;
			}
		}

		public static byte[] NegatedNH10
		{
			[DebuggerStepThrough]
			get
			{
				return negatedNh10;
			}
		}

		public static byte[] NH20
		{
			[DebuggerStepThrough]
			get
			{
				return nh20;
			}
		}

		public static byte[] NegatedNH20
		{
			[DebuggerStepThrough]
			get
			{
				return negatedNh20;
			}
		}

		public static byte[] HexStringToBits(string hexString, int codeLength = 0)
		{
			if (codeLength == 0)
			{
				codeLength = hexString.Length << 2;
			}
			byte[] array = new byte[codeLength];
			int num = 0;
			for (int i = 0; i < hexString.Length; i++)
			{
				int num2 = int.Parse(hexString[i].ToString(), NumberStyles.HexNumber);
				int num3 = 3;
				while (num3 >= 0 && num < codeLength)
				{
					int num4 = (num2 >> num3) & 1;
					array[num++] = (byte)num4;
					num3--;
				}
			}
			return array;
		}

		public static sbyte[] HexStringToSignedBits(string hexString, int codeLength = 0)
		{
			if (hexString.Length == 0)
			{
				codeLength = hexString.Length << 2;
			}
			sbyte[] array = new sbyte[codeLength];
			int num = 0;
			for (int i = 0; i < hexString.Length; i++)
			{
				int num2 = int.Parse(hexString[i].ToString(), NumberStyles.HexNumber);
				int num3 = 3;
				while (num3 >= 0 && num < codeLength)
				{
					int num4 = (num2 >> num3) & 1;
					num4 <<= 1;
					num4--;
					array[num++] = (sbyte)num4;
					num3--;
				}
			}
			return array;
		}

		public static (byte[] Sequence, byte[] NegatedSequence) HexStringToBitsTuple(string hexString, int codeLength = 0)
		{
			if (hexString.Length == 0)
			{
				codeLength = hexString.Length << 2;
			}
			byte[] array = new byte[codeLength];
			byte[] array2 = new byte[codeLength];
			int num = 0;
			for (int i = 0; i < hexString.Length; i++)
			{
				int num2 = int.Parse(hexString[i].ToString(), NumberStyles.HexNumber);
				int num3 = 3;
				while (num3 >= 0 && num < codeLength)
				{
					int num4 = (num2 >> num3) & 1;
					array[num] = (byte)num4;
					array2[num++] = (byte)((uint)num4 ^ 1u);
					num3--;
				}
			}
			return (array, array2);
		}

		public static (sbyte[] SignedSequence, sbyte[] NegatedSignedSequence) HexStringToSignedBitsTuple(string hexString, int codeLength = 0)
		{
			if (hexString.Length == 0)
			{
				codeLength = hexString.Length << 2;
			}
			sbyte[] array = new sbyte[codeLength];
			sbyte[] array2 = new sbyte[codeLength];
			int num = 0;
			for (int i = 0; i < hexString.Length; i++)
			{
				int num2 = int.Parse(hexString[i].ToString(), NumberStyles.HexNumber);
				int num3 = 3;
				while (num3 >= 0 && num < codeLength)
				{
					int num4 = (num2 >> num3) & 1;
					num4 <<= 1;
					num4--;
					array[num] = (sbyte)num4;
					array2[num++] = (sbyte)(-num4);
					num3--;
				}
			}
			return (array, array2);
		}

		public static IEnumerable<byte> BinaryStringToBits(string binaryString)
		{
			return binaryString.Select((char c) => (byte)(c - 48));
		}

		public static string BitsToBinaryString(IEnumerable<byte> bits)
		{
			return string.Concat(bits);
		}

		public static string HexStringToBinaryString(string hexString)
		{
			StringBuilder stringBuilder = new StringBuilder(hexString.Length << 2);
			for (int i = 0; i < hexString.Length; i++)
			{
				int num = int.Parse(hexString[i].ToString(), NumberStyles.HexNumber);
				for (int num2 = 3; num2 >= 0; num2--)
				{
					int value = (num >> num2) & 1;
					stringBuilder.Append(value);
				}
			}
			return stringBuilder.ToString();
		}

		public static string BitsToHexString(IEnumerable<byte> bits)
		{
			StringBuilder stringBuilder = new StringBuilder();
			using (IEnumerator<byte> enumerator = bits.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					uint current = enumerator.Current;
					current <<= 1;
					current |= (uint)(enumerator.MoveNext() ? enumerator.Current : 0);
					current <<= 1;
					current |= (uint)(enumerator.MoveNext() ? enumerator.Current : 0);
					current <<= 1;
					current |= (uint)(enumerator.MoveNext() ? enumerator.Current : 0);
					stringBuilder.Append($"{current:X}");
				}
			}
			return stringBuilder.ToString();
		}
	}
}
