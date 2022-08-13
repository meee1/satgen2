namespace Racelogic.DataSource.Nmea;

public class TxtData
{
	public string[] Data { get; set; }

	public void Clear()
	{
		Data = new string[0];
	}
}
