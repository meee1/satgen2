using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen.Galileo;

internal sealed class NavigationDataE5b : NavigationDataINav
{
	private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE5b);

	private static readonly SignalType signalType = Signal.AllSignals.First((Signal s) => s.NavigationDataInfos.Any((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE5b)).SignalType;

	private static readonly WordType[] pageFormats = new WordType[15]
	{
		WordType.Word1,
		WordType.Word3,
		WordType.Word5,
		WordType.Word79,
		WordType.Word810,
		WordType.Spare,
		WordType.Spare,
		WordType.Spare,
		WordType.Spare,
		WordType.Spare,
		WordType.Word2,
		WordType.Word4,
		WordType.Word6,
		WordType.Spare,
		WordType.Spare
	};

	public static NavigationDataInfo Info
	{
		[DebuggerStepThrough]
		get
		{
			return dataInfo;
		}
	}

	protected override SignalType SignalType
	{
		[DebuggerStepThrough]
		get
		{
			return signalType;
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

	protected override WordType[] PageFormats
	{
		[DebuggerStepThrough]
		get
		{
			return pageFormats;
		}
	}

	public NavigationDataE5b(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
		: base(in satIndex, almanac, in interval, simulatedSignals)
	{
	}

	protected override PagePartParams GetFirstPagePartParameters(int pageIndex, int subframeIndex, in GnssTime pageSlotTime)
	{
		int pagePartIndex = 0;
		return new PagePartParams(in pagePartIndex, in pageIndex, in subframeIndex, in pageSlotTime);
	}

	protected override PagePartParams GetSecondPagePartParameters(in int pageIndex, in int subframeIndex, in GnssTime pageSlotTime)
	{
		int pagePartIndex = 1;
		return new PagePartParams(in pagePartIndex, in pageIndex, in subframeIndex, in pageSlotTime);
	}

	protected override int GetFirstAlmanacIndex(in int subframeIndex)
	{
		return (subframeIndex >> 1) * 3;
	}

	protected override IEnumerable<byte> GetReservedBits1(PagePartParams pageParams)
	{
		return NavigationData.ZeroBits.Take(64);
	}
}
