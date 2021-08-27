using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	public sealed class LimeFileOutput : StreamOutput
	{
		private readonly ChannelPlan channelPlan;

		private readonly int wordLength;

		private readonly int wordLengthBytes;

		private readonly int sampleSize;

		private readonly int samplesInWord;

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
				return wordLength;
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

		public Quantization Quantization { get; }

		public LimeFileOutput(string filePath, IEnumerable<SignalType> signalTypes, Quantization quantization, in int sampleRate)
			: base(filePath)
		{
			Quantization = quantization;
			wordLength = (int)((quantization == Quantization.TwelveBit) ? Quantization.SixteenBit : quantization) << 1;
			wordLengthBytes = wordLength >> 3;
			sampleSize = wordLength;
			samplesInWord = 1;
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
			return Quantization switch
			{
				Quantization.TwelveBit => new TwelveBitQuantizer(buffer, in rms), 
				Quantization.SixteenBit => new SixteenBitQuantizer(buffer, in rms), 
				_ => new FloatQuantizer(buffer, in rms), 
			};
		}

		public ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, in int sampleRate)
		{
			Signal[] signals = Signal.GetSignals(signalTypes);
			FrequencyBand[] frequencyBands = Signal.GetFrequencyBands(signals);
			uint channelFrequency = (uint)frequencyBands[0];
			decimal actualFrequency = channelFrequency;
			Channel channel = new Channel(signals, frequencyBands, in channelFrequency, in actualFrequency, in sampleRate, in sampleRate, Quantization);
			int channelCount = 1;
			int dataRate = GetDataRate(in channelCount, in sampleRate);
			return new ChannelPlan(new Channel[1] { channel }, in dataRate);
		}

		private int GetDataRate(in int channelCount, in int sampleRate)
		{
			if (channelCount == 0)
			{
				return 0;
			}
			return sampleRate / samplesInWord * wordLengthBytes;
		}
	}
}
