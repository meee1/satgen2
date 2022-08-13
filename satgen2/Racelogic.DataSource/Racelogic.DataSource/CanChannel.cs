using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Racelogic.Core;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class CanChannel : BasePropertyChanged, IOtherDataSource, IEquatable<CanChannel>, IXmlSerializable
{
	private CanData value;

	public string Name { get; set; }

	public string Units { get; set; }

	public uint SerialNumber { get; set; }

	public byte ChannelNumber { get; set; }

	public byte UnitType { get; set; }

	public bool IsBeingSentOverSerial { get; set; }

	public CanData Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			OnPropertyChanged("Value");
		}
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (!string.IsNullOrEmpty(Name))
		{
			num ^= Name.GetHashCode();
		}
		if (!string.IsNullOrEmpty(Units))
		{
			num ^= Units.GetHashCode();
		}
		return num ^ SerialNumber.GetHashCode() ^ ChannelNumber.GetHashCode();
	}

	public bool Equals(CanChannel other)
	{
		if (other == null)
		{
			return false;
		}
		bool result = UnitType == other.UnitType && SerialNumber == other.SerialNumber && ChannelNumber == other.ChannelNumber;
		if (UnitType == 0 && SerialNumber == 0 && ChannelNumber == 0)
		{
			result = string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Units, other.Units, StringComparison.OrdinalIgnoreCase);
		}
		return result;
	}

	public static bool operator ==(CanChannel can1, CanChannel can2)
	{
		if ((object)can1 == can2)
		{
			return true;
		}
		if ((object)can1 == null || (object)can2 == null)
		{
			return false;
		}
		return can1.Equals(can2);
	}

	public static bool operator !=(CanChannel can1, CanChannel can2)
	{
		if (can1 == null && can2 == null)
		{
			return false;
		}
		if (can1 == null || can2 == null)
		{
			return true;
		}
		bool result = can1.UnitType != can2.UnitType || can1.SerialNumber != can2.SerialNumber || can1.ChannelNumber != can2.ChannelNumber;
		if (can1.UnitType == 0 && can1.SerialNumber == 0 && can1.ChannelNumber == 0)
		{
			result = !string.Equals(can1.Name, can2.Name, StringComparison.OrdinalIgnoreCase) || !string.Equals(can1.Units, can2.Units, StringComparison.OrdinalIgnoreCase);
		}
		return result;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
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
		UnitType = byte.Parse(reader.GetAttribute("UnitType"));
		SerialNumber = uint.Parse(reader.GetAttribute("SerialNumber"));
		ChannelNumber = byte.Parse(reader.GetAttribute("ChannelNumber"));
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("Name", Name);
		writer.WriteAttributeString("Units", Units);
		writer.WriteAttributeString("UnitType", UnitType.ToString());
		writer.WriteAttributeString("SerialNumber", SerialNumber.ToString());
		writer.WriteAttributeString("ChannelNumber", ChannelNumber.ToString());
	}

	public static string ConvertCanChannelUnit(string currentUnit)
	{
		if (string.IsNullOrEmpty(currentUnit))
		{
			return currentUnit;
		}
		switch (currentUnit.ToLower())
		{
		case "metre":
		case "metres":
		case "meters":
		case "meter":
		case "m":
		case "feet":
		case "ft":
		case "km":
		case "mi":
		case "nmi":
			return FormatOptions.Instance.Distance.Units.LocalisedString();
		case "hhmmss":
		case "s":
			return FormatOptions.Instance.Time.Units.LocalisedString();
		case "g":
			return FormatOptions.Instance.Acceleration.Units.LocalisedString();
		case "km/h":
		case "mph":
		case "m/s":
		case "ft/s":
		case "kts":
			return FormatOptions.Instance.Speed.Units.LocalisedString();
		case "radian":
		case "degree":
		case "degrees":
		case "°":
			return FormatOptions.Instance.Angle.Units.LocalisedString();
		case "db":
			return FormatOptions.Instance.Sound.Units.LocalisedString();
		default:
			return currentUnit;
		}
	}

	public IOtherDataSource Clone()
	{
		return new CanChannel
		{
			Name = Name,
			Units = Units,
			SerialNumber = SerialNumber,
			ChannelNumber = ChannelNumber,
			UnitType = UnitType,
			IsBeingSentOverSerial = IsBeingSentOverSerial,
			Value = new CanData(Value.ToDouble(UnitsGlobal.CurrentCulture))
		};
	}
}
