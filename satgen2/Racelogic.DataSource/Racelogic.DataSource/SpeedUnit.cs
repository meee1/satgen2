using Racelogic.Core;

namespace Racelogic.DataSource;

public enum SpeedUnit
{
	[LocalizableDescription("SpeedUnit_KilometresPerHour", typeof(Resources))]
	KilometresPerHour,
	[LocalizableDescription("SpeedUnit_MilesPerHour", typeof(Resources))]
	MilesPerHour,
	[LocalizableDescription("SpeedUnit_Knots", typeof(Resources))]
	Knots,
	[LocalizableDescription("SpeedUnit_MetresPerSecond", typeof(Resources))]
	MetresPerSecond,
	[LocalizableDescription("SpeedUnit_FeetPerSecond", typeof(Resources))]
	FeetPerSecond
}
