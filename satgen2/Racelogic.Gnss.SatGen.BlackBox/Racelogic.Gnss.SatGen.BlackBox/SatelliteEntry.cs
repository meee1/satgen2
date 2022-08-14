using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Racelogic.Maths;
using Racelogic.Utilities.Win;

namespace Racelogic.Gnss.SatGen.BlackBox;

public sealed class SatelliteEntry : BasePropertyChanged
{
	private readonly SatelliteBase satellite;

	private readonly SatelliteGroup group;

	private readonly SimulationParams simulationParameters;

	private readonly SignalType firstSignalType;

	private double level = 1.0;

	private int attenuation;

	private bool isActive = true;

	public int Id
	{
		[DebuggerStepThrough]
		get
		{
			return satellite.Id;
		}
	}

	public int Index
	{
		[DebuggerStepThrough]
		get
		{
			return satellite.Index;
		}
	}

	public string ConstellationShortName
	{
		[DebuggerStepThrough]
		get
		{
			return satellite.ConstellationType.ToShortName();
		}
	}

	private double Level
	{
		[DebuggerStepThrough]
		get
		{
			return level;
		}
		[DebuggerStepThrough]
		set
		{
			if (value != level && IsActive)
			{
				level = Math.Clamp(value, 0.0, 1.0);
				if (firstSignalType != SignalType.None)
				{
					simulationParameters.SignalLevels[firstSignalType][satellite.Index] = value;
				}
				Attenuation = level.LevelToGain();
				OnPropertyChangedUI("Level");
			}
		}
	}

	public double Attenuation
	{
		[DebuggerStepThrough]
		get
		{
			if (!IsEnabled)
			{
				return double.NegativeInfinity;
			}
			return attenuation;
		}
		[DebuggerStepThrough]
		set
		{
			int num = (int)Math.Round(value);
			if (num != attenuation && IsActive)
			{
				attenuation = num;
				Level = FastMath.GainToLevel(attenuation);
				OnPropertyChangedUI("Attenuation");
				OnPropertyChangedUI("AttenuationText");
			}
		}
	}

	public string AttenuationText
	{
		[DebuggerStepThrough]
		get
		{
			if (!IsEnabled)
			{
				return "-∞ dB";
			}
			return $"{Attenuation} dB";
		}
	}

	public bool IsEnabled
	{
		[DebuggerStepThrough]
		get
		{
			if (satellite.IsEnabled)
			{
				return IsActive;
			}
			return false;
		}
		[DebuggerStepThrough]
		set
		{
			if (IsActive)
			{
				satellite.IsEnabled = value;
				if (value && group.AttenuationsLinked)
				{
					Attenuation = group.LinkedAttenuation;
				}
				OnPropertyChangedUI("IsEnabled");
				OnPropertyChangedUI("Attenuation");
				OnPropertyChangedUI("AttenuationText");
			}
		}
	}

	public bool IsActive
	{
		[DebuggerStepThrough]
		get
		{
			return isActive;
		}
		[DebuggerStepThrough]
		private set
		{
			isActive = value;
		}
	}

	public SatelliteGroup Group
	{
		[DebuggerStepThrough]
		get
		{
			return group;
		}
	}

	public event EventHandler FlashShow;

	public event EventHandler FlashHide;

	public event EventHandler Deactivated;

	public SatelliteEntry(SatelliteBase satellite, SatelliteGroup group, SimulationParams simulationParameters)
	{
		this.satellite = satellite;
		this.simulationParameters = simulationParameters;
		this.group = group;
		IEnumerable<SignalType> source = from s in simulationParameters.Signals
			where s.ConstellationType == satellite.ConstellationType
			select s.SignalType;
		firstSignalType = source.FirstOrDefault((SignalType st) => simulationParameters.SignalLevels.ContainsKey(st));
		if (group.AttenuationsLinked)
		{
			Attenuation = group.LinkedAttenuation;
		}
		else
		{
			Attenuation = 0.0;
		}
	}

	public void Activate()
	{
		this.FlashShow?.Invoke(this, EventArgs.Empty);
	}

	public void Deactivate()
	{
		IsActive = false;
		this.FlashHide?.Invoke(this, EventArgs.Empty);
		Task.Delay(15000).ContinueWith(delegate
		{
			Application current = Application.Current;
			return (current == null) ? null : current.Dispatcher.BeginInvoke(delegate
			{
				this.Deactivated?.Invoke(this, EventArgs.Empty);
			});
		});
	}
}
