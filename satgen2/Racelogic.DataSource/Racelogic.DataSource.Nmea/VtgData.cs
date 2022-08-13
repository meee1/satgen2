namespace Racelogic.DataSource.Nmea;

public class VtgData
{
	public double TrueTrackMadeGood { get; set; }

	public double MagneticTrackMadeGood { get; set; }

	public double GroundSpeedKnots { get; set; }

	public SpeedKilometresPerHour GroundSpeedKilometresPerHour { get; set; }

	public NmeaModeIndicator ModeIndicator { get; set; }

	public void Clear()
	{
		TrueTrackMadeGood = 0.0;
		MagneticTrackMadeGood = 0.0;
		GroundSpeedKnots = 0.0;
		GroundSpeedKilometresPerHour = 0.0;
		ModeIndicator = NmeaModeIndicator.NoData;
	}
}
