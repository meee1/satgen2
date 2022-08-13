using Racelogic.Core;

namespace Racelogic.DataSource;

public enum AccelerationUnit
{
	[LocalizableDescription("AccelerationUnit_G", typeof(Resources))]
	G,
	[LocalizableDescription("AccelerationUnit_MetresPerSecondSquared", typeof(Resources))]
	MetresPerSecondSquared,
	[LocalizableDescription("AccelerationUnit_FeetPerSecondSquared", typeof(Resources))]
	FeetPerSecondSquared
}
