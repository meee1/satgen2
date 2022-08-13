namespace Racelogic.Comms.Serial;

public enum Header
{
	None,
	NewCan,
	NewPosition,
	VBox,
	Nmea,
	RacelogicCommand,
	RacelogicResponse
}
