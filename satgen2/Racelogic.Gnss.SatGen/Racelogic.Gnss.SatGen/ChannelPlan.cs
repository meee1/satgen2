using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen;

[DebuggerDisplay("{Channels.Count} channels,  SMP={SampleRate},  {Quantization}")]
public sealed class ChannelPlan
{
	private readonly IReadOnlyList<Channel?> channels;

	private readonly int sampleRate;

	private readonly Quantization quantization;

	private readonly int dataRate;

	public IReadOnlyList<Channel?> Channels
	{
		[DebuggerStepThrough]
		get
		{
			return channels;
		}
	}

	public int SampleRate
	{
		[DebuggerStepThrough]
		get
		{
			return sampleRate;
		}
	}

	public Quantization Quantization
	{
		[DebuggerStepThrough]
		get
		{
			return quantization;
		}
	}

	public int DataRate
	{
		[DebuggerStepThrough]
		get
		{
			return dataRate;
		}
	}

	internal ChannelPlan(): this(Array.Empty<Channel>(), 0)
	{
		Channel[] array = Array.Empty<Channel>();
		int num = 0;
		//this._002Ector(array, in num);
	}

	public ChannelPlan(IReadOnlyList<Channel?> channels, in int dataRate)
	{
		this.channels = channels;
		foreach (Channel channel in this.channels)
		{
			if (channel != null)
			{
				if (channel.SampleRate > sampleRate)
				{
					sampleRate = channel.SampleRate;
				}
				if (channel.Quantization > quantization)
				{
					quantization = channel.Quantization;
				}
			}
		}
		this.dataRate = dataRate;
	}

	internal decimal GetActualFrequency(Signal signal)
	{
		foreach (Channel channel in channels)
		{
			if (channel == null)
			{
				continue;
			}
			foreach (Signal signal2 in channel.Signals)
			{
				if (signal2 == signal)
				{
					return channel.ActualFrequency;
				}
			}
		}
		return signal.CenterFrequency;
	}
}
