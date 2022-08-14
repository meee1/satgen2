using System;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

internal sealed class ModulationTiered : ModulationBPSK
{
	private readonly byte[] secondaryCode;

	private readonly byte[]? negatedSecondaryCode;

	private readonly ulong chipOfWeek;

	public ModulationTiered(ModulationBank modulationBank, Signal signal, byte[]? data, in int bitRate, sbyte[] chipCode, sbyte[] negatedChipCode, byte[] secondaryCode, byte[]? negatedSecondaryCode, in double intervalLength, in GnssTime timeStamp)
		: base(modulationBank, signal, data, in bitRate, chipCode, negatedChipCode, in intervalLength, in timeStamp)
	{
		this.secondaryCode = secondaryCode;
		this.negatedSecondaryCode = negatedSecondaryCode;
		chipOfWeek = 0uL;
	}

	public ModulationTiered(ModulationBank modulationBank, Signal signal, in int bitRate, sbyte[] chipCode, sbyte[] negatedChipCode, byte[] secondaryCode, in double intervalLength, in decimal secondOfWeek, in GnssTime timeStamp)
		: base(modulationBank, signal, null, in bitRate, chipCode, negatedChipCode, in intervalLength, in timeStamp)
	{
		this.secondaryCode = secondaryCode;
		chipOfWeek = (ulong)Math.Round(secondOfWeek * (decimal)signal.ModulationRate);
	}

	public sealed override sbyte[] Modulate()
	{
		if (ChipCode == null)
		{
			throw new NotSupportedException("ChipCode cannot be null when calling ModulationTiered.Modulate()");
		}
		if (NegatedChipCode == null)
		{
			throw new NotSupportedException("NegatedChipCode cannot be null when calling ModulationTiered.Modulate()");
		}
		uint modulationRate = (uint)Signal.ModulationRate;
		sbyte[][] array = new sbyte[2][] { ChipCode, NegatedChipCode };
		sbyte[] pinnedBuffer;
		if (Data != null)
		{
			if (negatedSecondaryCode == null)
			{
				throw new NotSupportedException("negatedSecondaryCode cannot be null when calling ModulationTiered.Modulate() and Data is not null");
			}
			uint num = (uint)Math.Round((double)BitRate * IntervalLength);
			uint size = num * (modulationRate / BitRate);
			pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			int num2 = ChipCode!.Length;
			byte[][] array2 = new byte[2][] { secondaryCode, negatedSecondaryCode };
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				byte[] array3 = array2[Data[i]];
				for (int j = 0; j < array3.Length; j++)
				{
					Array.Copy(array[array3[j]], 0, pinnedBuffer, num3, num2);
					num3 += num2;
				}
			}
		}
		else
		{
			uint num4 = (uint)ChipCode!.Length;
			uint num5 = (uint)secondaryCode.Length;
			uint num6 = (uint)Math.Round((double)(modulationRate / num4) * IntervalLength);
			uint num7 = (uint)(chipOfWeek / num4 % num5);
			uint size2 = num6 * num4;
			pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size2);
			uint num8 = 0u;
			for (uint num9 = num6; num9 != 0; num9--)
			{
				Array.Copy(array[secondaryCode[num7]], 0L, pinnedBuffer, num8, num4);
				num8 += num4;
				if (++num7 >= num5)
				{
					num7 = 0u;
				}
			}
		}
		return pinnedBuffer;
	}
}
