using Racelogic.Core;

namespace Racelogic.DataSource;

public enum TemperatureUnit
{
	[LocalizableDescription("TemperatureUnit_DegreeC", typeof(Resources))]
	DegreeC,
	[LocalizableDescription("TemperatureUnit_Fahrenheit", typeof(Resources))]
	Fahrenheit,
	[LocalizableDescription("TemperatureUnit_Kelvin", typeof(Resources))]
	Kelvin
}
