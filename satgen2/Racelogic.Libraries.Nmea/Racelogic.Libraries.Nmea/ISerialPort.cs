using System;

namespace Racelogic.Libraries.Nmea;

public interface ISerialPort
{
	int BaudRate { get; set; }

	bool IsOpen { get; }

	string PortName { get; set; }

	event SerialDataReceived DataReceived;

	event EventHandler PortOpened;

	event EventHandler PortClosed;

	void Close();

	void Open();

	string ReadExisting();

	void Write(byte[] buffer, int offset, int count);
}
