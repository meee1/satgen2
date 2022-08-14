using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

internal sealed class RealTimeSimulation : Simulation
{
	private readonly Thread creatorThread;

	private readonly Thread processorThread;

	private readonly Thread outputThread;

	private int skipSliceProcessingCount;

	private int muteBufferUnderrunCount;

	private const int lockTimeout = 10000;

	private readonly SyncLock bufferUnderrunLock = new SyncLock("bufferUnderrunLock", 10000);

	private readonly List<AlignedBuffer<byte>> alignedBuffers = new List<AlignedBuffer<byte>>();

	private readonly Dictionary<Channel, ConcurrentQueue<Memory<byte>>> bufferDictionary = new Dictionary<Channel, ConcurrentQueue<Memory<byte>>>();

	private readonly ConcurrentQueue<(Range<GnssTime, GnssTimeSpan>, IReadOnlyList<Pvt>)> creationQueue = new ConcurrentQueue<(Range<GnssTime, GnssTimeSpan>, IReadOnlyList<Pvt>)>();

	private readonly ConcurrentQueue<SimulationSlice> processingQueue = new ConcurrentQueue<SimulationSlice>();

	private readonly ConcurrentQueue<SimulationSlice> outputQueue = new ConcurrentQueue<SimulationSlice>();

	private volatile bool initializationStarted;

	private SimulationSlice[]? dummySlices = new SimulationSlice[3];

	private bool disposedValue;

	public override double ProgressOffset
	{
		[DebuggerStepThrough]
		get
		{
			throw new NotImplementedException();
		}
	}

	public override double TimeInProcess
	{
		[DebuggerStepThrough]
		get
		{
			throw new NotImplementedException();
		}
	}

	public RealTimeSimulation(SimulationParams simulationParameters)
		: base(simulationParameters)
	{
		if (!simulationParameters.Trajectory.IsExternal)
		{
			throw new NotSupportedException("Only external synchronous trajectories are supported in RealTimeSimulation");
		}
		int num = GetConcurrency() + 1 + dummySlices!.Length;
		if (simulationParameters.Output is ILiveOutput liveOutput)
		{
			num += liveOutput.BufferCount;
			liveOutput.BufferUnderrun += new EventHandler<TimeSpan>(OnBufferUnderrun);
			liveOutput.PlaybackStarted += new EventHandler(OnLiveOutputPlaybackStarted);
		}
		Output output = simulationParameters.Output;
		double seconds = simulationParameters.SliceLength;
		int size = output.GetOutputByteCountForInterval(in seconds);
		foreach (Channel item2 in base.ChannelsInUse)
		{
			ConcurrentQueue<Memory<byte>> concurrentQueue = new ConcurrentQueue<Memory<byte>>();
			for (int i = 0; i <= num; i++)
			{
				AlignedBuffer<byte> item = new AlignedBuffer<byte>(in size);
				alignedBuffers.Add(item);
				concurrentQueue.Enqueue(item.Memory);
			}
			bufferDictionary.Add(item2, concurrentQueue);
		}
		base.SimulationParameters.Trajectory.NewSample += new EventHandler<EventArgs<GnssTime>>(OnNewTrajectorySample);
		creatorThread = new Thread(new ThreadStart(CreateSlices));
		processorThread = new Thread(new ThreadStart(ProcessSlices));
		outputThread = new Thread(new ThreadStart(OutputSlices));
		base.SimulationState = SimulationState.Ready;
	}

