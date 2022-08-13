namespace Racelogic.DataSource.Nmea;

public class GllData
{
	public LatitudeMinutes LatitudeMinutes { get; set; }

	public LongitudeMinutes LongitudeMinutes { get; set; }

	public TimeSeconds UtcFixTime { get; set; }

	public NmeaActiveIndicator Status { get; set; }

	public NmeaModeIndicator ModeIndicator { get; set; }

	public void Clear()
	{
		LatitudeMinutes = 0.0;
		LongitudeMinutes = 0.0;
		UtcFixTime = 0.0;
		Status = NmeaActiveIndicator.NoData;
		ModeIndicator = NmeaModeIndicator.NoData;
	}
}
