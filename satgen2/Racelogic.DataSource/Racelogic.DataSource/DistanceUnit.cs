using Racelogic.Core;

namespace Racelogic.DataSource;

public enum DistanceUnit
{
	[LocalizableDescription("DistanceUnit_Metres", typeof(Resources))]
	Metres,
	[LocalizableDescription("DistanceUnit_Feet", typeof(Resources))]
	Feet,
	[LocalizableDescription("DistanceUnit_Kilometres", typeof(Resources))]
	Kilometres,
	[LocalizableDescription("DistanceUnit_Miles", typeof(Resources))]
	Miles,
	[LocalizableDescription("DistanceUnit_NauticalMiles", typeof(Resources))]
	NauticalMiles
}
