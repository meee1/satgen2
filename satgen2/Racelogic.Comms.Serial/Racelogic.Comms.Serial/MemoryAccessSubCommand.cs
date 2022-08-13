namespace Racelogic.Comms.Serial;

public enum MemoryAccessSubCommand
{
	GetSeed = 1,
	Unlock,
	Lock,
	GetMemorySize,
	UploadDataToUnit,
	DownloadDataFromUnit,
	EraseUnitMemory,
	WritePage,
	BootloaderMode,
	GetSystemMemory
}
