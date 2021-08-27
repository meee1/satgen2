using System;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	internal class ModulationBPSK : Modulation
	{
		protected readonly byte[]? Data;

		protected readonly sbyte[]? ChipCode;

		protected readonly sbyte[]? NegatedChipCode;

		protected readonly uint BitRate;

		public ModulationBPSK(ModulationBank modulationBank, Signal signal, byte[]? data, in int bitRate, sbyte[]? chipCode, sbyte[]? negatedChipCode, in double intervalLength, in GnssTime timeStamp)
			: base(modulationBank, signal, in intervalLength, in timeStamp)
		{
			Data = data;
			BitRate = (uint)bitRate;
			ChipCode = chipCode;
			NegatedChipCode = negatedChipCode;
		}

		public override sbyte[] Modulate()
		{
			if (ChipCode == null)
			{
				throw new NotSupportedException("ChipCode cannot be null when calling ModulationBPSK.Modulate()");
			}
			if (NegatedChipCode == null)
			{
				throw new NotSupportedException("NegatedChipCode cannot be null when calling ModulationBPSK.Modulate()");
			}
			uint num = (uint)Math.Round((double)BitRate * IntervalLength);
			uint modulationRate = (uint)Signal.ModulationRate;
			uint size = num * (modulationRate / BitRate);
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint num2 = (uint)ChipCode!.Length;
			uint num3 = modulationRate / (BitRate * num2);
			if (Data != null)
			{
				sbyte[][] array = new sbyte[2][] { ChipCode, NegatedChipCode };
				uint num4 = 0u;
				for (int i = 0; i < num; i++)
				{
					sbyte[] sourceArray = array[Data[i]];
					uint num5 = num3;
					while (num5 != 0)
					{
						Array.Copy(sourceArray, 0L, pinnedBuffer, num4, num2);
						num5--;
						num4 += num2;
					}
				}
			}
			else
			{
				for (uint num6 = 0u; num6 < size; num6 += num2)
				{
					Array.Copy(ChipCode, 0L, pinnedBuffer, num6, num2);
				}
			}
			return pinnedBuffer;
		}
	}
}
