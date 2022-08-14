using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Racelogic.Geodetics;
//using Racelogic.Gnss.BladeRF;

namespace Racelogic.Gnss.SatGen;
/*
public sealed class BladeRFOutput : StreamOutput, ILiveOutput, INotifyPropertyChanged
{
	private readonly ChannelPlan channelPlan;

	private readonly BladeRFReplayer replayer;

	private readonly GnssTime? trueTimeStart;

	private bool isReady;

	private bool isAlive;

	private int sentBufferCount;

	private int bufferUnderrunCount;

	private readonly int sampleSize = 32;

	private readonly int wordLength = 32;

	private bool isDisposed;

	public sealed override ChannelPlan ChannelPlan
	{
		[DebuggerStepThrough]
		get
		{
			return channelPlan;
		}
	}

	public BladeRFReplayer Replayer
	{
		[DebuggerStepThrough]
		get
		{
			return replayer;
		}
	}

	internal override int SampleSize => sampleSize;

	internal override int WordLength => wordLength;

	internal override int SamplesInWord => 1;

	public bool IsReady
	{
		[DebuggerStepThrough]
		get
		{
			return isReady;
		}
		[DebuggerStepThrough]
		private set
		{
			isReady = value;
			OnPropertyChanged("IsReady");
		}
	}

	public bool IsAlive
	{
		[DebuggerStepThrough]
		get
		{
			return isAlive;
		}
		[DebuggerStepThrough]
		private set
		{
			isAlive = value;
			OnPropertyChanged("IsAlive");
		}
	}

	public int BufferUnderrunCount
	{
		[DebuggerStepThrough]
		get
		{
			return bufferUnderrunCount;
		}
		[DebuggerStepThrough]
		private set
		{
			bufferUnderrunCount = value;
			OnPropertyChanged("BufferUnderrunCount");
		}
	}

	public GnssTime? TrueTimeStart => trueTimeStart;

	public double DataTransferLatency => 0.02;

	public int BufferCount => 2;

	public bool IsLowLatency => true;

	public event EventHandler? PlaybackStarted;

	public event EventHandler<TimeSpan>? BufferUnderrun;

	public BladeRFOutput(IEnumerable<SignalType> signalTypes, in int sampleRate, GnssTime? trueTimeStart = null)
		: base(null)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		channelPlan = GetChannelPlan(signalTypes, in sampleRate);
		Channel channel = ChannelPlan.Channels[0] ?? throw new NotSupportedException("ChannelPlan.Channels cannot be null in the provided Channel in BladeRFOutput ");
		this.trueTimeStart = trueTimeStart;
		replayer = new BladeRFReplayer(channel.Frequency, sampleRate, channel.Bandwidth);
		if (!replayer.get_IsConnected())
		{
			throw new BladeRFException("No BladeRF detected! \nConnect BladeRF and try again.");
		}
		replayer.add_BufferUnderrun((EventHandler<TimeSpan>)OnBufferUnderrun);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal sealed override bool Write(SimulationSlice slice)
	{
		slice.State = SimulationSliceState.WritingStarted;
		Memory<byte> buffer = slice.GetOutputSignal();
		bool result = WriteBuffer(in buffer);
		slice.State = SimulationSliceState.WritingFinished;
		return result;
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	protected override bool WriteBuffer(in Memory<byte> buffer)
	{
		if (!IsAlive && trueTimeStart.HasValue && sentBufferCount >= 2)
		{
			DateTime triggerTime = trueTimeStart.Value.UtcTime - TimeSpan.FromSeconds(DataTransferLatency);
			IsReady = true;
			WaitForTime(in triggerTime);
			IsReady = false;
			IsAlive = true;
			this.PlaybackStarted?.Invoke(this, EventArgs.Empty);
			Replayer.Play();
		}
		Replayer.AddBuffer(ref buffer);
		sentBufferCount++;
		if (!IsAlive && !trueTimeStart.HasValue)
		{
			IsAlive = true;
			this.PlaybackStarted?.Invoke(this, EventArgs.Empty);
			Replayer.Play();
		}
		return true;
	}

	private static void WaitForTime(in DateTime triggerTime)
	{
		Stopwatch stopwatch = new Stopwatch();
		DateTime utcNow = DateTime.UtcNow;
		DateTime utcNow2 = DateTime.UtcNow;
		while (utcNow2 == utcNow)
		{
			utcNow2 = DateTime.UtcNow;
		}
		stopwatch.Start();
		while (utcNow2 + stopwatch.Elapsed < triggerTime)
		{
		}
	}

	private void OnBufferUnderrun(object? sender, TimeSpan delay)
	{
		BufferUnderrunCount++;
		this.BufferUnderrun?.Invoke(this, delay);
	}

	internal override Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms)
	{
		return new TwelveBitQuantizer(buffer, in rms);
	}

	public static ChannelPlan GetChannelPlan(IEnumerable<SignalType> signalTypes, in int sampleRate)
	{
		Signal[] signals = Signal.GetSignals(signalTypes);
		FrequencyBand[] frequencyBands = signals.Select((Signal s) => s.FrequencyBand).Distinct().ToArray();
		int desiredFrequency = (int)signals[0].FrequencyBand;
		GetActualFrequency(in desiredFrequency);
		_ = sampleRate / 1000000;
		Channel[] array = new Channel[1];
		uint channelFrequency = (uint)desiredFrequency;
		decimal actualFrequency = desiredFrequency;
		array[0] = new Channel(signals, frequencyBands, in channelFrequency, in actualFrequency, in sampleRate, in sampleRate, Quantization.TwelveBit);
		int dataRate = 16 * sampleRate >> 2;
		return new ChannelPlan(array, in dataRate);
	}

	private static decimal GetActualFrequency(in int desiredFrequency)
	{
		int num = 0;
		long num2;
		for (num2 = desiredFrequency; num2 <= 4961000000L; num2 <<= 1)
		{
			num++;
		}
		if (num2 > 5500000000L)
		{
			num2 >>= 1;
			num--;
		}
		long num3 = num2 / 40000000 * 40000000;
		long num4 = ((num2 - num3) * 1048576 + 20000000) / 40000000;
		decimal result = (decimal)num3 + (decimal)(num4 * 40000000) * 0.00000095367431640625m;
		for (int num5 = num; num5 > 0; num5--)
		{
			result *= 0.5m;
		}
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing && replayer != null)
			{
				replayer.remove_BufferUnderrun((EventHandler<TimeSpan>)OnBufferUnderrun);
				replayer.Dispose();
			}
		}
	}
}
*/