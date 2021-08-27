using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public sealed class LabSat3Output : LabSat2Output
	{
		private readonly ChannelPlan channelPlan;

		private readonly byte[] identifier = new byte[3] { 76, 83, 51 };

		private readonly int sampleSize;

		private readonly int wordLength;

		private readonly int samplesInWord;

		private const long fileSizeLimit = 2147483648L;

		private const int sampleRate = 16368000;

		private readonly string filePathStem;

		private int fileIndex;

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

		private protected sealed override byte[] Identifier
		{
			[DebuggerStepThrough]
			get
			{
				return identifier;
			}
		}

		protected sealed override byte HeaderVersion
		{
			[DebuggerStepThrough]
			get
			{
				return 1;
			}
		}

		protected sealed override byte DefaultFrequencyBandByte
		{
			[DebuggerStepThrough]
			get
			{
				return byte.MaxValue;
			}
		}

		protected sealed override byte QuantizationCode
		{
			[DebuggerStepThrough]
			get
			{
				return 0;
			}
		}

		public LabSat3Output(string filePath, IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
			: base(Path.Combine(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)), Path.GetFileNameWithoutExtension(filePath)) + "_0000.LS3", signalTypes, desiredQuantization)
		{
			channelPlan = GetChannelPlan(signalTypes, desiredQuantization);
			int num = channelPlan.Channels.Count((Channel ch) => ch != null);
			sampleSize = num * (int)channelPlan.Quantization << 1;
			wordLength = ((num == 3) ? 32 : 16);
			samplesInWord = wordLength / sampleSize;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
			string path = Path.Combine(Path.GetDirectoryName(filePath), fileNameWithoutExtension);
			filePathStem = Path.Combine(path, fileNameWithoutExtension);
		}

		internal unsafe sealed override bool Write(SimulationSlice slice)
		{
			slice.State = SimulationSliceState.WritingStarted;
			Memory<byte> buffer = slice.GetOutputSignal();
			bool result = false;
			if (base.OutputFile == null)
			{
				CreateHeader().CopyTo(buffer);
				int index = fileIndex++;
				if (CreateFile(in index))
				{
					double seconds = slice.Interval.Width.Seconds;
					int byteCount = GetOutputByteCountForInterval(in seconds);
					result = WriteBuffer(in buffer, in byteCount);
				}
			}
			else if (base.CurrentFileLength + buffer.Length <= 2147483648u)
			{
				double seconds = slice.Interval.Width.Seconds;
				int byteCount2 = GetOutputByteCountForInterval(in seconds);
				result = WriteBuffer(in buffer, in byteCount2);
			}
			else
			{
				int byteCount3 = (int)(2147483648u - base.CurrentFileLength);
				if (byteCount3 > 0 && !WriteBuffer(in buffer, in byteCount3))
				{
					slice.State = SimulationSliceState.WritingFinished;
					return false;
				}
				if (!buffer.IsEmpty)
				{
					int index = fileIndex++;
					if (!CreateFile(in index))
					{
						slice.State = SimulationSliceState.WritingFinished;
						return false;
					}
				}
				using MemoryHandle memoryHandle = buffer.Pin();
				IntPtr bufferPointer = (IntPtr)memoryHandle.Pointer + byteCount3;
				int byteCount4 = buffer.Length - byteCount3;
				result = WriteBuffer(in bufferPointer, in byteCount4);
			}
			slice.State = SimulationSliceState.WritingFinished;
			return result;
		}

		private bool CreateFile(in int index)
		{
			string text = $"{filePathStem}_{index:D4}.LS3";
			return CreateFile(text);
		}

		internal sealed override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
		{
			return new LabSat3Quantizer(in buffer, channel, ChannelPlan, in rms);
		}

		public new static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
		{
			Signal[] signals = Signal.GetSignals(signalTypes);
			FrequencyBand[] array = signals.Select((Signal s) => s.FrequencyBand).Distinct().ToArray();
			int channelCount = array.Length;
			Quantization quantization = LabSat2Output.LimitQuantization(desiredQuantization, in channelCount);
			Channel[] array2 = ReorderChannels(array).Select(delegate(FrequencyBand? band)
			{
				if (!band.HasValue)
				{
					return null;
				}
				Signal[] signals3 = signals.Where((Signal s) => s.FrequencyBand == band.Value).ToArray();
				uint channelFrequency2 = (uint)band.Value;
				decimal actualFrequency2 = LabSat2Output.GetActualFrequency(band.Value);
				FrequencyBand[] frequencyBands2 = new FrequencyBand[1] { band.Value };
				int num = 16368000;
				int bandwidth2 = 16368000;
				return new Channel(signals3, frequencyBands2, in channelFrequency2, in actualFrequency2, in num, in bandwidth2, quantization);
			}).ToArray();
			if (!array2.Any((Channel ch) => ch != null))
			{
				FrequencyBand[] frequencyBands = new FrequencyBand[1] { FrequencyBand.GalileoE1 };
				uint channelFrequency = 1575420000u;
				decimal actualFrequency = LabSat2Output.GetActualFrequency(FrequencyBand.GalileoE1);
				Channel[] array3 = new Channel[1];
				Signal[] signals2 = Array.Empty<Signal>();
				channelCount = 16368000;
				int bandwidth = 16368000;
				array3[0] = new Channel(signals2, frequencyBands, in channelFrequency, in actualFrequency, in channelCount, in bandwidth, quantization);
				array2 = array3;
			}
			int channelCount2 = array2.Count((Channel ch) => ch != null);
			int dataRate = GetDataRate(in channelCount2, quantization);
			return new ChannelPlan(array2, in dataRate);
		}

		private static int GetDataRate(in int channelCount, Quantization quantization)
		{
			int num = channelCount * (int)quantization << 1;
			int num2 = ((channelCount == 3) ? 32 : 16);
			int num3 = num2 / num;
			return 16368000 / num3 * (num2 >> 3);
		}

		private static FrequencyBand?[] ReorderChannels(FrequencyBand[] bands)
		{
			int num = bands.Length;
			List<FrequencyBand?> list = new List<FrequencyBand?>();
			switch (num)
			{
			case 1:
				list.Add(bands.First());
				break;
			case 2:
				if (bands.Contains(FrequencyBand.GalileoE1))
				{
					list.Add(FrequencyBand.GalileoE1);
				}
				if (bands.Contains(FrequencyBand.BeiDouB1))
				{
					list.Add(FrequencyBand.BeiDouB1);
				}
				if (bands.Contains(FrequencyBand.GlonassL1))
				{
					list.Add(FrequencyBand.GlonassL1);
				}
				break;
			case 3:
				list.Add(FrequencyBand.GalileoE1);
				list.Add(FrequencyBand.GlonassL1);
				list.Add(FrequencyBand.BeiDouB1);
				break;
			default:
				RLLogger.GetLogger().LogMessage($"Unexpected number of frequency bands for LabSat3: {num}");
				break;
			}
			return list.ToArray();
		}
	}
}
