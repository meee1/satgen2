using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum NmeaActiveIndicator
{
	[LocalizableDescription("NmeaActiveIndicator_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("NmeaActiveIndicator_Active", typeof(Resources))]
	Active = 65,
	[LocalizableDescription("NmeaActiveIndicator_Void", typeof(Resources))]
	Void = 86
}
