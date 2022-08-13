using System;

namespace Racelogic.Utilities.Sntp;

public class QueryServerCompletedEventArgs : EventArgs
{
	public SntpData Data { get; internal set; }

	public ErrorData ErrorData { get; internal set; }

	public bool LocalDateTimeUpdated { get; internal set; }

	public bool Succeeded { get; internal set; }

	internal QueryServerCompletedEventArgs()
	{
		ErrorData = new ErrorData();
	}
}
