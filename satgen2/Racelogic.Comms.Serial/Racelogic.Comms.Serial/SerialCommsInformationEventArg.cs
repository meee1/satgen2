using System;

namespace Racelogic.Comms.Serial;

public class SerialCommsInformationEventArgs : EventArgs
{
	public readonly InformationType InformationType;

	public readonly string Message;

	internal SerialCommsInformationEventArgs(InformationType informationType, string message)
	{
		InformationType = informationType;
		Message = message;
	}
}
