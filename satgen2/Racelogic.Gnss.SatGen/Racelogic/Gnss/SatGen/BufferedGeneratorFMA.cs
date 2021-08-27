using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class BufferedGeneratorFMA : Generator
	{
		private readonly struct SinCosEntry
		{
			public readonly double SinDiff;

			public readonly double Sin;

			public readonly double CosDiff;

			public readonly double Cos;

			public SinCosEntry(in double sin, in double sinDiff, in double cos, in double cosDiff)
			{
				Sin = sin;
				SinDiff = sinDiff;
				Cos = cos;
				CosDiff = cosDiff;
			}
		}

		private static readonly int cpuCount;

		private readonly Output output;

		private readonly int samplesInChunk;

		private readonly int samplesInLastChunk;

		private readonly int chunkCount;

		private readonly double trajectorySampleRate;

		private readonly int firstObservableOffset;

		private readonly ReadOnlyDictionary<ModulationType, SignalParams[]> signalParameters;

		private readonly AlignedBuffer<double> timePositions;

		private unsafe readonly double* timePositionsPointer;

		private readonly double samplingPeriod;

		private readonly AlignedBuffer<double> noiseSamples;

		private const int RmsSampleCount = 15000;

		private const int RmsDecimationRatio = 5;

		private readonly Func<double> measureRmsFunction;

		private double rms;

		private readonly Action? generateAction;

		private const int SinCosLookupCount = 64;

		private const int QuarterCycleLookupCount = 16;

		private const int HalfCycleLookupCount = 32;

		private const int ThreeQuartersCycleLookupCount = 48;

		private static readonly AlignedBuffer<SinCosEntry> sinCosLookup;

		private unsafe static readonly SinCosEntry* sinCosPointer;

		private bool isDisposed;

		unsafe static BufferedGeneratorFMA()
		{
			cpuCount = Environment.ProcessorCount;
			int size = 65;
			sinCosLookup = new AlignedBuffer<SinCosEntry>(in size);
			sinCosPointer = (SinCosEntry*)(void*)sinCosLookup.Pointer;
			double num = Math.PI / 32.0;
			double sin = 0.0;
			double cos = 1.0;
			Span<SinCosEntry> span = sinCosLookup.Span;
			for (int i = 1; i <= span.Length; i++)
			{
				double num2 = i switch
				{
					16 => 1.0, 
					32 => 0.0, 
					48 => -1.0, 
					64 => 0.0, 
					_ => Math.Sin((double)i * num), 
				};
				double num3 = i switch
				{
					16 => 0.0, 
					32 => -1.0, 
					48 => 0.0, 
					64 => 1.0, 
					_ => Math.Cos((double)i * num), 
				};
				ref SinCosEntry reference = ref span[i - 1];
				double sinDiff = num2 - sin;
				double cosDiff = num3 - cos;
				reference = new SinCosEntry(in sin, in sinDiff, in cos, in cosDiff);
				sin = num2;
				cos = num3;
			}
		}

		public unsafe BufferedGeneratorFMA(in Memory<byte> buffer, ModulationBank modulationBank, GeneratorParams parameters, SimulationParams simulationParameters) : base(in buffer, parameters)
		{
			ModulationBank modulationBank2 = modulationBank;
			GeneratorParams parameters2 = parameters;
            BufferedGeneratorFMA bufferedGeneratorFMA = this;
			double[] array = parameters2.TimePositions;
			int size = array.Length;
			timePositions = new AlignedBuffer<double>(in size);
			Span<double> span = timePositions.Span;
			for (int i = 0; i < array.Length; i++)
			{
				span[i] = array[i];
			}
			timePositionsPointer = (double*)(void*)timePositions.Pointer;
			noiseSamples = parameters2.NoiseSamples;
			firstObservableOffset = parameters2.FirstObservableIndexOffset;
			signalParameters = parameters2.SignalParameters;
			double seconds = parameters2.Interval.Width.Seconds;
			trajectorySampleRate = simulationParameters.Trajectory.SampleRate;
			output = simulationParameters.Output;
			samplingPeriod = 1.0 / (double)output.ChannelPlan.SampleRate;
			int samplesInWord = output.SamplesInWord;
			int sampleRate = output.ChannelPlan.SampleRate;
			int num = (int)Math.Round(seconds * (double)sampleRate);
			int result;
			int num2 = Math.DivRem(num, samplesInWord, out result);
			if (result > 0)
			{
				num = num2 * samplesInWord;
				seconds = (double)num / (double)sampleRate;
				parameters2.Interval = new Range<GnssTime, GnssTimeSpan>(parameters2.Interval.Start, parameters2.Interval.Start + GnssTimeSpan.FromSeconds(seconds));
			}
			samplesInChunk = num / cpuCount;
			int num3 = samplesInWord * (64 / output.WordLength);
			int num4 = 25000 / num3 * num3;
			if (samplesInChunk < num4)
			{
				samplesInChunk = num4;
			}
			else
			{
				samplesInChunk = samplesInChunk / num3 * num3;
			}
			chunkCount = num / samplesInChunk;
			samplesInLastChunk = num - chunkCount * samplesInChunk;
			if (samplesInLastChunk > 0)
			{
				chunkCount++;
			}
			GeneratorFeatures generatorFeatures = GeneratorFeatures.None;
			if (!noiseSamples.IsEmpty)
			{
				generatorFeatures |= GeneratorFeatures.Noise;
			}
			if ((from sp in signalParameters.Values.SelectMany((SignalParams[] sps) => sps)
				select sp.SignalLevel into lvl
				where lvl > 0.0
				select lvl).Distinct().Count() > 1)
			{
				generatorFeatures |= GeneratorFeatures.Levels;
			}
			IEnumerable<Signal> signals = parameters2.Channel.Signals;
			if (signals.Any())
			{
				ConstellationType[] array2 = signals.Select((Signal s) => s.ConstellationType).Distinct().ToArray();
				if (array2.Length > 1 /*&& (
                    from ct in array2
					select from s in signals
						where s.ConstellationType == ct
					select s.ModulationType into mm
					select mm.Select((ModulationType m) => bufferedGeneratorFMA.signalParameters[m].Count()).Sum()).All((int c) => c > 0)*/)
				{
					generatorFeatures |= GeneratorFeatures.MultiConstellaton;
				}
				if (signals.Select((Signal s) => s.FrequencyBand).Distinct().Count() > 1)
				{
					generatorFeatures |= GeneratorFeatures.MultiBand;
				}
				else if (signalParameters.Keys.All((ModulationType m) => m == ModulationType.InPhaseBPSK || m == ModulationType.QuadratureBPSK))
				{
					ModulationType[] source = signalParameters.Keys.Where((ModulationType m) => bufferedGeneratorFMA.signalParameters[m].Any()).ToArray();
					generatorFeatures = (source.All((ModulationType m) => m == ModulationType.InPhaseBPSK) ? (generatorFeatures | GeneratorFeatures.InPhaseBpsk) : (source.All((ModulationType m) => m == ModulationType.QuadratureBPSK) ? (generatorFeatures | GeneratorFeatures.QuadratureBpsk) : ((generatorFeatures.HasFlag(GeneratorFeatures.MultiConstellaton) || signalParameters.Values.Select(delegate(SignalParams[] v)
					{
						ModulationBank modulationBank3 = modulationBank2;
						GnssTime timeStamp = parameters2.Interval.Start;
						return modulationBank3.FindBuffer(in timeStamp, v.FirstOrDefault().ModulationPointer).Length;
					}).Distinct().Count() != 1) ? (generatorFeatures | GeneratorFeatures.DualBpsk) : (generatorFeatures | GeneratorFeatures.DualBpskSync))));
				}
				else
				{
					generatorFeatures |= GeneratorFeatures.SinBocBpsk;
				}
			}
			else
			{
				generatorFeatures = GeneratorFeatures.None;
			}
			if (generatorFeatures.HasFlag(GeneratorFeatures.MultiBand))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					generateAction = new Action(GenerateMultiBandForLevels);
				}
				else
				{
					generateAction = new Action(GenerateMultiBand);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.SinBocBpsk))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.MultiConstellaton))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
					{
						if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
						{
							generateAction = new Action(GenerateSinBocDualIndependentBpskForNoiseAndLevels);
						}
						else
						{
							generateAction = new Action(GenerateSinBocDualIndependentBpskForLevels);
						}
					}
					else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateSinBocDualIndependentBpskForNoise);
					}
					else
					{
						generateAction = new Action(GenerateSinBocDualIndependentBpsk);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateSinBocBpskForNoiseAndLevels);
					}
					else
					{
						generateAction = new Action(GenerateSinBocBpskForLevels);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
				{
					generateAction = new Action(GenerateSinBocBpskForNoise);
				}
				else
				{
					generateAction = new Action(GenerateSinBocBpsk);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.InPhaseBpsk))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateInPhaseBpskForNoiseAndLevels);
					}
					else
					{
						generateAction = new Action(GenerateInPhaseBpskForLevels);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
				{
					generateAction = new Action(GenerateInPhaseBpskForNoise);
				}
				else
				{
					generateAction = new Action(GenerateInPhaseBpsk);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.QuadratureBpsk))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateQuadratureBpskForNoiseAndLevels);
					}
					else
					{
						generateAction = new Action(GenerateQuadratureBpskForLevels);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
				{
					generateAction = new Action(GenerateQuadratureBpskForNoise);
				}
				else
				{
					generateAction = new Action(GenerateQuadratureBpsk);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.DualBpsk))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.MultiConstellaton))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
					{
						if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
						{
							generateAction = new Action(GenerateDualIndependentBpskForNoiseAndLevels);
						}
						else
						{
							generateAction = new Action(GenerateDualIndependentBpskForLevels);
						}
					}
					else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateDualIndependentBpskForNoise);
					}
					else
					{
						generateAction = new Action(GenerateDualIndependentBpsk);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateDualBpskForNoiseAndLevels);
					}
					else
					{
						generateAction = new Action(GenerateDualBpskForLevels);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
				{
					generateAction = new Action(GenerateDualBpskForNoise);
				}
				else
				{
					generateAction = new Action(GenerateDualBpsk);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.DualBpskSync))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.Levels))
				{
					if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
					{
						generateAction = new Action(GenerateDualBpskSyncForNoiseAndLevels);
					}
					else
					{
						generateAction = new Action(GenerateDualBpskSyncForLevels);
					}
				}
				else if (generatorFeatures.HasFlag(GeneratorFeatures.Noise))
				{
					generateAction = new Action(GenerateDualBpskSyncForNoise);
				}
				else
				{
					generateAction = new Action(GenerateDualBpskSync);
				}
			}
			else
			{
				RLLogger.GetLogger().LogMessage($"Null generateAction.  Flags:{generatorFeatures}");
				generateAction = new Action(GenerateMultiBandForLevels);
			}
			if (generatorFeatures.HasFlag(GeneratorFeatures.MultiBand))
			{
				measureRmsFunction = new Func<double>(GetMultiBandRMS);
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.SinBocBpsk))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.MultiConstellaton))
				{
					measureRmsFunction = new Func<double>(GetSinBocDualIndependentBpskRMS);
				}
				else
				{
					measureRmsFunction = new Func<double>(GetSinBocBpskRMS);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.DualBpsk) || generatorFeatures.HasFlag(GeneratorFeatures.DualBpskSync))
			{
				if (generatorFeatures.HasFlag(GeneratorFeatures.MultiConstellaton))
				{
					measureRmsFunction = new Func<double>(GetDualIndependentBpskRMS);
				}
				else
				{
					measureRmsFunction = new Func<double>(GetDualBpskRMS);
				}
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.InPhaseBpsk))
			{
				measureRmsFunction = new Func<double>(GetInPhaseBpskRMS);
			}
			else if (generatorFeatures.HasFlag(GeneratorFeatures.QuadratureBpsk))
			{
				measureRmsFunction = new Func<double>(GetQuadratureBpskRMS);
			}
			else
			{
				RLLogger.GetLogger().LogMessage($"Null measureRmsFunction.  Flags:{generatorFeatures}");
				measureRmsFunction = new Func<double>(GetMultiBandRMS);
			}
		}

		public sealed override Memory<byte> Generate()
		{
			generateAction?.Invoke();
			return base.Buffer;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateInPhaseBpsk()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateInPhaseBpskForNoise()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateInPhaseBpskForLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateInPhaseBpskForNoiseAndLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateQuadratureBpsk()
		{
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(item, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y, num3);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateQuadratureBpskForNoise()
		{
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num5 = Math.FusedMultiplyAdd(item, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateQuadratureBpskForLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(item, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y, num3);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateQuadratureBpskForNoiseAndLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num5 = Math.FusedMultiplyAdd(item, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpsk()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							double y2 = signalParams2.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y2, num3);
							num4 = Math.FusedMultiplyAdd(item, y2, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskForNoise()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							double y2 = signalParams2.ModulationPointer[num7];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y2, num4);
							num5 = Math.FusedMultiplyAdd(item, y2, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskForLevels()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y2, num3);
							num4 = Math.FusedMultiplyAdd(item, y2, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskForNoiseAndLevels()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y2, num4);
							num5 = Math.FusedMultiplyAdd(item, y2, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskSync()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							double y2 = signalParams2.ModulationPointer[num5];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y2, num3);
							num4 = Math.FusedMultiplyAdd(item, y2, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskSyncForNoise()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							double y2 = signalParams2.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y2, num4);
							num5 = Math.FusedMultiplyAdd(item, y2, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskSyncForLevels()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							double y2 = (double)signalParams2.ModulationPointer[num5] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num3 = Math.FusedMultiplyAdd(item2, y2, num3);
							num4 = Math.FusedMultiplyAdd(item, y2, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualBpskSyncForNoiseAndLevels()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < inPhaseParameters.Length; j++)
						{
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
							num4 = Math.FusedMultiplyAdd(item2, y2, num4);
							num5 = Math.FusedMultiplyAdd(item, y2, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualIndependentBpsk()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = signalParams2.ModulationPointer[num6];
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num4 = Math.FusedMultiplyAdd(item3, y2, num4);
							num3 = Math.FusedMultiplyAdd(item4, y2, num3);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualIndependentBpskForNoise()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = signalParams2.ModulationPointer[num7];
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num5 = Math.FusedMultiplyAdd(item3, y2, num5);
							num4 = Math.FusedMultiplyAdd(item4, y2, num4);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualIndependentBpskForLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num4 = Math.FusedMultiplyAdd(item3, y2, num4);
							num3 = Math.FusedMultiplyAdd(item4, y2, num3);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateDualIndependentBpskForNoiseAndLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num5 = Math.FusedMultiplyAdd(item3, y2, num5);
							num4 = Math.FusedMultiplyAdd(item4, y2, num4);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocBpsk()
		{
			SignalParams[] sinBocParameters = signalParameters[ModulationType.SinBOC].Where((SignalParams p) => !p.IsEmpty).ToArray();
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = new SignalParams[sinBocParameters.Length];
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = new SignalParams[sinBocParameters.Length];
			}
			AkimaCoeff[][] phaseAccumulators = sinBocParameters.Select((SignalParams sp) => sp.PhaseAccumulatorCoefficients).ToArray();
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < phaseAccumulators.Length; j++)
						{
							double cycles = Interpolate(in phaseAccumulators[j][num2], in offset);
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							SignalParams signalParams3 = sinBocParameters[j];
							double num5 = 0.0;
							double y = 0.0;
							if (!signalParams.IsEmpty)
							{
								int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
								num5 = signalParams.ModulationPointer[num6];
							}
							if (!signalParams2.IsEmpty)
							{
								int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
								y = signalParams2.ModulationPointer[num7];
							}
							int num8 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
							num5 += (double)signalParams3.ModulationPointer[num8];
							NormalizeCycles(ref cycles);
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, num5, num3);
							num4 = Math.FusedMultiplyAdd(item2, num5, num4);
							num3 = Math.FusedMultiplyAdd(item2, y, num3);
							num4 = Math.FusedMultiplyAdd(item, y, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocBpskForNoise()
		{
			SignalParams[] sinBocParameters = signalParameters[ModulationType.SinBOC].Where((SignalParams p) => !p.IsEmpty).ToArray();
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = new SignalParams[sinBocParameters.Length];
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = new SignalParams[sinBocParameters.Length];
			}
			AkimaCoeff[][] phaseAccumulators = sinBocParameters.Select((SignalParams sp) => sp.PhaseAccumulatorCoefficients).ToArray();
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < phaseAccumulators.Length; j++)
						{
							double cycles = Interpolate(in phaseAccumulators[j][num3], in offset);
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							SignalParams signalParams3 = sinBocParameters[j];
							double num6 = 0.0;
							double y = 0.0;
							if (!signalParams.IsEmpty)
							{
								int num7 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
								num6 = signalParams.ModulationPointer[num7];
							}
							if (!signalParams2.IsEmpty)
							{
								int num8 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
								y = signalParams2.ModulationPointer[num8];
							}
							int num9 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num3], in offset);
							num6 += (double)signalParams3.ModulationPointer[num9];
							NormalizeCycles(ref cycles);
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, num6, num4);
							num5 = Math.FusedMultiplyAdd(item2, num6, num5);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num5 = Math.FusedMultiplyAdd(item, y, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocBpskForLevels()
		{
			SignalParams[] sinBocParameters = signalParameters[ModulationType.SinBOC].Where((SignalParams p) => !p.IsEmpty).ToArray();
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = new SignalParams[sinBocParameters.Length];
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = new SignalParams[sinBocParameters.Length];
			}
			AkimaCoeff[][] phaseAccumulators = sinBocParameters.Select((SignalParams sp) => sp.PhaseAccumulatorCoefficients).ToArray();
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						for (int j = 0; j < phaseAccumulators.Length; j++)
						{
							double cycles = Interpolate(in phaseAccumulators[j][num2], in offset);
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							SignalParams signalParams3 = sinBocParameters[j];
							double num5 = 0.0;
							double y = 0.0;
							if (!signalParams.IsEmpty)
							{
								int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
								num5 = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							}
							if (!signalParams2.IsEmpty)
							{
								int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
								y = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
							}
							int num8 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
							num5 += (double)signalParams3.ModulationPointer[num8] * signalParams3.SignalLevel;
							NormalizeCycles(ref cycles);
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, num5, num3);
							num4 = Math.FusedMultiplyAdd(item2, num5, num4);
							num3 = Math.FusedMultiplyAdd(item2, y, num3);
							num4 = Math.FusedMultiplyAdd(item, y, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocBpskForNoiseAndLevels()
		{
			SignalParams[] sinBocParameters = signalParameters[ModulationType.SinBOC].Where((SignalParams p) => !p.IsEmpty).ToArray();
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = new SignalParams[sinBocParameters.Length];
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = new SignalParams[sinBocParameters.Length];
			}
			AkimaCoeff[][] phaseAccumulators = sinBocParameters.Select((SignalParams sp) => sp.PhaseAccumulatorCoefficients).ToArray();
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						for (int j = 0; j < phaseAccumulators.Length; j++)
						{
							double cycles = Interpolate(in phaseAccumulators[j][num3], in offset);
							SignalParams signalParams = inPhaseParameters[j];
							SignalParams signalParams2 = quadratureParameters[j];
							SignalParams signalParams3 = sinBocParameters[j];
							double num6 = 0.0;
							double y = 0.0;
							if (!signalParams.IsEmpty)
							{
								int num7 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
								num6 = (double)signalParams.ModulationPointer[num7] * signalParams.SignalLevel;
							}
							if (!signalParams2.IsEmpty)
							{
								int num8 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
								y = (double)signalParams2.ModulationPointer[num8] * signalParams2.SignalLevel;
							}
							int num9 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num3], in offset);
							num6 += (double)signalParams3.ModulationPointer[num9] * signalParams3.SignalLevel;
							NormalizeCycles(ref cycles);
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, num6, num4);
							num5 = Math.FusedMultiplyAdd(item2, num6, num5);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
							num5 = Math.FusedMultiplyAdd(item, y, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocDualIndependentBpsk()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				sinBocParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num5];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = signalParams2.ModulationPointer[num6];
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num4 = Math.FusedMultiplyAdd(item3, y2, num4);
							num3 = Math.FusedMultiplyAdd(item4, y2, num3);
						}
						array = sinBocParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams3 = array[j];
							double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num2], in offset);
							int num7 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles3);
							double y3 = signalParams3.ModulationPointer[num7];
							(double Sin, double Cos) tuple3 = SinCos(in cycles3);
							double item5 = tuple3.Sin;
							double item6 = tuple3.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item5, y3, num3);
							num4 = Math.FusedMultiplyAdd(item6, y3, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocDualIndependentBpskForNoise()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				sinBocParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = signalParams.ModulationPointer[num6];
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = signalParams2.ModulationPointer[num7];
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num5 = Math.FusedMultiplyAdd(item3, y2, num5);
							num4 = Math.FusedMultiplyAdd(item4, y2, num4);
						}
						array = sinBocParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams3 = array[j];
							double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num3], in offset);
							int num8 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles3);
							double y3 = signalParams3.ModulationPointer[num8];
							(double Sin, double Cos) tuple3 = SinCos(in cycles3);
							double item5 = tuple3.Sin;
							double item6 = tuple3.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item5, y3, num4);
							num5 = Math.FusedMultiplyAdd(item6, y3, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocDualIndependentBpskForLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				sinBocParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
							int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
							num4 = Math.FusedMultiplyAdd(item2, y, num4);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
							int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num4 = Math.FusedMultiplyAdd(item3, y2, num4);
							num3 = Math.FusedMultiplyAdd(item4, y2, num3);
						}
						array = sinBocParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams3 = array[j];
							double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num2], in offset);
							int num7 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
							NormalizeCycles(ref cycles3);
							double y3 = (double)signalParams3.ModulationPointer[num7] * signalParams3.SignalLevel;
							(double Sin, double Cos) tuple3 = SinCos(in cycles3);
							double item5 = tuple3.Sin;
							double item6 = tuple3.Cos;
							num3 = Math.FusedMultiplyAdd(0.0 - item5, y3, num3);
							num4 = Math.FusedMultiplyAdd(item6, y3, num4);
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateSinBocDualIndependentBpskForNoiseAndLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				inPhaseParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				quadratureParameters = Array.Empty<SignalParams>();
			}
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			else
			{
				sinBocParameters = Array.Empty<SignalParams>();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					int num = 862;
					Span<double> span = noiseSamples.Span;
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num2 = (double)i * samplingPeriod;
						int num3 = (int)(num2 * trajectorySampleRate) + firstObservableOffset;
						double offset = num2 - timePositionsPointer[num3];
						double num4 = span[--num];
						double num5 = span[--num];
						if (num == 0)
						{
							num = 862;
						}
						SignalParams[] array = inPhaseParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams = array[j];
							double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
							int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles);
							double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
							(double Sin, double Cos) tuple = SinCos(in cycles);
							double item = tuple.Sin;
							double item2 = tuple.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
							num5 = Math.FusedMultiplyAdd(item2, y, num5);
						}
						array = quadratureParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams2 = array[j];
							double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
							int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles2);
							double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
							(double Sin, double Cos) tuple2 = SinCos(in cycles2);
							double item3 = tuple2.Sin;
							double item4 = tuple2.Cos;
							num5 = Math.FusedMultiplyAdd(item3, y2, num5);
							num4 = Math.FusedMultiplyAdd(item4, y2, num4);
						}
						array = sinBocParameters;
						for (int j = 0; j < array.Length; j++)
						{
							SignalParams signalParams3 = array[j];
							double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num3], in offset);
							int num8 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num3], in offset);
							NormalizeCycles(ref cycles3);
							double y3 = (double)signalParams3.ModulationPointer[num8] * signalParams3.SignalLevel;
							(double Sin, double Cos) tuple3 = SinCos(in cycles3);
							double item5 = tuple3.Sin;
							double item6 = tuple3.Cos;
							num4 = Math.FusedMultiplyAdd(0.0 - item5, y3, num4);
							num5 = Math.FusedMultiplyAdd(item6, y3, num5);
						}
						add(num4, num5);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateMultiBand()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.SinBOC, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						if (inPhaseParameters != null)
						{
							SignalParams[] array = inPhaseParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams = array[j];
								double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
								int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles);
								double y = signalParams.ModulationPointer[num5];
								(double Sin, double Cos) tuple = SinCos(in cycles);
								double item = tuple.Sin;
								double item2 = tuple.Cos;
								num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
								num4 = Math.FusedMultiplyAdd(item2, y, num4);
							}
						}
						if (quadratureParameters != null)
						{
							SignalParams[] array = quadratureParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams2 = array[j];
								double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
								int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles2);
								double y2 = signalParams2.ModulationPointer[num6];
								(double Sin, double Cos) tuple2 = SinCos(in cycles2);
								double item3 = tuple2.Sin;
								double item4 = tuple2.Cos;
								num4 = Math.FusedMultiplyAdd(item3, y2, num4);
								num3 = Math.FusedMultiplyAdd(item4, y2, num3);
							}
						}
						if (sinBocParameters != null)
						{
							SignalParams[] array = sinBocParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams3 = array[j];
								double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num2], in offset);
								int num7 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles3);
								double y3 = signalParams3.ModulationPointer[num7];
								(double Sin, double Cos) tuple3 = SinCos(in cycles3);
								double item5 = tuple3.Sin;
								double item6 = tuple3.Cos;
								num3 = Math.FusedMultiplyAdd(0.0 - item5, y3, num3);
								num4 = Math.FusedMultiplyAdd(item6, y3, num4);
							}
						}
						add(num3, num4);
					}
				});
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private unsafe void GenerateMultiBandForLevels()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var inPhaseParameters))
			{
				inPhaseParameters = inPhaseParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var quadratureParameters))
			{
				quadratureParameters = quadratureParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.SinBOC, out var sinBocParameters))
			{
				sinBocParameters = sinBocParameters.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			ParallelEnumerable.Range(0, chunkCount).AsUnordered().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.ForAll(delegate(int chunkIndex)
				{
					_ = sinCosLookup.Span;
					int chunkOffsetSamples;
					int chunkLimitSamples;
					using Quantizer quantizer = GetQuantizer(chunkIndex, out chunkOffsetSamples, out chunkLimitSamples);
					Action<double, double> add = quantizer.Add;
					for (int i = chunkOffsetSamples; i < chunkLimitSamples; i++)
					{
						double num = (double)i * samplingPeriod;
						int num2 = (int)(num * trajectorySampleRate) + firstObservableOffset;
						double offset = num - timePositionsPointer[num2];
						double num3 = 0.0;
						double num4 = 0.0;
						if (inPhaseParameters != null)
						{
							SignalParams[] array = inPhaseParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams = array[j];
								double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
								int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles);
								double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
								(double Sin, double Cos) tuple = SinCos(in cycles);
								double item = tuple.Sin;
								double item2 = tuple.Cos;
								num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
								num4 = Math.FusedMultiplyAdd(item2, y, num4);
							}
						}
						if (quadratureParameters != null)
						{
							SignalParams[] array = quadratureParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams2 = array[j];
								double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
								int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles2);
								double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
								(double Sin, double Cos) tuple2 = SinCos(in cycles2);
								double item3 = tuple2.Sin;
								double item4 = tuple2.Cos;
								num4 = Math.FusedMultiplyAdd(item3, y2, num4);
								num3 = Math.FusedMultiplyAdd(item4, y2, num3);
							}
						}
						if (sinBocParameters != null)
						{
							SignalParams[] array = sinBocParameters;
							for (int j = 0; j < array.Length; j++)
							{
								SignalParams signalParams3 = array[j];
								double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num2], in offset);
								int num7 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
								NormalizeCycles(ref cycles3);
								double y3 = (double)signalParams3.ModulationPointer[num7] * signalParams3.SignalLevel;
								(double Sin, double Cos) tuple3 = SinCos(in cycles3);
								double item5 = tuple3.Sin;
								double item6 = tuple3.Cos;
								num3 = Math.FusedMultiplyAdd(0.0 - item5, y3, num3);
								num4 = Math.FusedMultiplyAdd(item6, y3, num4);
							}
						}
						add(num3, num4);
					}
				});
		}

		public override double MeasureRMS()
		{
			return measureRmsFunction();
		}

		public override void ApplyRMS(double rms)
		{
			this.rms = rms;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetInPhaseBpskRMS()
		{
			SignalParams[] value = ((!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out value)) ? Array.Empty<SignalParams>() : value.Where((SignalParams p) => !p.IsEmpty).ToArray());
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num2 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num7 = (double)i * samplingPeriod;
				int num3 = (int)(num7 * trajectorySampleRate) + firstObservableOffset;
				double offset = num7 - timePositionsPointer[num3];
				double num4;
				double num5;
				if (noiseSamples.IsEmpty)
				{
					num4 = 0.0;
					num5 = 0.0;
				}
				else
				{
					num4 = span[--num];
					num5 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				SignalParams[] array = value;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams = array[j];
					double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
					int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles);
					double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item2 = tuple.Cos;
					num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
					num5 = Math.FusedMultiplyAdd(item2, y, num5);
				}
				num2 += num4 * num4 + num5 * num5;
			}
			return Math.Sqrt(num2 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetQuadratureBpskRMS()
		{
			SignalParams[] value = ((!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out value)) ? Array.Empty<SignalParams>() : value.Where((SignalParams p) => !p.IsEmpty).ToArray());
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num2 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num7 = (double)i * samplingPeriod;
				int num3 = (int)(num7 * trajectorySampleRate) + firstObservableOffset;
				double offset = num7 - timePositionsPointer[num3];
				double num4;
				double num5;
				if (noiseSamples.IsEmpty)
				{
					num4 = 0.0;
					num5 = 0.0;
				}
				else
				{
					num4 = span[--num];
					num5 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				SignalParams[] array = value;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams = array[j];
					double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
					int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles);
					double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item2 = tuple.Cos;
					num5 = Math.FusedMultiplyAdd(item, y, num5);
					num4 = Math.FusedMultiplyAdd(item2, y, num4);
				}
				num2 += num4 * num4 + num5 * num5;
			}
			return Math.Sqrt(num2 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetDualBpskRMS()
		{
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var value))
			{
				value = Array.Empty<SignalParams>();
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var value2))
			{
				value2 = Array.Empty<SignalParams>();
			}
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num2 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num8 = (double)i * samplingPeriod;
				int num3 = (int)(num8 * trajectorySampleRate) + firstObservableOffset;
				double offset = num8 - timePositionsPointer[num3];
				double num4;
				double num5;
				if (noiseSamples.IsEmpty)
				{
					num4 = 0.0;
					num5 = 0.0;
				}
				else
				{
					num4 = span[--num];
					num5 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				for (int j = 0; j < value.Length; j++)
				{
					SignalParams signalParams = value[j];
					SignalParams signalParams2 = value2[j];
					double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
					int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
					int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles);
					double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
					double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item2 = tuple.Cos;
					num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
					num5 = Math.FusedMultiplyAdd(item2, y, num5);
					num4 = Math.FusedMultiplyAdd(item2, y2, num4);
					num5 = Math.FusedMultiplyAdd(item, y2, num5);
				}
				num2 += num4 * num4 + num5 * num5;
			}
			return Math.Sqrt(num2 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetDualIndependentBpskRMS()
		{
			SignalParams[] value = ((!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out value)) ? Array.Empty<SignalParams>() : value.Where((SignalParams p) => !p.IsEmpty).ToArray());
			SignalParams[] value2 = ((!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out value2)) ? Array.Empty<SignalParams>() : value2.Where((SignalParams p) => !p.IsEmpty).ToArray());
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num2 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num8 = (double)i * samplingPeriod;
				int num3 = (int)(num8 * trajectorySampleRate) + firstObservableOffset;
				double offset = num8 - timePositionsPointer[num3];
				double num4;
				double num5;
				if (noiseSamples.IsEmpty)
				{
					num4 = 0.0;
					num5 = 0.0;
				}
				else
				{
					num4 = span[--num];
					num5 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				SignalParams[] array = value;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams = array[j];
					double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
					int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles);
					double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item3 = tuple.Cos;
					num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
					num5 = Math.FusedMultiplyAdd(item3, y, num5);
				}
				array = value2;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams2 = array[j];
					double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
					int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles2);
					double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
					(double Sin, double Cos) tuple2 = SinCos(in cycles2);
					double item2 = tuple2.Sin;
					double item4 = tuple2.Cos;
					num5 = Math.FusedMultiplyAdd(item2, y2, num5);
					num4 = Math.FusedMultiplyAdd(item4, y2, num4);
				}
				num2 += num4 * num4 + num5 * num5;
			}
			return Math.Sqrt(num2 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetSinBocBpskRMS()
		{
			SignalParams[] array = signalParameters[ModulationType.SinBOC].Where((SignalParams p) => !p.IsEmpty).ToArray();
			if (!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var value))
			{
				value = new SignalParams[array.Length];
			}
			if (!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var value2))
			{
				value2 = new SignalParams[array.Length];
			}
			AkimaCoeff[][] array2 = array.Select((SignalParams sp) => sp.PhaseAccumulatorCoefficients).ToArray();
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num3 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num10 = (double)i * samplingPeriod;
				int num4 = (int)(num10 * trajectorySampleRate) + firstObservableOffset;
				double offset = num10 - timePositionsPointer[num4];
				double num5;
				double num6;
				if (noiseSamples.IsEmpty)
				{
					num5 = 0.0;
					num6 = 0.0;
				}
				else
				{
					num5 = span[--num];
					num6 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				for (int j = 0; j < array2.Length; j++)
				{
					double cycles = Interpolate(in array2[j][num4], in offset);
					SignalParams signalParams = value[j];
					SignalParams signalParams2 = value2[j];
					SignalParams signalParams3 = array[j];
					double num7 = 0.0;
					double y = 0.0;
					if (!signalParams.IsEmpty)
					{
						int num8 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num4], in offset);
						num7 = (double)signalParams.ModulationPointer[num8] * signalParams.SignalLevel;
					}
					if (!signalParams2.IsEmpty)
					{
						int num9 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num4], in offset);
						y = (double)signalParams2.ModulationPointer[num9] * signalParams2.SignalLevel;
					}
					int num2 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num4], in offset);
					num7 += (double)signalParams3.ModulationPointer[num2] * signalParams3.SignalLevel;
					NormalizeCycles(ref cycles);
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item2 = tuple.Cos;
					num5 = Math.FusedMultiplyAdd(0.0 - item, num7, num5);
					num6 = Math.FusedMultiplyAdd(item2, num7, num6);
					num5 = Math.FusedMultiplyAdd(item2, y, num5);
					num6 = Math.FusedMultiplyAdd(item, y, num6);
				}
				num3 += num5 * num5 + num6 * num6;
			}
			return Math.Sqrt(num3 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetSinBocDualIndependentBpskRMS()
		{
			SignalParams[] value = ((!signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out value)) ? Array.Empty<SignalParams>() : value.Where((SignalParams p) => !p.IsEmpty).ToArray());
			SignalParams[] value2 = ((!signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out value2)) ? Array.Empty<SignalParams>() : value2.Where((SignalParams p) => !p.IsEmpty).ToArray());
			SignalParams[] value3 = ((!signalParameters.TryGetValue(ModulationType.SinBOC, out value3)) ? Array.Empty<SignalParams>() : value3.Where((SignalParams p) => !p.IsEmpty).ToArray());
			int num = 862;
			Span<double> span = noiseSamples.Span;
			_ = sinCosLookup.Span;
			double num2 = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num9 = (double)i * samplingPeriod;
				int num3 = (int)(num9 * trajectorySampleRate) + firstObservableOffset;
				double offset = num9 - timePositionsPointer[num3];
				double num4;
				double num5;
				if (noiseSamples.IsEmpty)
				{
					num4 = 0.0;
					num5 = 0.0;
				}
				else
				{
					num4 = span[--num];
					num5 = span[--num];
					if (num == 0)
					{
						num = 862;
					}
				}
				SignalParams[] array = value;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams = array[j];
					double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num3], in offset);
					int num6 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles);
					double y = (double)signalParams.ModulationPointer[num6] * signalParams.SignalLevel;
					(double Sin, double Cos) tuple = SinCos(in cycles);
					double item = tuple.Sin;
					double item4 = tuple.Cos;
					num4 = Math.FusedMultiplyAdd(0.0 - item, y, num4);
					num5 = Math.FusedMultiplyAdd(item4, y, num5);
				}
				array = value2;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams2 = array[j];
					double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num3], in offset);
					int num7 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles2);
					double y2 = (double)signalParams2.ModulationPointer[num7] * signalParams2.SignalLevel;
					(double Sin, double Cos) tuple2 = SinCos(in cycles2);
					double item2 = tuple2.Sin;
					double item5 = tuple2.Cos;
					num5 = Math.FusedMultiplyAdd(item2, y2, num5);
					num4 = Math.FusedMultiplyAdd(item5, y2, num4);
				}
				array = value3;
				for (int j = 0; j < array.Length; j++)
				{
					SignalParams signalParams3 = array[j];
					double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num3], in offset);
					int num8 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num3], in offset);
					NormalizeCycles(ref cycles3);
					double y3 = (double)signalParams3.ModulationPointer[num8] * signalParams3.SignalLevel;
					(double Sin, double Cos) tuple3 = SinCos(in cycles3);
					double item3 = tuple3.Sin;
					double item6 = tuple3.Cos;
					num4 = Math.FusedMultiplyAdd(0.0 - item3, y3, num4);
					num5 = Math.FusedMultiplyAdd(item6, y3, num5);
				}
				num2 += num4 * num4 + num5 * num5;
			}
			return Math.Sqrt(num2 / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public unsafe double GetMultiBandRMS()
		{
			if (signalParameters.TryGetValue(ModulationType.InPhaseBPSK, out var value))
			{
				value = value.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.QuadratureBPSK, out var value2))
			{
				value2 = value2.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			if (signalParameters.TryGetValue(ModulationType.SinBOC, out var value3))
			{
				value3 = value3.Where((SignalParams p) => !p.IsEmpty).ToArray();
			}
			_ = sinCosLookup.Span;
			double num = 0.0;
			for (int i = 0; i < 15000; i += 5)
			{
				double num8 = (double)i * samplingPeriod;
				int num2 = (int)(num8 * trajectorySampleRate) + firstObservableOffset;
				double offset = num8 - timePositionsPointer[num2];
				double num3 = 0.0;
				double num4 = 0.0;
				if (value != null)
				{
					SignalParams[] array = value;
					for (int j = 0; j < array.Length; j++)
					{
						SignalParams signalParams = array[j];
						double cycles = Interpolate(in signalParams.PhaseAccumulatorCoefficients[num2], in offset);
						int num5 = (int)Interpolate(in signalParams.ChipIndexInterpolatorCoefficients[num2], in offset);
						NormalizeCycles(ref cycles);
						double y = (double)signalParams.ModulationPointer[num5] * signalParams.SignalLevel;
						(double Sin, double Cos) tuple = SinCos(in cycles);
						double item = tuple.Sin;
						double item4 = tuple.Cos;
						num3 = Math.FusedMultiplyAdd(0.0 - item, y, num3);
						num4 = Math.FusedMultiplyAdd(item4, y, num4);
					}
				}
				if (value2 != null)
				{
					SignalParams[] array = value2;
					for (int j = 0; j < array.Length; j++)
					{
						SignalParams signalParams2 = array[j];
						double cycles2 = Interpolate(in signalParams2.PhaseAccumulatorCoefficients[num2], in offset);
						int num6 = (int)Interpolate(in signalParams2.ChipIndexInterpolatorCoefficients[num2], in offset);
						NormalizeCycles(ref cycles2);
						double y2 = (double)signalParams2.ModulationPointer[num6] * signalParams2.SignalLevel;
						(double Sin, double Cos) tuple2 = SinCos(in cycles2);
						double item2 = tuple2.Sin;
						double item5 = tuple2.Cos;
						num4 = Math.FusedMultiplyAdd(item2, y2, num4);
						num3 = Math.FusedMultiplyAdd(item5, y2, num3);
					}
				}
				if (value3 != null)
				{
					SignalParams[] array = value3;
					for (int j = 0; j < array.Length; j++)
					{
						SignalParams signalParams3 = array[j];
						double cycles3 = Interpolate(in signalParams3.PhaseAccumulatorCoefficients[num2], in offset);
						int num7 = (int)Interpolate(in signalParams3.ChipIndexInterpolatorCoefficients[num2], in offset);
						NormalizeCycles(ref cycles3);
						double y3 = (double)signalParams3.ModulationPointer[num7] * signalParams3.SignalLevel;
						(double Sin, double Cos) tuple3 = SinCos(in cycles3);
						double item3 = tuple3.Sin;
						double item6 = tuple3.Cos;
						num3 = Math.FusedMultiplyAdd(0.0 - item3, y3, num3);
						num4 = Math.FusedMultiplyAdd(item6, y3, num4);
					}
				}
				num += num3 * num3 + num4 * num4;
			}
			return Math.Sqrt(num / 6000.0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private Quantizer GetQuantizer(int chunkIndex, out int chunkOffsetSamples, out int chunkLimitSamples)
		{
			chunkOffsetSamples = chunkIndex * samplesInChunk;
			int num = ((chunkIndex == chunkCount - 1 && samplesInLastChunk > 0) ? samplesInLastChunk : samplesInChunk);
			chunkLimitSamples = chunkOffsetSamples + num;
			int num2 = output.SamplesInWord * (64 / output.WordLength);
			int start = chunkOffsetSamples / num2 << 3;
			int length = num / num2 << 3;
			Memory<byte> memory = base.Buffer.Slice(start, length);
			return output.GetQuantizer(in memory, base.Parameters.Channel, in rms);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private unsafe static (double Sin, double Cos) SinCos(in double cycles)
		{
			double num3 = 64.0 * cycles;
			int num2 = (int)num3;
			double x = num3 - (double)num2;
			SinCosEntry sinCosEntry = sinCosPointer[num2];
			double item = Math.FusedMultiplyAdd(x, sinCosEntry.SinDiff, sinCosEntry.Sin);
			double item2 = Math.FusedMultiplyAdd(x, sinCosEntry.CosDiff, sinCosEntry.Cos);
			return (item, item2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static void NormalizeCycles(ref double cycles)
		{
			long num = (long)cycles;
			if (cycles < 0.0)
			{
				num--;
			}
			cycles -= num;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static double Interpolate(in AkimaCoeff coefficient, in double offset)
		{
			double y = Math.FusedMultiplyAdd(offset, coefficient.C0, coefficient.C1);
			double y2 = Math.FusedMultiplyAdd(offset, y, coefficient.C2);
			return Math.FusedMultiplyAdd(offset, y2, coefficient.C3);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing)
				{
					sinCosLookup.Dispose();
					timePositions.Dispose();
				}
			}
		}
	}
}
