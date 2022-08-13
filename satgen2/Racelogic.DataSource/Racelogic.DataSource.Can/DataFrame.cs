using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Racelogic.Utilities;

namespace Racelogic.DataSource.Can;

public class DataFrame : BasePropertyChanged, IEquatable<DataFrame>, ICloneable, IDataFrame
{
	private delegate void CheckValueDelegate();

	private int identifier;

	private byte dataLengthCode = 8;

	private bool isExtendedIdentifier;

	private bool isFlexibleDataRate;

	private bool isEncrypted;

	private ObservableCollection<IDataSignal> containedSignals = new ObservableCollection<IDataSignal>();

	private string name = string.Empty;

	public static Func<uint, bool, uint> CheckIdentifierIsValid = delegate(uint value, bool isExtended)
	{
		uint num = (isExtended ? 536870911u : 2047u);
		return (value <= num) ? value : num;
	};

	public int Identifier
	{
		get
		{
			return identifier;
		}
		set
		{
			identifier = value;
			OnPropertyChanged("Identifier");
		}
	}

	public byte DataLengthCode
	{
		get
		{
			return dataLengthCode;
		}
		set
		{
			dataLengthCode = value;
			OnPropertyChanged("DataLengthCode");
		}
	}

	public bool IsExtendedIdentifier
	{
		get
		{
			return isExtendedIdentifier;
		}
		set
		{
			isExtendedIdentifier = value;
			OnPropertyChanged("IsExtendedIdentifier");
		}
	}

	public bool IsEncrypted
	{
		get
		{
			return isEncrypted;
		}
		set
		{
			isEncrypted = value;
			OnPropertyChanged("IsEncrypted");
		}
	}

	public bool IsFlexibleDataRate
	{
		get
		{
			return isFlexibleDataRate;
		}
		set
		{
			isFlexibleDataRate = value;
			OnPropertyChanged("IsFlexibleDataRate");
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
			OnPropertyChanged("Name");
		}
	}

	public ObservableCollection<IDataSignal> ContainedSignals
	{
		get
		{
			return containedSignals;
		}
		set
		{
			containedSignals = value;
			OnPropertyChanged("ContainedSignals");
		}
	}

	public DataFrame()
	{
	}

	public DataFrame(List<IDataSignal> signals)
	{
		containedSignals = new ObservableCollection<IDataSignal>(signals);
	}

	public DataFrame(params IDataSignal[] signals)
		: this(signals.ToList())
	{
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is DataFrame))
		{
			return false;
		}
		return Equals((DataFrame)obj);
	}

	public bool Equals(DataFrame other)
	{
		if (other != null && Identifier == other.Identifier && DataLengthCode == other.DataLengthCode)
		{
			return IsExtendedIdentifier == other.IsExtendedIdentifier;
		}
		return false;
	}

	public static bool operator ==(DataFrame message1, DataFrame message2)
	{
		if ((object)message1 == message2)
		{
			return true;
		}
		if ((object)message1 == null || (object)message2 == null)
		{
			return false;
		}
		return message1.Equals(message2);
	}

	public static bool operator !=(DataFrame message1, DataFrame message2)
	{
		return !(message1 == message2);
	}

	public void Copy(IDataFrame source)
	{
		Identifier = source.Identifier;
		DataLengthCode = source.DataLengthCode;
		IsExtendedIdentifier = source.IsExtendedIdentifier;
		IsEncrypted = source.IsEncrypted;
		Name = source.Name;
		IsFlexibleDataRate = source.IsFlexibleDataRate;
	}

	public object Clone()
	{
		return new DataFrame
		{
			Identifier = Identifier,
			DataLengthCode = DataLengthCode,
			IsExtendedIdentifier = IsExtendedIdentifier,
			IsEncrypted = IsEncrypted,
			Name = Name,
			IsFlexibleDataRate = IsFlexibleDataRate
		};
	}
}
