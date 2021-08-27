using System;
using System.Collections.Generic;

namespace Racelogic.Gnss
{
	public static class CRC24Q
	{
		private const uint polynomial = 25578747u;

		private static readonly uint[] table;

		static CRC24Q()
		{
			table = new uint[256];
			table[1] = 25578747u;
			uint num = 25578747u;
			for (int num2 = 2; num2 < 256; num2 <<= 1)
			{
				if (((num <<= 1) & 0x1000000u) != 0)
				{
					num ^= 0x1864CFBu;
				}
				for (int i = 0; i < num2; i++)
				{
					table[num2 + i] = table[i] ^ num;
				}
			}
		}

		public static IEnumerable<byte> ComputeBytes(byte[] dataBits)
		{
			return UInt24ToBytes(Compute(dataBits));
		}

		private static IEnumerable<byte> UInt24ToBytes(uint value)
		{
			for (int i = 23; i >= 0; i--)
			{
				yield return (byte)((value >> i) & 1u);
			}
		}

		public static uint Compute(byte[] dataBits)
		{
			if (dataBits == null)
			{
				throw new ArgumentNullException("dataBits");
			}
			int num9 = dataBits.Length;
			int num2 = num9 >> 3 << 3;
			int num3 = num9 - num2;
			if (num3 == 0)
			{
				num3 = 8;
			}
			uint num4 = 0u;
			int num5 = 0;
			while (num5 < dataBits.Length)
			{
				uint num6 = 0u;
				for (int num7 = num3; num7 > 0; num7--)
				{
					num6 <<= 1;
					num6 |= dataBits[num5++];
				}
				uint num8 = num6 ^ ((num4 >> 16) & 0xFFu);
				num4 = (num4 << 8) ^ table[num8];
				num3 = 8;
			}
			return num4 & 0xFFFFFFu;
		}
	}
}
