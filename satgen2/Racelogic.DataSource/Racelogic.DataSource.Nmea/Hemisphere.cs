using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum Hemisphere
{
	[LocalizableDescription("Hemisphere_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("Hemisphere_North", typeof(Resources))]
	North = 78,
	[LocalizableDescription("Hemisphere_South", typeof(Resources))]
	South = 83,
	[LocalizableDescription("Hemisphere_East", typeof(Resources))]
	East = 69,
	[LocalizableDescription("Hemisphere_West", typeof(Resources))]
	West = 87
}
