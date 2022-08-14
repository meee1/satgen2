using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.LabSat;
using Racelogic.Gnss.SatGen.BlackBox.Properties;
using Racelogic.Utilities;
using Racelogic.WPF.Utilities;

namespace Racelogic.Gnss.SatGen.BlackBox;

public class MainWindow : Window, IDisposable, IComponentConnector
{
	private readonly SimulationViewModel viewModel;

	private const double trueTimeStartDelay = 10.0;

	private const double defaultHorizontalMargin = 20.0;

	private const double defaultVerticalMargin = 38.0;

	private const double clientAreaMargin = 2.0;

	private const int ticksPerSecond = 10000000;

	private readonly Size preferredViewSize;

	private readonly Simulation simulation;

	private readonly ILiveOutput liveOutput;

	private readonly SemaphoreSlim taskbarItemSemaphore = new SemaphoreSlim(1);

	private bool isDisposed;

	internal MainWindow root;

	internal Grid MainGrid;

	private bool _contentLoaded;

	public MainWindow()
	{
		InitializeComponent();
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
		TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);
		Application.Current.SessionEnding += OnSessionEnding;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs.Length != 2)
		{
			Environment.Exit(-2);
		}
		if (!File.Exists(commandLineArgs[1]))
		{
			Environment.Exit(-3);
		}
		ConfigFile config = ConfigFile.Read(commandLineArgs[1]);
		TrajectorySource trajectorySource = (Key.LeftShift.IsDownAsync() ? TrajectorySource.Joystick : TrajectorySource.NmeaFile);
		Output output = null;
		try
		{
			output = GetOutput(config);
		}
		catch (LabSatException ex)
		{
			MessageBox.Show(this, ex.Message, "Error");
			Application.Current.Shutdown();
			return;
		}
		output.Error += new EventHandler<ErrorEventArgs>(OnOutputError);
		liveOutput = output as ILiveOutput;
		simulation = GetSimulator(config, output, trajectorySource);
		if (simulation == null)
		{
			Environment.Exit(-4);
			return;
		}
		viewModel = new SimulationViewModel(simulation);
		base.DataContext = viewModel;
		Control control = ((liveOutput == null) ? ((Control)new DefaultView(viewModel)) : ((Control)new RealTimeView(viewModel)));
		control.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		preferredViewSize = control.DesiredSize;
		if (preferredViewSize.Width < control.MinWidth)
		{
			preferredViewSize.Width = control.MinWidth;
		}
		if (preferredViewSize.Height < control.MinHeight)
		{
			preferredViewSize.Height = control.MinHeight;
		}
		base.MinWidth = control.MinWidth + 20.0;
		base.MinHeight = control.MinHeight + 38.0;
		base.Width = preferredViewSize.Width + 20.0;
		base.Height = preferredViewSize.Height + 38.0;
		MainGrid.Children.Add(control);
		SystemUtils.SetThreadExecutionMode(ThreadExecutionModes.KeepSystemAwake);
		simulation.Completed += new EventHandler<SimulationCompletedEventArgs>(OnSimulationCompleted);
		base.Loaded += OnLoaded;
	}

	private static Simulation GetSimulator(ConfigFile config, Output output, TrajectorySource trajectorySource)
	{
		if (!File.Exists(config.NmeaFile))
		{
			throw new ArgumentException("NMEA file does not exist: " + config.NmeaFile, "config");
		}
		GnssTime startTime = ((!(output is ILiveOutput liveOutput) || !liveOutput.TrueTimeStart.HasValue) ? GnssTime.FromUtc(config.Date) : (liveOutput.TrueTimeStart.Value - GnssTimeSpan.FromSeconds(1)));
		Trajectory trajectory = ((trajectorySource != TrajectorySource.Joystick) ? new NmeaFileTrajectory(in startTime, config.NmeaFile, config.GravitationalModel) : new JoystickTrajectory(throttleSlider: Key.Z.IsDownAsync() ? JoystickSlider.ZAxis : (Key.D2.IsDownAsync() ? JoystickSlider.Slider2 : JoystickSlider.Slider1), startTime: in startTime, nmeaFileName: config.NmeaFile, gravitationalModel: config.GravitationalModel));
		Range<GnssTime, GnssTimeSpan> interval = trajectory.Interval;
		if (interval.Width.Seconds < 1.0)
		{
			string text = trajectory.ErrorMessage;
			if (string.IsNullOrWhiteSpace(text))
			{
				text = "Trajectory is shorter than one second";
			}
			RLLogger.GetLogger().LogMessage(text);
			MessageBox.Show(Application.Current.MainWindow, text, "SatGen error", MessageBoxButton.OK, MessageBoxImage.Hand);
			return null;
		}
		IReadOnlyList<ConstellationBase> readOnlyList = ConstellationBase.Create(config.SignalTypes);
		foreach (ConstellationBase item in readOnlyList)
		{
			string almanacPath = GetAlmanacPath(item.ConstellationType, config);
			if (!item.LoadAlmanac(almanacPath, in startTime))
			{
				string text2 = "Invalid " + item.ConstellationType.ToLongName() + " almanac file \"" + Path.GetFileName(almanacPath) + "\"";
				RLLogger.GetLogger().LogMessage(text2);
				MessageBox.Show(Application.Current.MainWindow, text2, "SatGen error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return null;
			}
			AlmanacBase? almanac = item.Almanac;
			GnssTime simulationTime = interval.Start;
			almanac!.UpdateAlmanacForTime(in simulationTime);
		}
		return Simulation.Create(new SimulationParams(config.SignalTypes, trajectory, in interval, output, readOnlyList, config.Mask, config.Attenuation));
	}

	private static string GetAlmanacPath(ConstellationType constellationType, ConfigFile config)
	{
		return constellationType switch
		{
			ConstellationType.Glonass => config.GlonassAlmanacFile, 
			ConstellationType.BeiDou => config.BeiDouAlmanacFile, 
			ConstellationType.Galileo => config.GalileoAlmanacFile, 
			ConstellationType.Navic => config.NavicAlmanacFile, 
			_ => config.GpsAlmanacFile, 
		};
	}

	private static Output GetOutput(ConfigFile config)
	{
		string text = config.OutputFile.ToLower();
		string text2 = Path.GetExtension(text)!.ToLowerInvariant();
		if (text2 == ".bin")
		{
			return new LabSat1Output(config.OutputFile);
		}
		Quantization bitsPerSample = (Quantization)config.BitsPerSample;
		if (text2 == ".ls3w")
		{
			return new LabSat3wOutput(config.OutputFile, config.SignalTypes, bitsPerSample);
		}
		if (text2 == ".ls2")
		{
			return new LabSat2Output(config.OutputFile, config.SignalTypes, bitsPerSample);
		}
		if (text == "%labsat2%")
		{
			IReadOnlyList<SignalType> signalTypes = config.SignalTypes;
			bool isLowLatency = true;
			bool handleBufferUnderrun = true;
			return new LabSat2LiveOutput(signalTypes, bitsPerSample, in isLowLatency, null, in handleBufferUnderrun);
		}
		if (text == "%labsat2rt%")
		{
			DateTime utcNow = DateTime.UtcNow;
			GnssTime value = GnssTime.FromUtc(new DateTime(utcNow.Ticks - utcNow.Ticks % 10000000) + TimeSpan.FromSeconds(10.0));
			IReadOnlyList<SignalType> signalTypes2 = config.SignalTypes;
			bool isLowLatency = true;
			GnssTime? trueTimeStart = value;
			bool handleBufferUnderrun = true;
			return new LabSat2LiveOutput(signalTypes2, bitsPerSample, in isLowLatency, trueTimeStart, in handleBufferUnderrun);
		}
		if (text2 == ".ls3")
		{
			return new LabSat3Output(config.OutputFile, config.SignalTypes, bitsPerSample);
		}
		throw new ArgumentException("No supported output type for file \"" + config.OutputFile + "\"", "config");
	}

	private void OnSimulationCompleted(object sender, SimulationCompletedEventArgs e)
	{
		simulation.Completed -= new EventHandler<SimulationCompletedEventArgs>(OnSimulationCompleted);
		simulation.SimulationParameters.Output.Error -= new EventHandler<ErrorEventArgs>(OnOutputError);
		AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
		TaskScheduler.UnobservedTaskException -= new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);
		if (simulation.SimulationParameters.LiveOutput != null)
		{
			Settings.Default.Save();
		}
		simulation.Dispose();
		SystemUtils.SetThreadExecutionMode();
		if (e.Cancelled)
		{
			Environment.Exit(1);
		}
		Environment.Exit(0);
	}

	private void OnOutputError(object sender, ErrorEventArgs e)
	{
		base.Dispatcher.BeginInvoke(delegate
		{
			MessageBox.Show(this, "Error writing output file. Disk full?", "SatGen error", MessageBoxButton.OK, MessageBoxImage.Hand);
			simulation.Completed -= new EventHandler<SimulationCompletedEventArgs>(OnSimulationCompleted);
			simulation.SimulationParameters.Output.Error -= new EventHandler<ErrorEventArgs>(OnOutputError);
			simulation.Dispose();
			AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
			TaskScheduler.UnobservedTaskException -= new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);
			SystemUtils.SetThreadExecutionMode();
			Environment.Exit(-5);
		}, DispatcherPriority.Input);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		base.OnClosing(e);
		if (simulation?.SimulationParameters.LiveOutput != null)
		{
			Settings.Default.Save();
		}
		Simulation obj = simulation;
		if (obj != null && obj.IsAlive)
		{
			e.Cancel = true;
			base.IsEnabled = false;
			base.Dispatcher.BeginInvoke(delegate
			{
				viewModel.CancelSimulation();
			}, DispatcherPriority.Input);
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		base.Loaded -= OnLoaded;
		double actualWidth = MainGrid.ActualWidth;
		double actualHeight = MainGrid.ActualHeight;
		base.Width += preferredViewSize.Width - actualWidth + 4.0;
		base.Height += preferredViewSize.Height - actualHeight + 4.0;
		Activate();
		base.Topmost = false;
		base.Dispatcher.BeginInvoke(delegate
		{
			if (simulation.SimulationState == SimulationState.Ready)
			{
				simulation.Start();
			}
			else
			{
				Task.Run(delegate
				{
					while (simulation.SimulationState < SimulationState.Ready)
					{
						Thread.Sleep(10);
					}
					simulation.Start();
				});
			}
		}, DispatcherPriority.SystemIdle);
	}

	private void OnWindowStateChanged(object sender, EventArgs e)
	{
		if (base.WindowState == WindowState.Maximized)
		{
			base.WindowState = WindowState.Normal;
		}
	}

	private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
	{
		Simulation obj = simulation;
		if (obj != null && obj.IsAlive)
		{
			simulation.Cancel();
			if (simulation.IsAlive)
			{
				e.Cancel = true;
			}
		}
	}

	private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Environment.Exit(-6);
	}

	private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		Environment.Exit(-7);
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
				(liveOutput as Output)?.Dispose();
				taskbarItemSemaphore?.Dispose();
				viewModel?.Dispose();
				simulation?.Dispose();
			}
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/Racelogic.Gnss.SatGen.BlackBox;V4.0.0.0;component/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			root = (MainWindow)target;
			root.StateChanged += OnWindowStateChanged;
			break;
		case 2:
			MainGrid = (Grid)target;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
