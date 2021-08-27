using System;
using Racelogic.Geodetics;
using Racelogic.Gnss.Galileo;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationAltBOC : ModulationBPSK
	{
		private readonly sbyte[][] modulationSequences;

		private readonly byte[] dataE5aI;

		private readonly byte[] dataE5bI;

		private readonly uint bitRateE5aI;

		private readonly uint bitRateE5bI;

		private readonly byte[] secondaryCodeE5aI;

		private readonly byte[] secondaryCodeE5bI;

		private readonly byte[] secondaryCodeE5aQ;

		private readonly byte[] secondaryCodeE5bQ;

		private readonly uint secondaryRate;

		private readonly uint e5aIBitInWeek;

		public ModulationAltBOC(ModulationBank modulationBank, Signal signal, byte[] dataE5aI, byte[] dataE5bI, in int bitRateE5aI, in int bitRateE5bI, in int secondaryRate, sbyte[][] modulationSequences, byte[] secondaryCodeE5aI, byte[] secondaryCodeE5bI, byte[] secondaryCodeE5aQ, byte[] secondaryCodeE5bQ, in double intervalLength, in decimal secondOfWeek, in GnssTime timeStamp)
			: base(modulationBank, signal, dataE5aI, in bitRateE5aI, null, null, in intervalLength, in timeStamp)
		{
			this.dataE5aI = dataE5aI;
			this.dataE5bI = dataE5bI;
			this.bitRateE5aI = (uint)bitRateE5aI;
			this.bitRateE5bI = (uint)bitRateE5bI;
			this.secondaryRate = (uint)secondaryRate;
			this.modulationSequences = modulationSequences;
			this.secondaryCodeE5aI = secondaryCodeE5aI;
			this.secondaryCodeE5bI = secondaryCodeE5bI;
			this.secondaryCodeE5aQ = secondaryCodeE5aQ;
			this.secondaryCodeE5bQ = secondaryCodeE5bQ;
			e5aIBitInWeek = (uint)(secondOfWeek * (decimal)bitRateE5aI).SafeFloor();
		}

		public sealed override sbyte[] Modulate()
		{
			uint num = (uint)Math.Round((double)bitRateE5aI * IntervalLength);
			uint num11 = (uint)Signal.ModulationRate / bitRateE5aI;
			uint size = num * num11;
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint num17 = secondaryRate / bitRateE5aI;
			uint num18 = secondaryRate / bitRateE5bI;
			uint num19 = bitRateE5bI / bitRateE5aI;
			uint num20 = num11 / num17;
			uint num21 = (uint)secondaryCodeE5aI.Length;
			uint num22 = (uint)secondaryCodeE5bI.Length;
			uint num23 = (uint)secondaryCodeE5aQ.Length;
			uint num2 = (uint)secondaryCodeE5bQ.Length;
			uint num24 = e5aIBitInWeek * num17;
			uint num3 = num24 % num21;
			uint num4 = num24 % num22;
			uint num5 = num24 % num23;
			uint num6 = num24 % num2;
			uint num7 = 0u;
			uint num8 = 0u;
			for (uint num9 = 0u; num9 < num; num9++)
			{
				uint num10 = dataE5aI[num9];
				for (uint num12 = num19; num12 != 0; num12--)
				{
					uint num13 = dataE5bI[num7++];
					for (uint num14 = num18; num14 != 0; num14--)
					{
						uint num15 = secondaryCodeE5aI[num3];
						uint num16 = secondaryCodeE5bI[num4];
						uint bitE5aI = num10 ^ num15;
						uint bitE5bI = num13 ^ num16;
						uint bitE5aQ = secondaryCodeE5aQ[num5];
						uint bitE5bQ = secondaryCodeE5bQ[num6];
						uint modulationIndex = CodeE5AltBOC.GetModulationIndex(bitE5aI, bitE5bI, bitE5aQ, bitE5bQ);
						Array.Copy(modulationSequences[modulationIndex], 0L, pinnedBuffer, num8, num20);
						num8 += num20;
						if (++num3 >= num21)
						{
							num3 = 0u;
						}
						if (++num4 >= num22)
						{
							num4 = 0u;
						}
						if (++num5 >= num23)
						{
							num5 = 0u;
						}
						if (++num6 >= num2)
						{
							num6 = 0u;
						}
					}
				}
			}
			return pinnedBuffer;
		}
	}
}
