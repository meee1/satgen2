using System;

namespace Racelogic.Comms;

public class RacelogicSerialPortException : Exception
{
	public RacelogicSerialPortException(string message)
		: base(message)
	{
	}

	public RacelogicSerialPortException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
