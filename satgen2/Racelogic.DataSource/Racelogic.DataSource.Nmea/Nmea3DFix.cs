using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum Nmea3DFix
{
	[LocalizableDescription("Nmea3DFix_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("Nmea3DFix_NA", typeof(Resources))]
	NA = 48,
	[LocalizableDescription("Nmea3DFix_NoFix", typeof(Resources))]
	NoFix = 49,
	[LocalizableDescription("Nmea3DFix_Fix2D", typeof(Resources))]
	Fix2D = 50,
	[LocalizableDescription("Nmea3DFix_Fix3D", typeof(Resources))]
	Fix3D = 51
}
