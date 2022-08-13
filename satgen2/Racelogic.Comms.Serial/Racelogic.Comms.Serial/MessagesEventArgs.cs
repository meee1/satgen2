using System;

namespace Racelogic.Comms.Serial;

public class MessagesEventArgs : EventArgs
{
	public readonly uint MessagesSent;

	public readonly string Status;

	public readonly double PercentComplete;

	public MessagesEventArgs(uint messagesSent, string status)
		: this(messagesSent, status, 0.0)
	{
	}

	public MessagesEventArgs(uint messagesSent, string status, double percentComplete)
	{
		MessagesSent = messagesSent;
		Status = status;
		PercentComplete = percentComplete;
	}
}
