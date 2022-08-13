namespace Racelogic.Comms.Serial;

public enum CanModuleConfigBits
{
	SendData = 0,
	ScaleOffset = 1,
	ExtraInfoZeroToThree = 2,
	NameZeroToSeven = 3,
	UnitsZeroToSeven = 4,
	NameUnitsExtraInfoFour = 5,
	GroupPollId = 6,
	ModuleData = 7,
	MfdComms = 8,
	SaveEeprom = 38,
	ConfigureCanHub = 48
}
