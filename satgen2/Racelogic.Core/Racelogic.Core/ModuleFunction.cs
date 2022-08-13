using System;

namespace Racelogic.Core;

[Flags]
public enum ModuleFunction
{
	None = 0,
	SupportsRacelogicCanModule = 1,
	SupportsModuleMode = 2,
	IsPhantomUnit = 4,
	SerialNumberFromList = 8,
	HasGpsEngine = 0x10,
	UsbSerial = 0x20,
	AllowDirectCommunicationWithGpsEngine = 0x40,
	PowerCycleAfterUpdate = 0x80,
	EncryptData = 0x100,
	SupportsExtendedSerialNumber = 0x200
}
