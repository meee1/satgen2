using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.SatGen.Gps;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

internal abstract class NavigationData
{
	private readonly int satIndex;

	private readonly int satId;

	private readonly AlmanacBase almanac;

	private readonly Range<GnssTime, GnssTimeSpan> interval;

	private readonly IEnumerable<SignalType> simulatedSignalTypes;

	private readonly IEnumerable<FrequencyBand> frequencyBands;

	private static readonly int minBitRate = (from s in Signal.AllSignals
		select s.NavigationDataInfos into ndds
		where ndds != null
		select ndds).SelectMany((IEnumerable<NavigationDataInfo> ndds) => ndds.Select((NavigationDataInfo ndd) => ndd.BitRate)).Min();

	protected static readonly byte[] ZeroBits = new byte[100];

	protected static readonly byte[] OneBits = Enumerable.Range(0, 195).Select((Func<int, byte>)((int n) => 1)).ToArray(195);

	protected static readonly byte[] ZeroAndOneBits = (from n in Enumerable.Range(0, 448)
		select (byte)((uint)n & 1u)).ToArray(448);

	protected static readonly byte[] OneAndZeroBits = (from n in Enumerable.Range(1, 238)
		select (byte)((uint)n & 1u)).ToArray(238);

	internal static int MinBitRate
	{
		[DebuggerStepThrough]
		get
		{
			return minBitRate;
		}
	}

	public abstract int BitRate { get; }

	public IEnumerable<SignalType> SimulatedSignalTypes
	{
		[DebuggerStepThrough]
		get
		{
			return simulatedSignalTypes;
		}
	}

	public IEnumerable<FrequencyBand> FrequencyBands
	{
		[DebuggerStepThrough]
		get
		{
			return frequencyBands;
		}
	}

	public Range<GnssTime, GnssTimeSpan> Interval
	{
		[DebuggerStepThrough]
		get
		{
			return interval;
		}
	}

	public AlmanacBase Almanac
	{
		[DebuggerStepThrough]
		get
		{
			return almanac;
		}
	}

	public int SatelliteId
	{
		[DebuggerStepThrough]
		get
		{
			return satId;
		}
	}

	public int SatelliteIndex
	{
		[DebuggerStepThrough]
		get
		{
			return satIndex;
		}
	}

	protected NavigationData(in int satIndex, AlmanacBase almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
	{
		this.almanac = almanac;
		this.satIndex = satIndex;
		satId = satIndex + 1;
		this.interval = interval;
		simulatedSignalTypes = simulatedSignals.Select((Signal s) => s.SignalType).ToArray();
		frequencyBands = simulatedSignals.Select((Signal s) => s.FrequencyBand).Distinct().ToArray();
	}

	public static void ClearFEC(NavigationDataType navigationDataType)
	{
		switch (navigationDataType)
		{
		case NavigationDataType.GpsL2C:
			NavigationDataL2C.ClearFEC();
			break;
		case NavigationDataType.GpsL5:
			NavigationDataL5I.ClearFEC();
			break;
		}
	}

	public static void FinalizeFEC(in GnssTime intervalEnd, NavigationDataType navigationDataType)
	{
		switch (navigationDataType)
		{
		case NavigationDataType.GpsL2C:
		{
			GnssTime intervalEnd3 = intervalEnd - GnssTimeSpan.FromSeconds(ConstellationBase.SignalTravelTimeLimits.Min);
			NavigationDataL2C.FinalizeFEC(in intervalEnd3);
			break;
		}
		case NavigationDataType.GpsL5:
		{
			GnssTime intervalEnd2 = intervalEnd - GnssTimeSpan.FromSeconds(ConstellationBase.SignalTravelTimeLimits.Min);
			NavigationDataL5I.FinalizeFEC(in intervalEnd2);
			break;
		}
		}
	}

	public abstract byte[] Generate();

	protected virtual void AddFirstWord(List<byte> data, IEnumerable<byte> rawWord)
	{
		IEnumerable<byte> collection = EncodeFirstWord(rawWord);
		data.AddRange(collection);
	}

	protected void AddWord(List<byte> data, IEnumerable<byte> rawWord)
	{
		IEnumerable<byte> collection = EncodeWord(rawWord);
		data.AddRange(collection);
	}

	protected virtual IEnumerable<byte> EncodeFirstWord(IEnumerable<byte> rawWord)
	{
		return EncodeWord(rawWord);
	}

	protected virtual IEnumerable<byte> EncodeWord(IEnumerable<byte> rawWord)
	{
		return rawWord;
	}

	protected static byte[] Dec2Bin(in double value, in int bitCount)
	{
		return Dec2Bin((long)value, in bitCount);
	}

	protected static byte[] Dec2Bin(in int value, in int bitCount)
	{
		return Dec2Bin((long)value, in bitCount);
	}

	protected static byte[] Dec2Bin(in uint value, in int bitCount)
	{
		return Dec2Bin((ulong)value, in bitCount);
	}

	protected static byte[] Dec2Bin(long value, in int bitCount)
	{
		if (value < 0)
		{
			value += 1L << bitCount;
		}
		return Dec2Bin((ulong)value, in bitCount);
	}

	public static byte[] Dec2Bin(ulong value, in int bitCount)
	{
		byte[] array = new byte[bitCount];
		for (int num = bitCount - 1; num >= 0; num--)
		{
			array[num] = (byte)(value & 1);
			value >>= 1;
		}
		return array;
	}

	protected internal static int Bin2Dec(byte[] binaryNumber)
	{
		int num = 0;
		int num2 = binaryNumber.Length - 1;
		for (int i = 0; i < binaryNumber.Length; i++)
		{
			if (binaryNumber[i] == 1)
			{
				num |= 1 << num2 - i;
			}
		}
		return num;
	}
}
