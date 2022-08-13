using System.Collections.Generic;

namespace Racelogic.Comms.Serial;

internal static class CanBootloaderCommands
{
	internal static List<KeyValuePair<string, CanBootloaderCommandDefinition>> List;

	static CanBootloaderCommands()
	{
		List<KeyValuePair<string, CanBootloaderCommandDefinition>> collection = new List<KeyValuePair<string, CanBootloaderCommandDefinition>>
		{
			new KeyValuePair<string, CanBootloaderCommandDefinition>("InitializeProgramming", new CanBootloaderCommandDefinition(5592405, new byte[8] { 82, 76, 67, 66, 50, 48, 48, 54 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("EraseChip", new CanBootloaderCommandDefinition(5592406, new byte[8] { 69, 114, 97, 115, 101, 45, 45, 45 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("NotUsed", new CanBootloaderCommandDefinition(5592407, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("SetFlashAddress", new CanBootloaderCommandDefinition(5592408, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("DataTransfer", new CanBootloaderCommandDefinition(5592409, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("Run", new CanBootloaderCommandDefinition(5592410, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("SetFlashAddress2", new CanBootloaderCommandDefinition(5592411, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 })),
			new KeyValuePair<string, CanBootloaderCommandDefinition>("DataTransfer2", new CanBootloaderCommandDefinition(5592412, new byte[8] { 32, 32, 32, 32, 32, 32, 32, 32 }))
		};
		List = new List<KeyValuePair<string, CanBootloaderCommandDefinition>>(collection);
	}
}
