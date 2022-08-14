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
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

internal sealed class DoubleBufferSimulation : Simulation
{
	private readonly Thread creatorThread;

	private readonly Thread[] processorThreads;

	private readonly Thread outputThread;

	private readonly SemaphoreSlim?[] sliceCreatedSemaphores;

	private readonly SemaphoreSlim?[] sliceProcessedSemaphores;

	private readonly SemaphoreSlim?[] sliceWrittenSemaphores;

	private readonly SimulationSlice?[] slices;

	private ManualResetEventSlim? pauseEventSlim = new ManualResetEventSlim(initialState: true, 0);

	private volatile bool allSlicesCreated;

	private volatile bool allSlicesProcessed;

	private readonly int concurrency;

	private readonly int bufferCount;

	private readonly double timeInProcess;

	private int skipSliceProcessingCount;

	private int muteBufferUnderrunCount;

	private const int lockTimeout = 10000;

	private readonly SyncLock bufferUnderrunLock = new SyncLock("bufferUnderrunLock", 10000);

	private readonly List<AlignedBuffer<byte>> alignedBuffers = new List<AlignedBuffer<byte>>();

	private readonly Dictionary<Channel, ConcurrentQueue<Memory<byte>>> bufferDictionary = new Dictionary<Channel, ConcurrentQueue<Memory<byte>>>();

	private bool isDisposed;

	public override double TimeInProcess
	{
		[DebuggerStepThrough]
		get
		{
			return timeInProcess;
		}
	}

	public override double ProgressOffset
	{
		[DebuggerStepThrough]
		get
		{
			return 0.2;
		}
	}

	private bool NoMoreItemsToProcess
	{
		get
		{
			if (outputErrorOccured)
			{
				return true;
			}
			if (slices.Any((SimulationSlice s) => (s?.State ?? SimulationSliceState.None) <= SimulationSliceState.ProcessingStarted))
			{
				return false;
			}
			if (!allSlicesCreated)
			{
				return false;
			}
			return true;
		}
	}

	private bool NoMoreItemsToWrite
	{
		get
		{
			if (outputErrorOccured)
			{
				return true;
			}
			if (slices.Any((SimulationSlice s) => (s?.State ?? SimulationSliceState.None) <= SimulationSliceState.WritingStarted))
			{
				return false;
			}
			if (!allSlicesProcessed || outputErrorOccured)
			{
				return false;
			}
			return true;
		}
	}

	public DoubleBufferSimulation(SimulationParams simulationParameters): base(simulationParameters)
	{
		SimulationParams simulationParameters2 = simulationParameters;
		//base._002Ector(simulationParameters2);
		DoubleBufferSimulation doubleBufferSimulation = this;
		concurrency = GetConcurrency(simulationParameters2.Output);
		timeInProcess = (double)concurrency * simulationParameters2.SliceLength;
		bufferCount = concurrency;
		if (simulationParameters2.Output is ILiveOutput liveOutput)
		{
			bufferCount += liveOutput.BufferCount;
			liveOutput.BufferUnderrun += new EventHandler<TimeSpan>(OnBufferUnderrun);
			liveOutput.PlaybackStarted += new EventHandler(OnLiveOutputPlaybackStarted);
		}
		slices = new SimulationSlice[concurrency];
		processorThreads = new Thread[concurrency];
		sliceCreatedSemaphores = new SemaphoreSlim[concurrency];
		sliceProcessedSemaphores = new SemaphoreSlim[concurrency];
		sliceWrittenSemaphores = new SemaphoreSlim[concurrency];
		for (int i = 0; i < concurrency; i++)
		{
			processorThreads[i] = new Thread(new ParameterizedThreadStart(ProcessSlice));
			sliceCreatedSemaphores[i] = new SemaphoreSlim(1);
			sliceProcessedSemaphores[i] = new SemaphoreSlim(1);
			sliceWrittenSemaphores[i] = new SemaphoreSlim(1);
		}
		Output output = simulationParameters2.Output;
		double seconds = simulationParameters2.SliceLength;
		int size = output.GetOutputByteCountForInterval(in seconds);
		foreach (Channel item2 in base.ChannelsInUse)
		{
			ConcurrentQueue<Memory<byte>> concurrentQueue = new ConcurrentQueue<Memory<byte>>();
			for (int j = 0; j <= bufferCount; j++)
			{
				AlignedBuffer<byte> item = new AlignedBuffer<byte>(in size);
				alignedBuffers.Add(item);
				concurrentQueue.Enqueue(item.Memory);
			}
			bufferDictionary.Add(item2, concurrentQueue);
		}
		creatorThread = new Thread(new ThreadStart(CreateSlices));
		outputThread = new Thread(new ThreadStart(OutputSlices));
		if (simulationParameters2.Trajectory.IsExternal)
		{
			Task.Run(delegate
			{
				doubleBufferSimulation.ReadVisibleSatellites(simulationParameters2);
			});
		}
		else
		{
			ReadVisibleSatellites(simulationParameters2);
		}
	}

