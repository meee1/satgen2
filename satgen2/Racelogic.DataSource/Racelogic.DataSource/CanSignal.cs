using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Racelogic.DataSource.Can;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class CanSignal : BasePropertyChanged, ICanSignal, IEquatable<CanSignal>, ICloneable, IXmlSerializable
{
	private delegate void CheckValueDelegate();

	private readonly DataSignal dataSignal;

	private readonly DataFrame dataFrame;

	public static Func<uint, bool, uint> CheckIdentifierIsValid = (uint value, bool isExtended) => DataFrame.CheckIdentifierIsValid(value, isExtended);

	public DataSignal DataSignal => dataSignal;

	public DataFrame DataFrame => dataFrame;

	public List<Tuple<ObdUnitTypes, double, double>> AvailableScaledUnits
	{
		get
		{
			return DataSignal.AvailableScaledUnits;
		}
		set
		{
			DataSignal.AvailableScaledUnits = value;
			OnPropertyChanged("AvailableScaledUnits");
		}
	}

	public int Identifier
	{
		get
		{
			return DataFrame.Identifier;
		}
		set
		{
			DataFrame.Identifier = value;
			OnPropertyChanged("Identifier");
		}
	}

	public float? Multiplexor
	{
		get
		{
			return DataSignal.Multiplexor;
		}
		set
		{
			DataSignal.Multiplexor = value;
			OnPropertyChanged("Multiplexor");
		}
	}

	public MultiplexorType MultiplexorType
	{
		get
		{
			return DataSignal.MultiplexorType;
		}
		set
		{
			DataSignal.MultiplexorType = value;
			OnPropertyChanged("MultiplexorType");
		}
	}

	public byte StartBit
	{
		get
		{
			return (byte)DataSignal.StartBit;
		}
		set
		{
			DataSignal.StartBit = value;
			OnPropertyChanged("StartBit");
		}
	}

	public byte OriginalStartBit
	{
		get
		{
			return (byte)DataSignal.OriginalStartBit;
		}
		set
		{
			DataSignal.OriginalStartBit = value;
			OnPropertyChanged("OriginalStartBit");
		}
	}

	public byte Length
	{
		get
		{
			return (byte)DataSignal.Length;
		}
		set
		{
			DataSignal.Length = value;
			OnPropertyChanged("Length");
		}
	}

	public byte DataLengthCode
	{
		get
		{
			if (DataFrame.DataLengthCode == 0)
			{
				DataFrame.DataLengthCode = (byte)((DataSignal.DataFormat == DataFormat.Single) ? 4 : 8);
			}
			return DataFrame.DataLengthCode;
		}
		set
		{
			if (Enum.IsDefined(typeof(DataLengthCodes), value))
			{
				DataFrame.DataLengthCode = value;
				OnPropertyChanged("DataLengthCode");
			}
		}
	}

	public virtual bool SupportsDataLengthCode => true;

	public bool IsExtended
	{
		get
		{
			return DataFrame.IsExtendedIdentifier;
		}
		set
		{
			DataFrame.IsExtendedIdentifier = value;
			OnPropertyChanged("IsExtended");
		}
	}

	public DataFormat DataFormat
	{
		get
		{
			return DataSignal.DataFormat;
		}
		set
		{
			DataSignal.DataFormat = value;
			OnPropertyChanged("DataFormat");
		}
	}

	public string Name
	{
		get
		{
			return DataSignal.Name;
		}
		set
		{
			DataSignal.Name = value;
			OnPropertyChanged("Name");
		}
	}

	public string Units
	{
		get
		{
			return DataSignal.Units;
		}
		set
		{
			DataSignal.Units = value;
			OnPropertyChanged("Units");
		}
	}

	public double Scale
	{
		get
		{
			return DataSignal.Scale;
		}
		set
		{
			DataSignal.Scale = value;
			OnPropertyChanged("Scale");
		}
	}

	public double Offset
	{
		get
		{
			return DataSignal.Offset;
		}
		set
		{
			DataSignal.Offset = value;
			OnPropertyChanged("Offset");
		}
	}

	public double Minimum
	{
		get
		{
			return DataSignal.Minimum;
		}
		set
		{
			DataSignal.Minimum = value;
			OnPropertyChanged("Minimum");
		}
	}

	public double Maximum
	{
		get
		{
			return DataSignal.Maximum;
		}
		set
		{
			DataSignal.Maximum = value;
			OnPropertyChanged("Maximum");
		}
	}

	public ByteOrder ByteOrder
	{
		get
		{
			return DataSignal.ByteOrder;
		}
		set
		{
			DataSignal.ByteOrder = value;
			OnPropertyChanged("ByteOrder");
		}
	}

	public bool IsEncrypted
	{
		get
		{
			return DataSignal.IsEncrypted;
		}
		set
		{
			DataSignal.IsEncrypted = value;
			OnPropertyChanged("IsEncrypted");
		}
	}

	public CanSignal()
	{
		dataFrame = new DataFrame();
		dataSignal = new DataSignal();
	}

	public CanSignal(DataFrame frame, DataSignal signal)
	{
		dataFrame = frame;
		dataSignal = signal;
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

	public bool Equals(CanSignal other)
	{
		if (other != null && Identifier == other.Identifier && StartBit == other.StartBit && Length == other.Length && DataLengthCode == other.DataLengthCode && IsExtended == other.IsExtended && DataFormat == other.DataFormat && Name == other.Name && Units == other.Units && Scale == other.Scale && Offset == other.Offset && Minimum == other.Minimum && Maximum == other.Maximum && ByteOrder == other.ByteOrder && MultiplexorType == other.MultiplexorType)
		{
			if (Multiplexor.HasValue == other.Multiplexor.HasValue)
			{
				if (Multiplexor.HasValue)
				{
					return Multiplexor.Value == other.Multiplexor.Value;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static bool operator ==(CanSignal signal1, CanSignal signal2)
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

	public static bool operator !=(CanSignal signal1, CanSignal signal2)
	{
		return !(signal1 == signal2);
	}

	public void Copy(ICanSignal source)
	{
		Identifier = source.Identifier;
		StartBit = source.StartBit;
		if (source is CanSignal canSignal)
		{
			OriginalStartBit = canSignal.OriginalStartBit;
			Minimum = canSignal.Minimum;
			Maximum = canSignal.Maximum;
		}
		Length = source.Length;
		DataLengthCode = source.DataLengthCode;
		IsExtended = source.IsExtended;
		DataFormat = source.DataFormat;
		Name = source.Name;
		Units = source.Units;
		Scale = source.Scale;
		Offset = source.Offset;
		ByteOrder = source.ByteOrder;
		IsEncrypted = source.IsEncrypted;
		Multiplexor = source.Multiplexor;
		MultiplexorType = source.MultiplexorType;
	}

	public object Clone()
	{
		return new CanSignal
		{
			Identifier = Identifier,
			StartBit = StartBit,
			Length = Length,
			DataLengthCode = DataLengthCode,
			IsExtended = IsExtended,
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
			IsEncrypted = IsEncrypted
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

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		reader.MoveToContent();
		Name = reader.GetAttribute("Name");
		Units = reader.GetAttribute("Units");
		Identifier = int.Parse(reader.GetAttribute("Identifier"));
		IsExtended = bool.Parse(reader.GetAttribute("IsExtended"));
		StartBit = byte.Parse(reader.GetAttribute("StartBit"));
		Length = byte.Parse(reader.GetAttribute("BitLength"));
		Scale = double.Parse(reader.GetAttribute("Scale"));
		Offset = double.Parse(reader.GetAttribute("Offset"));
		ByteOrder = (bool.Parse(reader.GetAttribute("Intel")) ? ByteOrder.Intel : ByteOrder.Motorola);
		if (double.TryParse(reader.GetAttribute("Maximum"), out var result))
		{
			Maximum = result;
		}
		if (double.TryParse(reader.GetAttribute("Minimum"), out result))
		{
			Minimum = result;
		}
		DataFormat = DataFormat.Unsigned;
		if (bool.Parse(reader.GetAttribute("IsSigned")))
		{
			DataFormat = DataFormat.Signed;
		}
		if (bool.Parse(reader.GetAttribute("IsPseudoSigned")))
		{
			DataFormat = DataFormat.PseudoSigned;
		}
		if (bool.Parse(reader.GetAttribute("IsDouble")))
		{
			DataFormat = DataFormat.Double;
		}
		if (bool.Parse(reader.GetAttribute("IsSingle")))
		{
			DataFormat = DataFormat.Single;
		}
		DataLengthCode = byte.Parse(reader.GetAttribute("DLC") ?? "8");
		Multiplexor = null;
		if (int.TryParse("Multiplexor", out var result2))
		{
			Multiplexor = result2;
		}
		MultiplexorType = MultiplexorType.Signal;
		string attribute = reader.GetAttribute("IsMultiplexorSignal");
		if (!string.IsNullOrEmpty(attribute) && bool.Parse(attribute))
		{
			MultiplexorType = MultiplexorType.MultiplexorSignal;
		}
		bool isEmptyElement = reader.IsEmptyElement;
		reader.ReadStartElement();
		if (!isEmptyElement)
		{
			reader.ReadEndElement();
		}
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("Name", Name);
		writer.WriteAttributeString("Units", Name);
		writer.WriteAttributeString("Identifier", Identifier.ToString());
		writer.WriteAttributeString("IsExtended", IsExtended.ToString());
		writer.WriteAttributeString("StartBit", StartBit.ToString());
		writer.WriteAttributeString("BitLength", Length.ToString());
		writer.WriteAttributeString("Scale", Scale.ToString());
		writer.WriteAttributeString("Offset", Offset.ToString());
		writer.WriteAttributeString("Intel", (ByteOrder == ByteOrder.Intel).ToString());
		if (!Maximum.Equals(double.MaxValue))
		{
			writer.WriteAttributeString("Maximum", Maximum.ToString());
		}
		if (!Minimum.Equals(double.MinValue))
		{
			writer.WriteAttributeString("Minimum", Minimum.ToString());
		}
		writer.WriteAttributeString("IsSigned", (DataFormat == DataFormat.Signed).ToString());
		writer.WriteAttributeString("IsPseudoSigned", (DataFormat == DataFormat.PseudoSigned).ToString());
		writer.WriteAttributeString("IsDouble", (DataFormat == DataFormat.Double).ToString());
		writer.WriteAttributeString("IsSingle", (DataFormat == DataFormat.Single).ToString());
		writer.WriteAttributeString("DLC", DataLengthCode.ToString());
		if (Multiplexor.HasValue)
		{
			writer.WriteAttributeString("Multiplexor", Multiplexor.Value.ToString());
		}
		writer.WriteAttributeString("IsMultiplexorSignal", (MultiplexorType == MultiplexorType.MultiplexorSignal).ToString());
		writer.WriteAttributeString("IsMultiplexedSignal", (MultiplexorType == MultiplexorType.MultiplexedSignal).ToString());
	}
}