	public override void Start()
	{
		foreach (NavigationDataType item in from ndi in base.SimulationParameters.Signals.SelectMany((Signal s) => s.NavigationDataInfos)
			select ndi.NavigationDataType)
		{
			NavigationData.ClearFEC(item);
		}
		if (base.SimulationParameters.LiveOutput != null)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		}
		else
		{
			base.ProgressEstimator.Start();
		}
		creatorThread.Start();
		processorThread.Start();
		outputThread.Start();
		base.SimulationState = SimulationState.Initializing;
	}

	public override void Pause()
	{
		throw new NotSupportedException("Pause() is not available in RealTimeSimulation");
	}

	public override void Resume()
	{
		throw new NotSupportedException("Resume() is not available in RealTimeSimulation");
	}

	public override async void Cancel()
	{
		if (base.IsAlive)
		{
			base.SimulationState = SimulationState.Cancelling;
			base.SimulationParameters.Trajectory.NewSample -= new EventHandler<EventArgs<GnssTime>>(OnNewTrajectorySample);
			await Task.Run(delegate
			{
				creatorThread.Join();
				processorThread.Join();
				outputThread.Join();
			}).ConfigureAwait(continueOnCapturedContext: false);
			OnCompleted(cancelled: true);
		}
	}

	public static int GetConcurrency()
	{
		return 1;
	}

	private void ReleaseBuffers()
	{
		foreach (SimulationSlice item in outputQueue)
		{
			RecycleSliceBuffers(item);
		}
		outputQueue.Clear();
		bufferDictionary.Clear();
		foreach (AlignedBuffer<byte> alignedBuffer in alignedBuffers)
		{
			alignedBuffer.Dispose();
		}
		alignedBuffers.Clear();
	}

	private void OnNewTrajectorySample(object? sender, EventArgs<GnssTime> e)
	{
		EventArgs<GnssTime> e2 = e;
		if (base.SimulationState <= SimulationState.Ready)
		{
			return;
		}
		if (base.SimulationState == SimulationState.Initializing)
		{
			if (!initializationStarted)
			{
				initializationStarted = true;
				Task.Run(delegate
				{
					EmptyRun(e2.Parameter);
				});
			}
		}
		else if (base.SimulationState == SimulationState.Running)
		{
			GnssTime start = e2.Parameter - 3 * base.SimulationParameters.Trajectory.SampleSpan;
			Range<GnssTime, GnssTimeSpan> sliceInterval = new Range<GnssTime, GnssTimeSpan>(start, base.SimulationParameters.Trajectory.SampleSpan);
			IReadOnlyList<Pvt> trajectorySlice = GetTrajectorySlice(in sliceInterval);
			creationQueue.Enqueue((sliceInterval, trajectorySlice));
			OnPropertyChanged("VisibleSats");
			GnssTime time = sliceInterval.End;
			OnProgressChanged(in time);
		}
	}

	private void EmptyRun(GnssTime trajectoryTime)
	{
		GnssTime start = trajectoryTime - 3 * base.SimulationParameters.Trajectory.SampleSpan;
		for (int i = 0; i < dummySlices!.Length; i++)
		{
			Range<GnssTime, GnssTimeSpan> sliceInterval = new Range<GnssTime, GnssTimeSpan>(start, base.SimulationParameters.Trajectory.SampleSpan);
			IReadOnlyList<Pvt> trajectorySlice = GetTrajectorySlice(in sliceInterval);
			SimulationSlice simulationSlice = CreateSlice(in sliceInterval, trajectorySlice);
			ProcessSlice(simulationSlice);
			dummySlices[i] = simulationSlice;
			start += base.SimulationParameters.Trajectory.SampleSpan;
		}
		base.SimulationState = SimulationState.Running;
	}

	private void CreateSlices()
	{
		Thread.CurrentThread.Name = "Slice Creator RT";
		Thread.CurrentThread.IsBackground = true;
		Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
		while (base.SimulationState < SimulationState.Cancelling)
		{
			(Range<GnssTime, GnssTimeSpan>, IReadOnlyList<Pvt>) result;
			while (!creationQueue.TryDequeue(out result) && base.SimulationState < SimulationState.Cancelling)
			{
				Thread.Yield();
			}
			if (base.SimulationState < SimulationState.Cancelling)
			{
				SimulationSlice item = CreateSlice(in result.Item1, result.Item2);
				processingQueue.Enqueue(item);
				continue;
			}
			break;
		}
	}

	private SimulationSlice CreateSlice(in Range<GnssTime, GnssTimeSpan> interval, IReadOnlyList<Pvt> trajectorySamples)
	{
		Dictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>> dictionary = new Dictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>>();
		foreach (ConstellationBase constellation in base.SimulationParameters.Constellations)
		{
			visibleSats[constellation.ConstellationType] = constellation.GetVisibleSats(trajectorySamples, base.SimulationParameters, out var observables);
			dictionary[constellation.ConstellationType] = observables;
		}
		Simulation.ApplySatCountLimit(visibleSats, base.SimulationParameters);
		SimulationSlice simulationSlice = new SimulationSlice(ModulationBank, in interval);
		foreach (Channel item in base.ChannelsInUse.AsParallel().WithDegreeOfParallelism(base.ChannelsInUse.Count).WithExecutionMode(ParallelExecutionMode.ForceParallelism))
		{
			GeneratorParams parameters = ConstellationBase.CreateSignalGeneratorParameters(ModulationBank, base.SimulationParameters, item, in interval, dictionary, visibleSats, 1);
			Memory<byte> result;
			while (!bufferDictionary[item].TryDequeue(out result))
			{
			}
			simulationSlice.CreateGenerator(ModulationBank, item, in result, parameters, base.SimulationParameters);
			ApplyAGC(item, simulationSlice, base.SimulationParameters);
		}
		return simulationSlice;
	}

	private void ProcessSlices()
	{
		Thread.CurrentThread.Name = "Slice Processor RT";
		Thread.CurrentThread.IsBackground = true;
		while (base.SimulationState < SimulationState.Cancelling)
		{
			SimulationSlice result;
			while (!processingQueue.TryDequeue(out result) && base.SimulationState < SimulationState.Cancelling)
			{
				Thread.Yield();
			}
			if (base.SimulationState < SimulationState.Cancelling)
			{
				ProcessSlice(result);
				outputQueue.Enqueue(result);
				continue;
			}
			break;
		}
	}

	private void ProcessSlice(SimulationSlice slice)
	{
		bool generate;
		using (bufferUnderrunLock.Lock())
		{
			generate = skipSliceProcessingCount == 0;
			if (skipSliceProcessingCount > 0)
			{
				skipSliceProcessingCount--;
			}
			if (muteBufferUnderrunCount > 0)
			{
				muteBufferUnderrunCount--;
			}
		}
		slice.Generate(in generate);
	}

	private void OutputSlices()
	{
		Thread.CurrentThread.Name = "Slice Writer RT";
		Thread.CurrentThread.IsBackground = true;
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		while (base.SimulationState < SimulationState.Cancelling)
		{
			SimulationSlice result;
			while (!outputQueue.TryDequeue(out result) && base.SimulationState < SimulationState.Cancelling)
			{
				Thread.Yield();
			}
			if (base.SimulationState >= SimulationState.Cancelling)
			{
				break;
			}
			if (dummySlices != null)
			{
				SimulationSlice[] array = dummySlices;
				foreach (SimulationSlice slice in array)
				{
					OutputSlice(slice);
				}
				dummySlices = null;
			}
			OutputSlice(result);
		}
	}

	private void OutputSlice(SimulationSlice slice)
	{
		if (!base.SimulationParameters.Output.Write(slice))
		{
			outputErrorOccured = true;
		}
		RecycleSliceBuffers(slice);
	}

	private void RecycleSliceBuffers(SimulationSlice slice)
	{
		foreach (Channel item in base.ChannelsInUse)
		{
			Memory<byte> buffer = slice.GetBuffer(item);
			bufferDictionary[item].Enqueue(buffer);
		}
	}

	protected override void OnCompleted(bool cancelled)
	{
		GCSettings.LatencyMode = GCLatencyMode.Interactive;
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
		base.OnCompleted(cancelled: false);
	}

	protected override void OnBufferUnderrun(object? sender, TimeSpan delay)
	{
		if (base.ProgressEstimator.CurrentIndex < 4)
		{
			return;
		}
		if (delay > TimeSpan.Zero && base.SimulationParameters.Trajectory is ILiveTrajectory liveTrajectory)
		{
			liveTrajectory.AdvanceSampleClock(1.5 * delay);
		}
		using (bufferUnderrunLock.Lock())
		{
			if (skipSliceProcessingCount > 0)
			{
				return;
			}
			if (muteBufferUnderrunCount > 0)
			{
				skipSliceProcessingCount = 1;
				return;
			}
			muteBufferUnderrunCount = 2;
		}
		if (base.SimulationParameters.SatCountLimitMode == SatCountLimitMode.Automatic)
		{
			int num = (from s in (from kvp in base.VisibleSats.ToArray()
					where base.SimulationParameters.Constellations.First((ConstellationBase c) => c.ConstellationType == kvp.Key).IsEnabled
					select kvp.Value).SelectMany((IEnumerable<SatelliteBase> ss) => ss)
				where s.IsEnabled
				select s).Count();
			int num2 = num / 30 + 1;
			if (num < base.SimulationParameters.AutomaticSatCountLimit - num2 - 1)
			{
				base.SimulationParameters.AutomaticSatCountLimit = num + num2;
			}
			else if (base.SimulationParameters.AutomaticSatCountLimit > 0)
			{
				base.SimulationParameters.AutomaticSatCountLimit--;
			}
		}
		base.OnBufferUnderrun(sender, delay);
	}

	private void OnLiveOutputPlaybackStarted(object? sender, EventArgs e)
	{
		base.ProgressEstimator.Start();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposedValue)
		{
			return;
		}
		disposedValue = true;
		if (disposing)
		{
			if (base.SimulationParameters.Output is ILiveOutput liveOutput)
			{
				liveOutput.BufferUnderrun -= new EventHandler<TimeSpan>(OnBufferUnderrun);
				liveOutput.PlaybackStarted -= new EventHandler(OnLiveOutputPlaybackStarted);
			}
			ReleaseBuffers();
		}
	}
}
