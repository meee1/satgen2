using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Racelogic.Gnss.LabSat;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public class LabSat2Output : StreamOutput
	{
		private readonly ChannelPlan channelPlan;

		private static readonly byte[] identifier = new byte[3] { 76, 83, 50 };

		private const int wordLength = 16;

		private const int sampleRate = 16368000;

		private readonly int sampleSize;

		private readonly int samplesInWord;

		private static readonly string executableFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location).FileVersion;

		public override ChannelPlan ChannelPlan
		{
			[DebuggerStepThrough]
			get
			{
				return channelPlan;
			}
		}

		protected virtual byte QuantizationCode
		{
			[DebuggerStepThrough]
			get
			{
				return (byte)(ChannelPlan.Quantization - 1);
			}
		}

		internal override int SampleSize
		{
			[DebuggerStepThrough]
			get
			{
				return sampleSize;
			}
		}

		internal override int WordLength
		{
			[DebuggerStepThrough]
			get
			{
				return 16;
			}
		}

		internal override int SamplesInWord
		{
			[DebuggerStepThrough]
			get
			{
				return samplesInWord;
			}
		}

		private protected virtual byte[] Identifier
		{
			[DebuggerStepThrough]
			get
			{
				return identifier;
			}
		}

		protected virtual byte HeaderVersion
		{
			[DebuggerStepThrough]
			get
			{
				return 2;
			}
		}

		protected virtual byte DefaultFrequencyBandByte
		{
			[DebuggerStepThrough]
			get
			{
				return 0;
			}
		}

		public LabSat2Output(string? filePath, IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
			: base(filePath)
		{
			channelPlan = GetChannelPlan(signalTypes, desiredQuantization);
			int num = channelPlan.Channels.Count((Channel ch) => ch != null);
			sampleSize = num * (int)channelPlan.Quantization << 1;
			samplesInWord = 16 / sampleSize;
		}

		internal override bool Write(SimulationSlice slice)
		{
			slice.State = SimulationSliceState.WritingStarted;
			if (base.OutputFile == null && base.FilePath != null)
			{
				if (!CreateFile(base.FilePath))
				{
					slice.State = SimulationSliceState.WritingFinished;
					return false;
				}
				byte[] buffer = CreateHeader();
				if (!WriteBuffer(buffer))
				{
					slice.State = SimulationSliceState.WritingFinished;
					return false;
				}
			}
			Memory<byte> buffer2 = slice.GetOutputSignal();
			double seconds = slice.Interval.Width.Seconds;
			int byteCount = GetOutputByteCountForInterval(in seconds);
			bool result = WriteBuffer(in buffer2, in byteCount);
			slice.State = SimulationSliceState.WritingFinished;
			return result;
		}

		internal override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
		{
			return new LabSat2Quantizer(in buffer, channel, ChannelPlan, in rms);
		}

		public static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
		{
			Signal[] signals = Signal.GetSignals(signalTypes);
			FrequencyBand[] array = signals.Select((Signal s) => s.FrequencyBand).Distinct().ToArray();
			int channelCount = array.Length;
			Quantization quantization = LimitQuantization(desiredQuantization, in channelCount);
			Channel[] array2 = ReorderChannels(array).Select(delegate(FrequencyBand? band)
			{
				if (!band.HasValue)
				{
					return null;
				}
				Signal[] signals3 = signals.Where((Signal s) => s.FrequencyBand == band.Value).ToArray();
				uint channelFrequency2 = (uint)band.Value;
				decimal actualFrequency2 = GetActualFrequency(band.Value);
				FrequencyBand[] frequencyBands2 = new FrequencyBand[1] { band.Value };
				int num = 16368000;
				int bandwidth2 = 16368000;
				return new Channel(signals3, frequencyBands2, in channelFrequency2, in actualFrequency2, in num, in bandwidth2, quantization);
			}).ToArray();
			if (!array2.Any((Channel ch) => ch != null))
			{
				FrequencyBand[] frequencyBands = new FrequencyBand[1] { FrequencyBand.GalileoE1 };
				uint channelFrequency = 1575420000u;
				decimal actualFrequency = GetActualFrequency(FrequencyBand.GalileoE1);
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
			int num2 = 16 / num;
			return 16368000 / num2 * 2;
		}

		protected static decimal GetActualFrequency(FrequencyBand frequencyBand)
		{
			if (frequencyBand == FrequencyBand.GlonassL1)
			{
				return 1601718745.6512451171875m;
			}
			return (uint)frequencyBand;
		}

		protected static Quantization LimitQuantization(Quantization desiredQuantization, in int channelCount)
		{
			if (channelCount > 1)
			{
				return Quantization.OneBit;
			}
			if (desiredQuantization >= Quantization.TwoBit)
			{
				return Quantization.TwoBit;
			}
			return Quantization.OneBit;
		}

		private static FrequencyBand?[] ReorderChannels(FrequencyBand[] bands)
		{
			int num = bands.Length;
			List<FrequencyBand?> list = new List<FrequencyBand?>();
			switch (num)
			{
			case 1:
			{
				FrequencyBand frequencyBand = bands.First();
				if (frequencyBand == FrequencyBand.GlonassL1)
				{
					list.Add(null);
				}
				list.Add(frequencyBand);
				break;
			}
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
			default:
				RLLogger.GetLogger().LogMessage($"Unexpected number of frequency bands for LabSat2: {num}");
				break;
			}
			return list.ToArray();
		}

		private byte GetChannelMappingCode()
		{
			int count = ChannelPlan.Channels.Count;
			if (ChannelPlan.Quantization == Quantization.OneBit)
			{
				if (count > 0 && ChannelPlan.Channels[0] != null)
				{
					if (count > 1 && ChannelPlan.Channels[1] != null)
					{
						return 0;
					}
					return 1;
				}
				return 2;
			}
			if (count > 0 && ChannelPlan.Channels[0] != null)
			{
				return 3;
			}
			return 4;
		}

		protected byte[] CreateHeader()
		{
			int num = ChannelPlan.Channels.Count((Channel ch) => ch != null);
			byte b = (byte)((ChannelPlan.Quantization == Quantization.TwoBit) ? 4 : ((byte)(num << 1)));
			byte channelMappingCode = GetChannelMappingCode();
			byte b2 = ((ChannelPlan.Channels.ElementAtOrDefault(0) == null) ? DefaultFrequencyBandByte : GetFrequencyBandCode(ChannelPlan.Channels[0]!.FrequencyBands.First()));
			byte b3 = ((ChannelPlan.Channels.ElementAtOrDefault(1) == null) ? DefaultFrequencyBandByte : GetFrequencyBandCode(ChannelPlan.Channels[1]!.FrequencyBands.First()));
			byte b4 = ((ChannelPlan.Channels.ElementAtOrDefault(2) == null) ? DefaultFrequencyBandByte : GetFrequencyBandCode(ChannelPlan.Channels[2]!.FrequencyBands.First()));
			byte b5 = (byte)((num == 3) ? 1 : 0);
			byte[] obj = new byte[32]
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 5, 0, 0, 0, 0, 5, 5, 0, 0,
				0, 0
			};
			obj[1] = b;
			obj[2] = channelMappingCode;
			obj[3] = QuantizationCode;
			obj[4] = b2;
			obj[5] = b3;
			obj[19] = b5;
			obj[20] = b4;
			byte[] array = obj;
			byte[] array2 = new byte[array.Length - 4];
			Array.Copy(array, array2, array2.Length);
			Array.Copy(CRC32.ComputeBigEndian(array2), 0, array, array.Length - 4, 4);
			string text = "SatGen3 " + executableFileVersion;
			while (text.Length < 20)
			{
				text += "\0";
			}
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			short sectionId = 2;
			int length = array.Length;
			LabSat2HeaderSection labSat2HeaderSection = new LabSat2HeaderSection(in sectionId, in length, array);
			sectionId = 3;
			length = bytes.Length;
			LabSat2HeaderSection labSat2HeaderSection2 = new LabSat2HeaderSection(in sectionId, in length, bytes);
			LabSat2HeaderSection[] headerSections = new LabSat2HeaderSection[2] { labSat2HeaderSection, labSat2HeaderSection2 };
			byte[] array3 = Identifier;
			byte headerVersion = HeaderVersion;
			return new LabSat2Header(array3, in headerVersion, headerSections).ToBytes();
		}

		protected static byte GetFrequencyBandCode(FrequencyBand frequencyBand)
		{
			return (byte)(frequencyBand switch
			{
				FrequencyBand.GalileoE1 => 0u, 
				FrequencyBand.GlonassL1 => 1u, 
				FrequencyBand.BeiDouB1 => 2u, 
				_ => throw new ArgumentException($"Unsupported frequency band in LabSat2Output.GetFrequencyBandCode({frequencyBand})"), 
			});
		}
	}
}
