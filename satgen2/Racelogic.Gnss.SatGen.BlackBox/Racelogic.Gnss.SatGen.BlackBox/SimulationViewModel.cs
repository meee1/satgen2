using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using Racelogic.Geodetics;
using Racelogic.Gnss.SatGen.BlackBox.Properties;
using Racelogic.Maths;
using Racelogic.Utilities;
using Racelogic.Utilities.Win;
using Racelogic.WPF.Utilities;

namespace Racelogic.Gnss.SatGen.BlackBox;

public class SimulationViewModel : Racelogic.Utilities.Win.BasePropertyChanged, IDisposable
{
	private readonly Simulation simulation;

	private readonly ILiveOutput liveOutput;

	private bool closeRequested;

	private double progress;

	private string progressMessage;

	private int lastSimulationProgressWholeSeconds;

	private ConnectionStatus connectionStatus;

	private TaskbarItemProgressState progressState = TaskbarItemProgressState.Normal;

	private readonly SemaphoreSlim bufferUnderrunSemaphore = new SemaphoreSlim(1);

	private const int bufferUnderrunTimeout = 2000;

	private bool firstFewSeconds = true;

	private readonly TimeSpan firstFewSecondsPeriod = TimeSpan.FromSeconds(4.0);

	private int bufferUnderrunCount;

	private readonly SatCountLimitMode[] availableSatCountLimitModes = new SatCountLimitMode[4]
	{
		SatCountLimitMode.Automatic,
		SatCountLimitMode.Constellation,
		SatCountLimitMode.Manual,
		SatCountLimitMode.None
	};

	private readonly SatelliteGroup[] visibleSatellites;

	private bool attenuationsLinked;

	private string title;

	private const string satGenName = "SatGen";

	private const string satGenRealTimeName = "SatGen Real Time";

	private readonly ICommand resetSatCountLimitCommand;

	private bool suppressBufferUnderrunCountReset;

	private static readonly Dispatcher dispatcher;

	private bool isDisposed;

	public Simulation Simulation => simulation;

	public ConnectionStatus ConnectionStatus
	{
		[DebuggerStepThrough]
		get
		{
			return connectionStatus;
		}
		[DebuggerStepThrough]
		set
		{
			connectionStatus = value;
			ProgressState = ConnectionStatusToProgressState(value);
			OnPropertyChangedUI("ConnectionStatus");
		}
	}

	public TaskbarItemProgressState ProgressState
	{
		[DebuggerStepThrough]
		get
		{
			return progressState;
		}
		[DebuggerStepThrough]
		set
		{
			progressState = value;
			OnPropertyChangedUI("ProgressState");
		}
	}

	public double Progress
	{
		[DebuggerStepThrough]
		get
		{
			return progress;
		}
		[DebuggerStepThrough]
		set
		{
			progress = value;
			OnPropertyChangedUI("Progress");
		}
	}

	public string ProgressMessage
	{
		[DebuggerStepThrough]
		get
		{
			return progressMessage;
		}
		[DebuggerStepThrough]
		set
		{
			progressMessage = value;
			OnPropertyChangedUI("ProgressMessage");
		}
	}

	public IEnumerable<SatelliteGroup> VisibleSatellites => visibleSatellites;

