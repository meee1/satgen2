namespace Racelogic.DataSource.Nmea;

public class GsvData
{
	public byte SatellitesInView { get; set; }

	public SatelliteInfo[] SatelliteInformation { get; set; }

	public void Clear()
	{
		SatellitesInView = 0;
		SatelliteInformation = new SatelliteInfo[0];
	}
}
