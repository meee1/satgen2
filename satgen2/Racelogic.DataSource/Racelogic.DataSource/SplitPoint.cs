using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Racelogic.Core;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

[ClassInterface(ClassInterfaceType.None)]
public class SplitPoint : BasePropertyChanged, IEquatable<SplitPoint>, ISplitDefinition, IXmlSerializable
{
	private class Point
	{
		internal double X { get; set; }

		internal double Y { get; set; }

		internal Point(double x, double y)
		{
			X = x;
			Y = y;
		}
	}

	private LongitudeMinutes xLongitude;

	private LongitudeMinutes xLongitudeMinus10;

	private LatitudeMinutes yLatitude;

	private LatitudeMinutes yLatitudeMinus10;

	private string name = string.Empty;

	private SplitType type;

	private string fileName;

	private string key = string.Empty;

	private bool useEgm96Geoid;

	private Geodetic reference;

	private Ecef referenceEcef;

	private LocalTangentPlane gateStartPoint;

	private LocalTangentPlane gateEndPoint;

	public string Description
	{
		get
		{
			string a = Type.LocalisedString();
			string text = string.Empty;
			if (!string.IsNullOrEmpty(name))
			{
				text = (string.Equals(a, name) ? "" : (name + " "));
			}
			double num = gateEndPoint.East - gateStartPoint.East;
			double num2 = gateEndPoint.North - gateStartPoint.North;
			double num3 = Math.Sqrt(num2 * num2 + num * num);
			double num4 = num2 / num;
			string text2 = ((reference != null) ? $"Z={num3:F3}; M={num4:F3}; S={GateStartPoint.North}, {GateStartPoint.East}; E={GateEndPoint.North}, {GateEndPoint.East};" : $"{LatitudeMinutes}, {LongitudeMinutes}");
			if (type != 0)
			{
				return Type.LocalisedString() + "  ( " + text + text2 + " )";
			}
			return Type.LocalisedString();
		}
	}

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
			OnPropertyChanged("FileName");
		}
	}

	public bool IsValid
	{
		get
		{
			bool result = Type != SplitType.None;
			if ((double)LongitudeMinutes == 0.0 && (double)LatitudeMinutes == 0.0 && (double)LongitudeMinutesMinus10 == 0.0 && (double)LatitudeMinutesMinus10 == 0.0)
			{
				result = false;
			}
			else if (Maths.Compare(LatitudeMinutes, 5400.0) == ValueIs.Equal && Maths.Compare(LongitudeMinutes, 10800.0) == ValueIs.Equal)
			{
				result = false;
			}
			return result;
		}
	}

	public LatitudeMinutes LatitudeMinutes
	{
		get
		{
			return yLatitude;
		}
		set
		{
			yLatitude = value;
			OnPropertyChanged("LatitudeMinutes");
			OnPropertyChanged("Description");
			OnPropertyChanged("IsValid");
		}
	}

	public LatitudeMinutes LatitudeMinutesMinus10
	{
		get
		{
			return yLatitudeMinus10;
		}
		set
		{
			yLatitudeMinus10 = value;
			OnPropertyChanged("LatitudeMinutesMinus10");
			OnPropertyChanged("Description");
			OnPropertyChanged("IsValid");
		}
	}

	public LongitudeMinutes LongitudeMinutes
	{
		get
		{
			return xLongitude;
		}
		set
		{
			xLongitude = value;
			OnPropertyChanged("LongitudeMinutes");
			OnPropertyChanged("Description");
			OnPropertyChanged("IsValid");
		}
	}

	public LongitudeMinutes LongitudeMinutesMinus10
	{
		get
		{
			return xLongitudeMinus10;
		}
		set
		{
			xLongitudeMinus10 = value;
			OnPropertyChanged("LongitudeMinutesMinus10");
			OnPropertyChanged("Description");
			OnPropertyChanged("IsValid");
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
			OnPropertyChanged("Description");
		}
	}

	public string TrackDatabaseIdentifier => type switch
	{
		SplitType.StartFinish => "sf", 
		SplitType.Finish => "ff", 
		SplitType.Split => "sx", 
		SplitType.SectorStart => "cs", 
		SplitType.SectorEnd => "ce", 
		SplitType.Start => "ss", 
		SplitType.PitLaneStart => "ps", 
		SplitType.PitLaneFinish => "pf", 
		_ => "", 
	};

	public bool UseEgm96Geoid => useEgm96Geoid;

	public SplitType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			OnPropertyChanged("Type");
			OnPropertyChanged("Description");
			OnPropertyChanged("IsValid");
		}
	}

	public double GateWidth
	{
		get
		{
			double num = gateEndPoint.East - gateStartPoint.East;
			double num2 = gateEndPoint.North - gateStartPoint.North;
			return Math.Sqrt(num2 * num2 + num * num);
		}
	}

	public Geodetic Reference => reference;

	public Ecef? ReferenceEcef => referenceEcef;

	public LocalTangentPlane GateStartPoint => gateStartPoint;

	public LocalTangentPlane GateEndPoint => gateEndPoint;

	public SplitPoint()
	{
	}

	public static SplitPoint Parse(string data)
	{
		string[] separator = new string[3]
		{
			new string(new char[2] { 'Â', '¬' }),
			new string(new char[1] { '¬' }),
			new string(new char[3] { 'ï', '¿', '½' })
		};
		string[] array = data.Split(separator, StringSplitOptions.None);
		if (array.Length < 2)
		{
			return null;
		}
		string[] array2 = array[0].Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries).Where(IsNumeric).ToArray();
		string name = string.Join(string.Empty, from x in array[0].Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries)
			where !IsNumeric(x)
			select x);
		if (array2.Length != 4)
		{
			return null;
		}
		string value = Enum.GetNames(typeof(SplitType)).FirstOrDefault((string n) => string.Equals(name.Replace("/", string.Empty).ToLowerInvariant(), n.ToLowerInvariant()));
		if (string.IsNullOrEmpty(value))
		{
			return null;
		}
		SplitType num = (SplitType)Enum.Parse(typeof(SplitType), value);
		if (string.IsNullOrEmpty(array[1]) && !string.IsNullOrEmpty(name))
		{
			array[1] = name;
		}
		return new SplitPoint(num, array[1].Trim(), new LongitudeMinutes(array2[0]), new LatitudeMinutes(array2[1]), new LongitudeMinutes(array2[2]), new LatitudeMinutes(array2[3]));
		static bool IsNumeric(string s)
		{
			double result;
			return double.TryParse(s, out result);
		}
	}

	public static SplitPoint GetEcefSplitPoint(LatitudeMinutes currentLatitude, LongitudeMinutes currentLongitude, LatitudeMinutes previousLatitude, LongitudeMinutes previousLongitude, double gateWidth = 25.0, SplitType splitType = SplitType.None, string splitName = "Unnamed")
	{
		SplitPoint ecefSplitPoint = GetEcefSplitPoint(Geodetic.FromDegrees((double)currentLatitude / 60.0, (double)currentLongitude / 60.0, 0.0), Geodetic.FromDegrees((double)previousLatitude / 60.0, (double)previousLongitude / 60.0, 0.0), gateWidth, splitType, splitName);
		ecefSplitPoint.LatitudeMinutes = currentLatitude;
		ecefSplitPoint.LongitudeMinutes = currentLongitude;
		ecefSplitPoint.LatitudeMinutesMinus10 = previousLatitude;
		ecefSplitPoint.LongitudeMinutesMinus10 = previousLongitude;
		return ecefSplitPoint;
	}

	public static SplitPoint GetEcefSplitPoint(Geodetic currentSample, Geodetic previousSample, double gateWidth = 25.0, SplitType splitType = SplitType.None, string splitName = "Unnamed")
	{
		Ecef ecef = currentSample.ToEcef2();
		LocalTangentPlane localTangentPlane = previousSample.ToEcef2().ToNed(ecef);
		Point point;
		Point point2;
		if (Maths.Compare(localTangentPlane.East, 0.0) != 0)
		{
			double num = gateWidth / 2.0;
			double num2 = num * num;
			double num3 = localTangentPlane.North / localTangentPlane.East;
			double num4 = num3 * num3;
			double num5 = Math.Sqrt(num2 * num4 / (num4 + 1.0));
			double num6 = (0.0 - num5) / num3;
			point = new Point(num5, num6);
			point2 = new Point(0.0 - num5, 0.0 - num6);
		}
		else
		{
			double num7 = 0.0;
			double num8 = gateWidth / 2.0;
			point = new Point(num8, num7);
			point2 = new Point(0.0 - num8, 0.0 - num7);
		}
		return new SplitPoint(splitType, splitName, currentSample, new LocalTangentPlane(point.Y, point.X, 0.0), new LocalTangentPlane(point2.Y, point2.X, 0.0));
	}

	public SplitPoint(SplitType type, string name, LongitudeMinutes longitude, LatitudeMinutes latitude, LongitudeMinutes longitudeOld, LatitudeMinutes latitudeOld, string fileName = null)
	{
		this.type = type;
		this.name = name;
		this.fileName = fileName;
		xLongitude = longitude;
		yLatitude = latitude;
		xLongitudeMinus10 = longitudeOld;
		yLatitudeMinus10 = latitudeOld;
	}

	public SplitPoint(ISplitDefinition split)
	{
		type = split.Type;
		name = split.Name;
		fileName = split.FileName;
		yLatitude = split.LatitudeMinutes;
		xLongitude = split.LongitudeMinutes;
		yLatitudeMinus10 = split.LatitudeMinutesMinus10;
		xLongitudeMinus10 = split.LongitudeMinutesMinus10;
	}

	private SplitPoint(SplitType type, string name, Geodetic reference, LocalTangentPlane gateStartPoint, LocalTangentPlane gateEndPoint)
	{
		this.type = type;
		this.name = name;
		this.reference = reference;
		this.gateStartPoint = gateStartPoint;
		this.gateEndPoint = gateEndPoint;
		if (reference != null)
		{
			referenceEcef = reference.ToEcef2();
		}
	}

	public SplitPoint(SplitType type, string name, Geodetic reference, LocalTangentPlane gateStartPoint, LocalTangentPlane gateEndPoint, bool useegm96Geoid)
		: this(type, name, reference, gateStartPoint, gateEndPoint)
	{
		useEgm96Geoid = useegm96Geoid;
	}

	public bool Equals(SplitPoint other)
	{
		if (other != null && Type == other.Type && (double)LongitudeMinutes == (double)other.LongitudeMinutes && (double)LatitudeMinutes == (double)other.LatitudeMinutes && (double)LongitudeMinutesMinus10 == (double)other.LongitudeMinutesMinus10)
		{
			return (double)LatitudeMinutesMinus10 == (double)other.LatitudeMinutesMinus10;
		}
		return false;
	}

	public bool Equals(SplitPoint other, int resolution)
	{
		if (other != null && Type == other.type && Math.Round(LongitudeMinutes, resolution) == Math.Round(other.LongitudeMinutes, resolution) && Math.Round(LatitudeMinutes, resolution) == Math.Round(other.LatitudeMinutes, resolution) && Math.Round(LongitudeMinutesMinus10, resolution) == Math.Round(other.LongitudeMinutesMinus10, resolution))
		{
			return Math.Round(LatitudeMinutesMinus10, resolution) == Math.Round(other.LatitudeMinutesMinus10, resolution);
		}
		return false;
	}

	public override string ToString()
	{
		return Description;
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		reader.MoveToContent();
		Type = (SplitType)Enum.Parse(typeof(SplitType), reader.GetAttribute("Type"));
		Name = reader.GetAttribute("Name");
		FileName = reader.GetAttribute("FileName");
		bool isEmptyElement = reader.IsEmptyElement;
		reader.ReadStartElement();
		if (!isEmptyElement)
		{
			LongitudeMinutes = double.Parse(reader.ReadElementString());
			LongitudeMinutesMinus10 = double.Parse(reader.ReadElementString());
			LatitudeMinutes = double.Parse(reader.ReadElementString());
			LatitudeMinutesMinus10 = double.Parse(reader.ReadElementString());
			reader.ReadEndElement();
		}
		OnPropertyChanged("IsValid");
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("Type", Type.ToString());
		writer.WriteAttributeString("Name", Name);
		if (!string.IsNullOrWhiteSpace(FileName))
		{
			writer.WriteAttributeString("FileName", FileName);
		}
		writer.WriteElementString("LongitudeMinutes", ((double)LongitudeMinutes).ToString());
		writer.WriteElementString("LongitudeMinutesMinus10", ((double)LongitudeMinutesMinus10).ToString());
		writer.WriteElementString("LatitudeMinutes", ((double)LatitudeMinutes).ToString());
		writer.WriteElementString("LatitudeMinutesMinus10", ((double)LatitudeMinutesMinus10).ToString());
	}
}
