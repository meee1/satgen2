using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

public sealed class EightBitOutput : StreamOutput
{
	private readonly ChannelPlan channelPlan;

	public sealed override ChannelPlan ChannelPlan
	{
		[DebuggerStepThrough]
		get
		{
			return channelPlan;
		}
	}

	internal override int SampleSize => 16;

	internal override int WordLength => 16;

	internal override int SamplesInWord => 1;

	public EightBitOutput(string filePath, IEnumerable<SignalType> signalTypes, in int sampleRate)
		: base(filePath)
	{
		channelPlan = GetChannelPlan(signalTypes, in sampleRate);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal sealed override bool Write(SimulationSlice slice)
	{
		slice.State = SimulationSliceState.WritingStarted;
		if (base.OutputFile == null && base.FilePath != null && !CreateFile(base.FilePath))
		{
			slice.State = SimulationSliceState.WritingFinished;
			return false;
		}
		Memory<byte> buffer = slice.GetOutputSignal();
		double seconds = slice.Interval.Width.Seconds;
		int byteCount = GetOutputByteCountForInterval(in seconds);
		bool result = WriteBuffer(in buffer, in byteCount);
		slice.State = SimulationSliceState.WritingFinished;
		return result;
	}

	internal override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
	{
		return new EightBitQuantizer(buffer, in rms);
	}

	public static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, in int sampleRate)
	{
		Signal[] signals = Signal.GetSignals(signalTypes);
		FrequencyBand[] frequencyBands = signals.Select((Signal s) => s.FrequencyBand).Distinct().ToArray();
		uint channelFrequency = (uint)signals[0].FrequencyBand;
		Channel[] array = new Channel[1];
		decimal actualFrequency = channelFrequency;
		array[0] = new Channel(signals, frequencyBands, in channelFrequency, in actualFrequency, in sampleRate, in sampleRate, Quantization.EightBit);
		int dataRate = sampleRate << 1;
		return new ChannelPlan(array, in dataRate);
	}
}
