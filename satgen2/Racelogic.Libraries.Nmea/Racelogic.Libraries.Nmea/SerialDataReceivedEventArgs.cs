using System.IO.Ports;

namespace Racelogic.Libraries.Nmea;

public class SerialDataReceivedEventArgs
{
	public string Data { get; set; }

	public SerialData EventType { get; set; }

	public SerialDataReceivedEventArgs(SerialData eventType)
	{
		EventType = eventType;
	}

	public SerialDataReceivedEventArgs(SerialData eventType, string data)
		: this(eventType)
	{
		Data = data;
	}
}
