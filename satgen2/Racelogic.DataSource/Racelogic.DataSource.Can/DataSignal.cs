using System;
using System.Collections.Generic;
using System.Text;
using Racelogic.Utilities;

namespace Racelogic.DataSource.Can;

public class DataSignal : BasePropertyChanged, IEquatable<DataSignal>, ICloneable, IDataSignal
{
	private delegate void CheckValueDelegate();

	private List<Tuple<ObdUnitTypes, double, double>> availableScaledUnits;

	private int originalStartBit;

	private int startBit;

	private int length = 1;

	private DataFormat dataFormat;

	private string name = string.Empty;

	private string units = string.Empty;

	private double scale = 1.0;

	private double offset;

	private double minimum = -1E+300;

	private double maximum = 1E+300;

	private ByteOrder byteOrder;

	private float? multiplexor;

	private MultiplexorType multiplexorType = MultiplexorType.Signal;

	private bool isEncrypted;

	private string group = string.Empty;

	public List<Tuple<ObdUnitTypes, double, double>> AvailableScaledUnits
	{
		get
		{
			return availableScaledUnits;
		}
		set
		{
			availableScaledUnits = value;
			OnPropertyChanged("AvailableScaledUnits");
		}
	}

	public float? Multiplexor
	{
		get
		{
			return multiplexor;
		}
		set
		{
			multiplexor = value;
			OnPropertyChanged("Multiplexor");
		}
	}

	public MultiplexorType MultiplexorType
	{
		get
		{
			return multiplexorType;
		}
		set
		{
			multiplexorType = value;
			OnPropertyChanged("MultiplexorType");
		}
	}

	public int StartBit
	{
		get
		{
			return startBit;
		}
		set
		{
			startBit = value;
			OnPropertyChanged("StartBit");
		}
	}

	public int OriginalStartBit
	{
		get
		{
			return originalStartBit;
		}
		set
		{
			originalStartBit = value;
			OnPropertyChanged("OriginalStartBit");
		}
	}

	public int Length
	{
		get
		{
			switch (dataFormat)
			{
			case DataFormat.Double:
				length = 64;
				break;
			case DataFormat.Single:
				length = 32;
				break;
			default:
				if (length == 0)
				{
					length = 1;
				}
				break;
			}
			return length;
		}
		set
		{
			length = value;
			OnPropertyChanged("Length");
		}
	}

	public DataFormat DataFormat
	{
		get
		{
			return dataFormat;
		}
		set
		{
			dataFormat = value;
			OnPropertyChanged("DataFormat");
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

	public string Units
	{
		get
		{
			return units;
		}
		set
		{
			units = value;
			OnPropertyChanged("Units");
		}
	}

	public double Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
			OnPropertyChanged("Scale");
		}
	}

	public double Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
			OnPropertyChanged("Offset");
		}
	}

	public double Minimum
	{
		get
		{
			return minimum;
		}
		set
		{
			minimum = value;
			OnPropertyChanged("Minimum");
		}
	}

	public double Maximum
	{
		get
		{
			return maximum;
		}
		set
		{
			maximum = value;
			OnPropertyChanged("Maximum");
		}
	}

	public ByteOrder ByteOrder
	{
		get
		{
			return byteOrder;
		}
		set
		{
			byteOrder = value;
			OnPropertyChanged("ByteOrder");
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

	public string Group
	{
		get
		{
			return group;
		}
		set
		{
			group = value;
			OnPropertyChanged("Group");
		}
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is CanSignal))
		{
			return false;
		}
		return Equals((CanSignal)obj);
	}

	public bool Equals(DataSignal other)
	{
		if (other != null && StartBit == other.StartBit && Length == other.Length && DataFormat == other.DataFormat && Name == other.Name && Units == other.Units && Scale == other.Scale && Offset == other.Offset && Minimum == other.Minimum && Maximum == other.Maximum && ByteOrder == other.ByteOrder && MultiplexorType == other.MultiplexorType && multiplexor.HasValue == other.multiplexor.HasValue)
		{
			if (multiplexor.HasValue)
			{
				return multiplexor.Value == other.multiplexor.Value;
			}
			return true;
		}
		return false;
	}

	public static bool operator ==(DataSignal signal1, DataSignal signal2)
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

	public static bool operator !=(DataSignal signal1, DataSignal signal2)
	{
		return !(signal1 == signal2);
	}

	public void Copy(DataSignal source)
	{
		StartBit = source.StartBit;
		if ((object)source != null)
		{
			OriginalStartBit = source.OriginalStartBit;
		}
		Minimum = source.Minimum;
		Maximum = source.Maximum;
		Length = source.Length;
		DataFormat = source.DataFormat;
		Name = source.Name;
		Units = source.Units;
		Scale = source.Scale;
		Offset = source.Offset;
		ByteOrder = source.ByteOrder;
		IsEncrypted = source.IsEncrypted;
		Multiplexor = source.Multiplexor;
		MultiplexorType = source.MultiplexorType;
		Group = source.Group;
	}

	public object Clone()
	{
		return new DataSignal
		{
			StartBit = StartBit,
			Length = Length,
			DataFormat = DataFormat,
			Name = Name,
			Units = Units,
			Scale = Scale,
			Offset = Offset,
			Minimum = Minimum,
			Maximum = Maximum,
			ByteOrder = ByteOrder,
			Multiplexor = Multiplexor,
			MultiplexorType = MultiplexorType,
			IsEncrypted = IsEncrypted,
			Group = Group
		};
	}

	public static string ConvertRacelogicUnits(int value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		sbyte b = (sbyte)(value >> 24);
		value &= 0xFFFFFF;
		if ((value & 0x800000) == 8388608)
		{
			value |= -16777216;
		}
		if (value < 0)
		{
			value = -value;
			stringBuilder.Append('-');
		}
		else
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(value.ToString());
		if (stringBuilder.Length > 2)
		{
			stringBuilder.Insert(2, UnitsGlobal.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		}
		stringBuilder.Append('E');
		if (b >= 0)
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(b.ToString());
		return stringBuilder.ToString();
	}
}
