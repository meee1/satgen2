using System;

namespace Racelogic.Comms.Serial;

public class VBoxMessageEventArgs : EventArgs
{
	public readonly SerialMessageStatus Status;

	internal VBoxMessageEventArgs(SerialMessageStatus status)
	{
		Status = status;
	}
}
