using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum NmeaModeIndicator
{
	[LocalizableDescription("NmeaModeIndicator_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("NmeaModeIndicator_Autonomous", typeof(Resources))]
	Autonomous = 65,
	[LocalizableDescription("NmeaModeIndicator_Differential", typeof(Resources))]
	Differential = 68,
	[LocalizableDescription("NmeaModeIndicator_Estimated", typeof(Resources))]
	Estimated = 69,
	[LocalizableDescription("NmeaModeIndicator_NotValid", typeof(Resources))]
	NotValid = 78,
	[LocalizableDescription("NmeaModeIndicator_Simulator", typeof(Resources))]
	Simulator = 83
}
