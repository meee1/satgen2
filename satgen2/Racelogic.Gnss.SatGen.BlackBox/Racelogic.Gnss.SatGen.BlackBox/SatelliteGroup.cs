using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Infralution.Localization.Wpf;
using Racelogic.DataTypes;
using Racelogic.Gnss.SatGen.BlackBox.Properties;
using Racelogic.Utilities;
using Racelogic.Utilities.Win;

namespace Racelogic.Gnss.SatGen.BlackBox;

public class SatelliteGroup : Racelogic.Utilities.Win.BasePropertyChanged, IDisposable
{
	private readonly Simulation simulation;

	private readonly ConstellationBase constellation;

	private bool attenuationsLinked;

	private double linkedAttenuation;

	private bool suppressLinkedAttenuationChange;

	private int satelliteCount;

	private bool isDisposed;

	public ObservableCollection<SatelliteEntry> Satellites
	{
		[DebuggerStepThrough]
		get;
	} = new ObservableCollection<SatelliteEntry>();


	public ConstellationType ConstellationType
	{
		[DebuggerStepThrough]
		get
		{
			return constellation.ConstellationType;
		}
	}

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
			if (value)
			{
				LinkedAttenuation = (from s in Satellites
					where s.IsEnabled
					select s.Attenuation).Median();
				foreach (SatelliteEntry satellite in Satellites)
				{
					satellite.Attenuation = LinkedAttenuation;
				}
			}
			OnPropertyChangedUI("AttenuationsLinked");
		}
	}

	public double LinkedAttenuation
	{
		[DebuggerStepThrough]
		get
		{
			return linkedAttenuation;
		}
		[DebuggerStepThrough]
		set
		{
			if (value != linkedAttenuation)
			{
				linkedAttenuation = value;
				OnPropertyChangedUI("LinkedAttenuation");
			}
		}
	}

	public Range<double> AttenuationRange
	{
		[DebuggerStepThrough]
		get;
	} = new Range<double>(-30.0, 0.0);


	public int SatelliteCount
	{
		[DebuggerStepThrough]
		get
		{
			return satelliteCount;
		}
		[DebuggerStepThrough]
		set
		{
			if (value != satelliteCount)
			{
				satelliteCount = value;
				OnPropertyChangedUI("SatelliteCount");
				OnPropertyChangedUI("ConstellationAndSatCountText");
			}
		}
	}

	public bool IsEnabled
	{
		[DebuggerStepThrough]
		get
		{
			return constellation.IsEnabled;
		}
		[DebuggerStepThrough]
		set
		{
			if (value != constellation.IsEnabled)
			{
				constellation.IsEnabled = value;
				OnPropertyChangedUI("IsEnabled");
				OnPropertyChangedUI("ConstellationAndSatCountText");
			}
		}
	}

	public string ConstellationAndSatCountText
	{
		get
		{
			int num = Satellites.Count((SatelliteEntry s) => s.IsActive);
			int num2 = (IsEnabled ? Satellites.Count((SatelliteEntry s) => s.IsEnabled) : 0);
			string text = ResourceEnumConverter.ConvertToString(ConstellationType);
			string text2 = ((num2 == num) ? num.ToString() : string.Format(Resources.SatCountFormat, num2, num));
			return text + "  (" + text2 + ")";
		}
	}

	public SatelliteGroup(Simulation simulation, ConstellationBase constellation)
	{
		this.simulation = simulation;
		this.constellation = constellation;
		simulation.PropertyChanged += OnSimulationPropertyChanged;
		CultureManager.UICultureChanged += OnCultureManagerUICultureChanged;
		UpdateVisibleSatellites(suppressFlashNewSat: true);
	}

	private void OnSimulationPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!(e.PropertyName == "VisibleSats"))
		{
			return;
		}
		Application current = Application.Current;
		if (current == null || current.Dispatcher.CheckAccess())
		{
			UpdateVisibleSatellites();
			return;
		}
		Application current2 = Application.Current;
		if (current2 != null)
		{
			current2.Dispatcher.BeginInvoke(delegate
			{
				UpdateVisibleSatellites();
			});
		}
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	private void UpdateVisibleSatellites(bool suppressFlashNewSat = false)
	{
		IEnumerable<SatelliteBase> source = simulation.VisibleSats[constellation.ConstellationType];
		IEnumerable<SatelliteBase> enumerable = source.Where((SatelliteBase s) => !Satellites.Select((SatelliteEntry ss) => ss.Id).Contains(s.Id));
		List<SatelliteEntry> newSatEntries = new List<SatelliteEntry>();
		foreach (SatelliteBase item in enumerable)
		{
			SatelliteEntry satelliteEntry = new SatelliteEntry(item, this, simulation.SimulationParameters)
			{
				Attenuation = 0.0
			};
			int i;
			for (i = 0; i < Satellites.Count && item.Id > Satellites[i].Id; i++)
			{
			}
			Satellites.Insert(i, satelliteEntry);
			newSatEntries.Add(satelliteEntry);
			satelliteEntry.PropertyChanged += OnSatEntryPropertyChanged;
			satelliteEntry.Deactivated += OnSatEntryDeactivated;
		}
		SatelliteEntry[] array = Satellites.ToArray();
		foreach (SatelliteEntry satEntry in array)
		{
			SatelliteBase satelliteBase = source.FirstOrDefault((SatelliteBase s) => s.Id == satEntry.Id);
			if (satelliteBase != null)
			{
				bool isEnabled = satEntry.IsEnabled;
				satEntry.IsEnabled = satelliteBase.IsEnabled;
				if (satEntry.IsEnabled && !isEnabled)
				{
					satEntry.Attenuation = (attenuationsLinked ? linkedAttenuation : 0.0);
				}
			}
			else
			{
				satEntry.PropertyChanged -= OnSatEntryPropertyChanged;
				satEntry.Deactivate();
			}
		}
		SatelliteCount = (constellation.IsEnabled ? Satellites.Where((SatelliteEntry s) => s.IsEnabled).Count() : 0);
		if (!newSatEntries.Any() || suppressFlashNewSat)
		{
			return;
		}
		Application current2 = Application.Current;
		if (current2 == null)
		{
			return;
		}
		current2.Dispatcher.BeginInvoke(delegate
		{
			foreach (SatelliteEntry item2 in newSatEntries)
			{
				item2.Activate();
			}
		}, DispatcherPriority.Loaded);
	}

	private void OnSatEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!(e.PropertyName == "Attenuation") || !AttenuationsLinked || suppressLinkedAttenuationChange)
		{
			return;
		}
		SatelliteEntry satelliteEntry = (SatelliteEntry)sender;
		if (!satelliteEntry.IsEnabled)
		{
			return;
		}
		suppressLinkedAttenuationChange = true;
		LinkedAttenuation = satelliteEntry.Attenuation;
		foreach (SatelliteEntry item in Satellites.Where((SatelliteEntry s) => s != satelliteEntry && s.IsEnabled))
		{
			item.Attenuation = LinkedAttenuation;
		}
		Application current = Application.Current;
		if (current != null)
		{
			current.Dispatcher.BeginInvoke(delegate
			{
				suppressLinkedAttenuationChange = false;
			}, DispatcherPriority.DataBind);
		}
	}

	private void OnSatEntryDeactivated(object sender, EventArgs e)
	{
		SatelliteEntry satEntry = (SatelliteEntry)sender;
		satEntry.Deactivated -= OnSatEntryDeactivated;
		Application current = Application.Current;
		if (current == null)
		{
			return;
		}
		current.Dispatcher.BeginInvoke(delegate
		{
			Satellites.Remove(satEntry);
			Application current2 = Application.Current;
			if (current2 != null)
			{
				current2.Dispatcher.BeginInvoke(delegate
				{
					OnPropertyChangedUI("ConstellationAndSatCountText");
				}, DispatcherPriority.Normal);
			}
		}, DispatcherPriority.Render);
	}

	private void OnCultureManagerUICultureChanged(object sender, EventArgs e)
	{
		OnPropertyChangedUI("ConstellationAndSatCountText");
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
		if (!disposing)
		{
			return;
		}
		simulation.PropertyChanged -= OnSimulationPropertyChanged;
		foreach (SatelliteEntry satellite in Satellites)
		{
			satellite.PropertyChanged -= OnSatEntryPropertyChanged;
			satellite.Deactivated -= OnSatEntryDeactivated;
		}
	}
}
