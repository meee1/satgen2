using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Racelogic.Comms.Serial.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = (resourceMan = new ResourceManager("Racelogic.Comms.Serial.Properties.Resources", typeof(Resources).Assembly));
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static string AdasSync_AllTargets => ResourceManager.GetString("AdasSync_AllTargets", resourceCulture);

	public static string AdasSync_Target1 => ResourceManager.GetString("AdasSync_Target1", resourceCulture);

	public static string AdasSync_Target2 => ResourceManager.GetString("AdasSync_Target2", resourceCulture);

	public static string AdasSync_Target3 => ResourceManager.GetString("AdasSync_Target3", resourceCulture);

	public static string BlockRequestFail => ResourceManager.GetString("BlockRequestFail", resourceCulture);

	public static string BluetoothFirmware => ResourceManager.GetString("BluetoothFirmware", resourceCulture);

	public static string Bootloader => ResourceManager.GetString("Bootloader", resourceCulture);

	public static string Bootstrap => ResourceManager.GetString("Bootstrap", resourceCulture);

	public static string CancelFileProgress => ResourceManager.GetString("CancelFileProgress", resourceCulture);

	public static string CancelToAbort => ResourceManager.GetString("CancelToAbort", resourceCulture);

	public static string ClearingRam => ResourceManager.GetString("ClearingRam", resourceCulture);

	public static string CommandNotRecognised => ResourceManager.GetString("CommandNotRecognised", resourceCulture);

	public static string CommandNotSupported => ResourceManager.GetString("CommandNotSupported", resourceCulture);

	public static string CommunicationError => ResourceManager.GetString("CommunicationError", resourceCulture);

	public static string ComPortStatus_Closed => ResourceManager.GetString("ComPortStatus_Closed", resourceCulture);

	public static string ComPortStatus_Opened => ResourceManager.GetString("ComPortStatus_Opened", resourceCulture);

	public static string CrcError => ResourceManager.GetString("CrcError", resourceCulture);

	public static string DataLengthError => ResourceManager.GetString("DataLengthError", resourceCulture);

	public static string DownloadDataFromUnit => ResourceManager.GetString("DownloadDataFromUnit", resourceCulture);

	public static string DownloadEepromError => ResourceManager.GetString("DownloadEepromError", resourceCulture);

	public static string DownloadingRam => ResourceManager.GetString("DownloadingRam", resourceCulture);

	public static string DownloadingScene => ResourceManager.GetString("DownloadingScene", resourceCulture);

	public static string DownloadingScreentshot => ResourceManager.GetString("DownloadingScreentshot", resourceCulture);

	public static string DownloadLaneError => ResourceManager.GetString("DownloadLaneError", resourceCulture);

	public static string EraseData => ResourceManager.GetString("EraseData", resourceCulture);

	public static string EraseDataFail => ResourceManager.GetString("EraseDataFail", resourceCulture);

	public static string EraseDataVerifyFail => ResourceManager.GetString("EraseDataVerifyFail", resourceCulture);

	public static string Error => ResourceManager.GetString("Error", resourceCulture);

	public static string ErrorCode_CommandNotEnabled => ResourceManager.GetString("ErrorCode_CommandNotEnabled", resourceCulture);

	public static string ErrorCode_CommandNotSupported => ResourceManager.GetString("ErrorCode_CommandNotSupported", resourceCulture);

	public static string ErrorCode_FailedToDeleteFile => ResourceManager.GetString("ErrorCode_FailedToDeleteFile", resourceCulture);

	public static string ErrorCode_FailedToGetFileInformation => ResourceManager.GetString("ErrorCode_FailedToGetFileInformation", resourceCulture);

	public static string ErrorCode_FailedToOpenDirectory => ResourceManager.GetString("ErrorCode_FailedToOpenDirectory", resourceCulture);

	public static string ErrorCode_FailedToOpenFileForWrite => ResourceManager.GetString("ErrorCode_FailedToOpenFileForWrite", resourceCulture);

	public static string ErrorCode_FailedToReadFile => ResourceManager.GetString("ErrorCode_FailedToReadFile", resourceCulture);

	public static string ErrorCode_FailedToUnlockMemory => ResourceManager.GetString("ErrorCode_FailedToUnlockMemory", resourceCulture);

	public static string ErrorCode_FailedToUnzipConfigurationFile => ResourceManager.GetString("ErrorCode_FailedToUnzipConfigurationFile", resourceCulture);

	public static string ErrorCode_FailedToWriteBlock => ResourceManager.GetString("ErrorCode_FailedToWriteBlock", resourceCulture);

	public static string ErrorCode_FailedToZipConfigurationDirectory => ResourceManager.GetString("ErrorCode_FailedToZipConfigurationDirectory", resourceCulture);

	public static string ErrorCode_FileDoesNotExist => ResourceManager.GetString("ErrorCode_FileDoesNotExist", resourceCulture);

	public static string ErrorCode_FileOperationInProgress => ResourceManager.GetString("ErrorCode_FileOperationInProgress", resourceCulture);

	public static string ErrorCode_IncorrectPayloadLength => ResourceManager.GetString("ErrorCode_IncorrectPayloadLength", resourceCulture);

	public static string ErrorCode_InvalidBlockLength => ResourceManager.GetString("ErrorCode_InvalidBlockLength", resourceCulture);

	public static string ErrorCode_InvalidChecksum => ResourceManager.GetString("ErrorCode_InvalidChecksum", resourceCulture);

	public static string ErrorCode_InvalidGateId => ResourceManager.GetString("ErrorCode_InvalidGateId", resourceCulture);

	public static string ErrorCode_InvalidRequestData => ResourceManager.GetString("ErrorCode_InvalidRequestData", resourceCulture);

	public static string ErrorCode_NoSpaceForDirectoryEntry => ResourceManager.GetString("ErrorCode_NoSpaceForDirectoryEntry", resourceCulture);

	public static string ErrorCode_RangeError => ResourceManager.GetString("ErrorCode_RangeError", resourceCulture);

	public static string ErrorCode_ReceivedWriteBlockWithNoWriteInProgress => ResourceManager.GetString("ErrorCode_ReceivedWriteBlockWithNoWriteInProgress", resourceCulture);

	public static string ErrorCode_ScreenshotGrabbingApplicationNotRunning => ResourceManager.GetString("ErrorCode_ScreenshotGrabbingApplicationNotRunning", resourceCulture);

	public static string ErrorCode_ScreentshotGrabbingFailed => ResourceManager.GetString("ErrorCode_ScreentshotGrabbingFailed", resourceCulture);

	public static string ErrorCode_ScreentshotGrabbingTimedOut => ResourceManager.GetString("ErrorCode_ScreentshotGrabbingTimedOut", resourceCulture);

	public static string ErrorCode_SpecifiedPathTooLong => ResourceManager.GetString("ErrorCode_SpecifiedPathTooLong", resourceCulture);

	public static string ErrorCode_UnitLocked => ResourceManager.GetString("ErrorCode_UnitLocked", resourceCulture);

	public static string ErrorCode_UnitReceivedUnexpectedBlockOKMessage => ResourceManager.GetString("ErrorCode_UnitReceivedUnexpectedBlockOKMessage", resourceCulture);

	public static string ErrorCode_UnknownCommand => ResourceManager.GetString("ErrorCode_UnknownCommand", resourceCulture);

	public static string ErrorCode_UnknownError => ResourceManager.GetString("ErrorCode_UnknownError", resourceCulture);

	public static string ErrorCode_UnknownSubCommand => ResourceManager.GetString("ErrorCode_UnknownSubCommand", resourceCulture);

	public static string ErrorReadingPortDescription => ResourceManager.GetString("ErrorReadingPortDescription", resourceCulture);

	public static string FailChecksum => ResourceManager.GetString("FailChecksum", resourceCulture);

	public static string FailChecksumDescription => ResourceManager.GetString("FailChecksumDescription", resourceCulture);

	public static string FailedtoClearSecurity => ResourceManager.GetString("FailedtoClearSecurity", resourceCulture);

	public static string FailedToGetHardwareVersion => ResourceManager.GetString("FailedToGetHardwareVersion", resourceCulture);

	public static string FailedToGetSecurity => ResourceManager.GetString("FailedToGetSecurity", resourceCulture);

	public static string FailSetSecurity => ResourceManager.GetString("FailSetSecurity", resourceCulture);

	public static string FailSetSecurityDescription => ResourceManager.GetString("FailSetSecurityDescription", resourceCulture);

	public static string FailSetSerial => ResourceManager.GetString("FailSetSerial", resourceCulture);

	public static string FailSetSerialDescription => ResourceManager.GetString("FailSetSerialDescription", resourceCulture);

	public static string FailUnlock => ResourceManager.GetString("FailUnlock", resourceCulture);

	public static string FailUnlockDescription => ResourceManager.GetString("FailUnlockDescription", resourceCulture);

	public static string Finish => ResourceManager.GetString("Finish", resourceCulture);

	public static string FirmwareRevision => ResourceManager.GetString("FirmwareRevision", resourceCulture);

	public static string FirmwareVersion => ResourceManager.GetString("FirmwareVersion", resourceCulture);

	public static string FixWmiErrorQuery => ResourceManager.GetString("FixWmiErrorQuery", resourceCulture);

	public static string FrontApplication => ResourceManager.GetString("FrontApplication", resourceCulture);

	public static string FrontPanelHardwareRevision => ResourceManager.GetString("FrontPanelHardwareRevision", resourceCulture);

	public static string FrontPanelVersion => ResourceManager.GetString("FrontPanelVersion", resourceCulture);

	public static string GetConfigurationFile => ResourceManager.GetString("GetConfigurationFile", resourceCulture);

	public static string GetConfigurationFileSizeError => ResourceManager.GetString("GetConfigurationFileSizeError", resourceCulture);

	public static string GetConfigurationFileSizeErrorIncorrectResponse => ResourceManager.GetString("GetConfigurationFileSizeErrorIncorrectResponse", resourceCulture);

	public static string GetEepromSize => ResourceManager.GetString("GetEepromSize", resourceCulture);

	public static string GetFirmwareString => ResourceManager.GetString("GetFirmwareString", resourceCulture);

	public static string GetFirmwareVersion => ResourceManager.GetString("GetFirmwareVersion", resourceCulture);

	public static string GetPortsError => ResourceManager.GetString("GetPortsError", resourceCulture);

	public static string GetScreenShot => ResourceManager.GetString("GetScreenShot", resourceCulture);

	public static string GetSerialNumber => ResourceManager.GetString("GetSerialNumber", resourceCulture);

	public static string GpsEngineNumber_One => ResourceManager.GetString("GpsEngineNumber_One", resourceCulture);

	public static string GpsEngineNumber_Two => ResourceManager.GetString("GpsEngineNumber_Two", resourceCulture);

	public static string HardwareCode => ResourceManager.GetString("HardwareCode", resourceCulture);

	public static string HardwareRevision => ResourceManager.GetString("HardwareRevision", resourceCulture);

	public static string IncorrectNumberOfBytesReceived => ResourceManager.GetString("IncorrectNumberOfBytesReceived", resourceCulture);

	public static string IncorrectResponse => ResourceManager.GetString("IncorrectResponse", resourceCulture);

	public static string IncorrectResponseDescription => ResourceManager.GetString("IncorrectResponseDescription", resourceCulture);

	public static string Information => ResourceManager.GetString("Information", resourceCulture);

	public static string InformationType_Error => ResourceManager.GetString("InformationType_Error", resourceCulture);

	public static string InformationType_Information => ResourceManager.GetString("InformationType_Information", resourceCulture);

	public static string InformationType_Warning => ResourceManager.GetString("InformationType_Warning", resourceCulture);

	public static string InternalBatteryVoltage => ResourceManager.GetString("InternalBatteryVoltage", resourceCulture);

	public static string InvalidAddress => ResourceManager.GetString("InvalidAddress", resourceCulture);

	public static string InvalidAddressDescription => ResourceManager.GetString("InvalidAddressDescription", resourceCulture);

	public static string InvalidChecksum => ResourceManager.GetString("InvalidChecksum", resourceCulture);

	public static string InvalidMemoryType => ResourceManager.GetString("InvalidMemoryType", resourceCulture);

	public static string InvalidNumberOfBytesInResponse => ResourceManager.GetString("InvalidNumberOfBytesInResponse", resourceCulture);

	public static string InvalidResponseId => ResourceManager.GetString("InvalidResponseId", resourceCulture);

	public static string InvalidSectors => ResourceManager.GetString("InvalidSectors", resourceCulture);

	public static string InvalidSectorsDescription => ResourceManager.GetString("InvalidSectorsDescription", resourceCulture);

	public static string InvalidUploadDataCount => ResourceManager.GetString("InvalidUploadDataCount", resourceCulture);

	public static string LastUpdatedBy => ResourceManager.GetString("LastUpdatedBy", resourceCulture);

	public static string MainApplication => ResourceManager.GetString("MainApplication", resourceCulture);

	public static string MakeNoise => ResourceManager.GetString("MakeNoise", resourceCulture);

	public static string MakeQuiet => ResourceManager.GetString("MakeQuiet", resourceCulture);

	public static string MemoryLocked => ResourceManager.GetString("MemoryLocked", resourceCulture);

	public static string MemoryLockedDescription => ResourceManager.GetString("MemoryLockedDescription", resourceCulture);

	public static string MemoryNotRead => ResourceManager.GetString("MemoryNotRead", resourceCulture);

	public static string NoResponse => ResourceManager.GetString("NoResponse", resourceCulture);

	public static string NoResponseDescription => ResourceManager.GetString("NoResponseDescription", resourceCulture);

	public static string PortNameDoesNotExist => ResourceManager.GetString("PortNameDoesNotExist", resourceCulture);

	public static string PortNameIsNull => ResourceManager.GetString("PortNameIsNull", resourceCulture);

	public static string PortNotOpen => ResourceManager.GetString("PortNotOpen", resourceCulture);

	public static string PowerCycleLabSat => ResourceManager.GetString("PowerCycleLabSat", resourceCulture);

	public static string PowerSupply => ResourceManager.GetString("PowerSupply", resourceCulture);

	public static string ProgramFlashFail => ResourceManager.GetString("ProgramFlashFail", resourceCulture);

	public static string ReadDataFromModule => ResourceManager.GetString("ReadDataFromModule", resourceCulture);

	public static string ReadingAvailableCanParametersFromMfd => ResourceManager.GetString("ReadingAvailableCanParametersFromMfd", resourceCulture);

	public static string ReadingAvailableParametersFromMfd => ResourceManager.GetString("ReadingAvailableParametersFromMfd", resourceCulture);

	public static string ReadingBluetoothFirmware => ResourceManager.GetString("ReadingBluetoothFirmware", resourceCulture);

	public static string ReadingDataFromMfd => ResourceManager.GetString("ReadingDataFromMfd", resourceCulture);

	public static string ReadingDataFromVBoxManager => ResourceManager.GetString("ReadingDataFromVBoxManager", resourceCulture);

	public static string ReadingFlashLayout => ResourceManager.GetString("ReadingFlashLayout", resourceCulture);

	public static string ReadingFrontPanelHardware => ResourceManager.GetString("ReadingFrontPanelHardware", resourceCulture);

	public static string ReadingGpsRevision => ResourceManager.GetString("ReadingGpsRevision", resourceCulture);

	public static string ReadingHardwareRevisions => ResourceManager.GetString("ReadingHardwareRevisions", resourceCulture);

	public static string ReadingLapTimesFromMfd => ResourceManager.GetString("ReadingLapTimesFromMfd", resourceCulture);

	public static string ReadingPcbInformation => ResourceManager.GetString("ReadingPcbInformation", resourceCulture);

	public static string ReadingPreSetTestSettings => ResourceManager.GetString("ReadingPreSetTestSettings", resourceCulture);

	public static string ReadingRealTimeClock => ResourceManager.GetString("ReadingRealTimeClock", resourceCulture);

	public static string ReadingSerialNumber => ResourceManager.GetString("ReadingSerialNumber", resourceCulture);

	public static string ReadingUnitInformation => ResourceManager.GetString("ReadingUnitInformation", resourceCulture);

	public static string ReceiveBufferFull => ResourceManager.GetString("ReceiveBufferFull", resourceCulture);

	public static string ReinitialisingCan => ResourceManager.GetString("ReinitialisingCan", resourceCulture);

	public static string ReloadingEeprom => ResourceManager.GetString("ReloadingEeprom", resourceCulture);

	public static string RequestingCalibration => ResourceManager.GetString("RequestingCalibration", resourceCulture);

	public static string RequestSceneList => ResourceManager.GetString("RequestSceneList", resourceCulture);

	public static string RequestSceneName => ResourceManager.GetString("RequestSceneName", resourceCulture);

	public static string ResponseTimeout => ResourceManager.GetString("ResponseTimeout", resourceCulture);

	public static string Revision => ResourceManager.GetString("Revision", resourceCulture);

	public static string ScanningCanBus => ResourceManager.GetString("ScanningCanBus", resourceCulture);

	public static string SecurityCleared => ResourceManager.GetString("SecurityCleared", resourceCulture);

	public static string SecurityDisabled => ResourceManager.GetString("SecurityDisabled", resourceCulture);

	public static string SecurityEnabled => ResourceManager.GetString("SecurityEnabled", resourceCulture);

	public static string SecurityLevelRetrieved => ResourceManager.GetString("SecurityLevelRetrieved", resourceCulture);

	public static string SendCommandFail => ResourceManager.GetString("SendCommandFail", resourceCulture);

	public static string SendingAdasSetupToTarget => ResourceManager.GetString("SendingAdasSetupToTarget", resourceCulture);

	public static string SendingMessageToGpsEngine => ResourceManager.GetString("SendingMessageToGpsEngine", resourceCulture);

	public static string SendingMessageToGpsEngine2 => ResourceManager.GetString("SendingMessageToGpsEngine2", resourceCulture);

	public static string SetConfigurationFile => ResourceManager.GetString("SetConfigurationFile", resourceCulture);

	public static string SetConfigurationFileSizeError => ResourceManager.GetString("SetConfigurationFileSizeError", resourceCulture);

	public static string SetOnScreenDisplay => ResourceManager.GetString("SetOnScreenDisplay", resourceCulture);

	public static string SetSceneIndex => ResourceManager.GetString("SetSceneIndex", resourceCulture);

	public static string Split => ResourceManager.GetString("Split", resourceCulture);

	public static string StartFinish => ResourceManager.GetString("StartFinish", resourceCulture);

	public static string StartGpsEngine => ResourceManager.GetString("StartGpsEngine", resourceCulture);

	public static string StopGpsEngine => ResourceManager.GetString("StopGpsEngine", resourceCulture);

	public static string UnableToOpenPort => ResourceManager.GetString("UnableToOpenPort", resourceCulture);

	public static string Unavailable => ResourceManager.GetString("Unavailable", resourceCulture);

	public static string UnitDisconnected => ResourceManager.GetString("UnitDisconnected", resourceCulture);

	public static string UnknownCommand => ResourceManager.GetString("UnknownCommand", resourceCulture);

	public static string UnknownCommandDescription => ResourceManager.GetString("UnknownCommandDescription", resourceCulture);

	public static string UnknownResponseLength => ResourceManager.GetString("UnknownResponseLength", resourceCulture);

	public static string Unlock => ResourceManager.GetString("Unlock", resourceCulture);

	public static string UnrecognisedCommand => ResourceManager.GetString("UnrecognisedCommand", resourceCulture);

	public static string UploadConfirmationFail => ResourceManager.GetString("UploadConfirmationFail", resourceCulture);

	public static string UploadDataToUnit => ResourceManager.GetString("UploadDataToUnit", resourceCulture);

	public static string UploadDataVerifyFail => ResourceManager.GetString("UploadDataVerifyFail", resourceCulture);

	public static string UploadingScene => ResourceManager.GetString("UploadingScene", resourceCulture);

	public static string UploadLength => ResourceManager.GetString("UploadLength", resourceCulture);

	public static string UploadLengthDescription => ResourceManager.GetString("UploadLengthDescription", resourceCulture);

	public static string VideoVBoxConfigFileInUse => ResourceManager.GetString("VideoVBoxConfigFileInUse", resourceCulture);

	public static string Volts => ResourceManager.GetString("Volts", resourceCulture);

	public static string WaitingUploadConfirmation => ResourceManager.GetString("WaitingUploadConfirmation", resourceCulture);

	public static string WmiError => ResourceManager.GetString("WmiError", resourceCulture);

	public static string WriteDataToModule => ResourceManager.GetString("WriteDataToModule", resourceCulture);

	public static string WriteFileBlockFail => ResourceManager.GetString("WriteFileBlockFail", resourceCulture);

	public static string WritingDataToMfd => ResourceManager.GetString("WritingDataToMfd", resourceCulture);

	public static string WritingDataToVBoxManager => ResourceManager.GetString("WritingDataToVBoxManager", resourceCulture);

	public static string XilinxCode => ResourceManager.GetString("XilinxCode", resourceCulture);

	public static string ZippedConfigFileDoesNotExist => ResourceManager.GetString("ZippedConfigFileDoesNotExist", resourceCulture);

	internal Resources()
	{
	}
}
