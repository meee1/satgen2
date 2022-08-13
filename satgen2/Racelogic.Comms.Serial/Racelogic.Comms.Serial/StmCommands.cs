namespace Racelogic.Comms.Serial;

internal enum StmCommands : byte
{
	GetVersionAndAllowedCommands = 0,
	GetVersionAndReadProtectionStatus = 1,
	GetId = 2,
	ReadMemory = 17,
	Go = 33,
	WriteMemory = 49,
	Erase = 67,
	EraseExtended = 68,
	WriteProtect = 99,
	WriteUnprotect = 115,
	ReadoutProtect = 130,
	ReadoutUnprotect = 146
}
