namespace Racelogic.Comms.Serial;

public enum TestSubCommand
{
	SetanalogueOutput = 1,
	SetDigitalOutput,
	GetTestCanStatus,
	GetInputs,
	SetRequestInputs,
	GetEepromStatus,
	EnterTestMode,
	SetGatePosition,
	LocalGyroCalibration,
	LocalGyroCalibrationStatus
}
