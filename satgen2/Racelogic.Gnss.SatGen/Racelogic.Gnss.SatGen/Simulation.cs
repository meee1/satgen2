using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aladdin.HASP;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public abstract class Simulation : BasePropertyChanged, IDisposable
{
	private enum OptimizationStrategy
	{
		Throughput,
		Latency
	}

	private readonly SimulationParams simulationParameters;

	private protected readonly ModulationBank ModulationBank;

	private readonly Channel[] channelsInUse;

	private volatile SimulationState simulationState;

	protected volatile bool outputErrorOccured;

	protected readonly Dictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSats = new Dictionary<ConstellationType, IEnumerable<SatelliteBase>>();

	private static readonly Dictionary<ConstellationType, int> satCountLeft;

	private static readonly Dictionary<ConstellationType, int> satCountToSimulate;

	private const int lockTimeout = 10000;

	private readonly SyncLock quantizationHistoryLock = new SyncLock("QuantizationHistory Lock", 10000);

	private readonly Dictionary<Channel, FixedSizeStack<double>> quantizationHistory = new Dictionary<Channel, FixedSizeStack<double>>();

	private bool initialCheckPerformed;

	private const double demoDurationLimit = 120.0;

	private bool demoDurationExceeded;

	private readonly ProgressEstimator progressEstimator = new ProgressEstimator();

	private int sliceIndex;

	private readonly int featureCheckSliceCount;

	private const int featureRetestPeriodMilliseconds = 2000;

	private readonly HaspFeature singleConstellationFeature = HaspFeature.FromFeature(31);

	private readonly HaspFeature dualConstellationFeature = HaspFeature.FromFeature(32);

	private readonly HaspFeature tripleConstellationFeature = HaspFeature.FromFeature(33);

	private readonly HaspFeature widebandFeature = HaspFeature.FromFeature(41);

	private readonly HaspFeature realTimeFeature = HaspFeature.FromFeature(40);

	private readonly SignalType[] basicSignalTypes = new SignalType[4]
	{
		SignalType.GpsL1CA,
		SignalType.GlonassL1OF,
		SignalType.BeiDouB1I,
		SignalType.GalileoE1BC
	};

	private int bufferUnderrunCount;

	internal static readonly bool IsFmaSuppported;

	private bool isDisposed;

	public SimulationParams SimulationParameters
	{
		[DebuggerStepThrough]
		get
		{
			return simulationParameters;
		}
	}

	public SimulationState SimulationState
	{
		[DebuggerStepThrough]
		get
		{
			return simulationState;
		}
		[DebuggerStepThrough]
		protected set
		{
			simulationState = value;
			OnPropertyChanged("SimulationState");
			OnPropertyChanged("IsAlive");
		}
	}

	public bool IsAlive
	{
		get
		{
			if (simulationState >= SimulationState.Initializing && simulationState <= SimulationState.Cancelling)
			{
				return !outputErrorOccured;
			}
			return false;
		}
	}

	protected ProgressEstimator ProgressEstimator
	{
		[DebuggerStepThrough]
		get
		{
			return progressEstimator;
		}
	}

	public ReadOnlyDictionary<ConstellationType, IEnumerable<SatelliteBase>> VisibleSats
	{
		[DebuggerStepThrough]
		get
		{
			return new ReadOnlyDictionary<ConstellationType, IEnumerable<SatelliteBase>>(visibleSats);
		}
	}

	public abstract double TimeInProcess { get; }

	public abstract double ProgressOffset { get; }

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

	protected IReadOnlyList<Channel> ChannelsInUse
	{
		[DebuggerStepThrough]
		get
		{
			return channelsInUse;
		}
	}

	public event EventHandler<TimeSpan>? BufferUnderrun;

	public event EventHandler<SimulationProgressChangedEventArgs>? ProgressChanged;

	public event EventHandler<SimulationCompletedEventArgs>? Completed;

	static Simulation()
	{
		satCountLeft = new Dictionary<ConstellationType, int>(Enum.GetValues(typeof(ConstellationType)).Length);
		satCountToSimulate = new Dictionary<ConstellationType, int>(Enum.GetValues(typeof(ConstellationType)).Length);
		double num = 12341234.123412341;
		double num2 = 1.000000000001;
		double num3 = 1.000000001;
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < 100000; i++)
		{
			num = Math.FusedMultiplyAdd(num2 * (double)i, num3 * (double)i, num);
			num = Math.FusedMultiplyAdd((0.0 - num2) * (double)i, num3 * (double)i, num);
		}
		stopwatch.Stop();
		IsFmaSuppported = num != 0.0 && stopwatch.ElapsedMilliseconds < 5;
	}

	protected Simulation(SimulationParams simulationParameters)
	{
		this.simulationParameters = simulationParameters;
		simulationParameters.Output.Error += new EventHandler<ErrorEventArgs>(OnOutputError);
		foreach (ConstellationType item in simulationParameters.Constellations.Select((ConstellationBase c) => c.ConstellationType))
		{
			visibleSats.Add(item, Array.Empty<SatelliteBase>());
		}
		channelsInUse = (from ch in simulationParameters.Output.ChannelPlan.Channels
			where ch != null
			select (ch)).ToArray();
		Channel[] array = channelsInUse;
		foreach (Channel key in array)
		{
			quantizationHistory[key] = new FixedSizeStack<double>(20);
		}
		featureCheckSliceCount = (int)(180.0 / simulationParameters.SliceLength);
		int concurrency = GetConcurrency(simulationParameters.Trajectory, simulationParameters.Output);
		ModulationBank = new ModulationBank(simulationParameters.SliceLength, concurrency);
	}

	public static Simulation Create(SimulationParams simulationParameters)
	{
		return GetOptimisationStrategy(simulationParameters.Trajectory) switch
		{
			OptimizationStrategy.Throughput => new DoubleBufferSimulation(simulationParameters), 
			OptimizationStrategy.Latency => new RealTimeSimulation(simulationParameters), 
			_ => new DoubleBufferSimulation(simulationParameters), 
		};
	}

	public static int GetConcurrency(Trajectory? trajectory, Output? output)
	{
		return GetOptimisationStrategy(trajectory) switch
		{
			OptimizationStrategy.Throughput => DoubleBufferSimulation.GetConcurrency(output), 
			OptimizationStrategy.Latency => RealTimeSimulation.GetConcurrency(), 
			_ => 1, 
		};
	}

	private static OptimizationStrategy GetOptimisationStrategy(Trajectory? trajectory)
	{
		if (trajectory != null && trajectory!.IsExternal)
		{
			return OptimizationStrategy.Latency;
		}
		return OptimizationStrategy.Throughput;
	}

	public abstract void Start();

	public abstract void Pause();

	public abstract void Resume();

	public abstract void Cancel();

	private protected void ApplyAGC(Channel channel, SimulationSlice slice, SimulationParams simulationParameters)
	{
		if (simulationParameters.Output.ChannelPlan.Quantization > Quantization.OneBit)
		{
			double item = slice.MeasureRMS(channel);
			double rms;
			using (quantizationHistoryLock.Lock())
			{
				FixedSizeStack<double> fixedSizeStack = quantizationHistory[channel];
				fixedSizeStack.Push(item);
				rms = fixedSizeStack.Average();
			}
			slice.ApplyRMS(channel, rms);
		}
	}

	internal static Range<GnssTime, GnssTimeSpan> GetObservationInterval(in Range<GnssTime, GnssTimeSpan> sliceInterval, SimulationParams simulationParameters)
	{
		decimal samplePeriod = simulationParameters.Trajectory.SamplePeriod;
		decimal num = simulationParameters.Trajectory.SampleRate;
		double num2 = 2.0 * (double)samplePeriod;
		double seconds = num2 + ConstellationBase.SignalTravelTimeLimits.Max;
		double seconds2 = ((num2 > ConstellationBase.SignalTravelTimeLimits.Min) ? (num2 - ConstellationBase.SignalTravelTimeLimits.Min) : 0.0);
		GnssTime gnssTime = sliceInterval.Start - GnssTimeSpan.FromSeconds(seconds);
		GnssTime gnssTime2 = sliceInterval.End + GnssTimeSpan.FromSeconds(seconds2);
		GnssTime value = GnssTime.FromGps(gnssTime.GpsWeek, (gnssTime.GpsSecondOfWeekDecimal * num).SafeFloor() * samplePeriod);
		GnssTime value2 = GnssTime.FromGps(gnssTime2.GpsWeek, (gnssTime2.GpsSecondOfWeekDecimal * num).SafeCeiling() * samplePeriod);
		return new Range<GnssTime, GnssTimeSpan>(value, value2);
	}

	protected void ReadVisibleSatellites(SimulationParams simulationParameters)
	{
		GnssTime start = simulationParameters.Interval.Start;
		Range<GnssTime, GnssTimeSpan> sliceInterval = new Range<GnssTime, GnssTimeSpan>(start, GnssTimeSpan.FromSeconds(simulationParameters.SliceLength));
		IReadOnlyList<Pvt> trajectorySlice = GetTrajectorySlice(in sliceInterval);
		foreach (ConstellationBase constellation in SimulationParameters.Constellations)
		{
			visibleSats[constellation.ConstellationType] = constellation.GetVisibleSats(trajectorySlice, simulationParameters, out var _);
		}
		OnPropertyChanged("VisibleSats");
		if (SimulationState < SimulationState.Ready)
		{
			SimulationState = SimulationState.Ready;
		}
	}

	protected IReadOnlyList<Pvt> GetTrajectorySlice(in Range<GnssTime, GnssTimeSpan> sliceInterval)
	{
		Range<GnssTime, GnssTimeSpan> interval = GetObservationInterval(in sliceInterval, SimulationParameters);
		IReadOnlyList<Pvt> samples;
		for (samples = SimulationParameters.Trajectory.GetSamples(in interval); samples == null; samples = SimulationParameters.Trajectory.GetSamples(in interval))
		{
			Thread.Yield();
		}
		return samples;
	}

	protected static void ApplySatCountLimit(IDictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSatsForAllConstellations, SimulationParams simulationParameters)
	{
		IDictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSatsForAllConstellations2 = visibleSatsForAllConstellations;
		bool flag = simulationParameters.SatCountLimitMode != simulationParameters.LastSatCountLimitMode;
		switch (simulationParameters.SatCountLimitMode)
		{
		case SatCountLimitMode.Automatic:
		{
			ConstellationType[] array = ConstellationBase.ConstellationTypes.Where((ConstellationType ct) => visibleSatsForAllConstellations2.Keys.Contains(ct)).ToArray();
			ConstellationType[] array2 = array;
			foreach (ConstellationType key in array2)
			{
				satCountLeft[key] = visibleSatsForAllConstellations2[key].Count();
				satCountToSimulate[key] = 0;
			}
			int num = Math.Min(satCountLeft.Values.Sum(), simulationParameters.AutomaticSatCountLimit);
			while (num > 0)
			{
				array2 = array;
				foreach (ConstellationType key2 in array2)
				{
					if (satCountLeft[key2] > 0 && num > 0)
					{
						satCountLeft[key2]--;
						satCountToSimulate[key2]++;
						num--;
					}
				}
			}
			array2 = array;
			foreach (ConstellationType constellationType in array2)
			{
				LimitSatCount(satCountToSimulate[constellationType], constellationType, visibleSatsForAllConstellations2);
			}
			break;
		}
		case SatCountLimitMode.Constellation:
			foreach (ConstellationType key3 in visibleSatsForAllConstellations2.Keys)
			{
				LimitSatCount(simulationParameters.SatCountLimits[key3], key3, visibleSatsForAllConstellations2);
			}
			break;
		default:
			if (!flag)
			{
				break;
			}
			foreach (ConstellationType key4 in visibleSatsForAllConstellations2.Keys)
			{
				LimitSatCount(int.MaxValue, key4, visibleSatsForAllConstellations2);
			}
			break;
		case SatCountLimitMode.Manual:
			break;
		}
		simulationParameters.LastSatCountLimitMode = simulationParameters.SatCountLimitMode;
	}

	private static void LimitSatCount(int satCount, ConstellationType constellationType, IDictionary<ConstellationType, IEnumerable<SatelliteBase>> visibleSatsForAllConstellations)
	{
		IEnumerable<SatelliteBase> source = visibleSatsForAllConstellations[constellationType];
		foreach (SatelliteBase item in source.Take(satCount))
		{
			item.IsEnabled = true;
		}
		foreach (SatelliteBase item2 in source.Skip(satCount))
		{
			item2.IsEnabled = false;
		}
	}

	protected virtual void OnBufferUnderrun(object? sender, TimeSpan delay)
	{
		BufferUnderrunCount++;
		this.BufferUnderrun?.Invoke(this, delay);
	}

	private void OnOutputError(object? sender, ErrorEventArgs e)
	{
		outputErrorOccured = true;
	}

	protected virtual void OnCompleted(bool cancelled)
	{
		SimulationState = (cancelled ? SimulationState.Cancelled : SimulationState.Finished);
		SimulationParameters.Trajectory.Dispose();
		SimulationParameters.Output.Error -= new EventHandler<ErrorEventArgs>(OnOutputError);
		SimulationParameters.Output.Dispose();
		this.Completed?.Invoke(this, new SimulationCompletedEventArgs(in cancelled));
	}

	protected void OnProgressChanged(in GnssTime time)
	{
		GnssTimeSpan simulatedTimeFromStart = time - SimulationParameters.Interval.Start;
		double secondsFromStart = simulatedTimeFromStart.Seconds;
		double progress = secondsFromStart / SimulationParameters.Interval.Width.Seconds;
		ProgressEstimator.Progress = progress;
		EventHandler<SimulationProgressChangedEventArgs>? progressChanged = this.ProgressChanged;
		if (progressChanged != null)
		{
			TimeSpan elapsedTime = ProgressEstimator.ElapsedTime;
			TimeSpan timeLeft = ProgressEstimator.TimeLeft;
			progressChanged!(this, new SimulationProgressChangedEventArgs(progress, in time, in simulatedTimeFromStart, in elapsedTime, in timeLeft));
		}
		if (!initialCheckPerformed)
		{
			initialCheckPerformed = true;
			/*Task.Run(delegate
			{
				bool allowRetest = false;
				CheckFeatures(in allowRetest);
			});*/
		}
		//CheckFeaturesPeriodically(in secondsFromStart);
		sliceIndex++;
	}

	private void CheckFeaturesPeriodically(in double secondsFromStart)
	{
		return;
		if (!demoDurationExceeded)
		{
			if (secondsFromStart >= 120.0 && !demoDurationExceeded)
			{
				demoDurationExceeded = true;
				bool allowRetest = false;
				CheckFeatures(in allowRetest);
			}
		}
		else if (sliceIndex % featureCheckSliceCount == 0)
		{
			Task.Run(delegate
			{
				bool allowRetest2 = true;
				CheckFeatures(in allowRetest2);
			});
		}
	}

	private void CheckFeatures(in bool allowRetest = true)
	{
		return;
		if (SimulationParameters.LiveOutput != null && !CheckFeature(realTimeFeature))
		{
			bool flag = false;
			if (IsAlive & allowRetest)
			{
				RLLogger.GetLogger().LogMessage($"RealTime dongle check failed, retrying in {2} seconds...");
				Thread.Sleep(2000);
				if (CheckFeature(realTimeFeature))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				Cancel();
				RLLogger.GetLogger().LogMessage("Error: RealTime simulation attempted without RealTime dongle");
				return;
			}
		}
		if (CheckFeature(widebandFeature))
		{
			return;
		}
		IEnumerable<Signal> signals = SimulationParameters.Signals;
		if (signals.All((Signal s) => basicSignalTypes.Contains(s.SignalType)))
		{
			int num = signals.Select((Signal s) => s.FrequencyBand).Distinct().Count();
			if (CheckFeature(singleConstellationFeature))
			{
				if (num <= 1)
				{
					return;
				}
			}
			else if (CheckFeature(dualConstellationFeature))
			{
				if (num <= 2)
				{
					return;
				}
			}
			else if (CheckFeature(tripleConstellationFeature) && num <= 3)
			{
				return;
			}
		}
		if (demoDurationExceeded)
		{
			Cancel();
			RLLogger.GetLogger().LogMessage("Demo duration exceeded");
		}
		else if (signals.Any((Signal s) => s.SignalType != SignalType.GpsL1CA))
		{
			Cancel();
			RLLogger.GetLogger().LogMessage("Error: trying to simulate signals other than GPS L1C/A in demo mode");
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	private static bool CheckFeature(HaspFeature feature)
    {
        return true;
		using Hasp hasp = new Hasp(feature);
		HaspStatus num = hasp.Login(VendorCode.Code);
		hasp.Logout();
		return num == HaspStatus.StatusOk;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (isDisposed)
		{
			return;
		}
		isDisposed = true;
		if (disposing)
		{
			Task.Run(delegate
			{
				Cancel();
			})?.Wait();
			SimulationParameters.Trajectory.Dispose();
			SimulationParameters.Output.Error -= new EventHandler<ErrorEventArgs>(OnOutputError);
			SimulationParameters.Output.Dispose();
			ModulationBank.Dispose();
		}
	}
}
