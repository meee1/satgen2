using Racelogic.Core;

namespace Racelogic.DataSource.Nmea;

public enum NmeaFixQuality
{
	[LocalizableDescription("NmeaFixQuality_NoData", typeof(Resources))]
	NoData = 0,
	[LocalizableDescription("NmeaFixQuality_Invalid", typeof(Resources))]
	Invalid = 48,
	[LocalizableDescription("NmeaFixQuality_GpsFix", typeof(Resources))]
	GpsFix = 49,
	[LocalizableDescription("NmeaFixQuality_DgpsFix", typeof(Resources))]
	DgpsFix = 50,
	[LocalizableDescription("NmeaFixQuality_PpsFix", typeof(Resources))]
	PpsFix = 51,
	[LocalizableDescription("NmeaFixQuality_Rtk", typeof(Resources))]
	Rtk = 52,
	[LocalizableDescription("NmeaFixQuality_FloatRtk", typeof(Resources))]
	FloatRtk = 53,
	[LocalizableDescription("NmeaFixQuality_Estimated", typeof(Resources))]
	Estimated = 54,
	[LocalizableDescription("NmeaFixQuality_Manual", typeof(Resources))]
	Manual = 55,
	[LocalizableDescription("NmeaFixQuality_Simulation", typeof(Resources))]
	Simulation = 56
}
