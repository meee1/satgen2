using System;

namespace Racelogic.DataSource.Nmea;

public class RmcData
{
	public TimeSeconds UtcFixTime { get; set; }

	public NmeaActiveIndicator Status { get; set; }

	public LatitudeMinutes LatitudeMinutes { get; set; }

	public LongitudeMinutes LongitudeMinutes { get; set; }

	public double SpeedOverGroundKnots { get; set; }

	public double TrackAngle { get; set; }

	public DateTime Date { get; set; }

	public double MagneticVariation { get; set; }

	public Hemisphere MagneticVariationHemisphere { get; set; }

	public NmeaModeIndicator ModeIndicator { get; set; }

	public void Clear()
	{
		UtcFixTime = 0.0;
		Status = NmeaActiveIndicator.NoData;
		LatitudeMinutes = 0.0;
		LongitudeMinutes = 0.0;
		SpeedOverGroundKnots = 0.0;
		TrackAngle = 0.0;
		MagneticVariation = 0.0;
		MagneticVariationHemisphere = Hemisphere.NoData;
		ModeIndicator = NmeaModeIndicator.NoData;
	}
}
