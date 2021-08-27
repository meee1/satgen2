using System;
using Racelogic.Geodetics;
using Racelogic.Gnss.Gps;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationM : Modulation
	{
		public ModulationM(ModulationBank modulationBank, Signal signal, double intervalLength, in GnssTime timeStamp)
			: base(modulationBank, signal, in intervalLength, in timeStamp)
		{
		}

		public sealed override sbyte[] Modulate()
		{
			uint size = (uint)Math.Round(IntervalLength * (double)Signal.ModulationRate);
			uint num = (uint)CodeM.SignedCode.Length;
			sbyte[] signedCode = CodeM.SignedCode;
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint num2 = size;
			uint num3 = 0u;
			while (num2 != 0)
			{
				uint num4 = ((num2 > num) ? num : num2);
				Array.Copy(signedCode, 0L, pinnedBuffer, num3, num4);
				num3 += num;
				num2 -= num;
			}
			return pinnedBuffer;
		}
	}
}
