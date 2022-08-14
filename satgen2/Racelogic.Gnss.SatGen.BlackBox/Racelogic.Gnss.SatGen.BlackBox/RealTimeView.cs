using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using Racelogic.WPF.Controls;

namespace Racelogic.Gnss.SatGen.BlackBox;

public class RealTimeView : UserControl, IComponentConnector, IStyleConnector
{
	private readonly SimulationViewModel viewModel;

	private const int minSatCount = 4;

	private const double sliderWidth = 46.0;

	private const double defaultHeight = 300.0;

	internal RealTimeView root;

	internal GroupBox StatusGroupBox;

	internal LedGauge StatusLed;

	internal TextBlock BufferUnderrunCountTextBlock;

	internal GroupBox SatLimitGroupBox;

	internal ComboBox SatCountLimitComboBox;

	internal ComboBox SatCountLimitComboBoxGhost;

	internal StackPanel AutomaticLimitStackPanel;

	internal NumericSpinner AutomaticLimitNumericSpinner;

	internal Button ResetButton;

	internal Label GpsLimitLabel;

	internal NumericSpinner GpsLimitNumericSpinner;

	internal Label GalileoLimitLabel;

	internal NumericSpinner GalileoLimitNumericSpinner;

	internal Label GlonassLimitLabel;

	internal NumericSpinner GloLimitNumericSpinner;

	internal Label BeiDouLimitLabel;

	internal NumericSpinner BeiDouLimitNumericSpinner;

	internal Button CancelButton;

	private bool _contentLoaded;

	public RealTimeView(SimulationViewModel viewModel)
	{
		InitializeComponent();
		this.viewModel = viewModel;
	}

	private async void OnCancelButtonClick(object sender, RoutedEventArgs e)
	{
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		CancelButton.Content = "Wait";
		base.IsEnabled = false;
		await Task.Run(delegate
		{
			viewModel.Simulation.Cancel();
		});
		Thread.CurrentThread.Priority = ThreadPriority.Normal;
	}

	private void OnSliderDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.OriginalSource is Path && sender is Slider slider)
		{
			slider.Value = slider.Maximum;
		}
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (constraint.Width == double.PositiveInfinity && constraint.Height == double.PositiveInfinity)
		{
			int num = viewModel.VisibleSatellites.Select((SatelliteGroup c) => c.Satellites.Count).Max();
			if (num < 4)
			{
				num = 4;
			}
			return new Size((double)num * 46.0, 300.0);
		}
		return base.MeasureOverride(constraint);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/Racelogic.Gnss.SatGen.BlackBox;V4.0.0.0;component/realtimeview.xaml", UriKind.Relative);
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
			root = (RealTimeView)target;
			break;
		case 2:
			StatusGroupBox = (GroupBox)target;
			break;
		case 3:
			StatusLed = (LedGauge)target;
			break;
		case 4:
			BufferUnderrunCountTextBlock = (TextBlock)target;
			break;
		case 5:
			SatLimitGroupBox = (GroupBox)target;
			break;
		case 6:
			SatCountLimitComboBox = (ComboBox)target;
			break;
		case 7:
			SatCountLimitComboBoxGhost = (ComboBox)target;
			break;
		case 8:
			AutomaticLimitStackPanel = (StackPanel)target;
			break;
		case 9:
			AutomaticLimitNumericSpinner = (NumericSpinner)target;
			break;
		case 10:
			ResetButton = (Button)target;
			break;
		case 11:
			GpsLimitLabel = (Label)target;
			break;
		case 12:
			GpsLimitNumericSpinner = (NumericSpinner)target;
			break;
		case 13:
			GalileoLimitLabel = (Label)target;
			break;
		case 14:
			GalileoLimitNumericSpinner = (NumericSpinner)target;
			break;
		case 15:
			GlonassLimitLabel = (Label)target;
			break;
		case 16:
			GloLimitNumericSpinner = (NumericSpinner)target;
			break;
		case 17:
			BeiDouLimitLabel = (Label)target;
			break;
		case 18:
			BeiDouLimitNumericSpinner = (NumericSpinner)target;
			break;
		case 19:
			CancelButton = (Button)target;
			CancelButton.Click += OnCancelButtonClick;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.8.1.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IStyleConnector.Connect(int connectionId, object target)
	{
		if (connectionId == 20)
		{
			((Slider)target).MouseDoubleClick += OnSliderDoubleClick;
		}
	}
}
