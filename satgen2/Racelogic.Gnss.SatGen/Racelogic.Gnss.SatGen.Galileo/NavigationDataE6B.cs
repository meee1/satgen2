using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo;

internal sealed class NavigationDataE6B : NavigationData
{
	private static readonly NavigationDataInfo dataInfo;

	private const int pageLength = 1;

	private static readonly byte[] encodedDummyPageData;

	public static NavigationDataInfo Info
	{
		[DebuggerStepThrough]
		get
		{
			return dataInfo;
		}
	}

	public override int BitRate
	{
		[DebuggerStepThrough]
		get
		{
			return dataInfo.BitRate;
		}
	}

	static NavigationDataE6B()
	{
		dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE6B);
		IEnumerable<byte> first = NavigationData.ZeroBits.Take(14);
		IEnumerable<byte> second = NavigationData.ZeroAndOneBits.Take(448);
		byte[] array = first.Concat(second).ToArray(462);
		IEnumerable<byte> second2 = CRC24Q.ComputeBytes(array);
		IEnumerable<byte> inputSequence = Enumerable.Concat(second: NavigationData.ZeroBits.Take(6), first: array.Concat(second2));
		int registerState = 0;
		int captureIndex = 0;
		byte[] data = new ConvolutionalEncoder(inputSequence, in registerState, in captureIndex, ConvolutionalEncoderOptions.NegateG2).ToArray(984);
		registerState = 123;
		captureIndex = 8;
		IEnumerable<byte> second4 = new BlockInterleaver<byte>(in registerState, in captureIndex).Interleave(data);
		encodedDummyPageData = new byte[16]
		{
			1, 0, 1, 1, 0, 1, 1, 1, 0, 1,
			1, 1, 0, 0, 0, 0
		}.Concat(second4).ToArray(1000);
	}

	public NavigationDataE6B(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
		: base(in satIndex, almanac, in interval, simulatedSignals)
	{
	}

	public override byte[] Generate()
	{
		int galileoNavicSecondOfWeek = base.Interval.Start.GalileoNavicSecondOfWeek;
		int galileoNavicWeek = base.Interval.Start.GalileoNavicWeek;
		int second = galileoNavicSecondOfWeek / 1;
		GnssTime gnssTime = GnssTime.FromGalileoNavic(galileoNavicWeek, second);
		int num = (int)((base.Interval.End - gnssTime).Seconds / 1.0).SafeCeiling();
		int index = (int)Math.Round((base.Interval.Start - gnssTime).Seconds * (double)BitRate);
		List<byte> list = new List<byte>(num * BitRate);
		for (int i = 0; i < num; i++)
		{
			list.AddRange(encodedDummyPageData);
		}
		int num2 = (int)Math.Round(base.Interval.Width.Seconds * (double)BitRate);
		byte[] array = new byte[num2];
		list.CopyTo(index, array, 0, num2);
		return array;
	}
}
