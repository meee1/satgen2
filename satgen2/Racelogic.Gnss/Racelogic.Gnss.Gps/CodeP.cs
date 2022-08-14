using Racelogic.Utilities;

namespace Racelogic.Gnss.Gps;

public static class CodeP
{
	private const int periodA = 4092;

	private const int periodB = 4093;

	private const int x1ACyclesInX1Epoch = 3750;

	private const int x2ACyclesInX2Epoch = 3750;

	private const int x2BCyclesInX2Epoch = 3749;

	private const int x2Precession = 37;

	private const int extendedX2APeriod = 4129;

	private const long chipsInX2Epoch = 15345037L;

	private static readonly uint[] x1ASeed = new uint[12]
	{
		0u, 0u, 1u, 0u, 0u, 1u, 0u, 0u, 1u, 0u,
		0u, 0u
	};

	private static readonly uint[] x1BSeed = new uint[12]
	{
		0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u,
		0u, 0u
	};

	private static readonly uint[] x2ASeed = new uint[12]
	{
		1u, 0u, 0u, 1u, 0u, 0u, 1u, 0u, 0u, 1u,
		0u, 1u
	};

	private static readonly uint[] x2BSeed = new uint[12]
	{
		0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u,
		0u, 0u
	};

	private static readonly int[] x1ATaps = new int[4] { 6, 8, 11, 12 };

	private static readonly int[] x1BTaps = new int[8] { 1, 2, 5, 8, 9, 10, 11, 12 };

	private static readonly int[] x2ATaps = new int[10] { 1, 3, 4, 5, 7, 8, 9, 10, 11, 12 };

	private static readonly int[] x2BTaps = new int[6] { 2, 3, 4, 8, 9, 12 };

	private static readonly byte[] x1ACode = FibonacciShiftRegister.Generate(x1ASeed, x1ATaps, 4092).ToArray(4092);

	private static readonly byte[] x1BCode = FibonacciShiftRegister.Generate(x1BSeed, x1BTaps, 4093).ToArray(4093);

	private static readonly byte[] x2ACode = FibonacciShiftRegister.Generate(x2ASeed, x2ATaps, 4092).ToArray(4092);

	private static readonly byte[] x2BCode = FibonacciShiftRegister.Generate(x2BSeed, x2BTaps, 4093).ToArray(4093);

	public const decimal X1EpochLength = 1.5m;

	public const int X1EpochsInWeek = 403200;

	public const long ChipsInX1Epoch = 15345000L;

	public static sbyte[] GetSignedSequence(in int satIndex, in int x1EpochIndexInWeek, sbyte[] buffer)
	{
		int num = satIndex + 1;
		int num2 = 0;
		bool flag = x1EpochIndexInWeek == 403199;
		int num3 = 0;
		int num4;
		int num5;
		int num6;
		if (x1EpochIndexInWeek == 0)
		{
			num4 = -num;
			num5 = -num;
			num6 = 0;
		}
		else
		{
			int num7 = (int)(((long)x1EpochIndexInWeek * 15345000L - num) % 15345037);
			num6 = num7 / 4092;
			if (num6 == 3750)
			{
				num6--;
			}
			num4 = num7 - num6 * 4092;
			int num8 = num7 / 4093;
			if (num8 == 3749)
			{
				num8--;
			}
			num5 = num7 - num8 * 4093;
		}
		for (int i = 0; i < 3750; i++)
		{
			bool flag2 = i == 3749;
			for (int j = 0; j < 4092; j++)
			{
				byte num9 = x1ACode[j];
				int num10 = x1BCode[num3];
				int num11 = num9 ^ num10;
				byte num12 = x2ACode[(num4 < 4092 && num4 >= 0) ? num4 : 4091];
				int num13 = x2BCode[(num5 < 4093 && num5 >= 0) ? num5 : 4092];
				int num14 = num12 ^ num13;
				int num15 = num11 ^ num14;
				num15 <<= 1;
				num15--;
				buffer[num2++] = (sbyte)num15;
				num3++;
				if (num3 == 4093)
				{
					num3 = (flag2 ? (num3 - 1) : 0);
				}
				num4++;
				num5++;
				if (flag && flag2)
				{
					continue;
				}
				if (num6 == 3749)
				{
					if (num4 == 4129)
					{
						num4 = 0;
						num6++;
						num5 = 0;
					}
					continue;
				}
				if (num4 == 4092)
				{
					num4 = 0;
					num6++;
				}
				if (num5 == 4093)
				{
					num5 = 0;
				}
			}
		}
		return buffer;
	}
}
