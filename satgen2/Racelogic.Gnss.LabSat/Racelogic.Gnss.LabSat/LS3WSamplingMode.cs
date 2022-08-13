using System.Diagnostics;

namespace Racelogic.Gnss.LabSat;

internal readonly struct LS3WSamplingMode
{
	private readonly int channelCount;

	private readonly int quantization;

	private readonly int samplesInWord64;

	private readonly int remainingBitsInWord64;

	internal int RemainingBitsInWord64
	{
		[DebuggerStepThrough]
		get
		{
			return remainingBitsInWord64;
		}
	}

	internal int SamplesInWord64
	{
		[DebuggerStepThrough]
		get
		{
			return samplesInWord64;
		}
	}

	internal int Quantization
	{
		[DebuggerStepThrough]
		get
		{
			return quantization;
		}
	}

	internal int ChannelCount
	{
		[DebuggerStepThrough]
		get
		{
			return channelCount;
		}
	}

	internal LS3WSamplingMode(in int channelCount, in int quantization, in int samplesInWord64, in int remainingBitsInWord64)
	{
		this.channelCount = channelCount;
		this.quantization = quantization;
		this.samplesInWord64 = samplesInWord64;
		this.remainingBitsInWord64 = remainingBitsInWord64;
	}
}
