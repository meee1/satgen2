using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	public sealed class FakeLabSat2LiveOutput : LabSat2Output, ILiveOutput, INotifyPropertyChanged
	{
		private bool isReady;

		private bool isAlive;

		private readonly bool isLowLatency;

		private readonly GnssTime? trueTimeStart;

		private int sentBufferCount;

		private readonly Stopwatch playStopwatch = new Stopwatch();

		private readonly long bufferSizeMilliseconds;

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
				return 0;
			}
		}

		public event EventHandler<TimeSpan>? BufferUnderrun;

		public event EventHandler? PlaybackStarted;

		public FakeLabSat2LiveOutput(IEnumerable<SignalType> signalTypes, Quantization desiredQuantization, in bool isLowLatency, GnssTime? trueTimeStart = null)
			: base(null, signalTypes, desiredQuantization)
		{
			this.isLowLatency = isLowLatency;
			this.trueTimeStart = trueTimeStart;
			bufferSizeMilliseconds = (IsLowLatency ? 100 : 1000);
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
				Play();
				IsAlive = true;
				this.PlaybackStarted?.Invoke(this, EventArgs.Empty);
			}
			SendBuffer();
			sentBufferCount++;
			if (!IsAlive && !trueTimeStart.HasValue)
			{
				Play();
				IsAlive = true;
				this.PlaybackStarted?.Invoke(this, EventArgs.Empty);
			}
			return true;
		}

		private void Play()
		{
			playStopwatch.Restart();
		}

		private void SendBuffer()
		{
			if (playStopwatch.IsRunning)
			{
				long num = (sentBufferCount - 2) * bufferSizeMilliseconds;
				long num2 = 300L;
				if (playStopwatch.ElapsedMilliseconds > num + bufferSizeMilliseconds + num2)
				{
					this.BufferUnderrun?.Invoke(this, TimeSpan.Zero);
				}
				while (playStopwatch.ElapsedMilliseconds < num)
				{
				}
			}
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
	}
}
