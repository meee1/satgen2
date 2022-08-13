using System;
using Racelogic.Core;

namespace Racelogic.DataSource;

[Serializable]
public class UnitInformation
{
	private byte? unitType;

	private int? serialNumber;

	private char? subType;

	private int firmwareVersion;

	private string firmwareString;

	private string gpsFirmware;

	private GpsEngineType gpsEngine;

	public byte? UnitType
	{
		get
		{
			return unitType;
		}
		set
		{
			unitType = value;
		}
	}

	public int? SerialNumber
	{
		get
		{
			return serialNumber;
		}
		set
		{
			serialNumber = value;
		}
	}

	public char? SubType
	{
		get
		{
			return subType;
		}
		set
		{
			subType = value;
		}
	}

	public int FirmwareVersion
	{
		get
		{
			return firmwareVersion;
		}
		set
		{
			firmwareVersion = value;
		}
	}

	public string FirmwareString
	{
		get
		{
			return firmwareString;
		}
		set
		{
			firmwareString = value;
		}
	}

	public Version Version => new Version((byte)(FirmwareVersion >> 24), (byte)(FirmwareVersion >> 16), (ushort)FirmwareVersion);

	public string GpsFirmware
	{
		get
		{
			return gpsFirmware;
		}
		set
		{
			gpsFirmware = value;
		}
	}

	public GpsEngineType GpsEngine
	{
		get
		{
			return gpsEngine;
		}
		set
		{
			gpsEngine = value;
		}
	}

	public UnitInformation()
	{
		unitType = null;
		serialNumber = null;
		subType = null;
		gpsEngine = GpsEngineType.Unknown;
		firmwareVersion = 0;
		firmwareString = string.Empty;
	}
}
