using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

internal sealed class GeneratorParams
{
	private Range<GnssTime, GnssTimeSpan> interval;

	private readonly Channel channel;

	private readonly double[] timePositions;

	private readonly ReadOnlyDictionary<ModulationType, SignalParams[]> signalParameters;

	private readonly int firstObservableIndexOffset;

	private readonly AlignedBuffer<double> noiseSamples;

	public Channel Channel
	{
		[DebuggerStepThrough]
		get
		{
			return channel;
		}
	}

	public double[] TimePositions
	{
		[DebuggerStepThrough]
		get
		{
			return timePositions;
		}
	}

	public ReadOnlyDictionary<ModulationType, SignalParams[]> SignalParameters
	{
		[DebuggerStepThrough]
		get
		{
			return signalParameters;
		}
	}

	public Range<GnssTime, GnssTimeSpan> Interval
	{
		[DebuggerStepThrough]
		get
		{
			return interval;
		}
		[DebuggerStepThrough]
		internal set
		{
			interval = value;
		}
	}

	public int FirstObservableIndexOffset
	{
		[DebuggerStepThrough]
		get
		{
			return firstObservableIndexOffset;
		}
	}

	public AlignedBuffer<double> NoiseSamples
	{
		[DebuggerStepThrough]
		get
		{
			return noiseSamples;
		}
	}

	public GeneratorParams(Channel outputChannel, double[] timePositions, IDictionary<ModulationType, SignalParams[]> signalParameters, in Range<GnssTime, GnssTimeSpan> interval, in int firstObservableIndexOffset, in AlignedBuffer<double> noiseSamples)
	{
		channel = outputChannel;
		this.timePositions = timePositions;
		this.signalParameters = new ReadOnlyDictionary<ModulationType, SignalParams[]>(signalParameters);
		this.interval = interval;
		this.firstObservableIndexOffset = firstObservableIndexOffset;
		this.noiseSamples = noiseSamples;
	}
}
