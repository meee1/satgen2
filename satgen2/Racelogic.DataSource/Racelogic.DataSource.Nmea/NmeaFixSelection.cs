using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum NmeaFixSelection
{
	[LocalizableDescription("NmeaFixSelection_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("NmeaFixSelection_Auto", typeof(Resources))]
	Auto = 65,
	[LocalizableDescription("NmeaFixSelection_Manual", typeof(Resources))]
	Manual = 77
}
