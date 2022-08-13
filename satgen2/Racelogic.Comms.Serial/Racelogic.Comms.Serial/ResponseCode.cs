namespace Racelogic.Comms.Serial;

public enum ResponseCode
{
	CommandNotRecognised = 0,
	CommandRecognisedButWithError = 1,
	CommandNotSupported = 2,
	KnownFault = 3,
	InterimResponse = 128,
	CopyBlockResponse = 203,
	ErrorMessage = 254,
	OK = 255
}
