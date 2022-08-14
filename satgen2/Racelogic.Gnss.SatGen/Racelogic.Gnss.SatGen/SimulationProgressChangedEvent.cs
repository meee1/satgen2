using System;
using System.Diagnostics;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

public sealed class SimulationProgressChangedEventArgs : EventArgs
{
	private readonly double progress;

	private readonly GnssTime simulatedTime;

	private readonly GnssTimeSpan simulatedTimeFromStart;

	private readonly TimeSpan elapsedTime;

	private readonly TimeSpan timeLeft;

	public double Progress
	{
		[DebuggerStepThrough]
		get
		{
			return progress;
		}
	}

	public GnssTime SimulatedTime
	{
		[DebuggerStepThrough]
		get
		{
			return simulatedTime;
		}
	}

	public GnssTimeSpan SimulatedTimeFromStart
	{
		[DebuggerStepThrough]
		get
		{
			return simulatedTimeFromStart;
		}
	}

	public TimeSpan ElapsedTime
	{
		[DebuggerStepThrough]
		get
		{
			return elapsedTime;
		}
	}

	public TimeSpan TimeLeft
	{
		[DebuggerStepThrough]
		get
		{
			return timeLeft;
		}
	}

	public SimulationProgressChangedEventArgs(double progress, in GnssTime simulatedTime, in GnssTimeSpan simulatedTimeFromStart, in TimeSpan elapsedTime, in TimeSpan timeLeft)
	{
		this.progress = progress;
		this.simulatedTime = simulatedTime;
		this.simulatedTimeFromStart = simulatedTimeFromStart;
		this.elapsedTime = elapsedTime;
		this.timeLeft = timeLeft;
	}
}
