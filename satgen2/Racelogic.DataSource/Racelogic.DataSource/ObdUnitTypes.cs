using Racelogic.Core;

namespace Racelogic.DataSource;

public enum ObdUnitTypes
{
	[LocalizableDescription("Rpm", typeof(Resources))]
	Rpm,
	[LocalizableDescription("TemperatureUnit_DegreeC", typeof(Resources))]
	DegreesC,
	[LocalizableDescription("TemperatureUnit_Fahrenheit", typeof(Resources))]
	DegreesF,
	[LocalizableDescription("Percent", typeof(Resources))]
	Percent,
	[LocalizableDescription("PressureUnit_KiloPascal", typeof(Resources))]
	kPa,
	[LocalizableDescription("PressureUnit_Bar", typeof(Resources))]
	Bar,
	[LocalizableDescription("PressureUnit_mBar", typeof(Resources))]
	mBar,
	[LocalizableDescription("PressureUnit_Psi", typeof(Resources))]
	Psi,
	[LocalizableDescription("GramsPerSec", typeof(Resources))]
	GramsPerSec,
	[LocalizableDescription("SpeedUnit_KilometresPerHour", typeof(Resources))]
	Kmh,
	[LocalizableDescription("SpeedUnit_MilesPerHour", typeof(Resources))]
	Mph,
	[LocalizableDescription("Miles", typeof(Resources))]
	Miles,
	[LocalizableDescription("Kilometres", typeof(Resources))]
	Kilometres,
	[LocalizableDescription("Degrees", typeof(Resources))]
	Degrees,
	[LocalizableDescription("Radians", typeof(Resources))]
	Radians
}
