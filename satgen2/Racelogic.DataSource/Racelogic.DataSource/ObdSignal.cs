using System;
using Newtonsoft.Json;
using Racelogic.Core;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class ObdSignal : BasePropertyChanged, ICloneable, IComparable
{
	private Tuple<ObdUnitTypes, double>[] availableUnits;

	private uint defaultPriority;

	private byte dataBytesReturned;

	private string formula;

	private bool log;

	private double maximum;

	private double minimum;

	private byte pid;

	private ObdSignalDescription description;

	private uint decimalPlaces = 2u;

	private byte priority;

	private Tuple<ObdUnitTypes, double> units;

	public static byte[] AvailablePriorities = new byte[7] { 1, 2, 3, 4, 5, 6, 7 };

	public static byte IntegerCount = 5;

	public Tuple<ObdUnitTypes, double>[] AvailableUnits
	{
		get
		{
			return availableUnits;
		}
		private set
		{
			availableUnits = value;
		}
	}

	public byte DataBytesReturned
	{
		get
		{
			return dataBytesReturned;
		}
		private set
		{
			dataBytesReturned = value;
		}
	}

	public uint DecimalPlaces
	{
		get
		{
			return decimalPlaces;
		}
		set
		{
			decimalPlaces = value;
			RaisePropertyChanged("DecimalPlaces");
			HasChanged = true;
		}
	}

	public uint DefaultPriority
	{
		get
		{
			return defaultPriority;
		}
		private set
		{
			defaultPriority = value;
		}
	}

	public ObdSignalDescription Description
	{
		get
		{
			return description;
		}
		private set
		{
			description = value;
		}
	}

	public string Formula
	{
		get
		{
			return formula;
		}
		private set
		{
			formula = value;
		}
	}

	public bool IsDistance
	{
		get
		{
			if (availableUnits != null && availableUnits.Length != 0)
			{
				if (availableUnits[0].Item1 != ObdUnitTypes.Kilometres)
				{
					return availableUnits[0].Item1 == ObdUnitTypes.Miles;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsSpeed
	{
		get
		{
			if (availableUnits != null && availableUnits.Length != 0)
			{
				if (availableUnits[0].Item1 != ObdUnitTypes.Kmh)
				{
					return availableUnits[0].Item1 == ObdUnitTypes.Mph;
				}
				return true;
			}
			return false;
		}
	}

	[JsonIgnore]
	public bool Log
	{
		get
		{
			return log;
		}
		set
		{
			log = value;
			RaisePropertyChanged("Log");
			HasChanged = true;
		}
	}

	public double Maximum
	{
		get
		{
			return maximum;
		}
		private set
		{
			maximum = value;
		}
	}

	public double Minimum
	{
		get
		{
			return minimum;
		}
		private set
		{
			minimum = value;
		}
	}

	public byte Pid
	{
		get
		{
			return pid;
		}
		private set
		{
			pid = value;
		}
	}

	[JsonIgnore]
	public byte Priority
	{
		get
		{
			return priority;
		}
		set
		{
			priority = value;
			RaisePropertyChanged("Priority");
			HasChanged = true;
		}
	}

	[JsonIgnore]
	public Tuple<ObdUnitTypes, double> Units
	{
		get
		{
			return units;
		}
		set
		{
			units = value;
			RaisePropertyChanged("Units");
			HasChanged = true;
		}
	}

	public bool HasChanged { get; private set; }

	public ObdSignal(ObdSignalDescription description, byte defaultPriority, Tuple<ObdUnitTypes, double>[] scaledUnits, byte pid, byte dataBytesReturned, double maximum, double minimum, string formula)
	{
		this.description = description;
		this.defaultPriority = defaultPriority;
		priority = defaultPriority;
		this.pid = pid;
		this.dataBytesReturned = dataBytesReturned;
		this.minimum = minimum;
		this.maximum = maximum;
		this.formula = formula;
		availableUnits = new Tuple<ObdUnitTypes, double>[scaledUnits.Length];
		for (int i = 0; i < scaledUnits.Length; i++)
		{
			availableUnits[i] = new Tuple<ObdUnitTypes, double>(scaledUnits[i].Item1, scaledUnits[i].Item2);
		}
		units = availableUnits[0];
	}

	public ObdSignal(ObdSignal signal)
	{
		description = signal.Description;
		defaultPriority = signal.DefaultPriority;
		priority = signal.Priority;
		decimalPlaces = signal.DecimalPlaces;
		pid = signal.Pid;
		dataBytesReturned = signal.DataBytesReturned;
		minimum = signal.Minimum;
		maximum = signal.Maximum;
		formula = signal.Formula;
		log = signal.Log;
		availableUnits = new Tuple<ObdUnitTypes, double>[signal.AvailableUnits.Length];
		for (int i = 0; i < signal.AvailableUnits.Length; i++)
		{
			availableUnits[i] = new Tuple<ObdUnitTypes, double>(signal.AvailableUnits[i].Item1, signal.AvailableUnits[i].Item2);
		}
		units = signal.Units;
	}

	[JsonConstructor]
	internal ObdSignal(Tuple<ObdUnitTypes, double>[] availableUnits, byte dataBytesReturned, uint decimalPlaces, byte defaultPriority, ObdSignalDescription description, string formula, double maximum, double minimum, byte pid)
	{
		this.availableUnits = new Tuple<ObdUnitTypes, double>[availableUnits.Length];
		for (int i = 0; i < availableUnits.Length; i++)
		{
			this.availableUnits[i] = new Tuple<ObdUnitTypes, double>(availableUnits[i].Item1, availableUnits[i].Item2);
		}
		units = this.availableUnits[0];
		this.dataBytesReturned = dataBytesReturned;
		this.decimalPlaces = decimalPlaces;
		this.defaultPriority = defaultPriority;
		priority = defaultPriority;
		this.description = description;
		this.formula = formula;
		this.maximum = maximum;
		this.minimum = minimum;
		this.pid = pid;
	}

	public ObdSignal()
	{
	}

	public void ClearHasChanged()
	{
		HasChanged = false;
	}

	public object Clone()
	{
		return new ObdSignal(this);
	}

	public int CompareTo(object obj)
	{
		return string.Compare(Description.LocalisedString(), ((ObdSignal)obj).Description.LocalisedString());
	}

	public void Copy(ObdSignal signalToCopy)
	{
		availableUnits = new Tuple<ObdUnitTypes, double>[signalToCopy.AvailableUnits.Length];
		for (int i = 0; i < signalToCopy.AvailableUnits.Length; i++)
		{
			availableUnits[i] = new Tuple<ObdUnitTypes, double>(signalToCopy.AvailableUnits[i].Item1, signalToCopy.AvailableUnits[i].Item2);
		}
		DataBytesReturned = signalToCopy.DataBytesReturned;
		DecimalPlaces = signalToCopy.DecimalPlaces;
		DefaultPriority = signalToCopy.DefaultPriority;
		Description = signalToCopy.Description;
		Formula = signalToCopy.Formula;
		Log = signalToCopy.Log;
		Maximum = signalToCopy.maximum;
		Minimum = signalToCopy.minimum;
		Pid = signalToCopy.Pid;
		Priority = signalToCopy.Priority;
		Units = signalToCopy.Units;
	}

	public override string ToString()
	{
		return Description.LocalisedString();
	}
}
