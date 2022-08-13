namespace Racelogic.Comms.Serial;

public enum SerialMessageStatus
{
	Ok,
	CRCError,
	UnexpectedResponseReceived,
	CommandNotRecognised,
	CommandNotSupported,
	ErrorInCommand,
	Timeout
}
