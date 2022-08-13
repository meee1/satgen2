using System.Collections.Generic;

namespace Racelogic.Libraries.Nmea;

public interface INmeaParser
{
	event GpsSampleReceived SampleReceived;

	event SerialDataReceived SerialDataReceived;

	void DataReceived(object sender, SerialDataReceivedEventArgs e);

	IEnumerable<GpsSample> ParseIncomingData(string s);
}
