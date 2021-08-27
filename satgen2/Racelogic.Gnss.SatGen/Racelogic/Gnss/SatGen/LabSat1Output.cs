using System;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	public sealed class LabSat1Output : StreamOutput
	{
		private readonly ChannelPlan channelPlan;

		private const int sampleRate = 16368000;

		private const int intermediateFrequency = 4092000;

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
				return 1;
			}
		}

		internal sealed override int WordLength
		{
			[DebuggerStepThrough]
			get
			{
				return 8;
			}
		}

		internal sealed override int SamplesInWord
		{
			[DebuggerStepThrough]
			get
			{
				return 8;
			}
		}

		public LabSat1Output(string fileName)
			: base(fileName)
		{
			channelPlan = GetChannelPlan();
		}

		internal override bool Write(SimulationSlice slice)
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
			return new LabSat1Quantizer(in buffer);
		}

		public static ChannelPlan GetChannelPlan()
		{
			Signal[] signals = Signal.GetSignals(SignalType.GpsL1CA);
			Signal signal = signals[0];
			FrequencyBand[] frequencyBands = new FrequencyBand[1] { signal.FrequencyBand };
			uint num = signal.CenterFrequency - 4092000;
			uint channelFrequency = signal.CenterFrequency;
			decimal actualFrequency = num;
			int num2 = 16368000;
			int bandwidth = 4092000;
			Channel channel = new Channel(signals, frequencyBands, in channelFrequency, in actualFrequency, in num2, in bandwidth, Quantization.OneBit);
			int dataRate = 2046000;
			return new ChannelPlan(new Channel[1] { channel }, in dataRate);
		}
	}
}
