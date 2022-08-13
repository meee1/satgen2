using System.Collections.Generic;

namespace Racelogic.DataSource.Nmea;

public class GsaData
{
	public NmeaFixSelection FixSelection { get; set; }

	public Nmea3DFix Fix { get; set; }

	public List<byte> SatellitePrn { get; set; }

	public double Pdop { get; set; }

	public double Hdop { get; set; }

	public double Vdop { get; set; }

	public GsaData()
	{
		SatellitePrn = new List<byte>();
	}

	public void Clear()
	{
		FixSelection = NmeaFixSelection.NoData;
		Fix = Nmea3DFix.NoData;
		SatellitePrn.Clear();
		Pdop = 0.0;
		Hdop = 0.0;
		Vdop = 0.0;
	}
}
