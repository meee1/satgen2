using System;

namespace Racelogic.Comms.Serial;

public class NewCanMessageEventArgs : EventArgs
{
	public readonly CanData Data;

	public readonly SerialMessageStatus Status;

	public NewCanMessageEventArgs(CanData data, SerialMessageStatus status)
	{
		Data = data;
		Status = status;
	}
}
