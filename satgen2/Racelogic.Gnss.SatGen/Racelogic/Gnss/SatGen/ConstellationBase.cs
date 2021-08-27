using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.SatGen.BeiDou;
using Racelogic.Gnss.SatGen.Galileo;
using Racelogic.Gnss.SatGen.Glonass;
using Racelogic.Gnss.SatGen.Gps;
using Racelogic.Gnss.SatGen.Navic;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public abstract class ConstellationBase
	{
		protected static readonly int maxModulationThreadCount = Environment.ProcessorCount;

		private readonly ConstellationType constellationType;

		private readonly Datum datum;

		private int[] lastVisibleSatIds = Array.Empty<int>();

		private SignalType[] signalTypes = Array.Empty<SignalType>();

		private Signal[] signals = Array.Empty<Signal>();

		private bool isEnabled = true;

		[DisallowNull]
		private AlmanacBase? almanac;

		private const int lockTimeout = 10000;

		private static readonly SyncLock phaseAccumulationLibraryLock = new SyncLock("PhaseAccumulationLibraryLock", 10000);

		[DisallowNull]
		private static FixedSizeDictionary<GnssTime, ConcurrentDictionary<SignalType, decimal[]>>? phaseAccumulationLibrary;

		private static readonly ConstellationType[] constellationTypes = new ConstellationType[5]
		{
			ConstellationType.Gps,
			ConstellationType.Galileo,
			ConstellationType.Glonass,
			ConstellationType.BeiDou,
			ConstellationType.Navic
		};

		internal const int MaxSatCount = 50;

		internal static readonly Range<double> SignalTravelTimeLimits = new Range<double>(0.001, 0.299999999999);

		public ConstellationType ConstellationType
		{
			[DebuggerStepThrough]
			get
			{
				return constellationType;
			}
		}

		public Datum ConstellationDatum
		{
			[DebuggerStepThrough]
			get
			{
				return datum;
			}
		}

		public bool IsEnabled
		{
			[DebuggerStepThrough]
			get
			{
				return isEnabled;
			}
			set
			{
				isEnabled = value;
				foreach (Signal signal in Signals)
				{
					signal.IsEnabled = value;
				}
			}
		}

		public IReadOnlyList<SignalType> SignalTypes
		{
			[DebuggerStepThrough]
			get
			{
				return signalTypes;
			}
			set
			{
				signals = (from t in value
					select Signal.AllSignals.Where((Signal a) => a.ConstellationType == ConstellationType).FirstOrDefault((Signal a) => a.SignalType == t) into s
					where s != null
					select s).ToArray();
				signalTypes = signals.Select((Signal s) => s.SignalType).ToArray();
			}
		}

		public IReadOnlyList<Signal> Signals
		{
			[DebuggerStepThrough]
			get
			{
				return signals;
			}
		}

		public AlmanacBase? Almanac
		{
			[DebuggerStepThrough]
			get
			{
				return almanac;
			}
			[DebuggerStepThrough]
			[param: DisallowNull]
			set
			{
				almanac = value;
			}
		}

		public static IEnumerable<ConstellationType> ConstellationTypes
		{
			[DebuggerStepThrough]
			get
			{
				return constellationTypes;
			}
		}

		protected ConstellationBase(ConstellationType constellationType, Datum datum)
		{
			this.constellationType = constellationType;
			this.datum = datum;
		}

		public static ConstellationBase Create(ConstellationType constellationType)
		{
			return constellationType switch
			{
				ConstellationType.Gps => new Racelogic.Gnss.SatGen.Gps.Constellation(), 
				ConstellationType.Glonass => new Racelogic.Gnss.SatGen.Glonass.Constellation(), 
				ConstellationType.BeiDou => new Racelogic.Gnss.SatGen.BeiDou.Constellation(), 
				ConstellationType.Galileo => new Racelogic.Gnss.SatGen.Galileo.Constellation(), 
				ConstellationType.Navic => new Racelogic.Gnss.SatGen.Navic.Constellation(), 
				_ => throw new ArgumentException("Unsupported constellation type " + constellationType.ToLongName(), "constellationType"), 
			};
		}

		public static IReadOnlyList<ConstellationBase> Create(IEnumerable<SignalType> signalTypes)
		{
			return (from ct in (from s in Signal.GetSignals(signalTypes)
					select s.ConstellationType).Distinct()
				select Create(ct)).ToArray();
		}

		public IReadOnlyList<SatelliteBase> GetVisibleSats(IReadOnlyList<Pvt> trajectorySamples, SimulationParams simulationParameters, out IReadOnlyList<IReadOnlyList<Observation>> observables)
		{
			if (trajectorySamples == null || !trajectorySamples.Any())
			{
				observables = Array.Empty<IReadOnlyList<Observation>>();
				return Array.Empty<SatelliteBase>();
			}
			observables = GetObservations(trajectorySamples, simulationParameters);
			IReadOnlyList<IReadOnlyList<Observation>> observables2 = observables;
			double? elevationMask = simulationParameters.ElevationMask;
			return GetVisibleSats(observables2, in elevationMask);
		}

		private SatelliteBase[] GetVisibleSats(IEnumerable<IEnumerable<Observation>> observables, in double? elevationMask)
		{
			if (observables == null || !observables.Any())
			{
				return Array.Empty<SatelliteBase>();
			}
			double elevationMaskToUse = elevationMask ?? GetTrueHorizon(in observables.First().First((Observation o) => o != null).ObservedFrom.Ecef);
			Observation[] startObservations = (from o in observables.First()
				where o != null && o.Satellite.IsHealthy && o.AzimuthElevation.Elevation >= elevationMaskToUse
				orderby o.AzimuthElevation.Elevation
				select o).ToArray();
			Observation[] endObservations = (from o in observables.Last()
				where o != null && o.Satellite.IsHealthy && o.AzimuthElevation.Elevation >= elevationMaskToUse
				orderby o.AzimuthElevation.Elevation descending
				select o).ToArray();
			Observation[] observationsToBeFinished = startObservations.Where((Observation o) => !endObservations.Select((Observation oo) => oo.Satellite.Id).Contains(o.Satellite.Id)).ToArray();
			IEnumerable<Observation> enumerable = endObservations.Where((Observation o) => !startObservations.Select((Observation oo) => oo.Satellite.Id).Contains(o.Satellite.Id));
			List<Observation> observations = startObservations.Where((Observation o) => lastVisibleSatIds.Contains(o.Satellite.Id) && !observationsToBeFinished.Select((Observation oo) => oo.Satellite.Id).Contains(o.Satellite.Id)).ToList();
			int[] reusedSatelliteIds = observations.Select((Observation o) => o.Satellite.Id).ToArray();
			IEnumerable<Observation> source3 = startObservations.Where((Observation o) => !reusedSatelliteIds.Contains(o.Satellite.Id));
			Observation[] remainingEndObservationsArray = endObservations.Where((Observation o) => !reusedSatelliteIds.Contains(o.Satellite.Id)).ToArray();
			IEnumerable<Observation> enumerable2 = from o in source3.Select(delegate(Observation startObs)
				{
					int satId = startObs.Satellite.Id;
					Observation observation = remainingEndObservationsArray.FirstOrDefault((Observation o) => o.Satellite.Id == satId);
					return (observation != null && observation.AzimuthElevation.Elevation > startObs.AzimuthElevation.Elevation) ? startObs : null;
				})
				where o != null
				select (o);
			if (enumerable.Any())
			{
				enumerable2 = from o in enumerable2.Concat(enumerable)
					orderby o.AzimuthElevation.Elevation
					select o;
			}
			Observation[] ascendingObservationsArray = enumerable2.ToArray();
			Observation[] source2 = (from o in startObservations
				where !reusedSatelliteIds.Contains(o.Satellite.Id) && !ascendingObservationsArray.Select((Observation oo) => oo.Satellite.Id).Contains(o.Satellite.Id)
				orderby o.AzimuthElevation.Elevation descending
				select o).ToArray();
			double mask = ((elevationMaskToUse > 0.0) ? elevationMaskToUse : 0.0);
			using (IEnumerator<Observation> enumerator = ascendingObservationsArray.SkipWhile((Observation o) => o.AzimuthElevation.Elevation < mask).GetEnumerator())
			{
				using IEnumerator<Observation> enumerator2 = source2.Where((Observation o) => o.AzimuthElevation.Elevation > mask).GetEnumerator();
				bool flag;
				do
				{
					flag = false;
					if (enumerator.MoveNext())
					{
						if (!observations.Contains(enumerator.Current))
						{
							observations.Add(enumerator.Current);
						}
						flag = true;
					}
					if (enumerator.MoveNext())
					{
						if (!observations.Contains(enumerator.Current))
						{
							observations.Add(enumerator.Current);
						}
						flag = true;
					}
					if (enumerator2.MoveNext())
					{
						if (!observations.Contains(enumerator2.Current))
						{
							observations.Add(enumerator2.Current);
						}
						flag = true;
					}
				}
				while (flag);
			}
			observations.AddRange((from o in ascendingObservationsArray.TakeWhile((Observation o) => o.AzimuthElevation.Elevation < mask)
				where !observations.Contains(o)
				select o).Reverse());
			observations.AddRange(from o in source2.SkipWhile((Observation o) => o.AzimuthElevation.Elevation > mask)
				where !observations.Contains(o)
				select o);
			return observations.Select((Observation o) => o.Satellite).ToArray(observations.Count);
		}

		private double GetTrueHorizon(in Ecef ecef)
		{
			double altitude = ecef.ToGeodetic(datum).Altitude;
			return GetTrueHorizon(in altitude);
		}

		public double GetTrueHorizon(in double altitude)
		{
			if (altitude <= 0.0)
			{
				return 0.0;
			}
			double semiMajorAxis = datum.SemiMajorAxis;
			return -Math.PI / 2.0 + Math.Asin(semiMajorAxis / (semiMajorAxis + altitude));
		}

		internal unsafe static GeneratorParams CreateSignalGeneratorParameters(ModulationBank modulationBank, SimulationParams simulationParameters, Channel channel, in Range<GnssTime, GnssTimeSpan> sliceInterval, IDictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>> allObservables, IDictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSats, int concurrency)
		{
			SimulationParams simulationParameters2 = simulationParameters;
			IDictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSats2 = visibleSats;
			IDictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>> allObservables2 = allObservables;
			ModulationBank modulationBank2 = modulationBank;
			int sampleRate = simulationParameters2.Trajectory.SampleRate;
			IReadOnlyList<IReadOnlyList<Observation>> readOnlyList = allObservables2.Values.FirstOrDefault();
			GnssTime gnssTime = readOnlyList?.FirstOrDefault()?.FirstOrDefault((Observation o) => o != null)?.Timestamp ?? sliceInterval.Start;
			int firstObservableIndexOffset = (int)Math.Round((sliceInterval.Start - gnssTime).SecondsDecimal * (decimal)sampleRate);
			ConstellationBase[] array = (from ct in channel.Signals.Select((Signal s) => s.ConstellationType).Distinct()
				select simulationParameters2.Constellations.First((ConstellationBase c) => c.constellationType == ct)).ToArray();
			foreach (ConstellationBase constellationBase in array)
			{
				constellationBase.lastVisibleSatIds = (constellationBase.IsEnabled ? (from s in visibleSats2[constellationBase.ConstellationType]
					where s.IsEnabled
					select s.Id).ToArray() : Array.Empty<int>());
			}
			double seconds = (sliceInterval.Start - simulationParameters2.Interval.Start).Seconds;
			int sliceIndex = (int)Math.Round(seconds / simulationParameters2.SliceLength);
			GnssTime sliceStart = sliceInterval.Start;
			double[] timeSeries = GetTimeSeries(in sliceStart, readOnlyList);
			SimulationParams simulationParameters3 = simulationParameters2;
			sliceStart = sliceInterval.End;
			int captureIndex = GetCaptureIndex(simulationParameters3, in sliceStart, readOnlyList);
			(ConcurrentDictionary<SignalType, decimal[]>, ConcurrentDictionary<SignalType, decimal[]>) tuple = CreatePhaseAccumulationLibraryEntries(in sliceInterval, channel.Signals, in concurrency);
			ConcurrentDictionary<SignalType, decimal[]> accumulatedPhasesForThisSlice = tuple.Item1;
			ConcurrentDictionary<SignalType, decimal[]> accumulatedPhasesForNextSlice = tuple.Item2;
			Range<GnssTime, GnssTimeSpan> sliceIntervalCopy = sliceInterval;
			Dictionary<ModulationType, SignalParams[]> signalParameters = (from s in channel.Signals
				group s by s.ModulationType).ToDictionary((IGrouping<ModulationType, Signal> g) => g.Key, (IGrouping<ModulationType, Signal> g) => g.AsParallel().WithDegreeOfParallelism(g.Count()).WithExecutionMode(ParallelExecutionMode.ForceParallelism)
				.SelectMany(delegate(Signal signal)
				{
					SignalType signalType = signal.SignalType;
					ConstellationType constellationType = signal.ConstellationType;
					IEnumerable<SatelliteBase> source = visibleSats2[constellationType];
					ConstellationBase constellation = simulationParameters2.Constellations.First((ConstellationBase c) => c.constellationType == constellationType);
					decimal[] capturedSignalPhases = new decimal[50];
					SignalParams[] result = source.AsParallel().WithDegreeOfParallelism(maxModulationThreadCount).WithExecutionMode(ParallelExecutionMode.ForceParallelism)
						.Select<SatelliteBase, SignalParams>(delegate(SatelliteBase sat)
						{
							if (!sat.IsEnabled)
							{
								return SignalParams.Empty;
							}
							int satIndex = sat.Index;
							IReadOnlyList<IReadOnlyList<Observation>> readOnlyList2 = allObservables2[constellationType];
							double[]? signalLevels = GetSignalLevels(in sliceIndex, readOnlyList2, signalType, simulationParameters2);
							double num = ((signalLevels != null) ? signalLevels[satIndex] : 1.0);
							num *= signal.NominalSignalLevel;
							if (num <= 0.0)
							{
								return SignalParams.Empty;
							}
							Observation[] array2 = readOnlyList2.Select((IReadOnlyList<Observation> o) => o[satIndex]).ToArray();
							if (!array2.Any((Observation obs) => obs == null))
							{
								FastAkimaDecimal dopplerIntegrator = GetDopplerIntegrator(signalType, timeSeries, array2);
								decimal[] array3 = new decimal[timeSeries.Length];
								decimal num2 = 0.0m;
								int fromIndex;
								for (int j = 1; j < timeSeries.Length; j++)
								{
									decimal num3 = num2;
									fromIndex = j - 1;
									num2 = (array3[j] = num3 + dopplerIntegrator.Integrate(in fromIndex, in j));
								}
								decimal accumulatedPhase = accumulatedPhasesForThisSlice[signalType][satIndex];
								decimal capturedPhase;
								FastAkima phaseAccumulator = GetPhaseAccumulator(signal, in satIndex, timeSeries, array3, accumulatedPhase, simulationParameters2, in captureIndex, out capturedPhase);
								capturedSignalPhases[satIndex] = capturedPhase;
								ModulationSignal modulation = constellation.GetModulation(modulationBank2, signal, array2, in sliceIntervalCopy);
								ModulationBank modulationBank3 = modulationBank2;
								GnssTime timeStamp = sliceIntervalCopy.Start;
								sbyte* modulationPointer = modulationBank3.FindBufferPointer(in timeStamp, modulation.Modulation);
								Signal signal3 = signal;
								double[] timeSeries2 = timeSeries;
								fromIndex = modulation.Offset;
								return new SignalParams(chipIndexInterpolatorCoeffs: GetModulationIndexInterpolator(signal3, timeSeries2, array2, in fromIndex).Coefficients, phaseAccumulatorCoeffs: phaseAccumulator.Coefficients, modulationPointer: modulationPointer, signalLevel: in num);
							}
							return SignalParams.Empty;
						})
						.ToArray();
					accumulatedPhasesForNextSlice[signalType] = capturedSignalPhases;
					return result;
				})
				.ToArray());
			foreach (NavigationDataType item in from ndi in channel.Signals.SelectMany((Signal s) => s.NavigationDataInfos)
				select ndi.NavigationDataType)
			{
				sliceStart = sliceInterval.End;
				NavigationData.FinalizeFEC(in sliceStart, item);
			}
			RejectMissingModulationSignals(channel, signalParameters);
			Signal signal2 = channel.Signals.First();
			double attenuation = simulationParameters2.Attenuation;
			AlignedBuffer<double> noiseSamples = Noise.GetNoiseSamples(signal2.FrequencyBand, in attenuation, simulationParameters2.Signals);
			return new GeneratorParams(channel, timeSeries, signalParameters, in sliceInterval, in firstObservableIndexOffset, in noiseSamples);
		}

		private static int GetCaptureIndex(SimulationParams simulationParameters, in GnssTime sliceIntervalEnd, IReadOnlyList<IReadOnlyList<Observation>>? observables)
		{
			Range<GnssTime, GnssTimeSpan> sliceInterval = new Range<GnssTime, GnssTimeSpan>(sliceIntervalEnd, sliceIntervalEnd);
			GnssTime start = Simulation.GetObservationInterval(in sliceInterval, simulationParameters).Start;
			int num = observables?.Count ?? 0;
			int i;
			for (i = 0; i < num && !((observables![i].FirstOrDefault((Observation o) => o != null)?.Timestamp ?? GnssTime.MinValue) == start); i++)
			{
			}
			return i;
		}

		private static (ConcurrentDictionary<SignalType, decimal[]> AccumulatedPhasesForThisSlice, ConcurrentDictionary<SignalType, decimal[]> AccumulatedPhasesForNextSlice) CreatePhaseAccumulationLibraryEntries(in Range<GnssTime, GnssTimeSpan> sliceInterval, IEnumerable<Signal> signals, in int concurrency)
		{
			ConcurrentDictionary<SignalType, decimal[]> value;
			ConcurrentDictionary<SignalType, decimal[]> value2;
			using (phaseAccumulationLibraryLock.Lock())
			{
				if (phaseAccumulationLibrary == null)
				{
					phaseAccumulationLibrary = new FixedSizeDictionary<GnssTime, ConcurrentDictionary<SignalType, decimal[]>>(concurrency);
				}
				if (!phaseAccumulationLibrary!.TryGetValue(sliceInterval.Start, out value) || !value.ContainsKey(signals.FirstOrDefault()?.SignalType ?? SignalType.None))
				{
					if (value == null)
					{
						value = new ConcurrentDictionary<SignalType, decimal[]>();
						phaseAccumulationLibrary![sliceInterval.Start] = value;
					}
					foreach (Signal signal in signals)
					{
						value[signal.SignalType] = new decimal[50];
					}
				}
				if (!phaseAccumulationLibrary!.TryGetValue(sliceInterval.End, out value2))
				{
					value2 = new ConcurrentDictionary<SignalType, decimal[]>();
					phaseAccumulationLibrary![sliceInterval.End] = value2;
				}
			}
			return (value, value2);
		}

		private static void RejectMissingModulationSignals(Channel channel, IDictionary<ModulationType, SignalParams[]> signalParameters)
		{
			bool flag = false;
			bool flag2 = channel.Signals.Select((Signal s) => s.FrequencyBand).Distinct().Skip(1)
				.Any();
			bool flag3 = channel.Signals.Select((Signal s) => s.ConstellationType).Distinct().Skip(1)
				.Any();
			if (!flag2 && !flag3)
			{
				foreach (SignalParams[] signalParams in signalParameters.Values)
				{
					for (int i = 0; i < signalParams.Length; i++)
					{
						if (!signalParams[i].IsEmpty)
						{
							continue;
						}
						flag = true;
						foreach (SignalParams[] item in signalParameters.Values.Where((SignalParams[] pp) => pp != signalParams))
						{
							item[i] = SignalParams.Empty;
						}
					}
				}
			}
			if (!(flag || flag2 || flag3))
			{
				return;
			}
			ModulationType[] array = signalParameters.Keys.ToArray();
			foreach (ModulationType key in array)
			{
				signalParameters[key] = signalParameters[key].Where((SignalParams s) => !s.IsEmpty).ToArray();
			}
		}

		private static double[] GetTimeSeries(in GnssTime sliceStart, IReadOnlyList<IEnumerable<Observation>>? observables)
		{
			if (observables == null || !observables.Any())
			{
				return Array.Empty<double>();
			}
			double[] array = new double[observables!.Count];
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Observation item in observables![i])
				{
					if (item != null)
					{
						array[i] = (item.Timestamp - sliceStart).Seconds;
						break;
					}
				}
			}
			return array;
		}

		private static FastAkimaDecimal GetDopplerIntegrator(SignalType signalType, double[] timeSeries, Observation[] observations)
		{
			decimal[] array = new decimal[timeSeries.Length];
			for (int i = 0; i < timeSeries.Length; i++)
			{
				array[i] = (decimal)timeSeries[i];
			}
			decimal[] array2 = new decimal[timeSeries.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = observations[j].SignalObservations[signalType].DopplerFrequency;
			}
			return new FastAkimaDecimal(array, array2);
		}

		private static FastAkima GetPhaseAccumulator(Signal signal, in int satIndex, double[] timeSeries, decimal[] dopplerIntegrationSeries, decimal accumulatedPhase, SimulationParams simulationParameters, in int captureIndex, out decimal capturedPhase)
		{
			decimal num = ((decimal)signal.NominalFrequency - simulationParameters.Output.ChannelPlan.GetActualFrequency(signal) + (((decimal?)signal.SlotFrequencies?[satIndex]) ?? 0.0m)) * simulationParameters.Trajectory.SamplePeriod;
			capturedPhase = 0.0m;
			double[] array = new double[timeSeries.Length];
			for (int i = 0; i < array.Length; i++)
			{
				decimal num2 = accumulatedPhase + dopplerIntegrationSeries[i];
				array[i] = (double)num2;
				if (i == captureIndex)
				{
					capturedPhase = num2 - num2.Floor();
				}
				accumulatedPhase += num;
			}
			return new FastAkima(timeSeries, array);
		}

		private static FastAkima GetModulationIndexInterpolator(Signal signal, double[] timeSeries, Observation[] observations, in int modulationOffset)
		{
			double num = signal.ModulationRate;
			double[] array = new double[timeSeries.Length];
			for (int i = 0; i < array.Length; i++)
			{
				double num2 = observations[i].SignalObservations[signal.SignalType].PseudoRange * 3.3356409519815204E-09;
				array[i] = (double)modulationOffset + (timeSeries[i] - num2) * num;
			}
			return new FastAkima(timeSeries, array);
		}

		private protected Dictionary<SignalType, SignalObservation> MakeSignalObservations(SatelliteBase satellite, in GnssTime observerTime, in Geodetic observerCoordinates, in Topocentric azimuthElevation, in double pseudoRangeSoFar, in double dopplerVelocity)
		{
			Dictionary<SignalType, SignalObservation> dictionary = new Dictionary<SignalType, SignalObservation>();
			foreach (Signal signal in Signals)
			{
				uint num = signal.NominalFrequency.SafeAdd(signal.SlotFrequencies?[satellite.Index] ?? 0);
				decimal dopplerFrequency = (decimal)num * (decimal)dopplerVelocity * 0.0000000033356409519815204958m;
				double frequency = num;
				double ionosphericDelay = GetIonosphericDelay(in observerTime, in observerCoordinates, in azimuthElevation, in frequency);
				double pseudoRange = pseudoRangeSoFar + ionosphericDelay;
				dictionary[signal.SignalType] = new SignalObservation(in pseudoRange, in dopplerFrequency);
			}
			return dictionary;
		}

		private protected virtual double GetIonosphericDelay(in GnssTime time, in Geodetic position, in Topocentric azimuthElevation, in double frequency)
		{
			double referenceFrequency = 1575420000.0;
			return Klobuchar.GetIonosphericDelay(in time, in position, in azimuthElevation, in frequency, in referenceFrequency);
		}

		internal abstract ModulationSignal GetModulation(ModulationBank modulationBank, Signal signal, IEnumerable<Observation> satObservations, in Range<GnssTime, GnssTimeSpan> sliceInterval);

		private protected abstract IReadOnlyList<IReadOnlyList<Observation>> GetObservations(IReadOnlyList<Pvt> trajectorySliceSamples, SimulationParams simulationParameters);

		public abstract Observation? Observe(SatelliteBase satellite, in Pvt observer, in double? elevationMask, in bool makeSignalObservations = true);

		internal IReadOnlyList<IReadOnlyList<Observation>> CreateObservables(IReadOnlyList<Pvt> trajectorySliceSamples, IEnumerable<SatelliteBase> almanacSatellites, in double? elevationMask)
		{
			int count = trajectorySliceSamples.Count;
			Observation[][] array = new Observation[count][];
			for (int i = 0; i < count; i++)
			{
				Pvt observer = trajectorySliceSamples[i];
				Observation[] array2 = new Observation[50];
				foreach (SatelliteBase almanacSatellite in almanacSatellites)
				{
					bool makeSignalObservations = true;
					Observation observation = Observe(almanacSatellite, in observer, in elevationMask, in makeSignalObservations);
					if (observation != null)
					{
						array2[almanacSatellite.Index] = observation;
					}
				}
				array[i] = array2;
			}
			return array;
		}

		private protected Range<double> GetSignalTravelTime(IEnumerable<Observation> observations, in int bitRate)
		{
			double num = (double)bitRate * 3.3356409519815204E-09;
			Range<double> range = observations.Select((Observation o) => o.SignalObservations.First().Value.PseudoRange).MinMax();
			double value3 = Math.Floor(range.Min * num - 0.5) / (double)bitRate;
			double value2 = Math.Ceiling(range.Max * num) / (double)bitRate;
			return new Range<double>(value3, value2);
		}

		private static double[]? GetSignalLevels(in int sliceIndex, IEnumerable<IEnumerable<Observation>> observables, SignalType signalType, SimulationParams simulationParameters)
		{
			return simulationParameters.SignalLevelMode switch
			{
				SignalLevelMode.Realistic => GetRealWorldSignalLevels(observables).ElementAtOrDefault(sliceIndex), 
				SignalLevelMode.Manual => simulationParameters.SignalLevels[signalType], 
				_ => null, 
			};
		}

		private static IReadOnlyList<double[]> GetRealWorldSignalLevels(IEnumerable<IEnumerable<Observation>> observables)
		{
			if (!observables.Any() || observables.First().All((Observation o) => o.Satellite == null))
			{
				return Array.Empty<double[]>();
			}
			return observables.Select((IEnumerable<Observation> obs) => obs.Select(delegate(Observation o)
			{
				if (o == null || o.Satellite == null || o.AzimuthElevation.Elevation < 0.0)
				{
					return 0.0;
				}
				double num = (o.ObservedFrom.Ecef.Position + o.LineOfSight).Magnitude() - Datum.WGS84.SemiMajorAxis;
				double num2 = o.LineOfSight.Magnitude();
				double num3 = num / num2;
				double num4 = num3 * num3;
				double num5 = o.AzimuthElevation.Elevation;
				if (num5 < 0.01)
				{
					num5 = 0.01;
				}
				double num6 = 1.0 / num5;
				double num7 = -9.73252335364463E-05 * num6 * num6 - 0.00295143976358626 * num6 + 1.00180590094222;
				Vector3D position = -1.0 * o.LineOfSight;
				Ecef ecef = new Ecef(in position, isAbsolute: false);
				position = o.ObservedFrom.Ecef.Position + o.LineOfSight;
				Geodetic referenceLocation = new Ecef(in position, isAbsolute: true).ToGeodetic();
				double num8 = Math.PI / 2.0 - Math.Abs(ecef.ToNed(in referenceLocation).ToAzimuthElevation().Elevation);
				double num9 = -269.284672532904 * num8 * num8 * num8 * num8 + 32.9351773487971 * num8 * num8 * num8 + 17.3527421323717 * num8 * num8 - 1.89761304544049 * num8 + 0.851545696054809;
				double num10 = -6.0 + 6.0 * (Math.PI / 2.0 - num5).Cos();
				double num11 = Math.Pow(10.0, 0.05 * num10);
				return 1.2 * num4 * num7 * num9 * num11;
			}).ToArray()).ToArray();
		}

		public bool LoadAlmanac(string path, in GnssTime simulationTime)
		{
			try
			{
				string extension = Path.GetExtension(path);
				using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (LoadAlmanac(stream, in simulationTime, extension))
				{
					Almanac!.FilePath = path;
					return Almanac!.OriginalSatellites.Any();
				}
			}
			catch (ArgumentException)
			{
			}
			catch (IOException)
			{
			}
			catch (SecurityException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (NotSupportedException)
			{
			}
			return false;
		}

		public abstract bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null);

		public static bool VerifyAlmanac(string almanacPath, ConstellationType constellationType, in DateTime almanacTime)
		{
			if (!File.Exists(almanacPath) || new FileInfo(almanacPath).Length < 100)
			{
				return false;
			}
			ConstellationBase constellationBase = Create(constellationType);
			GnssTime simulationTime = GnssTime.FromUtc(almanacTime);
			return constellationBase.LoadAlmanac(almanacPath, in simulationTime);
		}
	}
}
