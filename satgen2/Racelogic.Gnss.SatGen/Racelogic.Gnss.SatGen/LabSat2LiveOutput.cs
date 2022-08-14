using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Racelogic.Geodetics;
using Racelogic.Gnss.LabSat;

namespace Racelogic.Gnss.SatGen;

public sealed class LabSat2LiveOutput : LabSat2Output, ILiveOutput, INotifyPropertyChanged
{
	private readonly LabSat2Replayer replayer;

	private bool isReady;

	private bool isAlive;

	private readonly bool isLowLatency;

	private readonly GnssTime? trueTimeStart;

	private int sentBufferCount;

	private int bufferUnderrunCount;

	private bool isDisposed;

	public LabSat2Replayer Replayer
	{
		[DebuggerStepThrough]
		get
		{
			return replayer;
		}
	}

	public bool IsLowLatency
	{
		[DebuggerStepThrough]
		get
		{
			return isLowLatency;
		}
	}

	public GnssTime? TrueTimeStart
	{
		[DebuggerStepThrough]
		get
		{
			return trueTimeStart;
		}
	}

	public double DataTransferLatency
	{
		[DebuggerStepThrough]
		get
		{
			return 0.02;
		}
	}

	public int BufferCount
	{
		[DebuggerStepThrough]
		get
		{
			return 2;
		}
	}

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

	public event EventHandler<TimeSpan>? BufferUnderrun;

	public event EventHandler? PlaybackStarted;

	public LabSat2LiveOutput(IEnumerable<SignalType> signalTypes, Quantization desiredQuantization, in bool isLowLatency, GnssTime? trueTimeStart = null, in bool handleBufferUnderrun = true)
		: base(null, signalTypes, desiredQuantization)
	{
		this.isLowLatency = isLowLatency;
		this.trueTimeStart = trueTimeStart;
		ChannelBand[] frequencyBands = ChannelPlan.Channels.Select((Channel ch) => (ChannelBand)((ch == null) ? byte.MaxValue : LabSat2Output.GetFrequencyBandCode(ch.FrequencyBands.First()))).ToArray();
		int quantization = (int)ChannelPlan.Quantization;
		replayer = new LabSat2Replayer(frequencyBands, in quantization, in isLowLatency, in handleBufferUnderrun);
		if (!replayer.IsConnected)
		{
			throw new LabSatException("No LabSat2 detected!\nConnect LabSat2 and try again.");
		}
		replayer.BufferUnderrun += new EventHandler<TimeSpan>(OnBufferUnderrun);
	}

	internal override bool Write(SimulationSlice slice)
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
		Replayer.AddBuffer(in buffer);
		sentBufferCount++;
		if (!IsAlive && !trueTimeStart.HasValue && sentBufferCount >= 2)
		{
			IsAlive = true;
			Replayer.Play();
			this.PlaybackStarted?.Invoke(this, EventArgs.Empty);
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

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				replayer.BufferUnderrun -= new EventHandler<TimeSpan>(OnBufferUnderrun);
				replayer.Dispose();
			}
		}
	}
}
