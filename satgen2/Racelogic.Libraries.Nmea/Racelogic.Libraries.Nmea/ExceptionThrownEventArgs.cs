using System;

namespace Racelogic.Libraries.Nmea;

public class ExceptionThrownEventArgs : EventArgs
{
	public Exception Exception { get; private set; }

	public ExceptionThrownEventArgs(Exception e)
	{
		Exception = e;
	}
}
