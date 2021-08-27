using System;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationCBOC : ModulationBPSK
	{
		private readonly sbyte[][] modulationSequences;

		private readonly byte[] secondaryCode;

		private readonly uint secondaryCodeLength;

		private uint secondaryCodeIndex;

		public ModulationCBOC(ModulationBank modulationBank, Signal signal, byte[] data, in int bitRate, sbyte[][] modulationSequences, byte[] secondaryCode, in double intervalLength, in decimal secondOfWeek, in GnssTime timeStamp)
			: base(modulationBank, signal, data, in bitRate, null, null, in intervalLength, in timeStamp)
		{
			this.modulationSequences = modulationSequences;
			this.secondaryCode = secondaryCode;
			secondaryCodeLength = (uint)secondaryCode.Length;
			secondaryCodeIndex = (uint)(secondOfWeek * (decimal)bitRate).SafeFloor() % secondaryCodeLength;
		}

		public sealed override sbyte[] Modulate()
		{
			if (Data == null)
			{
				throw new NotSupportedException("Data cannot be null when calling ModulationCBOC.Modulate()");
			}
			uint num = (uint)Math.Round((double)BitRate * IntervalLength);
			uint num2 = (uint)Signal.ModulationRate / BitRate;
			uint size = num * num2;
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint num3 = 0u;
			for (uint num4 = 0u; num4 < num; num4++)
			{
				int num5 = (Data[num4] << 1) | secondaryCode[secondaryCodeIndex];
				Array.Copy(modulationSequences[num5], 0L, pinnedBuffer, num3, num2);
				num3 += num2;
				if (++secondaryCodeIndex >= secondaryCodeLength)
				{
					secondaryCodeIndex = 0u;
				}
			}
			return pinnedBuffer;
		}
	}
}
