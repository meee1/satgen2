using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class MathsChannel : BasePropertyChanged, IOtherDataSource, IEquatable<MathsChannel>, IXmlSerializable
{
	private CanData value;

	public string Name { get; set; }

	public string Units { get; set; }

	public object Formula { get; set; }

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

	public bool Equals(MathsChannel other)
	{
		if (other != null)
		{
			return Formula == other.Formula;
		}
		return false;
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
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("Name", Name);
		writer.WriteAttributeString("Units", Units);
	}

	public IOtherDataSource Clone()
	{
		return new MathsChannel
		{
			Name = Name,
			Units = Units,
			Formula = Formula,
			Value = value
		};
	}
}
