using System;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationCMCL : ModulationBPSK
	{
		private readonly sbyte[] clCode;

		private readonly decimal secondOfWeek;

		public ModulationCMCL(ModulationBank modulationBank, Signal signal, byte[] data, in int bitRate, sbyte[] cmCode, sbyte[] negatedCmCode, sbyte[] clCode, in Range<GnssTime, GnssTimeSpan> interval, in GnssTime timeStamp): base(modulationBank, signal, data, in bitRate, cmCode, negatedCmCode,  interval.Width.Seconds, in timeStamp)
		{
			double intervalLength = interval.Width.Seconds;
			
			this.clCode = clCode;
			secondOfWeek = interval.Start.GpsSecondOfWeekDecimal;
		}

		public sealed override sbyte[] Modulate()
		{
			if (ChipCode == null)
			{
				throw new NotSupportedException("ChipCode cannot be null when calling ModulationCMCL.Modulate()");
			}
			if (NegatedChipCode == null)
			{
				throw new NotSupportedException("NegatedChipCode cannot be null when calling ModulationCMCL.Modulate()");
			}
			uint num = (uint)Math.Round((double)BitRate * IntervalLength);
			uint modulationRate = (uint)Signal.ModulationRate;
			uint size = num * (modulationRate / BitRate);
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint num2 = modulationRate >> 1;
			ulong num8 = (ulong)Math.Round(secondOfWeek * (decimal)num2);
			uint num3 = (uint)clCode.Length;
			uint num4 = (uint)(num8 % num3);
			uint num5 = 0u;
			if (Data != null)
			{
				sbyte[][] array = new sbyte[2][] { ChipCode, NegatedChipCode };
				for (uint num6 = 0u; num6 < num; num6++)
				{
					sbyte[] array2 = array[Data[num6]];
					for (int i = 0; i < array2.Length; i++)
					{
						pinnedBuffer[num5++] = array2[i];
						pinnedBuffer[num5++] = clCode[num4++];
					}
					if (num4 == num3)
					{
						num4 = 0u;
					}
				}
			}
			else
			{
				for (uint num7 = num; num7 != 0; num7--)
				{
					for (int j = 0; j < ChipCode!.Length; j++)
					{
						pinnedBuffer[num5++] = ChipCode[j];
						pinnedBuffer[num5++] = clCode[num4++];
					}
					if (num4 == num3)
					{
						num4 = 0u;
					}
				}
			}
			return pinnedBuffer;
		}
	}
}
