using System;
using System.Collections.Generic;
using System.Linq;
using Racelogic.DataSource.Can;

namespace Racelogic.DataSource;

[Serializable]
public class CanFullSignal : CanSignal, IEquatable<CanFullSignal>
{
	private string group = string.Empty;

	private string make = string.Empty;

	private string model = string.Empty;

	private CANTranChannels canTranChannel = CANTranChannels.Brake;

	private CANTranOutputTypes canTranOutputType;

	private CANTranPriorities canTranPriority;

	private byte canTranOutputLevel;

	private double baudRate = 500.0;

	private string applicableVehicles = string.Empty;

	private string obdOrColor = string.Empty;

	private int unitSerialNumber;

	[NonSerialized]
	private byte[] _encryptedData;

	[NonSerialized]
	private bool _hasEncryptedDataSegment;

	public byte[] EncryptedData
	{
		get
		{
			return _encryptedData;
		}
		set
		{
			_encryptedData = value;
			HasEncryptedDataSegment = _encryptedData.Any((byte x) => x != 0);
			OnPropertyChanged("EncryptedData");
		}
	}

	public bool HasEncryptedDataSegment
	{
		get
		{
			return _hasEncryptedDataSegment;
		}
		private set
		{
			_hasEncryptedDataSegment = value;
			OnPropertyChanged("HasEncryptedDataSegment");
		}
	}

	public string Group
	{
		get
		{
			return group;
		}
		set
		{
			group = value;
			base.DataFrame.Name = value;
			OnPropertyChanged("Group");
		}
	}

	public string Make
	{
		get
		{
			return make;
		}
		set
		{
			make = value;
			OnPropertyChanged("Make");
		}
	}

	public string Model
	{
		get
		{
			return model;
		}
		set
		{
			model = value;
			OnPropertyChanged("Model");
		}
	}

	public string ApplicableVehicles
	{
		get
		{
			return applicableVehicles;
		}
		set
		{
			applicableVehicles = value;
			OnPropertyChanged("ApplicableVehicles");
		}
	}

	public string ObdOrColor
	{
		get
		{
			return obdOrColor;
		}
		set
		{
			obdOrColor = value;
			OnPropertyChanged("ObdOrColor");
		}
	}

	public CANTranChannels CanTranChannel
	{
		get
		{
			return canTranChannel;
		}
		set
		{
			canTranChannel = value;
			OnPropertyChanged("CanTranChannel");
		}
	}

	public CANTranOutputTypes CanTranOutputType
	{
		get
		{
			return canTranOutputType;
		}
		set
		{
			canTranOutputType = value;
			OnPropertyChanged("CanTranOutputType");
		}
	}

	public CANTranPriorities CanTranPriority
	{
		get
		{
			return canTranPriority;
		}
		set
		{
			canTranPriority = value;
			OnPropertyChanged("CanTranPriority");
		}
	}

	public byte CanTranOutputLevel
	{
		get
		{
			return canTranOutputLevel;
		}
		set
		{
			canTranOutputLevel = value;
			OnPropertyChanged("CanTranOutputLevel");
		}
	}

	public double BaudRate
	{
		get
		{
			return baudRate;
		}
		set
		{
			baudRate = value;
			OnPropertyChanged("BaudRate");
		}
	}

	public int UnitSerialNumber
	{
		get
		{
			return unitSerialNumber;
		}
		set
		{
			unitSerialNumber = value;
			OnPropertyChanged("UnitSerialNumber");
		}
	}

	public CanFullSignal()
	{
	}

	public CanFullSignal(DataFrame frame, DataSignal signal)
		: base(frame, signal)
	{
		Group = frame.Name;
	}

	public bool Equals(CanFullSignal other)
	{
		if (Equals((CanSignal)other) && Group == other.Group && Make == other.Make && Model == other.Model && ApplicableVehicles == other.ApplicableVehicles && ObdOrColor == other.ObdOrColor && CanTranChannel == other.CanTranChannel && CanTranOutputType == other.CanTranOutputType && CanTranPriority == other.CanTranPriority && CanTranOutputLevel == other.CanTranOutputLevel)
		{
			return BaudRate == other.BaudRate;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is CanFullSignal))
		{
			return false;
		}
		return Equals((CanFullSignal)obj);
	}

	public static bool operator ==(CanFullSignal signal1, CanFullSignal signal2)
	{
		if ((object)signal1 == signal2)
		{
			return true;
		}
		if ((object)signal1 == null || (object)signal2 == null)
		{
			return false;
		}
		return signal1.Equals(signal2);
	}

	public static bool operator !=(CanFullSignal signal1, CanFullSignal signal2)
	{
		return !(signal1 == signal2);
	}

	public new void Copy(ICanSignal source)
	{
		base.Copy(source);
		CanFullSignal canFullSignal = source as CanFullSignal;
		if (canFullSignal != null)
		{
			Group = canFullSignal.Group;
			Make = canFullSignal.Make;
			Model = canFullSignal.Model;
			ApplicableVehicles = canFullSignal.ApplicableVehicles;
			ObdOrColor = canFullSignal.ObdOrColor;
			CanTranChannel = canFullSignal.CanTranChannel;
			CanTranOutputType = canFullSignal.CanTranOutputType;
			CanTranPriority = canFullSignal.CanTranPriority;
			CanTranOutputLevel = canFullSignal.CanTranOutputLevel;
			BaudRate = canFullSignal.BaudRate;
			if (canFullSignal.EncryptedData != null)
			{
				EncryptedData = canFullSignal.EncryptedData;
			}
			base.AvailableScaledUnits = ((canFullSignal.AvailableScaledUnits == null) ? null : new List<Tuple<ObdUnitTypes, double, double>>(canFullSignal.AvailableScaledUnits));
		}
	}

	public new object Clone()
	{
		CanFullSignal canFullSignal = new CanFullSignal();
		canFullSignal.Copy(this);
		return canFullSignal;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
