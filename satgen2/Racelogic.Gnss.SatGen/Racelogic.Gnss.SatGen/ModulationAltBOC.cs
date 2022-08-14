using System;
using Racelogic.Geodetics;
using Racelogic.Gnss.Galileo;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

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
		uint num2 = (uint)Signal.ModulationRate / bitRateE5aI;
		uint size = num * num2;
		sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
		uint num3 = secondaryRate / bitRateE5aI;
		uint num4 = secondaryRate / bitRateE5bI;
		uint num5 = bitRateE5bI / bitRateE5aI;
		uint num6 = num2 / num3;
		uint num7 = (uint)secondaryCodeE5aI.Length;
		uint num8 = (uint)secondaryCodeE5bI.Length;
		uint num9 = (uint)secondaryCodeE5aQ.Length;
		uint num10 = (uint)secondaryCodeE5bQ.Length;
		uint num11 = e5aIBitInWeek * num3;
		uint num12 = num11 % num7;
		uint num13 = num11 % num8;
		uint num14 = num11 % num9;
		uint num15 = num11 % num10;
		uint num16 = 0u;
		uint num17 = 0u;
		for (uint num18 = 0u; num18 < num; num18++)
		{
			uint num19 = dataE5aI[num18];
			for (uint num20 = num5; num20 != 0; num20--)
			{
				uint num21 = dataE5bI[num16++];
				for (uint num22 = num4; num22 != 0; num22--)
				{
					uint num23 = secondaryCodeE5aI[num12];
					uint num24 = secondaryCodeE5bI[num13];
					uint bitE5aI = num19 ^ num23;
					uint bitE5bI = num21 ^ num24;
					uint bitE5aQ = secondaryCodeE5aQ[num14];
					uint bitE5bQ = secondaryCodeE5bQ[num15];
					uint modulationIndex = CodeE5AltBOC.GetModulationIndex(bitE5aI, bitE5bI, bitE5aQ, bitE5bQ);
					Array.Copy(modulationSequences[modulationIndex], 0L, pinnedBuffer, num17, num6);
					num17 += num6;
					if (++num12 >= num7)
					{
						num12 = 0u;
					}
					if (++num13 >= num8)
					{
						num13 = 0u;
					}
					if (++num14 >= num9)
					{
						num14 = 0u;
					}
					if (++num15 >= num10)
					{
						num15 = 0u;
					}
				}
			}
		}
		return pinnedBuffer;
	}
}
