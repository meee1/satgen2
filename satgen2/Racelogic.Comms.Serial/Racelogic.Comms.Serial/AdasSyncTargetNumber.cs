using Racelogic.Comms.Serial.Properties;
using Racelogic.Core;

namespace Racelogic.Comms.Serial;

public enum AdasSyncTargetNumber : byte
{
	[LocalizableDescription("AdasSync_AllTargets", typeof(Racelogic.Comms.Serial.Properties.Resources))]
	AllTargets,
	[LocalizableDescription("AdasSync_Target1", typeof(Racelogic.Comms.Serial.Properties.Resources))]
	Target1,
	[LocalizableDescription("AdasSync_Target2", typeof(Racelogic.Comms.Serial.Properties.Resources))]
	Target2,
	[LocalizableDescription("AdasSync_Target3", typeof(Racelogic.Comms.Serial.Properties.Resources))]
	Target3
}
