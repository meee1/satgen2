using System;

namespace Racelogic.DataSource.Can;

public class DataFieldValidator
{
	public (int Length, int StartBit) MakeSignalFit(byte dataLengthCode, int length, ByteOrder byteOrder, int startBit)
	{
		if (IsStateValid(dataLengthCode, length, byteOrder, startBit))
		{
			return (length, startBit);
		}
		return (AdjustLength(length, dataLengthCode), AdjustStartBit(dataLengthCode, length, byteOrder, startBit));
	}

	public bool IsStateValid(byte dataLengthCode, int length, ByteOrder byteOrder, int startBit)
	{
		if (IsLengthValid(length, dataLengthCode))
		{
			return IsStartBitValid(dataLengthCode, length, byteOrder, startBit);
		}
		return false;
	}

	public bool IsLengthValid(int length, byte dataLengthCode)
	{
		return length <= dataLengthCode * 8;
	}

	public int AdjustLength(int length, byte dataLengthCode)
	{
		return Math.Min(length, dataLengthCode * 8);
	}

	public bool IsStartBitValid(byte dataLengthCode, int length, ByteOrder byteOrder, int startBit)
	{
		if (startBit >= 0 && startBit < dataLengthCode * 8)
		{
			if (byteOrder != ByteOrder.Intel)
			{
				return IsMotorolaOrderStateValid(dataLengthCode, length, startBit);
			}
			return IsIntelOrderStateValid(dataLengthCode, length, startBit);
		}
		return false;
	}

	public int AdjustStartBit(byte dataLengthCode, int length, ByteOrder byteOrder, int startBit)
	{
		if (byteOrder != ByteOrder.Intel)
		{
			return AdjustMotorolaStartBit(dataLengthCode, length, startBit);
		}
		return AdjustIntelStartBit(dataLengthCode, length, startBit);
	}

	private int AdjustMotorolaStartBit(byte dataLengthCode, int length, int startBit)
	{
		return ReverseBytes(AdjustIntelStartBit(dataLengthCode, length, ReverseBytes(startBit, dataLengthCode)), dataLengthCode);
	}

	private int AdjustIntelStartBit(byte dataLengthCode, int length, int startBit)
	{
		if (startBit > 0)
		{
			return IntelRightMostStartBit(dataLengthCode, length);
		}
		return 0;
	}

	public int IntelRightMostStartBit(byte dataLengthCode, int length)
	{
		return 8 * dataLengthCode - length;
	}

	private bool IsIntelOrderStateValid(byte dataLengthCode, int length, int startBit)
	{
		return startBit + length <= 8 * dataLengthCode;
	}

	private bool IsMotorolaOrderStateValid(byte dataLengthCode, int length, int startBit)
	{
		return IsIntelOrderStateValid(dataLengthCode, length, ReverseBytes(startBit, dataLengthCode));
	}

	public int MotorolaLeftMostStartBit(int length)
	{
		return MotorolaMinimumStartBit(length) + (((length > 0) ? ((length - 1) & 7) : 0) ^ 7);
	}

	public int MotorolaMaximumLengthForStartBit(int startBit)
	{
		return (startBit & -8) - (startBit & 7) + 8;
	}

	public int MotorolaMinimumStartBit(int length)
	{
		if (length > 0)
		{
			return (length - 1) & -8;
		}
		return 0;
	}

	public int MotorolaMaximumStartBit(byte dataLengthCode, int length)
	{
		if (dataLengthCode != 0)
		{
			return ReverseBytes(Math.Min(7, IntelRightMostStartBit(dataLengthCode, length)), dataLengthCode);
		}
		return 0;
	}

	public int MotorolaRightMostStartBit(int dataLengthCode)
	{
		if (dataLengthCode > 0)
		{
			return (dataLengthCode - 1) * 8;
		}
		return 0;
	}

	public static int ReverseBytes(int startBit, int dataLengthCode)
	{
		if (dataLengthCode == 0)
		{
			return 0;
		}
		int num = startBit % 8;
		int num2 = startBit / 8;
		num2 = dataLengthCode - 1 - startBit / 8;
		return 8 * num2 + num;
	}

	public int MotorolaSkipInvalidBlock(int oldStartBit, int newStartBit, int length)
	{
		if (newStartBit > oldStartBit)
		{
			return MotorolaMinimumStartBit(length) + 8;
		}
		return MotorolaLeftMostStartBit(length);
	}
}
