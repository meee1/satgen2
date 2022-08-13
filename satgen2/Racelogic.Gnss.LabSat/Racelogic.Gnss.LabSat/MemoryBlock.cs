using System;
using System.Diagnostics;
using System.Linq;

namespace Racelogic.Gnss.LabSat;

internal abstract class MemoryBlock
{
	private readonly LabSat2 labSat;

	private readonly int baseAddress;

	private readonly byte[] memory;

	private readonly byte[] crc = new byte[4];

	private readonly byte[] memoryAsRead;

	protected int BaseAddress
	{
		[DebuggerStepThrough]
		get
		{
			return baseAddress;
		}
	}

	protected byte[] Memory
	{
		[DebuggerStepThrough]
		get
		{
			return memory;
		}
	}

	protected MemoryBlock(LabSat2 labSat, in int baseAddress, in int blockLength)
	{
		this.labSat = labSat;
		this.baseAddress = baseAddress;
		memory = labSat.ReadEepromBytes(in baseAddress, in blockLength);
		Array.Copy(memory, memory.Length - crc.Length, crc, 0, crc.Length);
		memoryAsRead = new byte[memory.Length];
		Array.Copy(memory, memoryAsRead, memory.Length);
		if (!CheckCrc())
		{
			memory = labSat.ReadDioEepromBytes(in baseAddress, in blockLength);
			Array.Copy(memory, memory.Length - crc.Length, crc, 0, crc.Length);
			if (CheckCrc())
			{
				WriteToLabSat();
			}
		}
	}

	internal void WriteToLabSat()
	{
		if (this.labSat == null)
		{
			return;
		}
		UpdateCrc();
		for (int i = 0; i < memory.Length; i++)
		{
			if (memory[i] != memoryAsRead[i])
			{
				int num = i;
				for (; i < memory.Length && memory[i] != memoryAsRead[i]; i++)
				{
				}
				byte[] array = new byte[i - num];
				Array.Copy(memory, num, array, 0, array.Length);
				LabSat2 labSat = this.labSat;
				int startAddress = baseAddress + num;
				labSat.WriteEepromBytes(in startAddress, array);
				LabSat2 labSat2 = this.labSat;
				startAddress = baseAddress + num;
				labSat2.WriteDioEepromBytes(in startAddress, array);
				Array.Copy(array, 0, memoryAsRead, num, array.Length);
			}
		}
	}

	private bool CheckCrc()
	{
		byte[] array = new byte[memory.Length - crc.Length];
		Array.Copy(memory, array, array.Length);
		return CRC32.ComputeBigEndian(array).SequenceEqual(crc);
	}

	private bool UpdateCrc()
	{
		byte[] array = new byte[memory.Length - crc.Length];
		Array.Copy(memory, array, array.Length);
		byte[] array2 = CRC32.ComputeBigEndian(array);
		if (array2.SequenceEqual(crc))
		{
			return false;
		}
		Array.Copy(array2, crc, crc.Length);
		Array.Copy(crc, 0, memory, memory.Length - crc.Length, crc.Length);
		return true;
	}
}
