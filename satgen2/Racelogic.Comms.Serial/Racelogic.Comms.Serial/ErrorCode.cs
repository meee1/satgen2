using System.ComponentModel;

namespace Racelogic.Comms.Serial;

[TypeConverter(typeof(LocalisedEnumeration))]
public enum ErrorCode : uint
{
	InvalidChecksum = 1u,
	UnknownCommand = 2u,
	UnknownSubCommand = 3u,
	FailedToUnlockMemory = 4u,
	CommandNotSupported = 5u,
	FileOperationInProgress = 6u,
	SpecifiedPathTooLong = 7u,
	FileDoesNotExist = 8u,
	FailedToGetFileInformation = 9u,
	FailedToReadFile = 10u,
	InvalidRequestData = 11u,
	FailedToOpenFileForWrite = 12u,
	ReceivedWriteBlockWithNoWriteInProgress = 13u,
	FailedToWriteBlock = 14u,
	InvalidBlockLength = 15u,
	FailedToDeleteFile = 16u,
	FailedToOpenDirectory = 17u,
	NoSpaceForDirectoryEntry = 18u,
	FailedToZipConfigurationDirectory = 19u,
	FailedToUnzipConfigurationFile = 20u,
	UnitLocked = 21u,
	UnitReceivedUnexpectedBlockOKMessage = 22u,
	ScreenshotGrabbingApplicationNotRunning = 23u,
	ScreentshotGrabbingTimedOut = 24u,
	ScreentshotGrabbingFailed = 25u,
	IncorrectPayloadLength = 26u,
	CommandNotEnabled = 27u,
	InvalidGateId = 28u,
	RangeError = 29u,
	UnknownError = uint.MaxValue
}
