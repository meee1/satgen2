using System.Diagnostics;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen.Galileo;

internal sealed class PagePartParams
{
	private readonly int pagePartIndex;

	private readonly int pageIndex;

	private readonly int subframeIndex;

	private readonly GnssTime pageTime;

	public int PagePartIndex
	{
		[DebuggerStepThrough]
		get
		{
			return pagePartIndex;
		}
	}

	public int PageIndex
	{
		[DebuggerStepThrough]
		get
		{
			return pageIndex;
		}
	}

	public int SubframeIndex
	{
		[DebuggerStepThrough]
		get
		{
			return subframeIndex;
		}
	}

	public GnssTime PageTime
	{
		[DebuggerStepThrough]
		get
		{
			return pageTime;
		}
	}

	public PagePartParams(in int pagePartIndex, in int pageIndex, in int subframeIndex, in GnssTime pageTime)
	{
		this.pagePartIndex = pagePartIndex;
		this.pageIndex = pageIndex;
		this.subframeIndex = subframeIndex;
		this.pageTime = pageTime;
	}
}
