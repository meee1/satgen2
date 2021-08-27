using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	public sealed class BladeRFFileOutput : StreamOutput
	{
		private readonly ChannelPlan channelPlan;

		private const int wordLength = 32;

		private const int wordLengthBytes = 4;

		private readonly int sampleSize = 32;

		private readonly int samplesInWord = 1;

		public sealed override ChannelPlan ChannelPlan
		{
			[DebuggerStepThrough]
			get
			{
				return channelPlan;
			}
		}

		internal sealed override int SampleSize
		{
			[DebuggerStepThrough]
			get
			{
				return sampleSize;
			}
		}

		internal sealed override int WordLength
		{
			[DebuggerStepThrough]
			get
			{
				return 32;
			}
		}

		internal sealed override int SamplesInWord
		{
			[DebuggerStepThrough]
			get
			{
				return samplesInWord;
			}
		}

		public BladeRFFileOutput(string filePath, IEnumerable<SignalType> signalTypes, in int sampleRate)
			: base(filePath)
		{
			channelPlan = GetChannelPlan(signalTypes, in sampleRate);
		}

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

		internal sealed override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
		{
			return new TwelveBitQuantizer(buffer, in rms);
		}

		public static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, in int sampleRate)
		{
			Signal[] signals = Signal.GetSignals(signalTypes);
			FrequencyBand[] frequencyBands = Signal.GetFrequencyBands(signals);
			uint channelFrequency = (uint)frequencyBands[0];
			decimal actualFrequency = channelFrequency;
			Channel channel = new Channel(signals, frequencyBands, in channelFrequency, in actualFrequency, in sampleRate, in sampleRate, Quantization.TwelveBit);
			int channelCount = 1;
			int dataRate = GetDataRate(in channelCount, Quantization.TwelveBit, in sampleRate);
			return new ChannelPlan(new Channel[1] { channel }, in dataRate);
		}

		private static int GetDataRate(in int channelCount, Quantization quantization, in int sampleRate)
		{
			if (channelCount == 0)
			{
				return 0;
			}
			int num = channelCount * (int)quantization << 1;
			int num2 = 32 / num;
			return sampleRate / num2 * 4;
		}
	}
}
