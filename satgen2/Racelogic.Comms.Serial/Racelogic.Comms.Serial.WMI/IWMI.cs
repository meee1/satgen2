using System.Collections.Generic;

namespace Racelogic.Comms.Serial.WMI;

internal interface IWMI
{
	IList<string> GetPropertyValues();
}
