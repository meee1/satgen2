namespace Racelogic.DataSource.Nmea;

public class GgaData
{
	public TimeSeconds UtcFixTime { get; set; }

	public LatitudeMinutes LatitudeMinutes { get; set; }

	public LongitudeMinutes LongitudeMinutes { get; set; }

	public NmeaFixQuality FixQuality { get; set; }

	public byte Satellites { get; set; }

	public double HorizontalDilutionOfPosition { get; set; }

	public DistanceMetres AltitudeAboveMeanSeaLevel { get; set; }

	public DistanceMetres HeightOfGeoidAboveWGS84Eellipsoid { get; set; }

	public void Clear()
	{
		UtcFixTime = 0.0;
		LatitudeMinutes = 0.0;
		LongitudeMinutes = 0.0;
		FixQuality = NmeaFixQuality.NoData;
		Satellites = 0;
		HorizontalDilutionOfPosition = 0.0;
		AltitudeAboveMeanSeaLevel = 0.0;
		HeightOfGeoidAboveWGS84Eellipsoid = 0.0;
	}
}
