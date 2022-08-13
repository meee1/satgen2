using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Racelogic.DataSource;

[DebuggerDisplay("{!string.IsNullOrEmpty(Name) ? Name : StandardChannel != DataSource.VBoxChannel.None ? StandardChannel.ToString() : StandardChannel2.ToString() }={Value}")]
public class RacelogicChannel : IXmlSerializable, IEquatable<RacelogicChannel>
{
	public static class Unit
	{
		public const string Metres = "m";

		public const string Degrees = "degrees";

		public const string G = "g";

		public const string Kph = "km/h";

		public const string dB = "dB";

		public const string MsSq = "m/s²";

		public const string Jerk = "m/s³";

		public const string Seconds = "s";
	}

	public string Name { get; set; }

	public string Units { get; set; }

	public double Value { get; set; }

	public VBoxChannel StandardChannel { get; set; }

	public VBoxChannel2 StandardChannel2 { get; set; }

	public RacelogicChannel(string name, double value)
		: this(name, value, VBoxChannel.None, "")
	{
	}

	public RacelogicChannel(string name, double value, VBoxChannel standardChannel)
		: this(name, value, standardChannel, "")
	{
	}

	public RacelogicChannel(string name, double value, VBoxChannel2 standardChannel2)
		: this(name, value, standardChannel2, "")
	{
	}

	public RacelogicChannel(string name, double value, string units)
		: this(name, value, VBoxChannel.None, units)
	{
	}

	public RacelogicChannel(string name, double value, VBoxChannel standardChannel, string units)
		: this(name, value, standardChannel, VBoxChannel2.None, units)
	{
	}

	public RacelogicChannel(string name, double value, VBoxChannel2 standardChannel2, string units)
		: this(name, value, VBoxChannel.None, standardChannel2, units)
	{
	}

	private RacelogicChannel(string name, double value, VBoxChannel standardChannel, VBoxChannel2 standardChannel2, string units)
	{
		Name = name;
		Units = units;
		Value = value;
		StandardChannel = standardChannel;
		StandardChannel2 = standardChannel2;
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
		if (Enum.TryParse<VBoxChannel>(reader.GetAttribute("VBoxChannel"), out var result))
		{
			StandardChannel = result;
		}
		else
		{
			StandardChannel = VBoxChannel.None;
		}
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("Name", Name);
		writer.WriteAttributeString("Units", Units);
		writer.WriteAttributeString("VBoxChannel", StandardChannel.ToString());
	}

	public bool Equals(RacelogicChannel other)
	{
		if (other == null)
		{
			return false;
		}
		if (StandardChannel != VBoxChannel.None)
		{
			return StandardChannel == other.StandardChannel;
		}
		if (StandardChannel != VBoxChannel.None)
		{
			return StandardChannel2 == other.StandardChannel2;
		}
		if (string.Equals(Name, other.Name))
		{
			if (!string.IsNullOrEmpty(Units) || !string.IsNullOrEmpty(other.Units))
			{
				return string.Equals(Units, other.Units);
			}
			return true;
		}
		return false;
	}
}
