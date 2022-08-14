using System;
using System.ComponentModel;
using System.Diagnostics;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

public interface ILiveOutput : INotifyPropertyChanged
{
	int BufferUnderrunCount
	{
		[DebuggerStepThrough]
		get;
	}

	double DataTransferLatency
	{
		[DebuggerStepThrough]
		get;
	}

	bool IsLowLatency
	{
		[DebuggerStepThrough]
		get;
	}

	GnssTime? TrueTimeStart
	{
		[DebuggerStepThrough]
		get;
	}

	int BufferCount
	{
		[DebuggerStepThrough]
		get;
	}

	bool IsReady
	{
		[DebuggerStepThrough]
		get;
	}

	bool IsAlive
	{
		[DebuggerStepThrough]
		get;
	}

	event EventHandler<TimeSpan> BufferUnderrun;

	event EventHandler PlaybackStarted;
}
