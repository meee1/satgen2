using System;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Gps;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationP : ModulationBPSK
	{
		private const int lockTimeout = 10000;

		private static readonly SyncLock pCodeLibraryLock = new SyncLock("P-Code Library Lock", 10000);

		private const decimal oneOverX1EpochLength = 0.6666666666666666666666666667m;

		private readonly int satIndex;

		private readonly decimal secondOfWeek;

		public ModulationP(ModulationBank modulationBank, Signal signal, in int satIndex, byte[] data, in int bitRate, in Range<GnssTime, GnssTimeSpan> interval, in GnssTime timeStamp): base(modulationBank, signal, data, in bitRate, null, null, interval.Width.Seconds, in timeStamp)
		{
			double intervalLength = interval.Width.Seconds;
			
			this.satIndex = satIndex;
			secondOfWeek = interval.Start.GpsSecondOfWeekDecimal;
		}

		public sealed override sbyte[] Modulate()
		{
			uint num20 = (uint)Math.Round((double)BitRate * IntervalLength);
			uint modulationRate = (uint)Signal.ModulationRate;
			uint num12 = modulationRate / BitRate;
			uint size = num20 * num12;
			sbyte[] pinnedBuffer = ModulationBank.GetPinnedBuffer(in TimeStamp, in size);
			uint x1EpochIndex = (uint)(secondOfWeek * 0.6666666666666666666666666667m).SafeFloor();
			sbyte[] orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
			uint num13 = (uint)Math.Round((secondOfWeek - (decimal)x1EpochIndex * 1.5m) * (decimal)modulationRate);
			uint num14 = num20 - 1;
			uint num15 = 0u;
			if (Data != null)
			{
				for (uint num16 = 0u; num16 <= num14; num16++)
				{
					if (Data[num16] > 0)
					{
						uint num17 = num13 + num12;
						uint num18 = num15;
						for (uint num19 = num13; num19 < num17; num19++)
						{
							int num10 = orCreateX1PCode[num19];
							pinnedBuffer[num18] = (sbyte)(-num10);
							num18++;
						}
					}
					else
					{
						Array.Copy(orCreateX1PCode, num13, pinnedBuffer, num15, num12);
					}
					num15 += num12;
					num13 += num12;
					if ((long)num13 >= 15345000L && num16 < num14)
					{
						x1EpochIndex++;
						if (x1EpochIndex == 403200)
						{
							x1EpochIndex = 0u;
						}
						orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
						num13 = 0u;
					}
				}
			}
			else
			{
				for (uint num11 = 0u; num11 <= num14; num11++)
				{
					Array.Copy(orCreateX1PCode, num13, pinnedBuffer, num15, num12);
					num15 += num12;
					num13 += num12;
					if ((long)num13 >= 15345000L && num11 < num14)
					{
						x1EpochIndex++;
						if (x1EpochIndex == 403200)
						{
							x1EpochIndex = 0u;
						}
						orCreateX1PCode = GetOrCreateX1PCode(ModulationBank, in satIndex, in x1EpochIndex);
						num13 = 0u;
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
}
