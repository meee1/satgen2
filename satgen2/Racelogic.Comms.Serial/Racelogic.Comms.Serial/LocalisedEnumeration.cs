using System;
using Racelogic.Comms.Serial.Properties;
using Racelogic.Core;

namespace Racelogic.Comms.Serial;

public class LocalisedEnumeration : GlobalisedEnumConverter
{
	public LocalisedEnumeration(Type type)
		: base(type, Racelogic.Comms.Serial.Properties.Resources.ResourceManager)
	{
	}
}
