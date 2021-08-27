using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class SimulationSlice : IDisposable
	{
		private readonly Range<GnssTime, GnssTimeSpan> interval;

		private readonly ModulationBank modulationBank;

		private readonly Dictionary<Channel, Generator> generators = new Dictionary<Channel, Generator>();

		private volatile SimulationSliceState state = SimulationSliceState.Ready;

		private bool isDisposed;

		public Range<GnssTime, GnssTimeSpan> Interval
		{
			[DebuggerStepThrough]
			get
			{
				return interval;
			}
		}

		public SimulationSliceState State
		{
			[DebuggerStepThrough]
			get
			{
				return state;
			}
			[DebuggerStepThrough]
			set
			{
				state = value;
			}
		}

		public SimulationSlice(ModulationBank modulationBank, in Range<GnssTime, GnssTimeSpan> interval)
		{
			this.interval = interval;
			this.modulationBank = modulationBank;
		}

		public void CreateGenerator(ModulationBank modulationBank, Channel channel, in Memory<byte> buffer, GeneratorParams parameters, SimulationParams simulationParameters)
		{
			if (Simulation.IsFmaSuppported)
			{
				generators[channel] = new BufferedGeneratorFMA(in buffer, modulationBank, parameters, simulationParameters);
			}
			else
			{
				generators[channel] = new BufferedGenerator(in buffer, modulationBank, parameters, simulationParameters);
			}
		}

		public double MeasureRMS(Channel channel)
		{
			if (!generators.TryGetValue(channel, out var value))
			{
				throw new ArgumentException($"No Generator found for channel {channel.Frequency}Hz", "channel");
			}
			return value.MeasureRMS();
		}

		public void ApplyRMS(Channel channel, double rms)
		{
			if (!generators.TryGetValue(channel, out var value))
			{
				throw new ArgumentException($"No Generator found for channel {channel.Frequency}Hz", "channel");
			}
			value.ApplyRMS(rms);
		}

		public Memory<byte> GetBuffer(Channel channel)
		{
			if (!generators.TryGetValue(channel, out var value))
			{
				throw new ArgumentException($"No Generator found for channel {channel.Frequency}Hz", "channel");
			}
			return value.Buffer;
		}

		public void Generate(in bool generate)
		{
			State = SimulationSliceState.ProcessingStarted;
			if (generate)
			{
				foreach (Generator item in generators.Values.AsParallel().WithDegreeOfParallelism(generators.Count).WithExecutionMode(ParallelExecutionMode.ForceParallelism))
				{
					item.Generate();
				}
			}
			ModulationBank obj = modulationBank;
			GnssTime timeStamp = interval.Start;
			obj.Recycle(in timeStamp);
			State = SimulationSliceState.ProcessingFinished;
		}

		public Memory<byte> GetOutputSignal()
		{
			if (generators.Count == 0 || generators.Count > 3)
			{
				throw new NotSupportedException(string.Format("Unsupported number of channels in {0}(): {1}", "GetOutputSignal", generators.Count));
			}
			Memory<byte>[] array = generators.Values.Select((Generator g) => g.Buffer).ToArray();
			int num = array.Length;
			if (num > 1)
			{
				Memory<byte> firstBuffer = array[0];
				ReadOnlyMemory<byte> secondBuffer = array[1];
				switch (num)
				{
				case 2:
					MixTwoSignals(in firstBuffer, in secondBuffer);
					break;
				case 3:
				{
					ReadOnlyMemory<byte> thirdBuffer = array[2];
					MixThreeSignals(in firstBuffer, in secondBuffer, in thirdBuffer);
					break;
				}
				}
			}
			return array[0];
		}

		private static void MixTwoSignals(in Memory<byte> firstBuffer, in ReadOnlyMemory<byte> secondBuffer)
		{
			Span<byte> span = firstBuffer.Span;
			ReadOnlySpan<byte> span2 = secondBuffer.Span;
			Span<Vector<ulong>> span3 = MemoryMarshal.Cast<byte, Vector<ulong>>(span);
			ReadOnlySpan<Vector<ulong>> readOnlySpan = MemoryMarshal.Cast<byte, Vector<ulong>>(span2);
			for (int num = span3.Length - 1; num >= 0; num--)
			{
				span3[num] |= readOnlySpan[num];
			}
			int num2 = span3.Length * Vector<ulong>.Count;
			Span<ulong> span4 = MemoryMarshal.Cast<byte, ulong>(span);
			if (num2 < span4.Length)
			{
				ReadOnlySpan<ulong> readOnlySpan2 = MemoryMarshal.Cast<byte, ulong>(span2);
				for (int i = num2; i < span4.Length; i++)
				{
					span4[i] |= readOnlySpan2[i];
				}
			}
		}

		private static void MixThreeSignals(in Memory<byte> firstBuffer, in ReadOnlyMemory<byte> secondBuffer, in ReadOnlyMemory<byte> thirdBuffer)
		{
			Span<byte> span = firstBuffer.Span;
			ReadOnlySpan<byte> span2 = secondBuffer.Span;
			ReadOnlySpan<byte> span3 = thirdBuffer.Span;
			Span<Vector<ulong>> span4 = MemoryMarshal.Cast<byte, Vector<ulong>>(span);
			ReadOnlySpan<Vector<ulong>> readOnlySpan = MemoryMarshal.Cast<byte, Vector<ulong>>(span2);
			ReadOnlySpan<Vector<ulong>> readOnlySpan2 = MemoryMarshal.Cast<byte, Vector<ulong>>(span3);
			for (int num = span4.Length - 1; num >= 0; num--)
			{
				span4[num] |= readOnlySpan[num] | readOnlySpan2[num];
			}
			int num2 = span4.Length * Vector<ulong>.Count;
			Span<ulong> span5 = MemoryMarshal.Cast<byte, ulong>(span);
			if (num2 < span5.Length)
			{
				ReadOnlySpan<ulong> readOnlySpan3 = MemoryMarshal.Cast<byte, ulong>(span2);
				ReadOnlySpan<ulong> readOnlySpan4 = MemoryMarshal.Cast<byte, ulong>(span3);
				for (int i = num2; i < span5.Length; i++)
				{
					span5[i] |= readOnlySpan3[i] | readOnlySpan4[i];
				}
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (isDisposed)
			{
				return;
			}
			isDisposed = true;
			if (!disposing)
			{
				return;
			}
			foreach (Generator value in generators.Values)
			{
				value?.Dispose();
			}
			generators.Clear();
		}
	}
}
