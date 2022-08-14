using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen;

[DebuggerDisplay("Freq={Frequency},  BW={Bandwidth}")]
public sealed class Channel
{
	private readonly IEnumerable<Signal> signals;

	private readonly IEnumerable<FrequencyBand> frequencyBands;

	private readonly uint frequency;

	private readonly decimal actualFrequency;

	private readonly int sampleRate;

	private readonly int bandwidth;

	private readonly Quantization quantization;

	public IEnumerable<Signal> Signals
	{
		[DebuggerStepThrough]
		get
		{
			return signals;
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

	public uint Frequency
	{
		[DebuggerStepThrough]
		get
		{
			return frequency;
		}
	}

	public decimal ActualFrequency
	{
		[DebuggerStepThrough]
		get
		{
			return actualFrequency;
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

	public int Bandwidth
	{
		[DebuggerStepThrough]
		get
		{
			return bandwidth;
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

	public Channel(IEnumerable<Signal> signals, IEnumerable<FrequencyBand> frequencyBands, in uint channelFrequency, in decimal actualFrequency, in int sampleRate, in int bandwidth, Quantization quantization)
	{
		this.signals = signals;
		this.frequencyBands = frequencyBands;
		frequency = channelFrequency;
		this.actualFrequency = actualFrequency;
		this.sampleRate = sampleRate;
		this.bandwidth = bandwidth;
		this.quantization = quantization;
	}
}