	public bool AttenuationsLinked
	{
		[DebuggerStepThrough]
		get
		{
			return attenuationsLinked;
		}
		[DebuggerStepThrough]
		set
		{
			if (value == attenuationsLinked)
			{
				return;
			}
			attenuationsLinked = value;
			if (visibleSatellites != null)
			{
				SatelliteGroup[] array = visibleSatellites;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].AttenuationsLinked = value;
				}
			}
			if (liveOutput != null)
			{
				Settings.Default.LiveAttenuationsLinked = value;
			}
			OnPropertyChangedUI("AttenuationsLinked");
		}
	}

	public string Title
	{
		[DebuggerStepThrough]
		get
		{
			return title;
		}
		[DebuggerStepThrough]
		private set
		{
			title = value;
			OnPropertyChangedUI("Title");
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
		set
		{
			bufferUnderrunCount = value;
			OnPropertyChangedUI("BufferUnderrunCount");
		}
	}

	public IEnumerable<SatCountLimitMode> AvailableSatCountLimitModes
	{
		[DebuggerStepThrough]
		get
		{
			return availableSatCountLimitModes;
		}
	}

	public SatCountLimitMode SatCountLimitMode
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimitMode;
		}
		[DebuggerStepThrough]
		set
		{
			if (value != simulation.SimulationParameters.SatCountLimitMode)
			{
				simulation.SimulationParameters.SatCountLimitMode = value;
				Settings.Default.LiveSatCountLimitMode = (int)value;
				OnPropertyChangedUI("SatCountLimitMode");
			}
		}
	}

	public int AutomaticSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.AutomaticSatCountLimit;
		}
		[DebuggerStepThrough]
		set
		{
			if (!suppressBufferUnderrunCountReset)
			{
				BufferUnderrunCount = 0;
			}
			simulation.SimulationParameters.AutomaticSatCountLimit = value;
		}
	}

	public ICommand ResetSatCountLimit => resetSatCountLimitCommand;

	public int GpsSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimits[ConstellationType.Gps];
		}
		[DebuggerStepThrough]
		set
		{
			if (!simulation.SimulationParameters.SatCountLimits.TryGetValue(ConstellationType.Gps, out var value2))
			{
				value2 = int.MinValue;
			}
			if (value != value2)
			{
				simulation.SimulationParameters.SatCountLimits[ConstellationType.Gps] = value;
				if (liveOutput != null)
				{
					Settings.Default.LiveGpsSatCountLimit = value;
				}
				OnPropertyChangedUI("GpsSatCountLimit");
			}
		}
	}

	public int GlonassSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimits[ConstellationType.Glonass];
		}
		[DebuggerStepThrough]
		set
		{
			if (!simulation.SimulationParameters.SatCountLimits.TryGetValue(ConstellationType.Glonass, out var value2))
			{
				value2 = int.MinValue;
			}
			if (value != value2)
			{
				simulation.SimulationParameters.SatCountLimits[ConstellationType.Glonass] = value;
				if (liveOutput != null)
				{
					Settings.Default.LiveGlonassSatCountLimit = value;
				}
				OnPropertyChangedUI("GlonassSatCountLimit");
			}
		}
	}

	public int BeiDouSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimits[ConstellationType.BeiDou];
		}
		[DebuggerStepThrough]
		set
		{
			if (!simulation.SimulationParameters.SatCountLimits.TryGetValue(ConstellationType.BeiDou, out var value2))
			{
				value2 = int.MinValue;
			}
			if (value != value2)
			{
				simulation.SimulationParameters.SatCountLimits[ConstellationType.BeiDou] = value;
				if (liveOutput != null)
				{
					Settings.Default.LiveBeiDouSatCountLimit = value;
				}
				OnPropertyChangedUI("BeiDouSatCountLimit");
			}
		}
	}

	public int GalileoSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimits[ConstellationType.Galileo];
		}
		[DebuggerStepThrough]
		set
		{
			if (!simulation.SimulationParameters.SatCountLimits.TryGetValue(ConstellationType.Galileo, out var value2))
			{
				value2 = int.MinValue;
			}
			if (value != value2)
			{
				simulation.SimulationParameters.SatCountLimits[ConstellationType.Galileo] = value;
				if (liveOutput != null)
				{
					Settings.Default.LiveGalileoSatCountLimit = value;
				}
				OnPropertyChangedUI("GalileoSatCountLimit");
			}
		}
	}

	public int NavicSatCountLimit
	{
		[DebuggerStepThrough]
		get
		{
			return simulation.SimulationParameters.SatCountLimits[ConstellationType.Navic];
		}
		[DebuggerStepThrough]
		set
		{
			if (!simulation.SimulationParameters.SatCountLimits.TryGetValue(ConstellationType.Navic, out var value2))
			{
				value2 = int.MinValue;
			}
			if (value != value2)
			{
				simulation.SimulationParameters.SatCountLimits[ConstellationType.Navic] = value;
				if (liveOutput != null)
				{
					Settings.Default.LiveNavicSatCountLimit = value;
				}
				OnPropertyChangedUI("NavicSatCountLimit");
			}
		}
	}

	public bool IsGpsPresent => simulation.VisibleSats.ContainsKey(ConstellationType.Gps);

	public bool IsGlonassPresent => simulation.VisibleSats.ContainsKey(ConstellationType.Glonass);

	public bool IsBeiDouPresent => simulation.VisibleSats.ContainsKey(ConstellationType.BeiDou);

	public bool IsGalileoPresent => simulation.VisibleSats.ContainsKey(ConstellationType.Galileo);

	public bool IsNavicPresent => simulation.VisibleSats.ContainsKey(ConstellationType.Navic);

	static SimulationViewModel()
	{
		dispatcher = Application.Current.Dispatcher;
		LocalizedEnumConverter.Add(typeof(SatCountLimitMode));
	}

	public SimulationViewModel(Simulation simulation)
	{
		this.simulation = simulation;
		simulation.SimulationParameters.Output.PropertyChanged += OnOutputPropertyChanged;
		liveOutput = simulation.SimulationParameters.LiveOutput;
		if (liveOutput != null)
		{
			ConnectionStatus = ConnectionStatus.Connected;
			SatCountLimitMode = (SatCountLimitMode)Settings.Default.LiveSatCountLimitMode;
			AutomaticSatCountLimit = Settings.Default.LiveAutomaticSatCountLimit;
			GpsSatCountLimit = Settings.Default.LiveGpsSatCountLimit;
			GlonassSatCountLimit = Settings.Default.LiveGlonassSatCountLimit;
			BeiDouSatCountLimit = Settings.Default.LiveBeiDouSatCountLimit;
			GalileoSatCountLimit = Settings.Default.LiveGalileoSatCountLimit;
			attenuationsLinked = Settings.Default.LiveAttenuationsLinked;
		}
		else
		{
			SatCountLimitMode = SatCountLimitMode.None;
			AutomaticSatCountLimit = int.MaxValue;
			GpsSatCountLimit = int.MaxValue;
			GlonassSatCountLimit = int.MaxValue;
			BeiDouSatCountLimit = int.MaxValue;
			GalileoSatCountLimit = int.MaxValue;
		}
		simulation.BufferUnderrun += new EventHandler<TimeSpan>(OnBufferUnderrun);
		simulation.ProgressChanged += new EventHandler<SimulationProgressChangedEventArgs>(OnProgressChanged);
		simulation.Completed += new EventHandler<SimulationCompletedEventArgs>(OnSimulationCompleted);
		simulation.SimulationParameters.PropertyChanged += OnSimulationParametersPropertyChanged;
		List<SatelliteGroup> list = new List<SatelliteGroup>();
		ConstellationBase constellationBase = simulation.SimulationParameters.Constellations.FirstOrDefault((ConstellationBase c) => c.ConstellationType == ConstellationType.Gps);
		if (constellationBase != null)
		{
			list.Add(new SatelliteGroup(simulation, constellationBase));
		}
		ConstellationBase constellationBase2 = simulation.SimulationParameters.Constellations.FirstOrDefault((ConstellationBase c) => c.ConstellationType == ConstellationType.Galileo);
		if (constellationBase2 != null)
		{
			list.Add(new SatelliteGroup(simulation, constellationBase2));
		}
		ConstellationBase constellationBase3 = simulation.SimulationParameters.Constellations.FirstOrDefault((ConstellationBase c) => c.ConstellationType == ConstellationType.Glonass);
		if (constellationBase3 != null)
		{
			list.Add(new SatelliteGroup(simulation, constellationBase3));
		}
		ConstellationBase constellationBase4 = simulation.SimulationParameters.Constellations.FirstOrDefault((ConstellationBase c) => c.ConstellationType == ConstellationType.BeiDou);
		if (constellationBase4 != null)
		{
			list.Add(new SatelliteGroup(simulation, constellationBase4));
		}
		ConstellationBase constellationBase5 = simulation.SimulationParameters.Constellations.FirstOrDefault((ConstellationBase c) => c.ConstellationType == ConstellationType.Navic);
		if (constellationBase5 != null)
		{
			list.Add(new SatelliteGroup(simulation, constellationBase5));
		}
		visibleSatellites = list.ToArray();
		SatelliteGroup[] array = visibleSatellites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].PropertyChanged += OnSatelliteGroupPropertyChanged;
		}
		attenuationsLinked = !attenuationsLinked;
		AttenuationsLinked = !attenuationsLinked;
		resetSatCountLimitCommand = new RelayCommand(delegate
		{
			AutomaticSatCountLimit = 999;
			BufferUnderrunCount = 0;
		}, (object p) => SatCountLimitMode == SatCountLimitMode.Automatic);
	}

	public void CancelSimulation()
	{
		closeRequested = true;
		simulation.Cancel();
	}

	private void OnSatelliteGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "SatelliteCount")
		{
			UpdateTitle(lastSimulationProgressWholeSeconds);
		}
	}

	private void OnProgressChanged(object sender, SimulationProgressChangedEventArgs e)
	{
		if (closeRequested)
		{
			Environment.Exit(0);
		}
		double num = e.SimulatedTimeFromStart.Seconds;
		Progress = e.Progress;
		if (liveOutput != null)
		{
			num -= simulation.TimeInProcess + simulation.ProgressOffset;
		}
		int num2 = (int)num.SafeFloor();
		if (num2 > lastSimulationProgressWholeSeconds)
		{
			lastSimulationProgressWholeSeconds = num2;
			if (e.TimeLeft == TimeSpan.Zero)
			{
				ProgressMessage = $"{e.Progress:P1} processed";
			}
			else if (e.TimeLeft.Days == 0)
			{
				ProgressMessage = $"{e.Progress:P1} processed, {e.TimeLeft:hh\\:mm\\:ss} until completion";
			}
			else
			{
				ProgressMessage = $"{e.Progress:P1} processed, {e.TimeLeft.Days:d} days {e.TimeLeft:hh\\:mm\\:ss} until completion";
			}
			UpdateTitle(num2);
			if (e.ElapsedTime >= firstFewSecondsPeriod)
			{
				firstFewSeconds = false;
			}
		}
	}

	private void UpdateTitle(double simulationProgressSeconds)
	{
		int num = visibleSatellites.Select((SatelliteGroup g) => g.SatelliteCount).Sum();
		GnssTime gnssTime = simulation.SimulationParameters.Interval.Start + GnssTimeSpan.FromSeconds(simulationProgressSeconds);
		string text = ((liveOutput == null) ? "SatGen" : "SatGen Real Time");
		TimeSpan timeSpan = TimeSpan.FromSeconds(simulationProgressSeconds);
		Title = $"{text} - {num} satellites - {gnssTime} UTC  (+{timeSpan})";
	}

	private void OnSimulationParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "AutomaticSatCountLimit")
		{
			if (liveOutput != null)
			{
				Settings.Default.LiveAutomaticSatCountLimit = simulation.SimulationParameters.AutomaticSatCountLimit;
			}
			dispatcher.Invoke(delegate
			{
				suppressBufferUnderrunCountReset = true;
				OnPropertyChangedUI("AutomaticSatCountLimit");
				suppressBufferUnderrunCountReset = false;
			}, DispatcherPriority.Normal);
		}
	}

	private async void OnBufferUnderrun(object sender, TimeSpan delay)
	{
		if (!firstFewSeconds && bufferUnderrunSemaphore.Wait(0))
		{
			ConnectionStatus = ConnectionStatus.BufferUnderrun;
			BufferUnderrunCount++;
			await Task.Delay(2000);
			if (liveOutput == null)
			{
				ConnectionStatus = ConnectionStatus.None;
			}
			else
			{
				ConnectionStatus = ((!liveOutput.IsAlive) ? ConnectionStatus.Connected : ConnectionStatus.Transmitting);
			}
			bufferUnderrunSemaphore?.Release();
		}
	}

	private void OnConnectionLost(object sender, EventArgs e)
	{
		RLLogger.GetLogger().LogMessage("Setting ConnectionStatus to ConnectionLost");
		ConnectionStatus = ConnectionStatus.ConnectionLost;
	}

	private void OnOutputPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "IsAlive")
		{
			if (((ILiveOutput)sender).IsAlive)
			{
				ConnectionStatus = ConnectionStatus.Transmitting;
			}
			else
			{
				ConnectionStatus = ConnectionStatus.Connected;
			}
		}
	}

	private static TaskbarItemProgressState ConnectionStatusToProgressState(ConnectionStatus status)
	{
		return status switch
		{
			ConnectionStatus.Connected => TaskbarItemProgressState.Paused, 
			ConnectionStatus.BufferUnderrun => TaskbarItemProgressState.Error, 
			ConnectionStatus.Transmitting => TaskbarItemProgressState.Normal, 
			_ => TaskbarItemProgressState.None, 
		};
	}

	private void OnSimulationCompleted(object sender, SimulationCompletedEventArgs e)
	{
		UnsubscribeSimulationEvents();
	}

	private void UnsubscribeSimulationEvents()
	{
		simulation.BufferUnderrun -= new EventHandler<TimeSpan>(OnBufferUnderrun);
		simulation.ProgressChanged -= new EventHandler<SimulationProgressChangedEventArgs>(OnProgressChanged);
		simulation.Completed -= new EventHandler<SimulationCompletedEventArgs>(OnSimulationCompleted);
		simulation.SimulationParameters.PropertyChanged -= OnSimulationParametersPropertyChanged;
		simulation.SimulationParameters.Output.PropertyChanged -= OnOutputPropertyChanged;
		SatelliteGroup[] array = visibleSatellites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].PropertyChanged -= OnSatelliteGroupPropertyChanged;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				bufferUnderrunSemaphore?.Dispose();
				UnsubscribeSimulationEvents();
			}
		}
	}
}
