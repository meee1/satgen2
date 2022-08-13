using System;

namespace Racelogic.Comms.Serial;

public class MemoryInfo
{
	public readonly MemoryType MemoryType;

	public readonly uint TotalMemory;

	public readonly uint FreeMemory;

	public MemoryInfo(MemoryType MemoryType, uint TotalMemory, uint FreeMemory)
	{
		this.MemoryType = MemoryType;
		this.TotalMemory = TotalMemory;
		this.FreeMemory = FreeMemory;
	}

	public MemoryInfo(byte[] data)
	{
		if (data.Length == 9)
		{
			MemoryType = (MemoryType)data[0];
			for (int i = 1; i < 5; i++)
			{
				TotalMemory <<= 8;
				TotalMemory |= data[i];
				FreeMemory <<= 8;
				FreeMemory |= data[i + 4];
			}
			return;
		}
		throw new ArgumentException("Racelogic.Comms.Serial.MemoryInfo(Byte[] data) - invalid length of data");
	}
}
