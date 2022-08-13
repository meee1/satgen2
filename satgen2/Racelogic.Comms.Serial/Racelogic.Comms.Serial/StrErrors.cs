using System.Collections.Generic;
using System.Collections.ObjectModel;
using Racelogic.Comms.Serial.Properties;

namespace Racelogic.Comms.Serial;

public static class StrErrors
{
	public static ReadOnlyCollection<StrErrorDefinition> List;

	static StrErrors()
	{
		List<StrErrorDefinition> list = new List<StrErrorDefinition>
		{
			new StrErrorDefinition(Resources.NoResponse, Resources.NoResponseDescription),
			new StrErrorDefinition(Resources.IncorrectResponse, Resources.IncorrectResponseDescription),
			new StrErrorDefinition(Resources.FailChecksum, Resources.FailChecksumDescription),
			new StrErrorDefinition(Resources.UnknownCommand, Resources.UnknownCommandDescription),
			new StrErrorDefinition(Resources.FailUnlock, Resources.FailUnlockDescription),
			new StrErrorDefinition(Resources.MemoryLocked, Resources.MemoryLockedDescription),
			new StrErrorDefinition(Resources.InvalidAddress, Resources.InvalidAddressDescription),
			new StrErrorDefinition(Resources.UploadLength, Resources.UploadLengthDescription),
			new StrErrorDefinition(Resources.InvalidSectors, Resources.InvalidSectorsDescription),
			new StrErrorDefinition(Resources.FailSetSerial, Resources.FailSetSerialDescription),
			new StrErrorDefinition(Resources.FailSetSecurity, Resources.FailSetSecurityDescription)
		};
		List = new ReadOnlyCollection<StrErrorDefinition>(list);
	}
}