	public override void Start()
	{
		if (base.SimulationState != SimulationState.Ready)
		{
			return;
		}
		base.SimulationState = SimulationState.Running;
		foreach (NavigationDataType item in from ndi in base.SimulationParameters.Signals.SelectMany((Signal s) => s.NavigationDataInfos)
			select ndi.NavigationDataType)
		{
			NavigationData.ClearFEC(item);
		}
		if (base.SimulationParameters.LiveOutput != null)
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
		}
		Trajectory trajectory = base.SimulationParameters.Trajectory;
		if (trajectory.IsExternal)
		{
			while (trajectory.Interval.IsEmpty)
			{
				Thread.Yield();
			}
			base.SimulationParameters.Interval = new Range<GnssTime, GnssTimeSpan>(trajectory.Interval.End, GnssTime.MaxValue);
		}
		SemaphoreSlim[] array = sliceWrittenSemaphores;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Wait();
		}
		creatorThread.Start();
		while (creatorThread.Name == null)
		{
			Thread.Yield();
		}
		for (int j = 0; j < processorThreads.Length; j++)
		{
			processorThreads[j].Start(j);
		}
		while (processorThreads.Any((Thread thread) => thread.Name == null))
		{
			Thread.Yield();
		}
		if (base.SimulationParameters.LiveOutput != null)
		{
			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
		}
		outputThread.Start();
		while (outputThread.Name == null)
		{
			Thread.Yield();
		}
		if (base.SimulationParameters.LiveOutput == null)
		{
			base.ProgressEstimator.Start();
		}
		GnssTime time = base.SimulationParameters.Interval.Start;
		OnProgressChanged(in time);
	}

	public override async void Pause()
	{
		SimulationState simulationState = base.SimulationState;
		if (simulationState < SimulationState.Initializing || simulationState > SimulationState.Running)
		{
			return;
		}
		if (base.SimulationState == SimulationState.Initializing)
		{
			await Task.Run(delegate
			{
				while (base.SimulationState < SimulationState.Running)
				{
					Thread.Sleep(200);
				}
			}).ConfigureAwait(continueOnCapturedContext: false);
		}
		base.SimulationState = SimulationState.Pausing;
		pauseEventSlim?.Reset();
		await Task.Run(delegate
		{
			while (!processorThreads.All((Thread t) => t.ThreadState.HasFlag(System.Threading.ThreadState.WaitSleepJoin) || t.ThreadState.HasFlag(System.Threading.ThreadState.Stopped)))
			{
				Thread.Sleep(200);
			}
		}).ConfigureAwait(continueOnCapturedContext: false);
		if (processorThreads.All((Thread t) => t.ThreadState.HasFlag(System.Threading.ThreadState.Stopped)))
		{
			base.SimulationState = SimulationState.Finished;
		}
		else
		{
			base.SimulationState = SimulationState.Paused;
		}
	}

	public override void Resume()
	{
		if (base.SimulationState == SimulationState.Paused)
		{
			base.SimulationState = SimulationState.Running;
			pauseEventSlim?.Set();
		}
	}

	public override async void Cancel()
	{
		if (!base.IsAlive)
		{
			return;
		}
		base.SimulationState = SimulationState.Cancelling;
		for (int i = 0; i < concurrency; i++)
		{
			sliceCreatedSemaphores[i]?.Release();
			sliceProcessedSemaphores[i]?.Release();
			sliceWrittenSemaphores[i]?.Release();
		}
		pauseEventSlim?.Set();
		await Task.Run(delegate
		{
			creatorThread.Join();
			for (int j = 0; j < processorThreads.Length; j++)
			{
				processorThreads[j].Join();
			}
			outputThread.Join();
		}).ConfigureAwait(continueOnCapturedContext: false);
		bool cancelled = !NoMoreItemsToWrite || outputErrorOccured;
		ReleaseBuffers();
		OnCompleted(cancelled);
	}

	private void CreateSlices()
	{
		SemaphoreSlim[] array = sliceCreatedSemaphores;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Wait();
		}
		Thread.CurrentThread.Name = "Slice Creator";
		Thread.CurrentThread.IsBackground = true;
		Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
		Trajectory trajectory = base.SimulationParameters.Trajectory;
		if (trajectory.IsExternal)
		{
			GnssTime end = trajectory.Interval.End;
			while (trajectory.Interval.End == end)
			{
				Thread.Yield();
			}
			int num = (int)(0.1 * (double)trajectory.SampleRate).SafeCeiling();
			int num2 = 4 + num;
			GnssTime value = trajectory.Interval.End - num2 * trajectory.SampleSpan;
			base.SimulationParameters.Interval = new Range<GnssTime, GnssTimeSpan>(value, base.SimulationParameters.Interval.End);
		}
		int num3 = 0;
		GnssTime start = base.SimulationParameters.Interval.Start;
		GnssTime end2 = base.SimulationParameters.Interval.End;
		GnssTimeSpan gnssTimeSpan = GnssTimeSpan.FromSeconds(base.SimulationParameters.SliceLength);
		int num4 = 0;
		while (base.SimulationState < SimulationState.Cancelling && start < end2)
		{
			GnssTimeSpan gnssTimeSpan2 = ((start + gnssTimeSpan > end2) ? (end2 - start) : gnssTimeSpan);
			Range<GnssTime, GnssTimeSpan> sliceInterval = new Range<GnssTime, GnssTimeSpan>(start, gnssTimeSpan2);
			SimulationSlice simulationSlice = CreateSlice(in sliceInterval);
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			sliceWrittenSemaphores[num4]?.Wait();
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			if (outputErrorOccured)
			{
				break;
			}
			slices[num4] = simulationSlice;
			sliceCreatedSemaphores[num4]?.Release();
			if (num3 < bufferCount)
			{
				Thread.CurrentThread.Priority = ThreadPriority.Highest;
				Thread.Sleep((int)((((base.SimulationParameters.LiveOutput == null) ? 0.1 : 0.3) * gnssTimeSpan2).Seconds * 1000.0));
				Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			}
			num3++;
			start += gnssTimeSpan;
			num4 = ++num4 % concurrency;
		}
		allSlicesCreated = true;
		array = sliceCreatedSemaphores;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Release();
		}
	}

	private SimulationSlice CreateSlice(in Range<GnssTime, GnssTimeSpan> sliceInterval)
	{
		IReadOnlyList<Pvt> trajectorySlice = GetTrajectorySlice(in sliceInterval);
		Dictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>> dictionary = new Dictionary<ConstellationType, IReadOnlyList<IReadOnlyList<Observation>>>();
		foreach (ConstellationBase constellation in base.SimulationParameters.Constellations)
		{
			visibleSats[constellation.ConstellationType] = constellation.GetVisibleSats(trajectorySlice, base.SimulationParameters, out var observables);
			dictionary[constellation.ConstellationType] = observables;
		}
		Simulation.ApplySatCountLimit(visibleSats, base.SimulationParameters);
		SimulationSlice simulationSlice = new SimulationSlice(ModulationBank, in sliceInterval);
		foreach (Channel item in base.ChannelsInUse.AsParallel().WithDegreeOfParallelism(base.ChannelsInUse.Count).WithExecutionMode(ParallelExecutionMode.ForceParallelism))
		{
			GeneratorParams parameters = ConstellationBase.CreateSignalGeneratorParameters(ModulationBank, base.SimulationParameters, item, in sliceInterval, dictionary, visibleSats, bufferCount);
			Memory<byte> result;
			while (!bufferDictionary[item].TryDequeue(out result))
			{
			}
			simulationSlice.CreateGenerator(ModulationBank, item, in result, parameters, base.SimulationParameters);
			ApplyAGC(item, simulationSlice, base.SimulationParameters);
		}
		OnPropertyChanged("VisibleSats");
		return simulationSlice;
	}

	private void ProcessSlice(object? indexParameter)
	{
		int num = (int)(indexParameter ?? ((object)0));
		sliceProcessedSemaphores[num]?.Wait();
		Thread.CurrentThread.Name = "Slice Processor " + num;
		Thread.CurrentThread.IsBackground = true;
		Thread.CurrentThread.Priority = ThreadPriority.Normal;
		while (base.SimulationState < SimulationState.Cancelling && !NoMoreItemsToProcess)
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			sliceCreatedSemaphores[num]?.Wait();
			pauseEventSlim?.Wait();
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			SimulationSlice simulationSlice = slices[num];
			if (base.SimulationState >= SimulationState.Cancelling || NoMoreItemsToProcess || simulationSlice == null || simulationSlice.State != SimulationSliceState.Ready)
			{
				break;
			}
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
			simulationSlice.Generate(in generate);
			sliceProcessedSemaphores[num]?.Release();
		}
		if (allSlicesCreated && NoMoreItemsToProcess)
		{
			allSlicesProcessed = true;
		}
		sliceProcessedSemaphores[num]?.Release();
	}

	private void OutputSlices()
	{
		SemaphoreSlim[] array = sliceWrittenSemaphores;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Release();
		}
		Thread.CurrentThread.Name = "Slice Writer";
		Thread.CurrentThread.IsBackground = true;
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		int num = 0;
		while (base.SimulationState < SimulationState.Cancelling && !NoMoreItemsToWrite)
		{
			sliceProcessedSemaphores[num]?.Wait();
			SimulationSlice simulationSlice = slices[num];
			if (base.SimulationState >= SimulationState.Cancelling || NoMoreItemsToWrite || simulationSlice == null || simulationSlice.State != SimulationSliceState.ProcessingFinished)
			{
				break;
			}
			if (!base.SimulationParameters.Output.Write(simulationSlice))
			{
				outputErrorOccured = true;
				break;
			}
			foreach (Channel item in base.ChannelsInUse)
			{
				Memory<byte> buffer = simulationSlice.GetBuffer(item);
				bufferDictionary[item].Enqueue(buffer);
			}
			sliceWrittenSemaphores[num]?.Release();
			GnssTime time = simulationSlice.Interval.End;
			OnProgressChanged(in time);
			num = (num + 1) % concurrency;
		}
		base.SimulationParameters.Output.Close();
		array = sliceWrittenSemaphores;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Release();
		}
		creatorThread.Join();
		for (int j = 0; j < processorThreads.Length; j++)
		{
			processorThreads[j].Join();
		}
		bool cancelled = (base.SimulationState == SimulationState.Cancelling && !NoMoreItemsToWrite) || outputErrorOccured;
		ReleaseBuffers();
		OnCompleted(cancelled);
	}

	private void OnLiveOutputPlaybackStarted(object? sender, EventArgs e)
	{
		base.ProgressEstimator.Start();
	}

	protected override void OnBufferUnderrun(object? sender, TimeSpan delay)
	{
		if (base.ProgressEstimator.CurrentIndex < concurrency << 2)
		{
			return;
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
			skipSliceProcessingCount = concurrency;
			muteBufferUnderrunCount = skipSliceProcessingCount << 1;
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

	protected override void OnCompleted(bool cancelled)
	{
		GCSettings.LatencyMode = GCLatencyMode.Interactive;
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Normal;
		base.OnCompleted(cancelled);
	}

	public static int GetConcurrency(Output? output)
	{
		if (!(output is ILiveOutput))
		{
			return 3;
		}
		return 2;
	}

	private void ReleaseBuffers()
	{
		bufferDictionary.Clear();
		foreach (AlignedBuffer<byte> alignedBuffer in alignedBuffers)
		{
			alignedBuffer.Dispose();
		}
		alignedBuffers.Clear();
		for (int i = 0; i < slices.Length; i++)
		{
			slices[i]?.Dispose();
			slices[i] = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (isDisposed)
		{
			return;
		}
		isDisposed = true;
		if (disposing)
		{
			pauseEventSlim?.Dispose();
			pauseEventSlim = null;
			for (int i = 0; i < concurrency; i++)
			{
				SemaphoreSlim? obj = sliceCreatedSemaphores[i];
				sliceCreatedSemaphores[i] = null;
				obj?.Dispose();
				SemaphoreSlim? obj2 = sliceProcessedSemaphores[i];
				sliceProcessedSemaphores[i] = null;
				obj2?.Dispose();
				SemaphoreSlim? obj3 = sliceWrittenSemaphores[i];
				sliceWrittenSemaphores[i] = null;
				obj3?.Dispose();
			}
			if (base.SimulationParameters.Output is ILiveOutput liveOutput)
			{
				liveOutput.BufferUnderrun -= new EventHandler<TimeSpan>(OnBufferUnderrun);
				liveOutput.PlaybackStarted -= new EventHandler(OnLiveOutputPlaybackStarted);
			}
			ReleaseBuffers();
		}
	}
}
