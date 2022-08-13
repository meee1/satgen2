using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Racelogic.Comms.Serial;

public static class StrCommands
{
	public static ReadOnlyCollection<KeyValuePair<string, StrCommandDefinition>> List;

	static StrCommands()
	{
		List<KeyValuePair<string, StrCommandDefinition>> list = new List<KeyValuePair<string, StrCommandDefinition>>
		{
			new KeyValuePair<string, StrCommandDefinition>("Hello", new StrCommandDefinition(0, 2, (short)6, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Download", new StrCommandDefinition(3, 2, null, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Upload", new StrCommandDefinition(4, 2, (short)12, null)),
			new KeyValuePair<string, StrCommandDefinition>("GPS Command", new StrCommandDefinition(6, 2, null, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("VBox Command", new StrCommandDefinition(7, 2, null, null)),
			new KeyValuePair<string, StrCommandDefinition>("Get seed", new StrCommandDefinition(18, 2, (short)6, (short)8)),
			new KeyValuePair<string, StrCommandDefinition>("Unlock", new StrCommandDefinition(19, 2, (short)8, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Initialise progressbar", new StrCommandDefinition(27, 2, (short)10, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Initialise", new StrCommandDefinition(28, 2, (short)22, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Get serialnumber", new StrCommandDefinition(32, 2, (short)6, (short)22)),
			new KeyValuePair<string, StrCommandDefinition>("Set serialnumber", new StrCommandDefinition(33, 2, (short)22, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Get hardware", new StrCommandDefinition(34, 2, (short)6, (short)10)),
			new KeyValuePair<string, StrCommandDefinition>("Set hardware", new StrCommandDefinition(35, 2, (short)10, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Get PCB information", new StrCommandDefinition(36, 2, (short)6, (short)262)),
			new KeyValuePair<string, StrCommandDefinition>("Set PCB information", new StrCommandDefinition(37, 2, (short)22, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Get Flash layout", new StrCommandDefinition(38, 2, (short)6, (short)262)),
			new KeyValuePair<string, StrCommandDefinition>("Set Flash layout", new StrCommandDefinition(39, 2, (short)262, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Get unit information", new StrCommandDefinition(40, 2, (short)6, (short)262)),
			new KeyValuePair<string, StrCommandDefinition>("Set unit information", new StrCommandDefinition(41, 2, (short)262, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Reset", new StrCommandDefinition(80, 2, (short)6, 0)),
			new KeyValuePair<string, StrCommandDefinition>("Download end", new StrCommandDefinition(81, 2, (short)6, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Set security", new StrCommandDefinition(94, 2, (short)7, 0)),
			new KeyValuePair<string, StrCommandDefinition>("Get security", new StrCommandDefinition(92, 2, (short)6, (short)11)),
			new KeyValuePair<string, StrCommandDefinition>("Checksum", new StrCommandDefinition(197, 2, (short)14, (short)10)),
			new KeyValuePair<string, StrCommandDefinition>("Erase flash", new StrCommandDefinition(229, 6, (short)8, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Erase flash MkII", new StrCommandDefinition(230, 6, (short)8, (short)6)),
			new KeyValuePair<string, StrCommandDefinition>("Mass Erase", new StrCommandDefinition(231, 2, (short)6, 0)),
			new KeyValuePair<string, StrCommandDefinition>("Erase App", new StrCommandDefinition(234, 2, (short)6, (short)6))
		};
		List = new ReadOnlyCollection<KeyValuePair<string, StrCommandDefinition>>(list);
	}
}
