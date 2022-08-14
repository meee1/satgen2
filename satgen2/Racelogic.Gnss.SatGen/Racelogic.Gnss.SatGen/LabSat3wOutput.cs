using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Racelogic.DataTypes;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public sealed class LabSat3wOutput : StreamOutput
{
	private readonly ChannelPlan channelPlan;

	private const int wordLength = 64;

	private const int wordLengthBytes = 8;

	private readonly int sampleSize;

	private readonly int samplesInWord;

	private static readonly string executableFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location).FileVersion;

	private static readonly string[] channelNames = new string[3] { "A", "B", "C" };

	private static readonly Range<int> bandwidthLimits = new Range<int>(10000000, 60000000);

	private static readonly Range<int> sampleRateLimits = new Range<int>(10500000, 61440000);

	private const int samplingFrequencyStep = 1500000;

	private const int bandwidthStep = 1000000;

	private const int sampleRateThreshold = 20000000;

	private const int lowSampleRateMargin = 500000;

	private const int highSampleRateMargin = 1000000;

	private const int dataRateLimit = 100663296;

	private const double oneOverSamplingFrequencyStep = 6.6666666666666671E-07;

	private const double oneOverBandwidthStep = 1E-06;

	private static readonly int individualSignalCount = Signal.IndividualSignalTypes.Count;

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
			return 64;
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

	public LabSat3wOutput(string filePath, IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
		: base(filePath)
	{
		channelPlan = GetChannelPlan(signalTypes, desiredQuantization);
		if (channelPlan.Channels.Any())
		{
			int num = channelPlan.Channels.Count((Channel ch) => ch != null);
			sampleSize = num * (int)channelPlan.Quantization << 1;
			samplesInWord = 64 / sampleSize;
		}
		else
		{
			RLLogger.GetLogger().LogMessage("ERROR: no signals specified for LabSat3wOutput or the specified signal combination cannot be simulated together.");
		}
	}

	internal sealed override bool Write(SimulationSlice slice)
	{
		slice.State = SimulationSliceState.WritingStarted;
		if (base.OutputFile == null)
		{
			if (base.FilePath == null || !CreateFile(base.FilePath))
			{
				slice.State = SimulationSliceState.WritingFinished;
				return false;
			}
			CreateConfigurationFile(base.FilePath);
		}
		Memory<byte> buffer = slice.GetOutputSignal();
		double seconds = slice.Interval.Width.Seconds;
		int byteCount = GetOutputByteCountForInterval(in seconds);
		bool result = WriteBuffer(in buffer, in byteCount);
		slice.State = SimulationSliceState.WritingFinished;
		return result;
	}

	private void CreateConfigurationFile(string dataFilePath)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dataFilePath);
		using TextWriter textWriter = new StreamWriter(Path.Combine(Path.GetDirectoryName(dataFilePath), fileNameWithoutExtension) + ".ini");
		textWriter.WriteLine("#LABSAT3W config file");
		textWriter.WriteLine();
		textWriter.WriteLine("[config]");
		textWriter.WriteLine("OSC=OCXO");
		textWriter.WriteLine($"SMP={ChannelPlan.SampleRate}");
		textWriter.WriteLine($"QUA={(int)ChannelPlan.Quantization}");
		textWriter.WriteLine($"CHN={ChannelPlan.Channels.Count((Channel b) => b != null)}");
		textWriter.WriteLine($"SFT={SampleSize}");
		textWriter.WriteLine("custom_profile=SatGen");
		for (int i = 0; i < ChannelPlan.Channels.Count; i++)
		{
			Channel channel = ChannelPlan.Channels[i];
			if (channel != null)
			{
				string text = channelNames[i];
				textWriter.WriteLine();
				textWriter.WriteLine("[channel " + text + "]");
				textWriter.WriteLine($"CF{text}={channel.Frequency}");
				textWriter.WriteLine($"BW{text}={channel.Bandwidth}");
			}
		}
		textWriter.WriteLine();
		textWriter.WriteLine("[notes]");
		textWriter.WriteLine("SRC=SatGen " + executableFileVersion);
		textWriter.WriteLine("FPGA=28");
	}

	internal sealed override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
	{
		return new LabSat3wQuantizer(in buffer, channel, ChannelPlan, in rms);
	}

	public static int VerifySignals(in ulong signals)
	{
		if (!Enum.TryParse<SignalType>(signals.ToString(), out var result))
		{
			return 0;
		}
		return (int)GetChannelPlan(Signal.GetIndividualSignalTypes(result), Quantization.Max).Quantization;
	}

	public static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, Quantization desiredQuantization)
	{
		Signal[] signals = Signal.GetSignals(signalTypes);
		if (!signals.Any())
		{
			Signal[] signals2 = Array.Empty<Signal>();
			FrequencyBand[] frequencyBands = new FrequencyBand[1] { FrequencyBand.GalileoE1 };
			uint channelFrequency = 1575420000u;
			uint desiredFrequency = 1575420000u;
			decimal actualFrequency = GetActualFrequency(in desiredFrequency);
			int sampleRate = sampleRateLimits.Min;
			int bandwidth = bandwidthLimits.Min;
			Channel channel = new Channel(signals2, frequencyBands, in channelFrequency, in actualFrequency, in sampleRate, in bandwidth, LimitQuantization(desiredQuantization));
			Channel[] array = new Channel[1] { channel };
			sampleRate = array.Length;
			Quantization quantization = channel.Quantization;
			bandwidth = channel.SampleRate;
			int dataRate = GetDataRate(in sampleRate, quantization, in bandwidth);
			return new ChannelPlan(array, in dataRate);
		}
		FrequencyBand[] frequencyBands2 = Signal.GetFrequencyBands(signals);
		int maxNumGroups = channelNames.Length;
		ChannelPlan channelPlan = null;
		foreach (List<List<FrequencyBand>> item in Combinatorics.GetAllPartitionsVolatile(frequencyBands2, maxNumGroups))
		{
			if (!FrequenciesWithinBandwidthLimit(item))
			{
				continue;
			}
			Signal[][] signalGroups = GetSignalGroups(item, signals);
			if (GroupBandwidthWithinLimit(signalGroups))
			{
				Quantization quantization2 = channelPlan?.Quantization ?? Quantization.None;
				ChannelPlan channelPlan2 = GetChannelPlan(signalGroups, desiredQuantization);
				Quantization quantization3 = channelPlan2.Quantization;
				if (quantization3 > quantization2 || (quantization3 == quantization2 && channelPlan2.DataRate < (channelPlan?.DataRate ?? 0)))
				{
					channelPlan = channelPlan2;
				}
			}
		}
		return channelPlan ?? new ChannelPlan();
	}

	private static ChannelPlan GetChannelPlan(Signal[][] signalGroups, Quantization desiredQuantization)
	{
		List<Channel> list = new List<Channel>();
		foreach (Signal[] item in signalGroups.OrderByDescending((Signal[] sg) => sg[0].NominalFrequency))
		{
			Range<uint> minBandwidthRange = GetMinBandwidthRange(item);
			int width = (int)minBandwidthRange.Width;
			width = 1000000 * (int)((double)width * 1E-06).SafeCeiling();
			width = bandwidthLimits.Cap(width);
			int num = ((width < 20000000) ? 500000 : 1000000);
			int num2 = width + num;
			num2 = 1500000 * (int)((double)num2 * 6.6666666666666671E-07).SafeCeiling();
			num2 = sampleRateLimits.Cap(num2);
			uint desiredFrequency = minBandwidthRange.Center;
			decimal actualFrequency = GetActualFrequency(in desiredFrequency);
			FrequencyBand[] frequencyBands = Signal.GetFrequencyBands(item);
			list.Add(new Channel(item, frequencyBands, in desiredFrequency, in actualFrequency, in num2, in width, desiredQuantization));
		}
		int maxSampleRate = GetMaxSampleRate(list);
		maxSampleRate = 1500000 * (int)((double)maxSampleRate * 6.6666666666666671E-07).SafeCeiling();
		maxSampleRate = sampleRateLimits.Cap(maxSampleRate);
		int num3 = maxSampleRate - ((maxSampleRate < 20000000) ? 500000 : 1000000);
		num3 = 1000000 * (int)((double)num3 * 1E-06).SafeFloor();
		num3 = bandwidthLimits.Cap(num3);
		Quantization quantization = LimitQuantization(desiredQuantization, in maxSampleRate, list.Count);
		Channel[] array = new Channel[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			Channel channel = list[i];
			Range<uint> maxBandwidthRange = GetMaxBandwidthRange(channel.Signals);
			int num4 = (int)(Math.Max(channel.Frequency - maxBandwidthRange.Min, maxBandwidthRange.Max - channel.Frequency) << 1);
			num4 = 1000000 * (int)((double)num4 * 1E-06).SafeCeiling();
			int bandwidth = ((num4 > num3) ? num3 : num4);
			IEnumerable<Signal> signals = channel.Signals;
			IEnumerable<FrequencyBand> frequencyBands2 = channel.FrequencyBands;
			uint channelFrequency = channel.Frequency;
			decimal actualFrequency2 = channel.ActualFrequency;
			Channel channel2 = (array[i] = new Channel(signals, frequencyBands2, in channelFrequency, in actualFrequency2, in maxSampleRate, in bandwidth, quantization));
		}
		int channelCount = array.Length;
		int dataRate = GetDataRate(in channelCount, quantization, in maxSampleRate);
		return new ChannelPlan(array, in dataRate);
	}

	private static Signal[][] GetSignalGroups(List<List<FrequencyBand>> partition, Signal[] signals)
	{
		Signal[][] array = new Signal[partition.Count][];
		for (int i = 0; i < partition.Count; i++)
		{
			List<Signal> list = new List<Signal>(individualSignalCount);
			foreach (FrequencyBand item in partition[i])
			{
				foreach (Signal signal in signals)
				{
					if (signal.FrequencyBand == item)
					{
						list.Add(signal);
					}
				}
			}
			array[i] = list.ToArray();
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool FrequenciesWithinBandwidthLimit(List<List<FrequencyBand>> partition)
	{
		foreach (List<FrequencyBand> item in partition)
		{
			uint num = uint.MaxValue;
			uint num2 = 0u;
			foreach (FrequencyBand item2 in item)
			{
				if ((uint)item2 < num)
				{
					num = (uint)item2;
				}
				if ((uint)item2 > num2)
				{
					num2 = (uint)item2;
				}
			}
			if (num2 - num >= bandwidthLimits.Max)
			{
				return false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool GroupBandwidthWithinLimit(Signal[][] signalGroups)
	{
		for (int i = 0; i < signalGroups.Length; i++)
		{
			if (GetMinBandwidthRange(signalGroups[i]).Width > bandwidthLimits.Max)
			{
				return false;
			}
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Range<uint> GetMinBandwidthRange(Signal[] signals)
	{
		uint num = uint.MaxValue;
		uint num2 = 0u;
		foreach (Signal obj in signals)
		{
			uint nominalFrequency = obj.NominalFrequency;
			int num3 = obj.MinBandwidth >> 1;
			int num4 = 0;
			int num5 = 0;
			IReadOnlyList<int> slotFrequencies = obj.SlotFrequencies;
			if (slotFrequencies != null)
			{
				foreach (int item in slotFrequencies)
				{
					if (item < num4)
					{
						num4 = item;
					}
					if (item > num5)
					{
						num5 = item;
					}
				}
			}
			uint num6 = nominalFrequency.SafeAdd(num4 - num3);
			uint num7 = nominalFrequency.SafeAdd(num5 + num3);
			if (num6 < num)
			{
				num = num6;
			}
			if (num7 > num2)
			{
				num2 = num7;
			}
		}
		return new Range<uint>(num, num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Range<uint> GetMaxBandwidthRange(IEnumerable<Signal> signals)
	{
		uint num = uint.MaxValue;
		uint num2 = 0u;
		foreach (Signal signal in signals)
		{
			uint nominalFrequency = signal.NominalFrequency;
			int num3 = signal.MaxBandwidth >> 1;
			int num4 = 0;
			int num5 = 0;
			IReadOnlyList<int> slotFrequencies = signal.SlotFrequencies;
			if (slotFrequencies != null)
			{
				foreach (int item in slotFrequencies)
				{
					if (item < num4)
					{
						num4 = item;
					}
					if (item > num5)
					{
						num5 = item;
					}
				}
			}
			uint num6 = nominalFrequency.SafeAdd(num4 - num3);
			uint num7 = nominalFrequency.SafeAdd(num5 + num3);
			if (num6 < num)
			{
				num = num6;
			}
			if (num7 > num2)
			{
				num2 = num7;
			}
		}
		return new Range<uint>(num, num2);
	}

	private static int GetMaxSampleRate(List<Channel> channelList)
	{
		if (channelList.Count == 0)
		{
			return sampleRateLimits.Min;
		}
		int num = 0;
		foreach (Channel channel in channelList)
		{
			if (channel.SampleRate > num)
			{
				num = channel.SampleRate;
			}
		}
		return num;
	}

	private static Quantization LimitQuantization(Quantization desiredQuantization)
	{
		if (desiredQuantization > Quantization.ThreeBit)
		{
			return Quantization.ThreeBit;
		}
		if (desiredQuantization < Quantization.OneBit)
		{
			return Quantization.OneBit;
		}
		return desiredQuantization;
	}

	private static Quantization LimitQuantization(Quantization desiredQuantization, in int sampleRate, int channelCount)
	{
		if (channelCount < 1)
		{
			channelCount = 1;
		}
		Quantization quantization = LimitQuantization(desiredQuantization);
		while (quantization > Quantization.OneBit && GetDataRate(in channelCount, quantization, in sampleRate) > 100663296)
		{
			quantization--;
		}
		return quantization;
	}

	private static int GetDataRate(in int channelCount, Quantization quantization, in int sampleRate)
	{
		if (channelCount == 0)
		{
			return 0;
		}
		int num = channelCount * (int)quantization << 1;
		int num2 = 64 / num;
		return sampleRate / num2 * 8;
	}

	private static decimal GetActualFrequency(in uint desiredFrequency)
	{
		int num = 0;
		long num2;
		for (num2 = desiredFrequency; num2 <= 1500000000; num2 <<= 1)
		{
			num++;
		}
		long num3 = num2 / 10000000 * 10000000;
		long num4 = ((num2 - num3) * 8388593 + 5000000) / 10000000;
		decimal result = (decimal)num3 + (decimal)num4 * 1.1920950271398314353789723736m;
		for (int num5 = num; num5 > 0; num5--)
		{
			result *= 0.5m;
		}
		return result;
	}
}
