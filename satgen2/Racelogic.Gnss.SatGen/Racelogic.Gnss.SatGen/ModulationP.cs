using System;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Gps;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

internal sealed class ModulationP : ModulationBPSK
{
	private const int lockTimeout = 10000;

	private static readonly SyncLock pCodeLibraryLock = new SyncLock("P-Code Library Lock", 10000);

	private const decimal oneOverX1EpochLength = 0.6666666666666666666666666667m;

	private readonly int satIndex;

	private readonly decimal secondOfWeek;

	public ModulationP(ModulationBank modulationBank, Signal signal, in int satIndex, byte[] data, in int bitRate, in Range<GnssTime, GnssTimeSpan> interval, in GnssTime timeStamp)
	{
		double intervalLength = interval.Width.Seconds;
		base._002Ector(modulationBank, signal, data, in bitRate, null, null, in intervalLength, in timeStamp);
		this.satIndex = satIndex;
		secondOfWeek = interval.Start.GpsSecondOfWeekDecimal;
	}

	public sealed override sbyte[] Modulate()
	{
		uint num = (uint)Math.Round((double)BitRate * IntervalLength);
		uint modulationRate = (uint)Signal.ModulationRate;
		uint num2 = modulationRate / BitRate;
		uint size = num * num2;
		sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
		uint x1EpochIndex = (uint)(secondOfWeek * 0.6666666666666666666666666667m).SafeFloor();
		sbyte[] orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
		uint num3 = (uint)Math.Round((secondOfWeek - (decimal)x1EpochIndex * 1.5m) * (decimal)modulationRate);
		uint num4 = num - 1;
		uint num5 = 0u;
		if (Data != null)
		{
			for (uint num6 = 0u; num6 <= num4; num6++)
			{
				if (Data[num6] > 0)
				{
					uint num7 = num3 + num2;
					uint num8 = num5;
					for (uint num9 = num3; num9 < num7; num9++)
					{
						int num10 = orCreateX1PCode[num9];
						pinnedBuffer[num8] = (sbyte)(-num10);
						num8++;
					}
				}
				else
				{
					Array.Copy(orCreateX1PCode, num3, pinnedBuffer, num5, num2);
				}
				num5 += num2;
				num3 += num2;
				if ((long)num3 >= 15345000L && num6 < num4)
				{
					x1EpochIndex++;
					if (x1EpochIndex == 403200)
					{
						x1EpochIndex = 0u;
					}
					orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
					num3 = 0u;
				}
			}
		}
		else
		{
			for (uint num11 = 0u; num11 <= num4; num11++)
			{
				Array.Copy(orCreateX1PCode, num3, pinnedBuffer, num5, num2);
				num5 += num2;
				num3 += num2;
				if ((long)num3 >= 15345000L && num11 < num4)
				{
					x1EpochIndex++;
					if (x1EpochIndex == 403200)
					{
						x1EpochIndex = 0u;
					}
					orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
					num3 = 0u;
				}
			}
		}
		return pinnedBuffer;
	}

	private static sbyte[] GetOrCreateX1PCode(ModulationBank modulationBank, in int satIndex, in uint x1EpochIndex)
	{
		sbyte[][] value;
		using (pCodeLibraryLock.Lock())
		{
			if (!modulationBank.PCodeX1Library.TryGetValue(x1EpochIndex, out value))
			{
				value = new sbyte[50][];
				modulationBank.PCodeX1Library.Add(x1EpochIndex, value);
			}
		}
		sbyte[] array = value[satIndex];
		if (array == null)
		{
			int size = 15345000;
			sbyte[] buffer = modulationBank.GetBuffer(in size);
			size = (int)x1EpochIndex;
			array = CodeP.GetSignedSequence(in satIndex, in size, buffer);
			value[satIndex] = array;
		}
		return array;
	}
}
