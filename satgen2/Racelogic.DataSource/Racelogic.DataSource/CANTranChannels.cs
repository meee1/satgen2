using Racelogic.Core;

namespace Racelogic.DataSource;

public enum CANTranChannels
{
	[LocalizableDescription("CANTranChannels_Lights", typeof(Resources))]
	Lights,
	[LocalizableDescription("CANTranChannels_Brake", typeof(Resources))]
	Brake,
	[LocalizableDescription("CANTranChannels_Reverse", typeof(Resources))]
	Reverse,
	[LocalizableDescription("CANTranChannels_RPM", typeof(Resources))]
	RPM,
	[LocalizableDescription("CANTranChannels_Speed", typeof(Resources))]
	Speed,
	[LocalizableDescription("CANTranChannels_IgnitionKey", typeof(Resources))]
	IgnitionKey
}
