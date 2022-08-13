namespace Racelogic.DataSource.Nmea;

public struct SatelliteInfo
{
	public byte PrnNumber { get; set; }

	public byte Elevation { get; set; }

	public int Azimuth { get; set; }

	public byte Snr { get; set; }

	public NmeaMessages NmeaMessageValue => NmeaMessages.Gsv;
}
