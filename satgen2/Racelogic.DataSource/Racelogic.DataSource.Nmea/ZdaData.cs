using System;

namespace Racelogic.DataSource.Nmea;

public class ZdaData
{
	public TimeSeconds Utctime { get; set; }

	public DateTime Date { get; set; }

	public byte LocalZoneHours { get; set; }

	public byte LocalZoneMinutes { get; set; }

	public void Clear()
	{
		Utctime = 0.0;
		LocalZoneHours = 0;
		LocalZoneMinutes = 0;
	}
}
